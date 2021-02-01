using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;

using Konata.Core.Utils;
using Konata.Core.Event;
using Konata.Core.Entity;
using Konata.Core.Service;
using Konata.Core.Packet;

namespace Konata.Core.Manager
{
    [Component("PacketComponent", "Konata Packet Translation Component")]
    public class PacketComponent : BaseComponent
    {
        private const string TAG = "PacketComponent";

        private Dictionary<string, ISSOService> _services;
        private Dictionary<Type, ISSOService> _servicesType;
        private Dictionary<Type, List<ISSOService>> _servicesEventType;

        private ConcurrentDictionary<int, TaskCompletionSource<BaseEvent>> _pendingRequests;

        private Sequence _serviceSequence;

        public PacketComponent()
        {
            _serviceSequence = new Sequence();
            _services = new Dictionary<string, ISSOService>();
            _servicesType = new Dictionary<Type, ISSOService>();
            _servicesEventType = new Dictionary<Type, List<ISSOService>>();
            _pendingRequests = new ConcurrentDictionary<int, TaskCompletionSource<BaseEvent>>();

            LoadService();
        }

        /// <summary>
        /// Load sso service
        /// </summary>
        private void LoadService()
        {
            // Initialize protocol event types
            foreach (var type in Reflection.GetChildClasses<ProtocolEvent>())
            {
                _servicesEventType.Add(type, new List<ISSOService>());
            }

            // Create sso services
            foreach (var type in Reflection.GetClassesByAttribute<SSOServiceAttribute>())
            {
                var eventAttrs = type.GetCustomAttributes<EventAttribute>();
                var serviceAttr = type.GetCustomAttribute<SSOServiceAttribute>();

                if (serviceAttr != null)
                {
                    var service = (ISSOService)Activator.CreateInstance(type);

                    // Bind service name with service
                    _services.Add(serviceAttr.ServiceName, service);
                    _servicesType.Add(type, service);

                    // Bind protocol event type with service
                    foreach (var attr in eventAttrs)
                    {
                        _servicesEventType[attr.GetType()].Add(service);
                    }
                }
            }
        }

        public override void EventHandler(KonataTask task)
        {
            var config = GetComponent<ConfigComponent>();

            if (task.EventPayload is PacketEvent packetEvent)
            {
                // Parse service message
                if (ServiceMessage.Parse(packetEvent.Buffer, config.SignInfo, out var serviceMsg))
                {
                    // Parse SSO frame
                    if (SSOFrame.Parse(serviceMsg, out var ssoFrame))
                    {
                        // Get SSO service by sso command
                        if (_services.TryGetValue(ssoFrame.Command, out var service))
                        {
                            // Translate bytes to ProtocolEvent 
                            if (service.Parse(ssoFrame, config.SignInfo, out var outEvent))
                            {
                                // Get pending request
                                if (_pendingRequests.TryRemove(ssoFrame.Sequence, out var request))
                                {
                                    request.SetResult(outEvent);
                                }
                                else
                                {
                                    PostEvent<BusinessComponent>(outEvent);
                                }
                            }
                            else LogW(TAG, $"This sso frame cannot be processed. { ssoFrame.Command } { ssoFrame.Payload }");
                        }
                        else LogW(TAG, $"Unsupported sso frame received. { ssoFrame.Command } { ssoFrame.Payload }");
                    }
                    else LogW(TAG, $"Parse sso frame failed. { ssoFrame.Command } { ssoFrame.Payload }");
                }
                else LogW(TAG, $"Parse service message failed. \n D2 => { config.SignInfo.D2Key }\n Buffer => { packetEvent.Buffer }");
            }

            // Protocol Event
            else if (task.EventPayload is ProtocolEvent protocolEvent)
            {
                if (!_servicesEventType.TryGetValue(protocolEvent.GetType(), out var serviceList))
                {
                    task.CompletionSource.SetResult(null);
                    return;
                }

                foreach (var service in serviceList)
                {
                    if (service.Build(_serviceSequence, protocolEvent, config.SignInfo, out var sequence, out var buffer))
                    {
                        PostEvent<SocketComponent>(new PacketEvent
                        {
                            Buffer = buffer,
                            EventType = PacketEvent.Type.Send
                        });

                    AddPending:
                        if (!_pendingRequests.TryAdd(sequence, task.CompletionSource))
                        {
                            _pendingRequests[sequence].SetCanceled();
                            _pendingRequests.TryRemove(sequence, out _);

                            goto AddPending;
                        }
                    }
                }
            }

            // Unsupported event
            else
            {
                LogW(TAG, "Unsupported Event received?");
            }
        }
    }
}