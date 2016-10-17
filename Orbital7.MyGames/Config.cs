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
    }
}
