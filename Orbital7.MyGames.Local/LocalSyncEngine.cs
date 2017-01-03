using System;
using System.Diagnostics;
using System.IO;

namespace Orbital7.MyGames.Local
{
    public class LocalSyncEngine
    {
        private IAccessProvider AccessProvider { get; set; }

        public LocalSyncEngine()
        {
            this.AccessProvider = new LocalAccessProvider();
        }

        public void SyncWithDevice(Catalog catalog, string deviceDirectoryKey)
        {
            // Load the device from the configuration.
            var config = Config.Load<CatalogConfig>(this.AccessProvider, catalog.Config.FolderPath);
            var device = config.FindDevice(deviceDirectoryKey);
            if (device == null)
                throw new Exception("Device " + deviceDirectoryKey + " could not be found");

            // Test connection.
            bool exists = this.AccessProvider.FolderExists(device.RomsPath);
            if (!exists)
                throw new Exception(device.Name + " game path is not accessible: " + device.RomsPath);

            // Loop.
            int index = 0;
            foreach (var gameList in catalog.GameLists)
            {
                index++;
                Debug.WriteLine("Syncing GameList " + index + "/" + catalog.GameLists.Count + ": " +
                    gameList.Platform.ToDisplayString());

                string devicePlatformPath = Path.Combine(device.RomsPath, Path.GetFileName(gameList.PlatformFolderPath));
                if (device.SyncPlatform(gameList.Platform))
                {
                    this.AccessProvider.EnsureFolderExists(devicePlatformPath);
                    string deviceImageFolderPath = this.AccessProvider.EnsureFolderExists(devicePlatformPath, GameList.ImagesFolderName);
                    ProcessDeviceFiles(devicePlatformPath, deviceImageFolderPath, gameList);
                    PushGameFilesToDevice(device, devicePlatformPath, deviceImageFolderPath, gameList);
                    this.AccessProvider.CopyFile(gameList.FilePath, GameList.GetFilePath(devicePlatformPath));
                }
                else if (this.AccessProvider.FolderExists(devicePlatformPath))
                {
                    this.AccessProvider.DeleteFolderContents(devicePlatformPath);
                }
            }

            // Update the device.
            device.LastSyncedDate = DateTime.UtcNow;
            catalog.Config.Save();
        }

        private void ProcessDeviceFiles(string devicePlatformPath, string deviceImageFolderPath, GameList gameList)
        {
            var gameFileExtensions = GameList.GetPlatformFileExtensions(gameList.Platform);
            var saveStateFileExtensions = Catalog.GetSaveStateFileExtensions();

            foreach (string deviceFilePath in this.AccessProvider.GetFilePaths(devicePlatformPath))
            {
                string filename = Path.GetFileName(deviceFilePath);
                string fileExtension = Path.GetExtension(filename).ToLower();
                if (gameFileExtensions.Contains(fileExtension))
                {
                    if (!gameList.Contains(filename) && (gameList.Platform != Platform.NeoGeo && filename != "neogeo.zip"))
                    {
                        // Delete game files (could be more than one).
                        var deviceGameFilePaths = this.AccessProvider.GetFilePaths(devicePlatformPath,
                            Path.GetFileNameWithoutExtension(filename) + ".*");
                        foreach (var deviceGameFilePath in deviceGameFilePaths)
                            this.AccessProvider.DeleteFile(deviceGameFilePath);

                        // Delete game config files (should only be one, or not exist at all).
                        var deviceGameConfigFilePath = Path.Combine(devicePlatformPath, filename + ".cfg");
                        if (this.AccessProvider.FileExists(deviceGameConfigFilePath))
                            this.AccessProvider.DeleteFile(deviceGameConfigFilePath);

                        // Delete game image file (should only be one, but we don't know the extension).
                        var deviceImageFilePaths = this.AccessProvider.GetFilePaths(deviceImageFolderPath,
                            Game.GetImageFilenameWithoutExtension(filename) + ".*");
                        foreach (var deviceImageFilePath in deviceImageFilePaths)
                            this.AccessProvider.DeleteFile(deviceImageFilePath);
                    }
                }
                else if (saveStateFileExtensions.Contains(fileExtension))
                {
                    string localPath = Path.Combine(gameList.PlatformFolderPath, GameList.SaveStatesFolderName, filename);
                    if (!this.AccessProvider.FileExists(localPath) || this.AccessProvider.IsNewerCopyRequired(deviceFilePath, localPath))
                        this.AccessProvider.CopyFile(deviceFilePath, localPath);
                }
            }
        }

        private void PushGameFilesToDevice(Device device, string devicePlatformPath, string deviceImageFolderPath, GameList gameList)
        {
            foreach (Game game in gameList)
            {
                // Copy game files.
                var gameFilePaths = this.AccessProvider.GetFilePaths(gameList.PlatformFolderPath,
                    Path.GetFileNameWithoutExtension(game.GameFilename) + ".*");
                foreach (var gameFilePath in gameFilePaths)
                {
                    string deviceFilePath = Path.Combine(devicePlatformPath, Path.GetFileName(gameFilePath));
                    if (this.AccessProvider.IsDifferentCopyRequired(gameFilePath, deviceFilePath))
                    {
                        Debug.WriteLine(" - Copying " + Path.GetFileName(gameFilePath));
                        this.AccessProvider.CopyFile(gameFilePath, deviceFilePath);
                    }
                }

                // Copy save states.
                var saveStateFilePaths = this.AccessProvider.GetFilePaths(Path.Combine(gameList.PlatformFolderPath, GameList.SaveStatesFolderName),
                    Path.GetFileNameWithoutExtension(game.GameFilename) + ".*");
                foreach (var saveStateFilePath in saveStateFilePaths)
                {
                    string deviceFilePath = Path.Combine(devicePlatformPath, Path.GetFileName(saveStateFilePath));
                    if (this.AccessProvider.IsDifferentCopyRequired(saveStateFilePath, deviceFilePath))
                    {
                        Debug.WriteLine(" - Copying " + Path.GetFileName(saveStateFilePath));
                        this.AccessProvider.CopyFile(saveStateFilePath, deviceFilePath);
                    }
                }

                // Write game config file.
                string deviceGameConfigFilePath = Path.Combine(devicePlatformPath, game.GameFilename + ".cfg");
                if (game.GameConfig != Game.DEFAULT_GAME_CONFIG)
                {
                    string localContents = GetLocalGameConfigContents(game, device);
                    if (!this.AccessProvider.FileExists(deviceGameConfigFilePath) || this.AccessProvider.ReadAllText(deviceGameConfigFilePath) != localContents)
                        this.AccessProvider.WriteAllText(deviceGameConfigFilePath, localContents);
                }
                else if (this.AccessProvider.FileExists(deviceGameConfigFilePath))
                {
                    this.AccessProvider.DeleteFile(deviceGameConfigFilePath);
                }

                // Copy image file.
                if (game.HasImage)
                {
                    var deviceImageFilePath = Path.Combine(deviceImageFolderPath, game.ImageFilename);
                    if (this.AccessProvider.IsDifferentCopyRequired(game.ImageFilePath, deviceImageFilePath))
                        this.AccessProvider.CopyFile(game.ImageFilePath, deviceImageFilePath);
                }
            }
        }

        private string GetLocalGameConfigContents(Game game, Device device)
        {
            string filePath = String.Empty;
            if (game.GameConfig == Game.CUSTOM_GAME_CONFIG)
                filePath = CatalogEditor.GetLocalCustomGameConfigFilePath(game, device);
            else if (game.GameConfig != Game.DEFAULT_GAME_CONFIG)
                filePath = Path.Combine(game.GameList.PlatformFolderPath, GameList.GameConfigsFolderName,
                    device.DirectoryKey, game.GameConfig);

            if (File.Exists(filePath))
                return File.ReadAllText(filePath);
            else
                return null;
        }
    }
}
