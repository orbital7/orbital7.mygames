using System;
using System.Collections.Generic;
using System.IO;
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

            // Validate.
            await ValidateAsync(catalog, device);

            // Validate complete, record.
            this.Catalog = catalog;
            this.Device = device;
        }

        protected abstract Task ValidateAsync(Catalog catalog, Device device);

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

        protected void NotifyProgress(DeviceSyncEngineProgressDelegate progress, string description)
        {
            progress?.Invoke(new DeviceSyncEngineProgress()
            {
                GameListIndex = this.Index,
                GameListsCount = this.Catalog.GameLists.Count,
                Description = description,
            });
        }

        protected void DeleteGameFiles(string platformFolderPath, string imagesFolderPath, string filename)
        {
            // Delete game files (could be more than one).
            var deviceGameFilePaths = Directory.GetFiles(platformFolderPath,
                Path.GetFileNameWithoutExtension(filename) + ".*");
            foreach (var deviceGameFilePath in deviceGameFilePaths)
                File.Delete(deviceGameFilePath);

            // Delete game config files (should only be one, or not exist at all).
            var deviceGameConfigFilePath = Path.Combine(platformFolderPath, filename + ".cfg");
            if (File.Exists(deviceGameConfigFilePath))
                File.Delete(deviceGameConfigFilePath);

            // Delete game image file (should only be one, but we don't know the extension).
            var deviceImageFilePaths = Directory.GetFiles(imagesFolderPath,
                Game.GetImageFilenameWithoutExtension(filename) + ".*");
            foreach (var deviceImageFilePath in deviceImageFilePaths)
                File.Delete(deviceImageFilePath);
        }
    }
}
