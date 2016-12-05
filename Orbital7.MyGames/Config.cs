using Orbital7.Extensions;
using Orbital7.Extensions.Windows;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orbital7.MyGames
{
    public class Config
    {
        public const string FILENAME = "MyGames.config";

        public string GamesFolderPath { get; set; }

        public List<PlatformConfig> PlatformConfigs { get; set; } = new List<PlatformConfig>();

        public List<Device> Devices { get; set; } = new List<Device>();

        public Config()
        {

        }

        public Config(string gamesFolderPath)
            : this()
        {
            this.GamesFolderPath = gamesFolderPath;
        }

        public static string GetFilePath(string folderPath)
        {
            return Path.Combine(folderPath, FILENAME);
        }

        public static Config Load(string filePath = null)
        {
            if (String.IsNullOrEmpty(filePath))
                filePath = GetFilePath(FileSystemHelper.GetExecutingAssemblyFolder());

            if (File.Exists(filePath))
                return XMLSerializationHelper.LoadFromXML<Config>(File.ReadAllText(filePath));
            else
                return null;
        }

        public void Save(string filePath = null)
        {
            if (String.IsNullOrEmpty(filePath))
                filePath = GetFilePath(FileSystemHelper.GetExecutingAssemblyFolder());

            File.WriteAllText(filePath, XMLSerializationHelper.SerializeToXML(this));
        }

        public Device FindDevice(string directoryKey)
        {
            return (from x in this.Devices
                    where x.DirectoryKey.ToLower() == directoryKey.ToLower()
                    select x).FirstOrDefault();
        }

        public PlatformConfig FindPlatformConfig(Platform platform)
        {
            return (from x in this.PlatformConfigs
                    where x.Platform == platform
                    select x).FirstOrDefault();
        }
    }
}
