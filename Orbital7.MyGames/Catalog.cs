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

        public Catalog(string folderPath)
        {
            foreach (var platformFolderPath in Directory.GetDirectories(folderPath))
            {
                var platform = GameList.GetPlatform(Path.GetFileName(platformFolderPath));
                if (platform.HasValue)
                    this.GameLists.Add(GameList.Load(platformFolderPath));
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

        public void SyncWithDevice(string deviceAddress)
        {
            // Load the device from the configuration.
            var config = Config.Load();
            var device = config.FindDevice(deviceAddress);
            if (device == null)
                throw new Exception("Device with address " + deviceAddress + " could not be found");

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
                DeleteNonReferencedGames(devicePlatformPath, deviceImageFolderPath, gameList);
                CopyReferencedGames(devicePlatformPath, deviceImageFolderPath, gameList);
                gameList.Save(devicePlatformPath);
            }

            // Update the device.
            device.LastSyncedDate = DateTime.UtcNow;
            config.Save();
        }

        private void DeleteNonReferencedGames(string devicePlatformPath, string deviceImageFolderPath, GameList gameList)
        {
            var fileExtensions = GameList.GetPlatformFileExtensions(gameList.Platform);

            foreach (string filePath in Directory.GetFiles(devicePlatformPath))
            {
                string filename = Path.GetFileName(filePath);
                if (fileExtensions.Contains(Path.GetExtension(filename).ToLower()))
                {
                    if (!gameList.Contains(filename) && (gameList.Platform != Platform.NeoGeo && filename != "neogeo.zip"))
                    {
                        // Delete game files (could be more than one).
                        var gameFilePaths = Directory.GetFiles(devicePlatformPath,
                            Path.GetFileNameWithoutExtension(filename) + ".*");
                        foreach (var gameFilePath in gameFilePaths)
                            File.Delete(gameFilePath);

                        // Delete game image file (should only be one).
                        var imageFilePaths = Directory.GetFiles(deviceImageFolderPath,
                            Game.GetImageFilenameWithoutExtension(filename) + ".*");
                        foreach (var imageFilePath in imageFilePaths)
                            File.Delete(imageFilePath);
                    }
                }
            }
        }

        private void CopyReferencedGames(string devicePlatformPath, string deviceImageFolderPath, GameList gameList)
        {
            foreach (Game game in gameList)
            {
                // Copy game files.
                var gameFilePaths = Directory.GetFiles(gameList.PlatformFolderPath,
                    Path.GetFileNameWithoutExtension(game.GameFilename) + ".*");
                foreach (var gameFilePath in gameFilePaths)
                {
                    string deviceFilePath = Path.Combine(devicePlatformPath, Path.GetFileName(gameFilePath));
                    if (IsCopyRequired(gameFilePath, deviceFilePath))
                    {
                        Debug.WriteLine(" - Copying " + Path.GetFileName(gameFilePath));
                        File.Copy(gameFilePath, deviceFilePath, true);
                    }
                }

                // Copy image files.
                if (game.HasImage)
                {
                    var deviceImageFilePath = Path.Combine(deviceImageFolderPath, game.ImageFilename);
                    if (IsCopyRequired(game.ImageFilePath, deviceImageFilePath))
                        File.Copy(game.ImageFilePath, deviceImageFilePath, true);
                }
            }
        }

        private bool IsCopyRequired(string gameFilePath, string deviceFilePath)
        {
            if (File.Exists(deviceFilePath))
            {
                var sourceProperties = new FileInfo(gameFilePath);
                var destProperties = new FileInfo(deviceFilePath);
                return sourceProperties.LastWriteTimeUtc != destProperties.LastWriteTimeUtc ||
                       sourceProperties.Length != destProperties.Length;
            }
            else
            {
                return true;
            }
        }
    }
}
