using Orbital7.Extensions.Windows;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Orbital7.MyGames
{
    public abstract class Scraper
    {
        protected const string USER_AGENT = "orbital7.mygames.scraper";

        public abstract string SourceName { get; }

        public virtual int Priority
        {
            get { return 0; }
        }

        public abstract Game SearchExact(Platform platform, string gameName);

        public abstract List<Game> Search(Platform platform, string gameName);

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
        
        protected void SetGameImage(Game game, string imageURL)
        {
            if (!String.IsNullOrEmpty(imageURL))
            {
                try
                {
                    var webClient = new WebClient();
                    webClient.Headers.Add("user-agent", USER_AGENT);
                    byte[] imageContents = webClient.DownloadData(imageURL);
                    SetGameImage(game, imageContents, Path.GetFileName(imageURL));
                }
                catch
                {
                    // Do nothing.
                }
            }
        }

        protected void SetGameImage(Game game, byte[] imageContents, string imageFilename)
        {
            if (imageContents != null && imageContents.Length > 0)
            {
                Bitmap bitmap = DrawingHelper.LoadBitmap(imageContents);
                game.Image = bitmap.EnsureMaximumSize(640, 640, true);
                game.ImagePath = imageFilename;
            }
        }
    }
}
