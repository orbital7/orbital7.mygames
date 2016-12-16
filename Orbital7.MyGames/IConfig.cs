using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orbital7.MyGames
{
    public interface IConfig
    {
        string FolderPath { get; set; }

        string RomsFolderPath { get; set; }

        List<PlatformConfig> PlatformConfigs { get; set; }
    }
}
