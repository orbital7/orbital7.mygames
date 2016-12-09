using ImageSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Orbital7.MyGames
{
    public abstract class Scraper
    {
        protected const string USER_AGENT = "orbital7.mygames.scraper";

        public string ConfigFolderPath { get; private set; }

        public abstract string SourceName { get; }

        public Scraper(string configFolderPath)
        {
            this.ConfigFolderPath = configFolderPath;
        }

        public abstract Task<object> SearchExactAsync(Platform platform, string gameName);

        public abstract Task<object> SearchAsync(Platform platform, string gameName);

        public override string ToString()
        {
            return this.SourceName;
        }

        protected void SetGameReleaseDate(Game game, string releaseDate)
        {
            if (!String.IsNullOrEmpty(releaseDate))
            {
                try
                {
                    game.ReleaseDate = DateTime.Parse(releaseDate);
                }
                catch { }
            }
        }
        
        protected void SetGameImage(Game game, string imageUrl)
        {
            game.ImageFilePath = imageUrl;
            game.HasImage = true;
        }
    }
}
