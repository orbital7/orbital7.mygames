using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orbital7.MyGames
{
    public abstract class Scraper
    {
        public abstract string SourceName { get; }

        public abstract Game SearchExact(Platform platform, string gameName);

        public abstract List<Game> Search(Platform platform, string gameName);

        public override string ToString()
        {
            return this.SourceName;
        }
    }
}
