using System;
using System.Text;

using Konata.Core.Service;

namespace Konata.Core.Manager
{
    [Component("ConfigComponent", "Konata Config Management Component")]
    public class ConfigComponent : BaseComponent
    {
        public SignInfo SignInfo { get; private set; }

        public ConfigComponent()
        {

        }

        public void LoadConfig(BotConfig config)
        {

        }
    }
}
