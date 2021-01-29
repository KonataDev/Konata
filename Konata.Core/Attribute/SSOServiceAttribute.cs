using System;

namespace Konata.Core.Attribute
{
    /// <summary>
    /// SSO Service Attribute
    /// </summary>
    public class SSOServiceAttribute : System.Attribute
    {
        public string ServiceName { get; set; } = "";

        public string Description { get; set; } = "";

        public SSOServiceAttribute(string name, string description)
        {
            ServiceName = name;
            Description = description;
        }
    }
}
