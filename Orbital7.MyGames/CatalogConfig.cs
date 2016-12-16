using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orbital7.MyGames
{
    public class CatalogConfig : Config
    {
        public List<PlatformConfig> PlatformConfigs { get; set; } = new List<PlatformConfig>();

        public List<Device> Devices { get; set; } = new List<Device>();

        public CatalogConfig(IAccessProvider accessProvider)
            : base(accessProvider) { }

        public CatalogConfig(IAccessProvider accessProvider, string folderPath)
            : base(accessProvider, folderPath) { }

        public CatalogConfig(IAccessProvider accessProvider, string folderPath, string romsFolderPath)
            : base(accessProvider, folderPath, romsFolderPath) { }

        public override string ToString()
        {
            return "Catalog Config";
        }

        public Device FindDevice(string directoryKey)
        {
            return (from x in this.Devices
                    where x.DirectoryKey.ToLower() == directoryKey.ToLower()
                    select x).FirstOrDefault();
        }

        public PlatformConfig FindPlatformConfig(Platform platform)
        {
            return (from x in this.PlatformConfigs
                    where x.Platform == platform
                    select x).FirstOrDefault();
        }
    }
}
