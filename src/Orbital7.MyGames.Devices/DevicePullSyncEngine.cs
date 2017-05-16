using Orbital7.Extensions.Windows;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Orbital7.MyGames.Devices
{
    public enum DevicePullSyncAction
    {
        FullSync,

        SaveStatesAndConfigsSync,

        SaveStatesPush,
    }

    public class DevicePullSyncEngine : DeviceSyncEngine
    {
        private DevicePullSyncAction Action { get; set; }

        public event DeviceSyncEngineProgressDelegate Progress;

        public DevicePullSyncEngine(DevicePullSyncAction action)
            : base()
        {
            this.Action = action;
        }

        protected override async Task ValidateAsync(Catalog catalog, Device device)
        {
            // Test connection.
            var deviceConfig = (DeviceConfig)this.Catalog.Config;
            bool exists = await this.AccessProvider.FolderExistsAsync(deviceConfig.CatalogFolderPath);
            if (!exists)
                throw new Exception(device.Name + " game path is not accessible: " + device.RomsPath);
        }

        protected override async Task SyncAsync()
        {
            var deviceConfig = (DeviceConfig)this.Catalog.Config;

            switch (this.Action)
            {
                case DevicePullSyncAction.FullSync:
                    await UpdateCatalogGameListsAsync(deviceConfig);
                    await UpdateCatalogContentsAsync(deviceConfig, true);
                    break;

                case DevicePullSyncAction.SaveStatesAndConfigsSync:
                    await UpdateCatalogContentsAsync(deviceConfig, false);
                    break;

                case DevicePullSyncAction.SaveStatesPush:
                    await PushSaveStatesAsync(deviceConfig);
                    break;
            }

            await deviceConfig.SaveAsync();

            // Notify complete.
            base.NotifySyncComplete(this.Progress);
        }

        private async Task UpdateCatalogGameListsAsync(DeviceConfig deviceConfig)
        {
            NotifyProgress(this.Progress, "Updating Catalog Game Lists\n");

            foreach (string folderPath in await this.AccessProvider.GetFolderPathsAsync(deviceConfig.RomsFolderPath))
            {
                if (this.Cancel)
                    break;

                string platformFolderName = Path.GetDirectoryName(folderPath);
                var platform = GameList.GetPlatform(platformFolderName);

                if (platform.HasValue)
                {
                    if (deviceConfig.Device.SyncPlatform(platform.Value))
                    {
                        string sourcePath = GameList.GetFilePath(Path.Combine(deviceConfig.CatalogFolderPath, platformFolderName));
                        string destPath = GameList.GetFilePath(folderPath);

                        if (await this.AccessProvider.IsNewerCopyRequiredAsync(sourcePath, destPath))
                        {
                            NotifyProgress(this.Progress, " - Downloading " + Path.GetFileName(sourcePath) + " for " + platformFolderName + "\n");
                            await this.AccessProvider.CopyFileAsync(sourcePath, destPath);
                        }
                    }
                    else
                    {
                        NotifyProgress(this.Progress, " - Removing files for " + platformFolderName + "\n");
                        FileSystemHelper.DeleteFolderContents(folderPath);

                    }
                }
            }

            // Reload the catalog.
            this.Catalog = new Catalog(this.Catalog.Config);
        }

        private async Task UpdateCatalogContentsAsync(DeviceConfig deviceConfig, bool updateGames)
        {
            var saveStateFileExtensions = Catalog.GetSaveStateFileExtensions();
            
            foreach (var gameList in this.Catalog.GameLists)
            {
                if (this.Cancel)
                    break;

                NotifyProgress(this.Progress, "Updating " + gameList.Platform + " Contents\n");
                this.Index++;

                string platformFolderName = Path.GetDirectoryName(gameList.PlatformFolderPath);

                string catalogSaveStatesFolderPath = await this.AccessProvider.EnsureFolderExistsAsync(
                    Path.Combine(deviceConfig.CatalogFolderPath, platformFolderName, GameList.SaveStatesFolderName));

                var gameConfigs = await DownloadCatalogGameConfigsAsync(deviceConfig, platformFolderName);

                foreach (Game game in gameList)
                {
                    if (this.Cancel)
                        break;

                    // Update game.
                    if (updateGames)
                        await UpdateGameAsync(deviceConfig, platformFolderName, game);

                    // Update save states.
                    var deviceSaveStateFilePaths = Directory.GetFiles(Path.Combine(gameList.PlatformFolderPath,
                        GameList.SaveStatesFolderName), Path.GetFileNameWithoutExtension(game.GameFilename) + ".*");
                    foreach (var deviceSaveStateFilePath in deviceSaveStateFilePaths)
                    {
                        if (this.Cancel)
                            break;

                        await UpdateSaveStateFileAsync(deviceSaveStateFilePath, catalogSaveStatesFolderPath);
                    }

                    // Update config file.
                    await UpdateGameConfigFileAsync(gameConfigs, game);
                }

                // Remove games.
                if (updateGames)
                    RemoveGames(deviceConfig, gameList);
            }

            // Record.
            deviceConfig.LastSaveStatesPush = DateTime.UtcNow;
        }

        private void RemoveGames(DeviceConfig deviceConfig, GameList gameList)
        {
            var gameFileExtensions = GameList.GetPlatformFileExtensions(gameList.Platform);
            FileSystemHelper.EnsureFolderExists(gameList.ImagesFolderPath);

            foreach (string deviceFilePath in Directory.GetFiles(gameList.PlatformFolderPath))
            {
                if (this.Cancel)
                    break;

                string filename = Path.GetFileName(deviceFilePath);
                string fileExtension = Path.GetExtension(filename).ToLower();
                if (gameFileExtensions.Contains(fileExtension))
                {
                    if (!gameList.Contains(filename) && (gameList.Platform != Platform.NeoGeo && filename != "neogeo.zip"))
                    {
                        NotifyProgress(this.Progress, " - Removing " + filename + "\n");
                        base.DeleteGameFiles(gameList.PlatformFolderPath, gameList.ImagesFolderPath, filename);
                    }
                }
            }
        }

        private async Task UpdateGameAsync(DeviceConfig deviceConfig, string platformFolderName, Game game)
        {
            // Game.
            string catalogGamePath = Path.Combine(deviceConfig.CatalogFolderPath, platformFolderName, game.GameFilename);
            if (await this.AccessProvider.IsDifferentCopyRequiredAsync(catalogGamePath, game.GameFilePath))
            {
                NotifyProgress(this.Progress, " - Downloading " + game.GameFilename + "\n");
                await this.AccessProvider.WriteAllBytesAsync(game.GameFilePath,
                    await this.AccessProvider.ReadAllBytesAsync(catalogGamePath));
            }

            // Game Image.
            string catalogGameImagePath = Path.Combine(deviceConfig.CatalogFolderPath, platformFolderName, GameList.ImagesFolderName, game.ImageFilename);
            if (await this.AccessProvider.IsDifferentCopyRequiredAsync(catalogGameImagePath, game.ImageFilePath))
            {
                NotifyProgress(this.Progress, " - Downloading " + game.ImageFilename + "\n");
                await this.AccessProvider.WriteAllBytesAsync(game.ImageFilePath,
                    await this.AccessProvider.ReadAllBytesAsync(catalogGameImagePath));
            }
        }

        private async Task UpdateGameConfigFileAsync(List<Tuple<string, string>> gameConfigs, Game game)
        {
            string gameConfigContent = String.Empty;
            string deviceGameConfigFilename = game.GameFilename + ".config";

            if (game.GameConfig == Game.CUSTOM_GAME_CONFIG)
            {
                gameConfigContent = (from x in gameConfigs
                                     where x.Item1.ToLower() == deviceGameConfigFilename
                                     select x.Item2).FirstOrDefault();
            }
            else if (!String.IsNullOrEmpty(game.GameConfig) && game.GameConfig != Game.DEFAULT_GAME_CONFIG)
            {
                gameConfigContent = (from x in gameConfigs
                                         where x.Item1.ToLower() == game.GameConfig.ToLower()
                                         select x.Item2).FirstOrDefault();
            }

            if (!String.IsNullOrEmpty(gameConfigContent))
            {
                string deviceGameConfigFilePath = Path.Combine(game.GameList.PlatformFolderPath, deviceGameConfigFilename);
                if (!await this.AccessProvider.FileExistsAsync(deviceGameConfigFilename) || 
                    await this.AccessProvider.ReadAllTextAsync(deviceGameConfigFilename) != gameConfigContent)
                {
                    await this.AccessProvider.WriteAllTextAsync(deviceGameConfigFilePath, gameConfigContent);
                }
            }
        }

        private async Task UpdateSaveStateFileAsync(string deviceSaveStateFilePath, string catalogSaveStatesFolderPath)
        {
            // Compare the catalog file to the device file.
            string saveStateFilename = Path.GetFileName(deviceSaveStateFilePath);
            string catalogSaveStateFilePath = Path.Combine(catalogSaveStatesFolderPath, saveStateFilename);
            int compareResult = await this.AccessProvider.CompareFileCopiesAsync(catalogSaveStateFilePath, deviceSaveStateFilePath);
            if (compareResult > 0)
            {
                NotifyProgress(this.Progress, " - Downloading " + saveStateFilename + "\n");
                await this.AccessProvider.CopyFileAsync(catalogSaveStateFilePath, deviceSaveStateFilePath);
            }
            else if (compareResult < 0)
            {
                NotifyProgress(this.Progress, " - Uploading " + saveStateFilename + "\n");
                await this.AccessProvider.CopyFileAsync(deviceSaveStateFilePath, catalogSaveStateFilePath);
            }
        }

        private async Task<List<Tuple<string, string>>> DownloadCatalogGameConfigsAsync(DeviceConfig deviceConfig, string platformFolderName)
        {
            var gameConfigs = new List<Tuple<string, string>>();

            string catalogGameConfigsFolderPath = await this.AccessProvider.EnsureFolderExistsAsync(
                    Path.Combine(deviceConfig.CatalogFolderPath, platformFolderName, GameList.GameConfigsFolderName));

            foreach (string filePath in await this.AccessProvider.GetFilePathsAsync(catalogGameConfigsFolderPath))
            {
                gameConfigs.Add(new Tuple<string, string>(Path.GetFileName(filePath),
                    await this.AccessProvider.ReadAllTextAsync(filePath)));
            }

            return gameConfigs;
        }

        private async Task PushSaveStatesAsync(DeviceConfig deviceConfig)
        {
            var saveStateFileExtensions = Catalog.GetSaveStateFileExtensions();

            foreach (var gameList in this.Catalog.GameLists)
            {
                if (this.Cancel)
                    break;

                string platformFolderName = Path.GetDirectoryName(gameList.PlatformFolderPath);

                string catalogSaveStatesFolderPath = await this.AccessProvider.EnsureFolderExistsAsync(
                    Path.Combine(deviceConfig.CatalogFolderPath, platformFolderName, GameList.SaveStatesFolderName));

                foreach (var filePath in await this.AccessProvider.GetFilePathsAsync(gameList.PlatformFolderPath))
                {
                    if (saveStateFileExtensions.Contains(Path.GetExtension(filePath).ToLower()))
                    {
                        string saveStateFilename = Path.GetFileName(filePath);
                        string catalogSaveStateFilePath = Path.Combine(catalogSaveStatesFolderPath, saveStateFilename);

                        // Upload if necessary.
                        if ((deviceConfig.LastSaveStatesPush.HasValue && new FileInfo(filePath).LastWriteTimeUtc > deviceConfig.LastSaveStatesPush.Value) ||
                            (await this.AccessProvider.IsNewerCopyRequiredAsync(filePath, catalogSaveStateFilePath)))
                        {
                            NotifyProgress(this.Progress, " - Uploading " + saveStateFilename + "\n");
                            await this.AccessProvider.CopyFileAsync(filePath, catalogSaveStateFilePath);
                        }
                    }
                }

                // Record.
                deviceConfig.LastSaveStatesPush = DateTime.UtcNow;
            }
        }
    }
}
