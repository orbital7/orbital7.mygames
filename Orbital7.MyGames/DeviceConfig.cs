using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orbital7.MyGames
{
    public class DeviceConfig : Config
    {
        public Device Device { get; set; }

        public DeviceConfig(IAccessProvider accessProvider)
            : base(accessProvider) { }

        public DeviceConfig(IAccessProvider accessProvider, string folderPath)
            : base(accessProvider, folderPath) { }

        public DeviceConfig(IAccessProvider accessProvider, string folderPath, string romsFolderPath)
            : base(accessProvider, folderPath, romsFolderPath) { }

        public override string ToString()
        {
            return this.Device.Name + " Config";
        }
    }
}
