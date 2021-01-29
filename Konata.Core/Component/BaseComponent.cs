﻿using System;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

using Konata.Core.Event;
using Konata.Core.Entity;

namespace Konata.Core.Manager
{
    public class BaseComponent
    {
        public BaseEntity Entity { get; set; }
        public ActionBlock<KonataTask> EventPipeline { get; set; }

        public virtual void EventHandler(KonataTask task) { }

        protected Task<BaseEvent> PostEvent<T>(BaseEvent anyEvent)
            where T : BaseComponent => Entity.PostEvent<T>(anyEvent);

        protected void BroadcastEvent(BaseEvent anyEvent)
            => Entity.BroadcastEvent(anyEvent);

        protected T GetComponent<T>()
            where T : BaseComponent => Entity?.GetComponent<T>();

        #region Log Methods

        protected void Log(LogLevel logLevel, string tag, string content)
            => Entity.PostEntityEvent(new LogEvent
            {
                logLevel = logLevel,
                eventMessage = $"{tag} {content}"
            });

        protected void LogV(string tag, string content)
            => Log(LogLevel.Verbose, tag, content);

        protected void LogI(string tag, string content)
            => Log(LogLevel.Information, tag, content);

        protected void LogW(string tag, string content)
            => Log(LogLevel.Warning, tag, content);

        protected void LogE(string tag, string content)
            => Log(LogLevel.Exception, tag, content);

        protected void LogF(string tag, string content)
            => Log(LogLevel.Fatal, tag, content);

        #endregion
    }
}
