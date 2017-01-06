using Orbital7.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Orbital7.MyGames
{
    public class Catalog
    {
        public List<GameList> GameLists { get; set; } = new List<GameList>();

        [XmlIgnore]
        public Config Config { get; private set; }

        [XmlIgnore]
        public IAccessProvider AccessProvider { get; private set; }

        public Catalog(Config config)
        {
            this.Config = config;
            this.AccessProvider = config.AccessProvider;

            AsyncHelper.RunSync(() => InitializeAsync());
        }

        private async Task InitializeAsync()
        {
            foreach (var platformFolderPath in await this.AccessProvider.GetFolderPathsAsync(this.Config.RomsFolderPath))
            {
                var platform = GameList.GetPlatform(Path.GetFileName(platformFolderPath));
                if (platform.HasValue)
                    this.GameLists.Add(await GameList.LoadAsync(this.AccessProvider, platformFolderPath));
            }
        }
                
        public static List<string> GetSaveStateFileExtensions()
        {
            return new List<string>()
            {
                ".state",
                ".state1",
                ".state2",
                ".state3",
                ".state4",
                ".state5",
                ".state6",
                ".state7",
                ".state8",
                ".state9",
            };
        }
    }
}
