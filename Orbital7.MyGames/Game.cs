using ImageSharp;
using Orbital7.Extensions;
using System;
using System.Collections.Generic;
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
        private const string PATH_PREFIX = "./";
        public const string DEFAULT_EMULATOR = "Default";
        public const string DEFAULT_GAME_CONFIG = "Default";
        public const string CUSTOM_GAME_CONFIG = "Custom";
        public const int MAX_IMAGE_WIDTH = 640;
        public const int MAX_IMAGE_HEIGHT = 640;

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

        [XmlElement("hasimage")]
        public bool HasImage { get; set; }

        [XmlIgnore]
        public GameList GameList { get; internal set; }

        [XmlIgnore]
        public string GameFilePath { get; internal set; }

        [XmlIgnore]
        public string ImageFilePath { get; internal set; }

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
            set { this.GamePath = PATH_PREFIX + value; }
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

        internal void Initialize(GameList gameList)
        {
            this.GameList = gameList;
            SetFilePaths();
        }

        internal void SetFilePaths()
        {
            if (this.GameList != null)
            {
                string pathSeparator = this.GameList.PlatformFolderPath.Contains("/") ? "/" : "\\";
                this.GameFilePath = Path.Combine(this.GameList.PlatformFolderPath, 
                    FileSystemHelper.NormalizePathSeparator(this.GamePath.Replace(PATH_PREFIX, ""), pathSeparator));

                if (!String.IsNullOrEmpty(this.ImagePath))
                {
                    this.ImageFilePath = Path.Combine(this.GameList.PlatformFolderPath, 
                        FileSystemHelper.NormalizePathSeparator(this.ImagePath.Replace(PATH_PREFIX, ""), pathSeparator));
                    this.HasImage = this.GameList.AccessProvider.FileExists(this.ImageFilePath);
                }
            }
        }

        internal void UpdateFilename(string updatedFilename)
        {
            this.GameFilename = updatedFilename;
            if (this.HasImage)
                this.ImagePath = Path.Combine(PATH_PREFIX + GameList.ImagesFolderName, 
                                              GetImageFilenameWithoutExtension(updatedFilename) + 
                                              Path.GetExtension(this.ImagePath));

            SetFilePaths();
        }
        
        internal static string GetImageFilenameWithoutExtension(string gameFilename)
        {
            return Path.GetFileNameWithoutExtension(gameFilename) + "-image";
        }

        
    }
}
