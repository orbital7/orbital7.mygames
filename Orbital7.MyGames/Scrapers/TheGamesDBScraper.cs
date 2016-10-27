using Orbital7.Extensions.Windows;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

namespace Orbital7.MyGames.Scrapers
{
    public class TheGamesDBScraper : Scraper
    {
        public override string SourceName
        {
            get { return "theGamesDB.net"; }
        }

        public override Game SearchExact(Platform platform, string gameName)
        {
            Game game = null;

            var doc = PerformSearch(platform, gameName, true);
            var gameNode = doc.DocumentElement.SelectSingleNode("Game");
            if (gameNode != null)
                game = ParseGame(doc, gameNode);

            return game;
        }

        public override List<Game> Search(Platform platform, string gameName)
        {
            List<Game> games = new List<Game>();

            var doc = PerformSearch(platform, gameName, false);
            foreach (XmlNode gameNode in doc.DocumentElement.SelectNodes("Game"))
                games.Add(ParseGame(doc, gameNode));

            return games;
        }

        private XmlDocument PerformSearch(Platform platform, string gameName, bool exact)
        {
            // Search.
            string url = "http://thegamesdb.net/api/GetGame.php?" + 
                         (exact ? "exactname=" : "name=") +
                         HttpUtility.UrlEncode(gameName) + "&platform=" + 
                         HttpUtility.UrlEncode(platform.ToDisplayString());     // TODO: The Platform Display attributes should not be tied to TheGamesDB platform constants.
            string xml = WebHelper.DownloadSource(url);

            // Parse.
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);

            return doc;
        }

        private Game ParseGame(XmlDocument doc, XmlNode gameNode)
        {
            string imageBaseURL = XMLHelper.GetNodeValue(doc.DocumentElement, "baseImgUrl");

            // Create the game.
            Game game = new Game();
            game.ID = XMLHelper.GetNodeValue(gameNode, "id");
            game.Genre = XMLHelper.GetNodeValue(gameNode, "Genres/genre");
            game.Description = XMLHelper.GetNodeValue(gameNode, "Overview");
            game.Developer = XMLHelper.GetNodeValue(gameNode, "Developer");
            game.Publisher = XMLHelper.GetNodeValue(gameNode, "Publisher");

            // Parse the rating.
            string rating = XMLHelper.GetNodeValue(gameNode, "Rating");
            if (!String.IsNullOrEmpty(rating))
                game.Rating = Convert.ToDouble(rating) / 10;     // Move from scale of 10 to scale of 1.

            // Parse the game name.
            string name = XMLHelper.GetNodeValue(gameNode, "GameTitle");
            if (name.Contains(" ("))
                name = name.Substring(0, name.IndexOf(" ("));
            game.Name = name;

            // Parse release date.
            string releaseDate = XMLHelper.GetNodeValue(gameNode, "ReleaseDate");
            if (!String.IsNullOrEmpty(releaseDate))
            {
                try
                {
                    game.ReleaseDate = DateTime.Parse(releaseDate);
                }
                catch { }
            }

            // Download front boxart image.
            string imagePartialURL = XMLHelper.GetNodeValue(gameNode, "Images/boxart[@side='front']");
            if (!String.IsNullOrEmpty(imagePartialURL))
            {
                string imageURL = imageBaseURL + imagePartialURL;
                byte[] contents = WebHelper.DownloadFileContents(imageURL);
                if (contents != null && contents.Length > 0)
                {
                    using (Bitmap bitmap = DrawingHelper.LoadBitmap(contents))
                    {
                        game.Image = bitmap.EnsureMaximumSize(600, 600, true);
                        game.ImagePath = imagePartialURL;
                    }
                }
            }
            // Else download front boxart thumbnail.
            else
            {
                string imageThumbURL = XMLHelper.GetAttributeValue(gameNode, "Images/boxart[@side='front']", "thumb");
                // TODO: Determine whether this is actually necessary and/or usable given thumbnail size.
            }

            return game;
        }
    }
}
