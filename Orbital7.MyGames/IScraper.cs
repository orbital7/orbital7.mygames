using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orbital7.MyGames
{
    public interface IScraper
    {
        string SourceName { get; }

        Game SearchExact(Platform platform, string gameName);

        List<Game> Search(Platform platform, string gameName);
    }
}
