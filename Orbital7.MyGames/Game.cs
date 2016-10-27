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
        public string Emulator { get; set; } = "Default";

        [XmlElement("platform")]
        public Platform Platform { get; set; }

        [XmlIgnore]
        public Bitmap Image { get; set; }

        [XmlIgnore]
        public GameList Parent { get; internal set; }

        [XmlIgnore]
        public string GameFilePath { get; internal set; }

        [XmlIgnore]
        public string ImageFilePath { get; internal set; }

        [XmlIgnore]
        public bool HasImage
        {
            get { return this.Image != null && !String.IsNullOrEmpty(this.ImagePath); }
        }

        [XmlIgnore]
        public string GameFilename
        {
            get { return Path.GetFileName(this.GamePath); }
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

        public Game(string filename)
            : this()
        {
            this.GamePath = "./" + filename;
        }

        public override string ToString()
        {
            if (!String.IsNullOrEmpty(this.Name))
                return this.Name;
            else
                return this.GamePath;
        }

        internal void SetFilePaths()
        {
            if (this.Parent != null)
            {
                this.GameFilePath = Path.Combine(this.Parent.PlatformFolderPath, FileSystemHelper.ToWindowsPath(this.GamePath));

                if (!String.IsNullOrEmpty(this.ImagePath))
                    this.ImageFilePath = Path.Combine(this.Parent.PlatformFolderPath, FileSystemHelper.ToWindowsPath(this.ImagePath));
            }
        }

        internal void UpdateFilename(string updatedFilename)
        {
            this.GamePath = "./" + updatedFilename;
            if (this.HasImage)
                this.ImagePath = "./images/" + Path.GetFileNameWithoutExtension(updatedFilename) + "-image" + Path.GetExtension(this.ImagePath);

            SetFilePaths();
        }
    }
}
