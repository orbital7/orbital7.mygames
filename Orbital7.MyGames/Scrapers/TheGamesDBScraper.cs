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

        public override int Priority
        {
            get { return 100; }
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
            var gameNodes = doc.DocumentElement.SelectNodes("Game");
            foreach (XmlNode gameNode in gameNodes)
                games.Add(ParseGame(doc, gameNode));

            return games;
        }

        private XmlDocument PerformSearch(Platform platform, string gameName, bool exact)
        {
            // Search.
            string url = "http://thegamesdb.net/api/GetGame.php?" +
                         (exact ? "exactname=" : "name=") +
                         HttpUtility.UrlEncode(gameName) + "&platform=" +
                         HttpUtility.UrlEncode(GetPlatformKey(platform));
            string xml = WebHelper.DownloadSource(url);

            // Parse.
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);

            // Validate.
            if (doc.DocumentElement.Name == "Error")
                throw new Exception(XMLHelper.GetNodeValue(doc.DocumentElement, "."));

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
            base.SetGameReleaseDate(game, XMLHelper.GetNodeValue(gameNode, "ReleaseDate"));

            // Parse the rating.
            string rating = XMLHelper.GetNodeValue(gameNode, "Rating");
            if (!String.IsNullOrEmpty(rating))
                game.Rating = Convert.ToDouble(rating) / 10;     // Move from scale of 10 to scale of 1.

            // Parse the game name.
            string name = XMLHelper.GetNodeValue(gameNode, "GameTitle");
            if (name.Contains(" ("))
                name = name.Substring(0, name.IndexOf(" ("));
            game.Name = name;

            // Download front boxart image.
            string imagePartialURL = XMLHelper.GetNodeValue(gameNode, "Images/boxart[@side='front']");
            string imageThumbURL = XMLHelper.GetAttributeValue(gameNode, "Images/boxart[@side='front']", "thumb");
            if (!String.IsNullOrEmpty(imagePartialURL))
                base.SetGameImage(game, imageBaseURL + imagePartialURL);
            // Else download front boxart thumbnail.
            else if(!String.IsNullOrEmpty(imageThumbURL))
                base.SetGameImage(game, imageBaseURL + imageThumbURL);

            return game;
        }

        public static string GetPlatformKey(Platform platform)
        {
            switch (platform)
            {

                case Platform.The3DOCompany_3DO:
                    return "3DO";

                case Platform.Arcade:
                    return "Arcade";

                case Platform.Atari_2600:
                    return "Atari 2600";

                case Platform.Atari_5200:
                    return "Atari 5200";

                case Platform.Atari_7800:
                    return "Atari 7800";

                case Platform.Atari_Jaguar:
                    return "Atari Jaguar";

                case Platform.Atari_Jaguar_CD:
                    return "Atari Jaguar CD";

                case Platform.Atari_XE:
                    return "Atari XE";

                case Platform.Colecovision:
                    return "Colecovision";

                case Platform.Commodore_64:
                    return "Commodore 64";

                case Platform.Intellivision:
                    return "Intellivision";

                case Platform.Apple_Mac_OS:
                    return "Mac OS";

                case Platform.Microsoft_Xbox:
                    return "Microsoft Xbox";

                case Platform.Microsoft_Xbox360:
                    return "Microsoft Xbox 360";

                case Platform.NeoGeo:
                    return "Neo Geo";

                case Platform.Nintendo_64:
                    return "Nintendo 64";

                case Platform.Nintendo_DS:
                    return "Nintendo DS";

                case Platform.Nintendo_NES:
                    return "Nintendo Entertainment System (NES)";

                case Platform.Nintendo_SNES:
                    return "Super Nintendo (SNES)";

                case Platform.Nintendo_Gameboy:
                    return "Nintendo Gameboy";

                case Platform.Nintendo_Gameboy_Advance:
                    return "Nintendo Gameboy Advance";

                case Platform.Nintendo_GameCube:
                    return "Nintendo GameCube";

                case Platform.Nintendo_Wii:
                    return "Nintendo Wii";

                case Platform.Nintendo_Wii_U:
                    return "Nintendo Wii U";

                case Platform.PC:
                    return "PC";

                case Platform.Sega_32X:
                    return "Sega 32X";

                case Platform.Sega_CD:
                    return "Sega CD";

                case Platform.Sega_Dreamcast:
                    return "Sega Dreamcast";

                case Platform.Sega_Game_Gear:
                    return "Sega Game Gear";

                case Platform.Sega_Genesis:
                    return "Sega Genesis";

                case Platform.Sega_Master_System:
                    return "Sega Master System";

                case Platform.Sega_Mega_Drive:
                    return "Sega Mega Drive";

                case Platform.Sega_Saturn:
                    return "Sega Saturn";

                case Platform.Sony_Playstation:
                    return "Sony Playstation";

                case Platform.Sony_Playstation_2:
                    return "Sony Playstation 2";

                case Platform.Sony_Playstation_3:
                    return "Sony Playstation 3";

                case Platform.Sony_Playstation_Vita:
                    return "Sony Playstation Vita";

                case Platform.Sony_PSP:
                    return "Sony PSP";

                case Platform.NEC_TurboGrafx_16:
                    return "TurboGrafx 16";

                case Platform.NEC_TurboGrafx_CD:
                    return "TurboGrafx CD";

                default:
                    throw new Exception("Platform " + platform.ToDisplayString() + " not supported");
            }
        }
    }
}
