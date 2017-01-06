using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orbital7.MyGames
{
    public class DeviceSyncEngineProgress
    {
        public int GameListIndex { get; set; }

        public int GameListsCount { get; set; }

        public string Description { get; set; }

        public int ProgressPercent
        {
            get { return Convert.ToInt32(this.GameListIndex / this.GameListsCount * 100); }
        }
    }
}
