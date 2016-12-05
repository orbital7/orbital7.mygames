using Orbital7.Extensions.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orbital7.MyGames
{
    public class Catalog
    {
        public List<GameList> GameLists { get; set; } = new List<GameList>();

        public Catalog(Config config)
        {
            foreach (var platformFolderPath in Directory.GetDirectories(config.GamesFolderPath))
            {
                var platform = GameList.GetPlatform(Path.GetFileName(platformFolderPath));
                if (platform.HasValue)
                    this.GameLists.Add(GameList.Load(platformFolderPath, config));
            }
        }

        public List<Game> GatherIncompleteGames()
        {
            return (from x in this.GameLists
                    from Game y in x
                    where !y.HasImage
                    select y).ToList();
        }

        public static List<Game> IdentifyIncompleteGames(List<Game> games)
        {
            return (from x in games
                    where !x.HasImage
                    orderby x.Name
                    select x).ToList();
        }

        public void SyncWithDevice(string deviceDirectoryKey)
        {
            // Load the device from the configuration.
            var config = Config.Load();
            var device = config.FindDevice(deviceDirectoryKey);
            if (device == null)
                throw new Exception("Device " + deviceDirectoryKey + " could not be found");

            // Test connection.
            bool exists = Directory.Exists(device.GamesPath);
            if (!exists)
                throw new Exception(device.Name + " game path is not accessible: " + device.GamesPath);

            // Loop.
            int index = 0;
            foreach (var gameList in this.GameLists)
            {
                index++;
                Debug.WriteLine("Syncing GameList " + index + "/" + this.GameLists.Count + ": " + 
                    gameList.Platform.ToDisplayString());

                string devicePlatformPath = Path.Combine(device.GamesPath, Path.GetFileName(gameList.PlatformFolderPath));
                string deviceImageFolderPath = FileSystemHelper.EnsureFolderExists(devicePlatformPath, GameList.ImagesFolderName);
                ProcessDeviceFiles(devicePlatformPath, deviceImageFolderPath, gameList);
                PushGameFilesToDevice(device, devicePlatformPath, deviceImageFolderPath, gameList);
                gameList.Save(devicePlatformPath);
            }

            // Update the device.
            device.LastSyncedDate = DateTime.UtcNow;
            config.Save();
        }

        private void ProcessDeviceFiles(string devicePlatformPath, string deviceImageFolderPath, GameList gameList)
        {
            var gameFileExtensions = GameList.GetPlatformFileExtensions(gameList.Platform);
            var saveStateFileExtensions = GetSaveStateFileExtensions();

            foreach (string deviceFilePath in Directory.GetFiles(devicePlatformPath))
            {
                string filename = Path.GetFileName(deviceFilePath);
                string fileExtension = Path.GetExtension(filename).ToLower();
                if (gameFileExtensions.Contains(fileExtension))
                {
                    if (!gameList.Contains(filename) && (gameList.Platform != Platform.NeoGeo && filename != "neogeo.zip"))
                    {
                        // Delete game files (could be more than one).
                        var gameFilePaths = Directory.GetFiles(devicePlatformPath,
                            Path.GetFileNameWithoutExtension(filename) + ".*");
                        foreach (var gameFilePath in gameFilePaths)
                            File.Delete(gameFilePath);

                        // Delete game config files (should only be one, or not exist at all).
                        var configFilePath = Path.Combine(devicePlatformPath, filename + ".cfg");
                        if (File.Exists(configFilePath))
                            File.Delete(configFilePath);

                        // Delete game image file (should only be one, but we don't know the extension).
                        var imageFilePaths = Directory.GetFiles(deviceImageFolderPath,
                            Game.GetImageFilenameWithoutExtension(filename) + ".*");
                        foreach (var imageFilePath in imageFilePaths)
                            File.Delete(imageFilePath);
                    }
                }
                else if (saveStateFileExtensions.Contains(fileExtension))
                {
                    string localPath = Path.Combine(gameList.PlatformFolderPath, GameList.SaveStatesFolderName, filename);
                    if (!File.Exists(localPath) || IsNewerCopyRequired(deviceFilePath, localPath))
                        File.Copy(deviceFilePath, localPath, true);
                }
            }
        }

        private void PushGameFilesToDevice(Device device, string devicePlatformPath, string deviceImageFolderPath, GameList gameList)
        {
            foreach (Game game in gameList)
            {
                // Copy game files.
                var gameFilePaths = Directory.GetFiles(gameList.PlatformFolderPath,
                    Path.GetFileNameWithoutExtension(game.GameFilename) + ".*");
                foreach (var gameFilePath in gameFilePaths)
                {
                    string deviceFilePath = Path.Combine(devicePlatformPath, Path.GetFileName(gameFilePath));
                    if (IsDifferentCopyRequired(gameFilePath, deviceFilePath))
                    {
                        Debug.WriteLine(" - Copying " + Path.GetFileName(gameFilePath));
                        File.Copy(gameFilePath, deviceFilePath, true);
                    }
                }

                // Copy save states.
                var saveStateFilePaths = Directory.GetFiles(Path.Combine(gameList.PlatformFolderPath, GameList.SaveStatesFolderName),
                    Path.GetFileNameWithoutExtension(game.GameFilename) + ".*");
                foreach (var saveStateFilePath in saveStateFilePaths)
                {
                    string deviceFilePath = Path.Combine(devicePlatformPath, Path.GetFileName(saveStateFilePath));
                    if (IsDifferentCopyRequired(saveStateFilePath, deviceFilePath))
                    {
                        Debug.WriteLine(" - Copying " + Path.GetFileName(saveStateFilePath));
                        File.Copy(saveStateFilePath, deviceFilePath, true);
                    }
                }

                // Write gameconfig file.
                string deviceButtonMappingFilePath = Path.Combine(devicePlatformPath, game.GameFilename + ".cfg");
                if (game.GameConfig != Game.DEFAULT_GAME_CONFIG)
                {
                    string localContents = game.GetLocalGameConfigContents(device);
                    if (!File.Exists(deviceButtonMappingFilePath) || File.ReadAllText(deviceButtonMappingFilePath) != localContents)
                        File.WriteAllText(deviceButtonMappingFilePath, localContents);
                }
                else if (File.Exists(deviceButtonMappingFilePath))
                {
                    File.Delete(deviceButtonMappingFilePath);
                }

                // Copy image file.
                if (game.HasImage)
                {
                    var deviceImageFilePath = Path.Combine(deviceImageFolderPath, game.ImageFilename);
                    if (IsDifferentCopyRequired(game.ImageFilePath, deviceImageFilePath))
                        File.Copy(game.ImageFilePath, deviceImageFilePath, true);
                }
            }
        }

        private bool IsDifferentCopyRequired(string sourcePath, string destinationPath)
        {
            if (File.Exists(destinationPath))
            {
                var sourceProperties = new FileInfo(sourcePath);
                var destProperties = new FileInfo(destinationPath);
                return sourceProperties.LastWriteTimeUtc != destProperties.LastWriteTimeUtc ||
                       sourceProperties.Length != destProperties.Length;
            }
            else
            {
                return true;
            }
        }

        private bool IsNewerCopyRequired(string sourcePath, string destinationPath)
        {
            if (File.Exists(destinationPath))
            {
                var sourceProperties = new FileInfo(sourcePath);
                var destProperties = new FileInfo(destinationPath);
                return sourceProperties.LastWriteTimeUtc > destProperties.LastWriteTimeUtc;
            }
            else
            {
                return true;
            }
        }

        public static List<string> GetSaveStateFileExtensions()
        {
            return new List<string>()
            {
                ".state",
                ".state1",
                ".state2",
                ".state3",
                ".state4",
                ".state5",
                ".state6",
                ".state7",
                ".state8",
                ".state9",
            };
        }
    }
}
