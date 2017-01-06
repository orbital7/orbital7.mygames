using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orbital7.MyGames
{
    public delegate void DeviceSyncEngineProgressDelegate(DeviceSyncEngineProgress updatedProgress);

    public abstract class DeviceSyncEngine
    {
        protected IAccessProvider AccessProvider { get; set; }

        protected int Index { get; set; }

        protected Catalog Catalog { get; set; }

        protected Device Device { get; set; }

        protected bool Cancel { get; set; }

        private void Clear()
        {
            this.Catalog = null;
            this.Device = null;
            this.Index = 0;
            this.Cancel = false;
        }

        public async Task LoadAsync(Catalog catalog, Device device)
        {
            // Clear.
            Clear();

            // Test connection.
            bool exists = await this.AccessProvider.FolderExistsAsync(device.RomsPath);
            if (!exists)
                throw new Exception(device.Name + " game path is not accessible: " + device.RomsPath);

            // Validate complete, record.
            this.Catalog = catalog;
            this.Device = device;
        }

        public async Task StartSyncAsync()
        {
            // Ensure device exists.
            if (this.Device == null)
                throw new Exception("No device has been loaded into the Device Sync Engine");

            await SyncAsync();
        }

        protected abstract Task SyncAsync();

        public void StopSync()
        {
            this.Cancel = true;
        }
    }
}
