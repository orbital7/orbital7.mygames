﻿using Orbital7.Extensions;
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

        public string FolderPath { get; set; }

        public string RomsFolderPath { get; set; }

        public List<PlatformConfig> PlatformConfigs { get; set; } = new List<PlatformConfig>();

        public List<Device> Devices { get; set; } = new List<Device>();

        public Config()
        {

        }

        public Config(string folderPath)
            : this()
        {
            this.FolderPath = folderPath;
        }

        public Config(string folderPath, string romsFolderPath)
            : this(folderPath)
        {
            this.RomsFolderPath = romsFolderPath;
        }

        private static string GetFilePath(string folderPath)
        {
            return Path.Combine(folderPath, FILENAME);
        }

        public static Config Load(string folderPath)
        {
            string filePath = GetFilePath(folderPath);
            if (File.Exists(filePath))
            {
                var config = XMLSerializationHelper.LoadFromXML<Config>(File.ReadAllText(filePath));
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
            File.WriteAllText(GetFilePath(this.FolderPath), XMLSerializationHelper.SerializeToXML(this));
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
