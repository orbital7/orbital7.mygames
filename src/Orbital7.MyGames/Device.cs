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
        AllExceptSelections,

        OnlySelections,
    }

    public class Device
    {
        public string DirectoryKey { get; set; }
        
        public string Name { get; set; }

        public string Address { get; set; }

        public DateTime? LastSyncedDate { get; set; }

        public DeviceSyncType SyncType { get; set; } = DeviceSyncType.AllExceptSelections;

        public List<Platform> SyncPlatformSelections { get; set; } = new List<Platform>();

        public string RomsPath
        {
            get { return Path.Combine("\\\\" + this.Address, "roms"); }
        }

        public override string ToString()
        {
            return this.Name;
        }

        public void Validate()
        {
            if (String.IsNullOrEmpty(this.Name))
                throw new Exception("A value is required for Name");
            else if (String.IsNullOrEmpty(this.DirectoryKey))
                throw new Exception("A value is required for Directory Key");
            else if (!this.DirectoryKey.IsWindowsFileSystemSafe())
                throw new Exception("The Directory Key cannot contain the characters " + StringExtensions.IllegalWindowsFileSystemChars);
            else if (String.IsNullOrEmpty(this.Address))
                throw new Exception("A share-name or ip address is required for Address");
            else if (this.SyncType == DeviceSyncType.OnlySelections && this.SyncPlatformSelections.Count == 0)
                throw new Exception("Sync has been set for Only Selections, but no Selections have been added");
        }

        public bool SyncPlatform(Platform platform)
        {
            if (this.SyncType == DeviceSyncType.AllExceptSelections)
                return !this.SyncPlatformSelections.Contains(platform);
            else if (this.SyncType == DeviceSyncType.OnlySelections)
                return this.SyncPlatformSelections.Contains(platform);
            else
                return false;
        }
    }
}
