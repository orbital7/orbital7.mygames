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
        public IAccessProvider AccessProvider { get; internal set; }

        public string FolderPath { get; set; }

        public string RomsFolderPath { get; set; }

        public List<PlatformConfig> PlatformConfigs { get; set; } = new List<PlatformConfig>();

        public Config()
        {

        }

        public Config(IAccessProvider accessProvider)
            : this()
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

        public static async Task<T> LoadAsync<T>(IAccessProvider accessProvider, string folderPath) where T : Config
        {
            string filePath = GetFilePath(folderPath);
            if (await accessProvider.FileExistsAsync(filePath))
            {
                T config = XMLSerializationHelper.LoadFromXML<T>(await accessProvider.ReadAllTextAsync(filePath));
                config.FolderPath = folderPath;
                config.AccessProvider = accessProvider;
                return config;
            }
            else
            {
                return null;
            }
        }

        public async Task SaveAsync()
        {
            await this.AccessProvider.WriteAllTextAsync(GetFilePath(this.FolderPath), XMLSerializationHelper.SerializeToXML(this));
        }

        public PlatformConfig FindPlatformConfig(Platform platform)
        {
            return (from x in this.PlatformConfigs
                    where x.Platform == platform
                    select x).FirstOrDefault();
        }

        public T Clone<T>() where T : Config
        {
            var config = XMLSerializationHelper.CloneObject<T>((T)this);
            config.AccessProvider = this.AccessProvider;
            return config;
        }
    }
}
