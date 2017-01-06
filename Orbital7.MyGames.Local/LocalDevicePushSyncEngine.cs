using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Orbital7.MyGames.Local
{
    public class LocalDevicePushSyncEngine : DeviceSyncEngine
    {
        public event DeviceSyncEngineProgressDelegate Progress;

        public LocalDevicePushSyncEngine()
        {
            this.AccessProvider = new LocalAccessProvider();
        }

        protected override async Task SyncAsync()
        {
            // Loop.
            foreach (var gameList in this.Catalog.GameLists)
            {
                if (this.Cancel)
                    break;

                NotifyProgress("Syncing " + gameList.Platform.ToDisplayString());
                this.Index++;

                string devicePlatformPath = Path.Combine(this.Device.RomsPath, Path.GetFileName(gameList.PlatformFolderPath));
                if (this.Device.SyncPlatform(gameList.Platform))
                {
                    await this.AccessProvider.EnsureFolderExistsAsync(devicePlatformPath);
                    string deviceImageFolderPath = await this.AccessProvider.EnsureFolderExistsAsync(devicePlatformPath, 
                        GameList.ImagesFolderName);
                    await ProcessDeviceFilesAsync(devicePlatformPath, deviceImageFolderPath, gameList);
                    await PushGameFilesToDeviceAsync(devicePlatformPath, deviceImageFolderPath, gameList);
                    await this.AccessProvider.CopyFileAsync(gameList.FilePath, GameList.GetFilePath(devicePlatformPath));
                }
                else if (await this.AccessProvider.FolderExistsAsync(devicePlatformPath))
                {
                    await this.AccessProvider.DeleteFolderContentsAsync(devicePlatformPath);
                }
            }

            // Update the device.
            this.Device.LastSyncedDate = DateTime.UtcNow;
            await this.Catalog.Config.SaveAsync();
        }

        private void NotifyProgress(string description)
        {
            this.Progress?.Invoke(new DeviceSyncEngineProgress()
            {
                GameListIndex = this.Index,
                GameListsCount = this.Catalog.GameLists.Count,
                Description = description,
            });
        }

        private async Task ProcessDeviceFilesAsync(string devicePlatformPath, string deviceImageFolderPath, GameList gameList)
        {
            var gameFileExtensions = GameList.GetPlatformFileExtensions(gameList.Platform);
            var saveStateFileExtensions = Catalog.GetSaveStateFileExtensions();

            foreach (string deviceFilePath in await this.AccessProvider.GetFilePathsAsync(devicePlatformPath))
            {
                if (this.Cancel)
                    break;

                string filename = Path.GetFileName(deviceFilePath);
                string fileExtension = Path.GetExtension(filename).ToLower();
                if (gameFileExtensions.Contains(fileExtension))
                {
                    if (!gameList.Contains(filename) && (gameList.Platform != Platform.NeoGeo && filename != "neogeo.zip"))
                    {
                        // Delete game files (could be more than one).
                        var deviceGameFilePaths = await this.AccessProvider.GetFilePathsAsync(devicePlatformPath,
                            Path.GetFileNameWithoutExtension(filename) + ".*");
                        foreach (var deviceGameFilePath in deviceGameFilePaths)
                            await this.AccessProvider.DeleteFileAsync(deviceGameFilePath);

                        // Delete game config files (should only be one, or not exist at all).
                        var deviceGameConfigFilePath = Path.Combine(devicePlatformPath, filename + ".cfg");
                        if (await this.AccessProvider.FileExistsAsync(deviceGameConfigFilePath))
                            await this.AccessProvider.DeleteFileAsync(deviceGameConfigFilePath);

                        // Delete game image file (should only be one, but we don't know the extension).
                        var deviceImageFilePaths = await this.AccessProvider.GetFilePathsAsync(deviceImageFolderPath,
                            Game.GetImageFilenameWithoutExtension(filename) + ".*");
                        foreach (var deviceImageFilePath in deviceImageFilePaths)
                            await this.AccessProvider.DeleteFileAsync(deviceImageFilePath);
                    }
                }
                else if (saveStateFileExtensions.Contains(fileExtension))
                {
                    string localPath = Path.Combine(gameList.PlatformFolderPath, GameList.SaveStatesFolderName, filename);
                    if (!await this.AccessProvider.FileExistsAsync(localPath) || await this.AccessProvider.IsNewerCopyRequiredAsync(deviceFilePath, localPath))
                        await this.AccessProvider.CopyFileAsync(deviceFilePath, localPath);
                }
            }
        }

        private async Task PushGameFilesToDeviceAsync(string devicePlatformPath, 
            string deviceImageFolderPath, GameList gameList)
        {
            foreach (Game game in gameList)
            {
                if (this.Cancel)
                    break;

                // Copy game files.
                var gameFilePaths = await this.AccessProvider.GetFilePathsAsync(gameList.PlatformFolderPath,
                    Path.GetFileNameWithoutExtension(game.GameFilename) + ".*");
                foreach (var gameFilePath in gameFilePaths)
                {
                    if (this.Cancel)
                        break;

                    string deviceFilePath = Path.Combine(devicePlatformPath, Path.GetFileName(gameFilePath));
                    if (await this.AccessProvider.IsDifferentCopyRequiredAsync(gameFilePath, deviceFilePath))
                    {
                        NotifyProgress(" - Copying " + Path.GetFileName(gameFilePath));
                        await this.AccessProvider.CopyFileAsync(gameFilePath, deviceFilePath);
                    }
                }

                // Copy save states.
                var saveStateFilePaths = await this.AccessProvider.GetFilePathsAsync(Path.Combine(gameList.PlatformFolderPath, 
                    GameList.SaveStatesFolderName), Path.GetFileNameWithoutExtension(game.GameFilename) + ".*");
                foreach (var saveStateFilePath in saveStateFilePaths)
                {
                    if (this.Cancel)
                        break;

                    string deviceFilePath = Path.Combine(devicePlatformPath, Path.GetFileName(saveStateFilePath));
                    if (await this.AccessProvider.IsDifferentCopyRequiredAsync(saveStateFilePath, deviceFilePath))
                    {
                        NotifyProgress(" - Copying " + Path.GetFileName(saveStateFilePath));
                        await this.AccessProvider.CopyFileAsync(saveStateFilePath, deviceFilePath);
                    }
                }

                // Write game config file.
                string deviceGameConfigFilePath = Path.Combine(devicePlatformPath, game.GameFilename + ".cfg");
                if (game.GameConfig != Game.DEFAULT_GAME_CONFIG)
                {
                    string localContents = await GetLocalGameConfigContentsAsync(game);
                    if (!await this.AccessProvider.FileExistsAsync(deviceGameConfigFilePath) || await this.AccessProvider.ReadAllTextAsync(deviceGameConfigFilePath) != localContents)
                        await this.AccessProvider.WriteAllTextAsync(deviceGameConfigFilePath, localContents);
                }
                else if (await this.AccessProvider.FileExistsAsync(deviceGameConfigFilePath))
                {
                    await this.AccessProvider.DeleteFileAsync(deviceGameConfigFilePath);
                }

                // Copy image file.
                if (game.HasImage)
                {
                    var deviceImageFilePath = Path.Combine(deviceImageFolderPath, game.ImageFilename);
                    if (await this.AccessProvider.IsDifferentCopyRequiredAsync(game.ImageFilePath, deviceImageFilePath))
                        await this.AccessProvider.CopyFileAsync(game.ImageFilePath, deviceImageFilePath);
                }
            }
        }

        private async Task<string> GetLocalGameConfigContentsAsync(Game game)
        {
            string filePath = String.Empty;
            if (game.GameConfig == Game.CUSTOM_GAME_CONFIG)
                filePath = await CatalogEditor.GetLocalCustomGameConfigFilePathAsync(game, this.Device);
            else if (game.GameConfig != Game.DEFAULT_GAME_CONFIG)
                filePath = Path.Combine(game.GameList.PlatformFolderPath, GameList.GameConfigsFolderName,
                    this.Device.DirectoryKey, game.GameConfig);

            if (File.Exists(filePath))
                return File.ReadAllText(filePath);
            else
                return null;
        }
    }
}
