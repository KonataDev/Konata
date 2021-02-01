using System;
using System.Text;

using Konata.Core.Event;

namespace Konata
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class EventAttribute : Attribute
    {
        public Type Event { get; set; }

        public EventAttribute(Type type)
        {
            Event = type;
        }
    }
}
