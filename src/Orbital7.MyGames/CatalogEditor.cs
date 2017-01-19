using ImageSharp;
using Orbital7.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Orbital7.MyGames
{
    public class CatalogEditor
    {
        public IAccessProvider AccessProvider { get; private set; }

        public CatalogConfig Config { get; private set; }

        public Catalog Catalog { get; private set; }

        public CatalogEditor(CatalogConfig catalogConfig)
        {
            this.Config = catalogConfig;
            this.AccessProvider = catalogConfig.AccessProvider;

            this.Catalog = new Catalog(catalogConfig);
            AsyncHelper.RunSync(() => InitializeAsync());
        }

        private async Task InitializeAsync()
        {
            foreach (var gameList in this.Catalog.GameLists)
            {
                await SyncGameListFilesAsync(gameList);
                SaveGameList(gameList);

                foreach (Game game in gameList)
                    await UpdateLocalCustomGameConfigFileAsync(game);
            }
        }

        public void SaveGameList(GameList gameList)
        {
            string filePath = GameList.GetFilePath(gameList.PlatformFolderPath);
            this.AccessProvider.WriteAllTextAsync(filePath, XMLSerializationHelper.SerializeToXML(gameList).Replace(
                "<Game ", "<game ").Replace("</Game>", "</game>").Replace("<Game>", "<game>"));   // TODO: Fix.
        }

        public async Task SyncGameListFilesAsync(GameList gameList)
        {
            var fileExtensions = GameList.GetPlatformFileExtensions(gameList.Platform);

            // Remove deleted.
            for (int i = gameList.Count - 1; i >= 0; i--)
            {
                var game = gameList[i];
                game.Platform = gameList.Platform;

                string filePath = Path.Combine(gameList.PlatformFolderPath, game.GameFilename);
                if (!await this.AccessProvider.FileExistsAsync(filePath))
                {
                    gameList.Remove(game);
                    if (!String.IsNullOrEmpty(game.ImagePath))
                    {
                        string imagePath = Path.Combine(gameList.ImagesFolderPath, game.ImageFilename);
                        if (await this.AccessProvider.FileExistsAsync(imagePath))
                            await this.AccessProvider.DeleteFileAsync(imagePath);
                    }
                }
            }

            // Add missing.
            foreach (var filePath in Directory.GetFiles(gameList.PlatformFolderPath))
            {
                string filename = Path.GetFileName(filePath);
                if (fileExtensions.Contains(Path.GetExtension(filename).ToLower()) && !gameList.Contains(filename))
                    gameList.Add(new Game(gameList.Platform, filename));
            }
        }

        public List<Game> GatherIncompleteGames()
        {
            return (from x in this.Catalog.GameLists
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

        public async Task SaveGameAsync(Game game, byte[] updatedImageContents)
        {
            await SaveGameAsync(game, updatedImageContents != null ? 
                new Image(new MemoryStream(updatedImageContents)) : null);
        }

        public async Task SaveGameAsync(Game game, Image image)
        {
            // Ensure images folder exists.
            await this.AccessProvider.EnsureFolderExistsAsync(game.GameList.ImagesFolderPath);

            // Look for rename.
            if (game.GameFilename.ToLower() != Path.GetFileName(game.GameFilePath).ToLower())
            {
                // Record old values.
                string gameFilePath = game.GameFilePath;
                string imageFilePath = game.ImageFilePath;

                // Update values.
                await game.UpdateFilenameAsync(game.GameFilename);

                // Rename files.
                File.Move(gameFilePath, game.GameFilePath);
                if (!String.IsNullOrEmpty(imageFilePath) && File.Exists(imageFilePath))
                    File.Move(imageFilePath, game.ImageFilePath);
            }
            else
            {
                await game.SetFilePathsAsync();
            }

            // Update the image.
            if (image != null)
            {
                using (var stream = new FileStream(game.ImageFilePath, FileMode.Create))
                {
                    var sizedImage = image.EnsureMaximumSize(640, 640);
                    game.ImageWidth = sizedImage.Width;
                    game.ImageHeight = sizedImage.Height;
                    sizedImage.Save(stream, ImageSharpHelper.GetImageFormat(Path.GetExtension(game.ImagePath)));
                }
                await game.SetFilePathsAsync();
            }

            // Update the game config.
            await UpdateLocalCustomGameConfigFileAsync(game);

            // Save.
            SaveGameList(game.GameList);
        }

        private async Task UpdateLocalCustomGameConfigFileAsync(Game game)
        {
            foreach (var device in this.Config.Devices)
            {
                string filePath = await GetLocalCustomGameConfigFilePathAsync(game, device);
                if (game.GameConfig == Game.CUSTOM_GAME_CONFIG && !File.Exists(filePath))
                    await this.AccessProvider.WriteAllTextAsync(filePath, "#TODO");
                else if (game.GameConfig != Game.CUSTOM_GAME_CONFIG && File.Exists(filePath))
                    await this.AccessProvider.DeleteFileAsync(filePath);
            }
        }

        public async Task MatchGameAsync(Game game, Game matchedGame, byte[] imageContents)
        {
            await MatchGameAsync(game, matchedGame, imageContents != null ?
                new Image(new MemoryStream(imageContents)) : null);
        }

        public async Task MatchGameAsync(Game game, Game matchedGame, Image image)
        {
            // Copy over the properties.
            if (game.IsFilenameEditable)
                game.GameFilename = matchedGame.GameFilename;
            game.Name = matchedGame.Name;
            game.Publisher = matchedGame.Publisher;
            game.Developer = matchedGame.Developer;
            game.Rating = matchedGame.Rating;
            game.ReleaseDate = matchedGame.ReleaseDate;
            game.Genre = matchedGame.Genre;
            game.Description = matchedGame.Description;
            game.ImagePath = matchedGame.ImagePath;

            // Update.
            await SaveGameAsync(game, image);
        }

        public async Task DeleteGameAsync(Game game)
        {
            if (File.Exists(game.GameFilePath))
            {
                // It's possible that a game is comprised of multiple files with the same name
                // but different extensions (such as CD games, etc.).
                var filePaths = await this.AccessProvider.GetFilePathsAsync(Path.GetDirectoryName(game.GameFilePath),
                    Path.GetFileNameWithoutExtension(game.GameFilePath) + ".*");
                foreach (var filePath in filePaths)
                    await this.AccessProvider.DeleteFileAsync(filePath);
            }

            if (await this.AccessProvider.FileExistsAsync(game.ImageFilePath))
                await this.AccessProvider.DeleteFileAsync(game.ImageFilePath);

            game.GameList.Remove(game);
            SaveGameList(game.GameList);
        }

        public List<string> GetAvailableEmulators(Platform platform)
        {
            var list = new List<string>() { Game.DEFAULT_EMULATOR };

            var platformConfig = this.Config.FindPlatformConfig(platform);
            if (platformConfig != null)
                list.AddRange(platformConfig.Emulators);

            return list;
        }

        public List<string> GetAvailableGameConfigs(Platform platform)
        {
            var list = new List<string>() { Game.DEFAULT_GAME_CONFIG, Game.CUSTOM_GAME_CONFIG };

            var platformConfig = this.Config.FindPlatformConfig(platform);
            if (platformConfig != null)
                list.AddRange(platformConfig.GameConfigs);

            return list;
        }

        public static async Task<string> GetLocalCustomGameConfigFilePathAsync(Game game, Device device)
        {
            string deviceGameConfigsPath = await game.GameList.AccessProvider.EnsureFolderExistsAsync(
                game.GameList.PlatformFolderPath, GameList.GameConfigsFolderName, device.DirectoryKey);
            return Path.Combine(deviceGameConfigsPath, game.GameFilename + ".cfg");
        }
    }
}
