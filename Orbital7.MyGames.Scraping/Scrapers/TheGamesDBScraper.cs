using Orbital7.Extensions;
using Orbital7.Extensions.Windows;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.XPath;

namespace Orbital7.MyGames.Scraping.Scrapers
{
    public class TheGamesDBScraper : Scraper
    {
        public override string SourceName
        {
            get { return "theGamesDB.net"; }
        }

        public TheGamesDBScraper(string configFolderPath)
            : base(configFolderPath)
        {

        }

        public override async Task<object> SearchExactAsync(Platform platform, string gameName)
        {
            Game game = null;

            var navigator = await PerformSearchAsync(platform, gameName, true);
            string imageBaseURL = XMLHelper.GetNodeValue(navigator, "baseImgUrl");

            var gameNavigator = navigator.SelectSingleNode("Game");
            if (gameNavigator != null)
                game = ParseGame(gameNavigator, imageBaseURL);

            return game;
        }

        public override async Task<object> SearchAsync(Platform platform, string gameName)
        {
            List<Game> games = new List<Game>();

            var navigator = await PerformSearchAsync(platform, gameName, false);
            string imageBaseURL = XMLHelper.GetNodeValue(navigator, "baseImgUrl");

            var gameNodes = navigator.Select("Game");
            foreach (XPathNavigator gameNavigator in gameNodes)
                games.Add(ParseGame(gameNavigator, imageBaseURL));

            return games;
        }

        private async Task<XPathNavigator> PerformSearchAsync(Platform platform, string gameName, bool exact)
        {
            // Search.
            string url = "http://thegamesdb.net/api/GetGame.php?" +
                         (exact ? "exactname=" : "name=") +
                         gameName.UrlEncode() + "&platform=" +
                         GetPlatformKey(platform).UrlEncode();
            var stream = await HttpHelper.DownloadStreamAsync(url);

            // Parse.
            var doc = new XPathDocument(stream);
            var navigator = doc.CreateNavigator();

            // Validate.
            var error = navigator.SelectSingleNode("Error");
            if (error != null)
                throw new Exception(error.Value);

            return navigator.SelectSingleNode("/Data");
        }

        private Game ParseGame(XPathNavigator gameNavigator, string imageBaseURL)
        {
            // Create the game.
            Game game = new Game();
            game.ID = XMLHelper.GetNodeValue(gameNavigator, "id");
            game.Genre = XMLHelper.GetNodeValue(gameNavigator, "Genres/genre");
            game.Description = XMLHelper.GetNodeValue(gameNavigator, "Overview");
            game.Developer = XMLHelper.GetNodeValue(gameNavigator, "Developer");
            game.Publisher = XMLHelper.GetNodeValue(gameNavigator, "Publisher");
            base.SetGameReleaseDate(game, XMLHelper.GetNodeValue(gameNavigator, "ReleaseDate"));

            // Parse the rating.
            string rating = XMLHelper.GetNodeValue(gameNavigator, "Rating");
            if (!String.IsNullOrEmpty(rating))
                game.Rating = Convert.ToDouble(rating) / 10;     // Move from scale of 10 to scale of 1.

            // Parse the game name.
            string name = XMLHelper.GetNodeValue(gameNavigator, "GameTitle");
            if (name.Contains(" ("))
                name = name.Substring(0, name.IndexOf(" ("));
            game.Name = name;

            // Download front boxart image.
            string imagePartialURL = XMLHelper.GetNodeValue(gameNavigator, "Images/boxart[@side='front']");
            string imageThumbURL = XMLHelper.GetAttributeValue(gameNavigator, "Images/boxart[@side='front']", "thumb");
            if (!String.IsNullOrEmpty(imagePartialURL))
                base.SetGameImage(game, imageBaseURL + imagePartialURL);
            //// Else download front boxart thumbnail.
            //else if(!String.IsNullOrEmpty(imageThumbURL))
            //    base.SetGameImage(game, imageBaseURL + imageThumbURL);

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

                case Platform.Atari_Lynx:
                    return "Atari Lynx";

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

                case Platform.NeoGeo_Pocket:
                    return "Neo Geo Pocket";

                case Platform.NeoGeo_Pocket_Color:
                    return "Neo Geo Pocket Color";

                case Platform.Nintendo_64:
                    return "Nintendo 64";

                case Platform.Nintendo_DS:
                    return "Nintendo DS";

                case Platform.Nintendo_NES:
                    return "Nintendo Entertainment System (NES)";

                case Platform.Nintendo_SNES:
                    return "Super Nintendo (SNES)";

                case Platform.Nintendo_Gameboy:
                    return "Nintendo Game Boy";

                case Platform.Nintendo_Gameboy_Advance:
                    return "Nintendo Game Boy Advance";

                case Platform.Nintendo_Gameboy_Color:
                    return "Nintendo Gameboy Color";

                case Platform.Nintendo_GameCube:
                    return "Nintendo GameCube";

                case Platform.Nintendo_Virtual_Boy:
                    return "Nintendo Virtual Boy";

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
