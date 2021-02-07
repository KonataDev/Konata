using System;
using System.Text;

namespace Konata.Core
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class ParseEventAttribute : Attribute
    {
        public Type Event { get; set; }

        public ParseEventAttribute(Type type)
        {
            Event = type;
        }
    }
}
