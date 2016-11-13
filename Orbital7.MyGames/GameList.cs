using Orbital7.Extensions;
using Orbital7.Extensions.Windows;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Orbital7.MyGames
{
    [XmlRoot("gameList")]
    public class GameList : CollectionBase
    {
        [XmlIgnore]
        public string PlatformFolderPath { get; set; }

        [XmlIgnore]
        public Platform Platform { get; set; }

        [XmlElement("game")]
        public Game this[int index]
        {
            get { return this.InnerList[index] as Game; }
            set { this.InnerList[index] = value; }
        }

        public GameList()
        {

        }

        public static string GetFilePath(string folderPath)
        {
            return Path.Combine(folderPath, "gamelist.xml");
        }

        public static GameList Load(string folderPath)
        {
            GameList gameList = null;

            // Validate.
            var platform = GetPlatform(Path.GetFileName(folderPath));
            if (!platform.HasValue)
                throw new Exception("Unable to determine platform from folder " + folderPath);

            // Load.
            string filePath = GetFilePath(folderPath);
            if (File.Exists(filePath))
                gameList = XMLSerializationHelper.LoadFromXML<GameList>(File.ReadAllText(filePath).Replace(
                    "<game ", "<Game ").Replace("</game>", "</Game>").Replace("<game>", "<Game>"));   // TODO: Fix.
            else
                gameList = new GameList();

            // Ensure the images folder exists.
            FileSystemHelper.EnsureFolderExists(folderPath, "images");

            // Update.
            gameList.PlatformFolderPath = folderPath;
            gameList.Platform = platform.Value;
            gameList.SyncWithFileSystem();
            gameList.Save();

            // Initialize.
            gameList.Initialize();
                        
            return gameList;
        }

        public void Save()
        {
            string filePath = GetFilePath(this.PlatformFolderPath);
            File.WriteAllText(filePath, XMLSerializationHelper.SerializeToXML(this).Replace(
                "<Game ", "<game ").Replace("</Game>", "</game>").Replace("<Game>", "<game>"));   // TODO: Fix.
        }

        public Game Add(Game game)
        {
            if (game != null)
                this.InnerList.Add(game);

            return game;
        }

        public void AddRange(ICollection games)
        {
            if (games != null)
                this.InnerList.AddRange(games);
        }

        public void Remove(Game game)
        {
            if (game != null)
                this.InnerList.Remove(game);
        }

        public bool Contains(string filename)
        {
            return (from Game x in this.InnerList
                    where x.GamePath == "./" + filename
                    select x.ID).Count() > 0;
        }

        public static Platform? GetPlatform(string folderName)
        {
            // TODO: Complete for all supported platforms.
            switch (folderName.ToLower())
            {
                case "neogeo":
                    return Platform.NeoGeo;

                case "nes":
                    return Platform.Nintendo_NES;

                case "snes":
                    return Platform.Nintendo_SNES;

                case "arcade":
                    return Platform.Arcade;

                case "megadrive":
                case "genesis":
                    return Platform.Sega_Mega_Drive;

                case "segacd":
                    return Platform.Sega_CD;

                case "sega32x":
                    return Platform.Sega_32X;

                case "pcengine":
                    return Platform.NEC_TurboGrafx_16;

                default:
                    return null;
            }
        }

        public static List<string> GetPlatformFileExtensions(Platform platform)
        {
            // TODO: Complete for all supported platforms.
            switch (platform)
            {
                case Platform.Arcade:
                case Platform.NeoGeo:
                    return new List<string>() { ".fba", ".zip" };

                case Platform.Sega_Genesis:
                case Platform.Sega_Mega_Drive:
                    return new List<string>() { ".smd", ".bin", ".gen", ".md", ".sg", ".zip" };

                case Platform.Nintendo_64:
                    return new List<string>() { ".z64", ".n64", ".v64", ".zip" };

                case Platform.Nintendo_NES:
                    return new List<string>() { ".nes", ".zip" };

                case Platform.NEC_TurboGrafx_16:
                    return new List<string>() { ".pce", ".cue", ".zip" };

                case Platform.Sega_32X:
                    return new List<string>() { ".32x", ".smd", ".bin", ".md", ".zip" };

                case Platform.Sega_CD:
                    return new List<string>() { ".iso", ".cue" };

                case Platform.Nintendo_SNES:
                    return new List<string>() { ".bin", ".smc", ".sfc", ".fig", ".swc", ".mgd", ".zip" };
            }

            return null;
        }

        public void SyncWithFileSystem()
        {
            var fileExtensions = GetPlatformFileExtensions(this.Platform);

            // Remove deleted.
            for (int i = this.Count - 1; i >= 0; i--)
            {
                var game = this[i];
                game.Platform = this.Platform;

                string filePath = Path.Combine(this.PlatformFolderPath, FileSystemHelper.ToWindowsPath(game.GamePath));
                if (!File.Exists(filePath))
                {
                    this.InnerList.Remove(game);
                    string imagePath = Path.Combine(this.PlatformFolderPath, FileSystemHelper.ToWindowsPath(game.ImagePath));
                    if (File.Exists(imagePath))
                        File.Delete(imagePath);
                }
            }

            // Add missing.
            foreach (var filePath in Directory.GetFiles(this.PlatformFolderPath))
            {
                string filename = Path.GetFileName(filePath);
                if (fileExtensions.Contains(Path.GetExtension(filename).ToLower()) && !this.Contains(filename))
                    this.Add(new Game(this.Platform, filename));
            }
        }

        internal void Initialize()
        {
            foreach (Game game in this.InnerList)
            {
                game.GameList = this;
                game.SetFilePaths();
                if (!String.IsNullOrEmpty(game.ImageFilePath))
                    game.Image = DrawingHelper.LoadBitmap(game.ImageFilePath);
            }
        }
    }
}
