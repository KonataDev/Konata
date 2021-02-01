using System;
using System.Text;

namespace Konata.Core.Event
{
    public class BaseEvent
    {
        public ulong EventTime { get; set; }

        public string EventMessage { get; set; }
    }
}
