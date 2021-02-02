using System;
using System.Text;

namespace Konata.Core
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
