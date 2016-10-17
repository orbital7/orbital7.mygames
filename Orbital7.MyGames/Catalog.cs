using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orbital7.MyGames
{
    public class Catalog
    {
        public List<GameList> GameLists { get; set; } = new List<GameList>();

        public Catalog(string folderPath)
        {
            foreach (var platformFolderPath in Directory.GetDirectories(folderPath))
            {
                var platform = GameList.GetPlatform(Path.GetFileName(platformFolderPath));
                if (platform.HasValue)
                    this.GameLists.Add(GameList.Load(platformFolderPath));
            }
        }
    }
}
