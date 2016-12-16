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
    public abstract class Config
    {
        public const string FILENAME = "MyGames.config";

        [XmlIgnore]
        public IAccessProvider AccessProvider { get; private set; }

        public string FolderPath { get; set; }

        public string RomsFolderPath { get; set; }

        public Config(IAccessProvider accessProvider)
        {
            this.AccessProvider = accessProvider;
        }

        public Config(IAccessProvider accessProvider, string folderPath)
            : this(accessProvider)
        {
            this.FolderPath = folderPath;
        }

        public Config(IAccessProvider accessProvider, string folderPath, string romsFolderPath)
            : this(accessProvider, folderPath)
        {
            this.RomsFolderPath = romsFolderPath;
        }

        private static string GetFilePath(string folderPath)
        {
            return Path.Combine(folderPath, FILENAME);
        }

        public static T Load<T>(IAccessProvider accessProvider, string folderPath) where T : Config
        {
            string filePath = GetFilePath(folderPath);
            if (accessProvider.FileExists(filePath))
            {
                T config = XMLSerializationHelper.LoadFromXML<T>(accessProvider.ReadAllText(filePath));
                config.FolderPath = folderPath;
                return config;
            }
            else
            {
                return null;
            }
        }

        public void Save()
        {
            this.AccessProvider.WriteAllText(GetFilePath(this.FolderPath), XMLSerializationHelper.SerializeToXML(this));
        }
    }
}
