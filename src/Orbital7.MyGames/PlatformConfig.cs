using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orbital7.MyGames
{
    public class PlatformConfig
    {
        public Platform Platform { get; set; }

        public List<string> Emulators { get; set; } = new List<string>();

        public List<string> GameConfigs { get; set; } = new List<string>();

        public override string ToString()
        {
            return this.Platform.ToDisplayString();
        }
    }
}
