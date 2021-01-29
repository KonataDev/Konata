using System;
using System.Text;

namespace Konata.Core.Attribute
{
    /// <summary>
    /// Konata Component Attribute
    /// </summary>
    public class ComponentAttribute : System.Attribute
    {
        public string ComponentName { get; set; } = "";

        public string Description { get; set; } = "";

        public ComponentAttribute(string name, string description)
        {
            ComponentName = name;
            Description = description;
        }
    }
}
