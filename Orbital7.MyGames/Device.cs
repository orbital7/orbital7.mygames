using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orbital7.MyGames
{
    public enum DeviceSyncType
    {
        AllExcept,

        OnlySelected,
    }

    public class Device
    {
        public string DirectoryKey { get; set; }
        
        public string Name { get; set; }

        public string Address { get; set; }

        public DateTime? LastSyncedDate { get; set; }

        public DeviceSyncType SyncType { get; set; } = DeviceSyncType.AllExcept;

        public List<Platform> SyncPlatformExceptions { get; set; } = new List<Platform>();

        public List<Platform> SyncPlatformSelections { get; set; } = new List<Platform>();

        public string RomsPath
        {
            get { return Path.Combine("\\\\" + this.Address, "roms"); }
        }

        public override string ToString()
        {
            return this.Name;
        }

        public bool SyncPlatform(Platform platform)
        {
            if (this.SyncType == DeviceSyncType.AllExcept)
                return !this.SyncPlatformExceptions.Contains(platform);
            else if (this.SyncType == DeviceSyncType.OnlySelected)
                return this.SyncPlatformSelections.Contains(platform);
            else
                return false;
        }
    }
}
