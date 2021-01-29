using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Collections.Generic;

using Konata.Core.Event;
using Konata.Core.Manager;

namespace Konata.Core.Entity
{
    public class BaseEntity
    {
        private ActionBlock<BaseEvent> _eventHandler;
        private Dictionary<Type, BaseComponent> _componentDict
            = new Dictionary<Type, BaseComponent>();

        public void SetEventHandler(ActionBlock<BaseEvent> handler)
            => _eventHandler = handler;

        /// <summary>
        /// Get component which attached on this entity
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetComponent<T>()
            where T : BaseComponent
        {
            if (!_componentDict.TryGetValue(typeof(T), out BaseComponent component))
            {
                return default(T);
            }
            return (T)component;
        }

        /// <summary>
        /// Add component to this entity
        /// </summary>
        /// <param name="component"></param>
        public void AddComponent(BaseComponent component)
        {
            if (_componentDict.TryGetValue(component.GetType(), out var _))
            {
                _componentDict[component.GetType()] = component;
                return;
            }

            component.Entity = this;
            _componentDict.Add(component.GetType(), component);
        }

        /// <summary>
        /// Delete component
        /// </summary>
        /// <param name="type"></param>
        public void RemoveComponent(Type type)
        {
            if (!_componentDict.TryGetValue(type, out BaseComponent component))
            {
                return;
            }
            _componentDict.Remove(type);
            return;
        }

        /// <summary>
        /// Post an event to entity
        /// </summary>
        /// <param name="anyEvent"></param>
        public void PostEntityEvent(BaseEvent anyEvent)
            => _eventHandler.SendAsync(anyEvent);

        /// <summary>
        /// Post an event to any component attached under this entity
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="anyEvent"></param>
        /// <returns></returns>
        public Task<BaseEvent> PostEvent<T>(BaseEvent anyEvent)
            where T : BaseComponent
        {
            var task = new KonataTask(anyEvent);
            {
                GetComponent<T>().EventPipeline.SendAsync(task);
            }

            return task.Payload.Task;
        }

        /// <summary>
        /// Broad an event to all components
        /// </summary>
        /// <param name="anyEvent"></param>
        public void BroadcastEvent(BaseEvent anyEvent)
        {
            foreach (var component in _componentDict)
            {
                component.Value.EventPipeline.SendAsync(new KonataTask(anyEvent));
            }
        }

        public static Task RunAsync(Action action)
        {
            var taskSource = new TaskCompletionSource<Object>();

            ThreadPool.QueueUserWorkItem((_) =>
            {
                try
                {
                    action();
                    taskSource.SetResult(null);
                }
                catch (Exception e)
                {
                    taskSource.SetException(e);
                }
            });

            return taskSource.Task;
        }
    }

    public class KonataTask
    {
        public TaskCompletionSource<BaseEvent> Payload { get; }

        public KonataTask(BaseEvent e)
        {
            Payload = new TaskCompletionSource<BaseEvent>(e);
        }
    }
}
