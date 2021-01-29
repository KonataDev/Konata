using System;
using System.Text;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;

using Konata.Core.Event;
using Konata.Core.Entity;
using Konata.Core.Service;
using Konata.Core.Attribute;

namespace Konata.Core.Manager
{
    [Component("PacketComponent", "Konata Packet Translation Component")]
    public class PacketComponent : BaseComponent
    {
        private Dictionary<string, ISSOService> _ssoServices;
        private Dictionary<Type, ISSOService> _ssoServicesType;

        private int _globalSequence;
        private ConcurrentDictionary<string, int> _serviceSequence;

        /// <summary>
        /// Get sequence with auto increment
        /// </summary>
        public int NewSequence { get => GetNewSequence(); }

        /// <summary>
        /// Get/Set current sequence
        /// </summary>
        public int CurrentSequence { get => _globalSequence; }

        /// <summary>
        /// Get Session
        /// </summary>
        public uint Session { get; set; }

        public PacketComponent()
        {
            _globalSequence = 10000;
            _serviceSequence = new ConcurrentDictionary<string, int>();

            LoadSSOService();
        }

        /// <summary>
        /// Load sso service
        /// </summary>
        private void LoadSSOService()
        {
            // Create sso services
            foreach (var type in typeof(ISSOService).Assembly.GetTypes())
            {
                var attribute = (SSOServiceAttribute)type.GetCustomAttributes(typeof(SSOServiceAttribute));
                if (attribute != null && typeof(SSOServiceAttribute).IsAssignableFrom(type))
                {
                    var service = (ISSOService)Activator.CreateInstance(type);
                    {
                        _ssoServices.Add(attribute.ServiceName, service);
                        _ssoServicesType.Add(type, service);
                    }
                }
            }
        }

        /// <summary>
        /// Get sequence with auto increment
        /// </summary>
        /// <returns></returns>
        public int GetNewSequence()
        {
            Interlocked.CompareExchange(ref _globalSequence, 10000, int.MaxValue);
            return Interlocked.Add(ref _globalSequence, 1);
        }

        /// <summary>
        /// Get sequence by service name
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        public int GetServiceSequence(string service)
        {
            // Get service sequence by name
            if (_serviceSequence.TryGetValue(service, out var sequence))
            {
                return sequence;
            }

            sequence = GetNewSequence();

            // Record this sequence
            if (_serviceSequence.TryAdd(service, sequence))
            {
                return sequence;
            }

            throw new Exception("Get service sequence failed.");
        }

        /// <summary>
        /// Destroy sequence by service name
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        public bool DestroyServiceSequence(string service)
            => _serviceSequence.TryRemove(service, out var _) || true;

        /// <summary>
        /// Get SSO service
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private T GetSSOService<T>()
            where T : ISSOService
        {
            if (!_ssoServicesType.TryGetValue(typeof(T), out var service))
            {
                return default;
            }
            return (T)service;
        }

        public override void EventHandler(KonataTask task)
        {

        }
    }
}
