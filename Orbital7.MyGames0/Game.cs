using Orbital7.Extensions.Windows;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Orbital7.MyGames
{
    [XmlRoot("game")]
    public class Game
    {
        public const string DEFAULT_EMULATOR = "Default";
        public const string DEFAULT_GAME_CONFIG = "Default";
        public const string CUSTOM_GAME_CONFIG = "Custom";

        [XmlAttribute(AttributeName = "source")]
        public string Source { get; set; }

        [XmlAttribute(AttributeName = "id")]
        public string ID { get; set; }

        [XmlElement("path")]
        public string GamePath { get; set; }

        [XmlElement("name")]
        public string Name { get; set; }

        [XmlElement("desc")]
        public string Description { get; set; }

        [XmlElement("image")]
        public string ImagePath { get; set; }

        [XmlElement("rating")]
        public double Rating { get; set; }

        [XmlElement("releasedate")]
        public string ReleaseDateString { get; set; }

        [XmlElement("developer")]
        public string Developer { get; set; }

        [XmlElement("publisher")]
        public string Publisher { get; set; }

        [XmlElement("genre")]
        public string Genre { get; set; }

        [XmlElement("players")]
        public string Players { get; set; }

        [XmlElement("emulator")]
        public string Emulator { get; set; } = DEFAULT_EMULATOR;

        [XmlElement("gameconfig")]
        public string GameConfig { get; set; } = DEFAULT_GAME_CONFIG;

        [XmlElement("platform")]
        public Platform Platform { get; set; }

        [XmlIgnore]
        public Bitmap Image { get; set; }

        [XmlIgnore]
        public GameList GameList { get; internal set; }

        [XmlIgnore]
        public string GameFilePath { get; internal set; }

        [XmlIgnore]
        public string ImageFilePath { get; internal set; }

        [XmlIgnore]
        public bool HasImage
        {
            get { return this.Image != null && !String.IsNullOrEmpty(this.ImagePath); }
        }

        public string ImageFilename
        {
            get { return Path.GetFileName(this.ImagePath); }
        }

        public bool IsFilenameEditable
        {
            get { return this.Platform != Platform.Arcade && this.Platform != Platform.NeoGeo; }
        }

        public string FileSummary
        {
            get
            {
                if (this.GameList != null)
                {
                    string value = this.GamePath;
                    if (this.Emulator != DEFAULT_EMULATOR || this.GameConfig != DEFAULT_GAME_CONFIG)
                    {
                        value += "  [";
                        if (this.Emulator != DEFAULT_EMULATOR)
                            value += this.Emulator + " ";
                        if (this.GameConfig != DEFAULT_GAME_CONFIG)
                            value += "using " + this.GameConfig;
                        value = value.Trim() + "]";
                    }

                    return value;
                }
                else
                {
                    return this.Source;
                }
            }
        }

        [XmlIgnore]
        public string GameFilename
        {
            get { return Path.GetFileName(this.GamePath); }
            set { this.GamePath = "./" + value; }
        }

        [XmlIgnore]
        public DateTime? ReleaseDate
        {
            get
            {
                if (!String.IsNullOrEmpty(this.ReleaseDateString))
                    return DateTime.ParseExact(this.ReleaseDateString, "yyyyMMddTHHmmss", null);
                else
                    return null;
            }
            set
            {
                if (value.HasValue)
                    this.ReleaseDateString = value.Value.ToString("yyyyMMddTHHmmss");
                else
                    this.ReleaseDateString = null;
            }
        }

        public Game()
        {

        }

        public Game(Platform platform, string filename)
            : this()
        {
            this.Platform = platform;
            this.GameFilename = filename;
        }

        public override string ToString()
        {
            if (!String.IsNullOrEmpty(this.Name))
                return this.Name;
            else
                return this.GamePath;
        }

        internal void Initialize(GameList gameList, Config config)
        {
            this.GameList = gameList;

            SetFilePaths();
            UpdateLocalCustomButtonMappingFile(config);
            if (!String.IsNullOrEmpty(this.ImageFilePath))
                this.Image = DrawingHelper.LoadBitmap(this.ImageFilePath);

            
        }

        private void UpdateLocalCustomButtonMappingFile(Config config)
        {
            foreach (var device in config.Devices)
            {
                string filePath = GetLocalCustomGameConfigFilePath(device);
                if (this.GameConfig == CUSTOM_GAME_CONFIG && !File.Exists(filePath))
                    File.WriteAllText(filePath, "#TODO");
                else if (this.GameConfig != CUSTOM_GAME_CONFIG && File.Exists(filePath))
                    File.Delete(filePath);                
            }
        }

        internal void SetFilePaths()
        {
            if (this.GameList != null)
            {
                this.GameFilePath = Path.Combine(this.GameList.PlatformFolderPath, FileSystemHelper.ToWindowsPath(this.GamePath));

                if (!String.IsNullOrEmpty(this.ImagePath))
                    this.ImageFilePath = Path.Combine(this.GameList.PlatformFolderPath, FileSystemHelper.ToWindowsPath(this.ImagePath));
            }
        }

        internal void UpdateFilename(string updatedFilename)
        {
            this.GameFilename = updatedFilename;
            if (this.HasImage)
                this.ImagePath = "./images/" + GetImageFilenameWithoutExtension(updatedFilename) + Path.GetExtension(this.ImagePath);

            SetFilePaths();
        }

        public void UpdateImage(Bitmap image)
        {
            if (image != null)
            {
                this.Image = image.ToBitmap(System.Drawing.Imaging.ImageFormat.Png);
                this.ImagePath = "Temp.png";
                UpdateFilename(this.GameFilename);
            }
        }
        
        internal static string GetImageFilenameWithoutExtension(string gameFilename)
        {
            return Path.GetFileNameWithoutExtension(gameFilename) + "-image";
        }

        public void SyncWithFileSystem()
        {
            // Look for rename.
            if (this.GameFilename.ToLower() != Path.GetFileName(this.GameFilePath).ToLower())
            {
                // Record old values.
                string gameFilePath = this.GameFilePath;
                string imageFilePath = this.ImageFilePath;

                // Update values.
                this.UpdateFilename(this.GameFilename);

                // Rename files.
                File.Move(gameFilePath, this.GameFilePath);
                if (!String.IsNullOrEmpty(imageFilePath) && File.Exists(imageFilePath))
                    File.Move(imageFilePath, this.ImageFilePath);
            }
            else
            {
                SetFilePaths();
            }

            // Update the button mapping.
            UpdateLocalCustomButtonMappingFile(Config.Load());

            // Ensure image is saved.
            if (this.HasImage)
                this.Image.Save(this.ImageFilePath);

            // Save.
            this.GameList.Save();
        }

        public void Match(Game matchedGame)
        {
            // Copy over the properties.
            if (this.IsFilenameEditable)
                this.GameFilename = matchedGame.GameFilename;
            this.Name = matchedGame.Name;
            this.Publisher = matchedGame.Publisher;
            this.Developer = matchedGame.Developer;
            this.Rating = matchedGame.Rating;
            this.ReleaseDate = matchedGame.ReleaseDate;
            this.Genre = matchedGame.Genre;
            this.Description = matchedGame.Description;
            this.ImagePath = matchedGame.ImagePath;
            this.Image = matchedGame.Image;

            // Update.
            this.SyncWithFileSystem();
        }

        public void Delete()
        {
            if (File.Exists(this.GameFilePath))
            {
                // It's possible that a game is comprised of multiple files with the same name
                // but different extensions (such as CD games, etc.).
                var filePaths = Directory.GetFiles(Path.GetDirectoryName(this.GameFilePath),
                    Path.GetFileNameWithoutExtension(this.GameFilePath) + ".*");
                foreach (var filePath in filePaths)
                    File.Delete(filePath);
            }

            if (File.Exists(this.ImageFilePath))
                File.Delete(this.ImageFilePath);

            this.GameList.Remove(this);
            this.GameList.Save();
        }

        public List<string> GetAvailableEmulators(string configFilePath = null)
        {
            var list = new List<string>() { DEFAULT_EMULATOR };

            var platformConfig = Config.Load(configFilePath).FindPlatformConfig(this.Platform);
            if (platformConfig != null)
                list.AddRange(platformConfig.Emulators);

            return list;
        }

        public List<string> GetAvailableGameConfigs(string configFilePath = null)
        {
            var list = new List<string>() { DEFAULT_GAME_CONFIG, CUSTOM_GAME_CONFIG };

            var platformConfig = Config.Load(configFilePath).FindPlatformConfig(this.Platform);
            if (platformConfig != null)
                list.AddRange(platformConfig.GameConfigs);

            return list;
        }


        public string GetLocalCustomGameConfigFilePath(Device device)
        {
            return Path.Combine(this.GameList.PlatformFolderPath, GameList.GameConfigsFolderName, 
                device.DirectoryKey, this.GameFilename + ".cfg");
        }

        public string GetLocalGameConfigContents(Device device)
        {
            string filePath = String.Empty;
            if (this.GameConfig == CUSTOM_GAME_CONFIG)
                filePath = GetLocalCustomGameConfigFilePath(device);
            else if (this.GameConfig != DEFAULT_GAME_CONFIG)
                filePath = Path.Combine(this.GameList.PlatformFolderPath, GameList.GameConfigsFolderName,
                    device.DirectoryKey, this.GameConfig);

            if (File.Exists(filePath))
                return File.ReadAllText(filePath);
            else
                return null;
        }
    }
}
