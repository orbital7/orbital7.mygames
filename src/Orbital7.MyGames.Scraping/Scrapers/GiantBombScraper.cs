using Orbital7.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;

namespace Orbital7.MyGames.Scraping.Scrapers
{
    public class GiantBombScraper : Scraper
    {
        public override string SourceName
        {
            get { return "GiantBomb.com"; }
        }

        public GiantBombScraper(string configFolderPath)
            : base(configFolderPath)
        {

        }

        public override async Task<object> SearchExactAsync(Platform platform, string gameName)
        {
            Game game = null;

            var resultsNode = await PerformSearchAsync(platform, gameName);
            var gameNode = resultsNode.SelectSingleNode("game[name/text()='" + gameName.Replace("'", "&quot") + "']");
            if (gameNode != null)
                game = ParseGame(gameNode);

            return game;
        }

        public override async Task<object> SearchAsync(Platform platform, string gameName)
        {
            List<Game> games = new List<Game>();

            var resultsNode = await PerformSearchAsync(platform, gameName);
            foreach (XmlNode gameNode in resultsNode.SelectNodes("game"))
                games.Add(ParseGame(gameNode));

            return games;
        }

        private async Task<XmlNode> PerformSearchAsync(Platform platform, string gameName)
        {
            // Retrieve API key (to get a key, visit http://www.giantbomb.com/api/).
            string apiKeyFilePath = Path.Combine(this.ConfigFolderPath, "GiantBombAPIKey.txt");
            if (!File.Exists(apiKeyFilePath))
                throw new Exception("File containing GiantBomb API Key not found at: " + apiKeyFilePath);
            string apiKey = File.ReadAllText(apiKeyFilePath);

            // Wait 1 second to comply with service frequency guidelines.
            System.Threading.Thread.Sleep(1000);

            // Search.
            string url = "http://www.giantbomb.com/api/games/?api_key=" + apiKey + "&filter=" + 
                         "name:" + gameName.UrlEncode() + "," +
                         "platforms:" + GetPlatformKey(platform).UrlEncode();
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("user-agent", Scraper.USER_AGENT);
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var xml = await response.Content.ReadAsStringAsync();

            // Parse.
            var doc = new XmlDocument();
            doc.LoadXml(xml);

            // Validate.
            if (doc.DocumentElement.GetNodeValue(".//status_code") != "1")
                throw new Exception(doc.DocumentElement.GetNodeValue(".//error"));

            return doc.DocumentElement;
        }

        private Game ParseGame(XmlNode gameNode)
        {
            // Create the game.
            Game game = new Game();
            game.ID = gameNode.GetNodeValue("id");
            game.Name = gameNode.GetNodeValue("name");
            game.Description = gameNode.GetNodeValue("deck");
            base.SetGameReleaseDate(game, gameNode.GetNodeValue("original_release_date"));
            base.SetGameImage(game, gameNode.GetNodeValue("image/super_url"));

            // TODO: To get the remaining values, we would need to call the game-specific API.
            //game.Developer = 
            //game.Publisher = 
            //game.Rating = 
            //game.Genre = 

            return game;
        }

        public static string GetPlatformKey(Platform platform)
        {
            switch (platform)
            {
                case Platform.The3DOCompany_3DO:
                    throw new NotImplementedException();

                case Platform.Arcade:
                    return "84";

                case Platform.Atari_2600:
                    return "40";

                case Platform.Atari_5200:
                    return "67";

                case Platform.Atari_7800:
                    return "70";

                case Platform.Atari_Jaguar:
                    throw new NotImplementedException();

                case Platform.Atari_Jaguar_CD:
                    throw new NotImplementedException();

                case Platform.Atari_XE:
                    throw new NotImplementedException();

                case Platform.Colecovision:
                    throw new NotImplementedException();

                case Platform.Commodore_64:
                    return "14";

                case Platform.Intellivision:
                    throw new NotImplementedException();

                case Platform.Apple_Mac_OS:
                    throw new NotImplementedException();

                case Platform.Microsoft_Xbox:
                    return "32";

                case Platform.Microsoft_Xbox360:
                    return "20";

                case Platform.NeoGeo:
                    return "25";

                case Platform.Nintendo_64:
                    return "43";

                case Platform.Nintendo_DS:
                    return "52";

                case Platform.Nintendo_NES:
                    return "21";

                case Platform.Nintendo_SNES:
                    return "9";

                case Platform.Nintendo_Gameboy:
                    return "3";

                case Platform.Nintendo_Gameboy_Advance:
                    return "4";

                case Platform.Nintendo_GameCube:
                    return "23";

                case Platform.Nintendo_Wii:
                    return "36";

                case Platform.Nintendo_Wii_U:
                    return "139";

                case Platform.PC:
                    return "94";

                case Platform.Sega_32X:
                    return "31";

                case Platform.Sega_CD:
                    return "29";

                case Platform.Sega_Dreamcast:
                    return "37";

                case Platform.Sega_Game_Gear:
                    return "5";

                case Platform.Sega_Mega_Drive:
                case Platform.Sega_Genesis:
                    return "6";

                case Platform.Sega_Master_System:
                    return "8";

                case Platform.Sega_Saturn:
                    return "42";

                case Platform.Sony_Playstation:
                    return "22";

                case Platform.Sony_Playstation_2:
                    return "19";

                case Platform.Sony_Playstation_3:
                    return "35";

                case Platform.Sony_Playstation_Vita:
                    return "129";

                case Platform.Sony_PSP:
                    return "18";

                case Platform.NEC_TurboGrafx_16:
                    return "53";

                case Platform.NEC_TurboGrafx_CD:
                    return "55";

                default:
                    throw new Exception("Platform " + platform.ToDisplayString() + " not supported");
            }
        }
    }
}
