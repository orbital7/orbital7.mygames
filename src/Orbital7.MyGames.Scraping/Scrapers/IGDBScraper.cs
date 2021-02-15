using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Orbital7.MyGames.Scraping.Scrapers
{
    public class IGDBScraper : Scraper
    {
        public override string SourceName => "IGDB";

        public IGDBScraper(string configFolderPath)
            : base(configFolderPath)
        {

        }

        public async override Task<object> SearchAsync(
            Platform platform, 
            string gameName)
        {
            return await PerformSearchAsync(platform, gameName, false);
        }

        public async override Task<object> SearchExactAsync(
            Platform platform, 
            string gameName)
        {
            var games = await PerformSearchAsync(platform, gameName, true);
            if (games.Count == 1)
                return games[0];
            else
                return null;
        }

        private async Task<List<Game>> PerformSearchAsync(
            Platform platform, 
            string gameName,
            bool isExact)
        {
            // Retrieve API key (to get a key, visit https://api.igdb.com/).
            string apiKeyFilePath = Path.Combine(this.ConfigFolderPath, "IGDBAPIKey.txt");
            if (!File.Exists(apiKeyFilePath))
                throw new Exception("File containing IGDB API Key not found at: " + apiKeyFilePath);
            var apiKeyContents = File.ReadAllText(apiKeyFilePath);
            var apiKeyLines = apiKeyContents.ParseLines();

            // Search.
            var apiClient = new Orbital7.Apis.IGDB.IGDBApiClient(
                apiKeyLines[0],
                apiKeyLines[1]);
            var scrapedGames = await apiClient.QueryGamesAsync(
                fields: "cover,id,name,summary,total_rating",
                search: gameName,
                where: $"platforms = ({GetPlatformId(platform)})");

            var games = new List<Game>();
            if (scrapedGames.Count > 0)
            {
                if (isExact)
                {
                    var game = await ToGameAsync(
                        apiClient,
                        scrapedGames[0]);
                    if (game != null)
                        games.Add(game);
                }
                else
                {
                    foreach (var scrapedGame in scrapedGames)
                    {
                        var game = await ToGameAsync(
                            apiClient,
                            scrapedGame);
                        if (game != null)
                            games.Add(game);
                    }
                }
            }

            return games;
        }

        private async Task<Game> ToGameAsync(
            Orbital7.Apis.IGDB.IGDBApiClient apiClient,
            Orbital7.Apis.IGDB.Game scrapedGame)
        {
            // Get the cover.
            if (scrapedGame.cover.HasValue)
            {
                var scrapedCovers = await apiClient.QueryCoversAsync(
                    fields: "image_id",
                    where: $"id = {scrapedGame.cover.Value}");
                if (scrapedCovers.Count > 0)
                {
                    // Create the game.
                    Game game = new Game();
                    game.ID = scrapedGame.id.ToString();
                    game.Name = scrapedGame.name;
                    game.Description = scrapedGame.summary;
                    game.Rating = scrapedGame.total_rating.HasValue ? 
                        Math.Round(scrapedGame.total_rating.Value / 10, 1) : 0;
                    //base.SetGameReleaseDate(game, gameNode.GetNodeValue("original_release_date"));
                    base.SetGameImage(game, 
                        apiClient.GetImageJpegUrl(
                            scrapedCovers[0].image_id, 
                            Apis.IGDB.ImageSizeType.t_720p));

                    return game;
                }
            }

            return null;
        }

        public static int GetPlatformId(Platform platform)
        {
            switch (platform)
            {
                case Platform.The3DOCompany_3DO:
                    return 50;

                case Platform.Amstrad_CPC:
                    return 25;

                case Platform.Arcade:
                    return 52;

                case Platform.Atari_2600:
                    return 59;

                case Platform.Atari_5200:
                    return 66;

                case Platform.Atari_7800:
                    return 60;

                case Platform.Atari_Jaguar:
                case Platform.Atari_Jaguar_CD:
                    return 62;

                case Platform.Atari_XE:
                    throw new NotImplementedException();

                case Platform.Colecovision:
                    return 68;

                case Platform.Commodore_64:
                    return 15;

                case Platform.Intellivision:
                    return 67;

                case Platform.Apple_Mac_OS:
                    return 14;

                case Platform.MSX:
                    return 27;

                case Platform.Microsoft_Xbox:
                    return 11;

                case Platform.Microsoft_Xbox360:
                    return 12;

                case Platform.NeoGeo:
                    return 80;

                case Platform.NeoGeo_Pocket:
                    return 119;

                case Platform.NeoGeo_Pocket_Color:
                    return 120;

                case Platform.Nintendo_64:
                    return 4;

                case Platform.Nintendo_DS:
                    return 20;

                case Platform.Nintendo_NES:
                    return 18;

                case Platform.Nintendo_SNES:
                    return 19;

                case Platform.Nintendo_Gameboy:
                    return 33;

                case Platform.Nintendo_Gameboy_Advance:
                    return 24;

                case Platform.Nintendo_Gameboy_Color:
                    return 22;

                case Platform.Nintendo_GameCube:
                    return 21;

                case Platform.Nintendo_Wii:
                    return 5;

                case Platform.Nintendo_Wii_U:
                    return 41;

                case Platform.PC:
                    return 13;

                case Platform.Sega_32X:
                    return 30;

                case Platform.Sega_CD:
                    return 78;

                case Platform.Sega_Dreamcast:
                    return 23;

                case Platform.Sega_Game_Gear:
                    return 35;

                case Platform.Sega_Mega_Drive:
                case Platform.Sega_Genesis:
                    return 29;

                case Platform.Sega_Master_System:
                    return 64;

                case Platform.Sega_Saturn:
                    return 32;

                case Platform.Sony_Playstation:
                    return 7;

                case Platform.Sony_Playstation_2:
                    return 8;

                case Platform.Sony_Playstation_3:
                    return 9;

                case Platform.Sony_Playstation_Vita:
                    return 46;

                case Platform.Sony_PSP:
                    return 38;

                case Platform.NEC_TurboGrafx_16:
                    return 86;

                case Platform.NEC_TurboGrafx_CD:
                    return 150;

                case Platform.Vectrex:
                    return 70;

                case Platform.ZX_Spectrum:
                    return 26;

                default:
                    throw new Exception("Platform " + platform.ToDisplayString() + " not supported");
            }
        }
    }
}
