using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orbital7.MyGames
{
    public class Device
    {
        public string Name { get; set; }

        public string Address { get; set; }

        public DateTime? LastSyncedDate { get; set; }

        public string GamesPath
        {
            get { return Path.Combine("\\\\" + this.Address, "roms"); }
        }

        public override string ToString()
        {
            return this.Name;
        }
    }
}
