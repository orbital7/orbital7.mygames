using Orbital7.Extensions.Windows;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Web;
using System.Xml;

namespace Orbital7.MyGames
{
    public class ScraperEngine
    {
        private delegate object PerformScraperSearch(Platform platform, string gameName);

        public Game SearchExact(IScraper scraper, Platform platform, string filename)
        {
            var game = (Game)ExecuteSearch(platform, filename, scraper.SearchExact);
            UpdateGameResult(game, scraper, platform, filename);
            return game;
        }

        public List<Game> Search(IScraper scraper, Platform platform, string filename)
        {
            var games = (List<Game>)ExecuteSearch(platform, filename, scraper.Search);
            foreach (var game in games)
                UpdateGameResult(game, scraper, platform, filename);
            return games;
        }

        private static void UpdateGameResult(Game game, IScraper scraper, Platform platform, string filename)
        {
            game.Platform = platform;
            game.Source = scraper.SourceName;
            game.UpdateFilename(filename);
        }

        private object ExecuteSearch(Platform platform, string filename, PerformScraperSearch PerformScraperSearch)
        {
            object result = null;

            // Search.
            string gameName = GetGameName(platform, filename);
            result = PerformScraperSearch(platform, gameName);

            // If not found, try converting " - " to ": ".
            if (result == null && gameName.Contains(" - "))
            {
                int index = gameName.IndexOf(" - ");
                gameName = gameName.Substring(0, index) + ": " + gameName.Substring(index + 3, gameName.Length - index - 3);
                result = PerformScraperSearch(platform, gameName);
            }

            // If still not found, but we contain ": ", parse everything behind that out.
            if (result == null && gameName.Contains(": "))
            {
                gameName = gameName.Substring(0, gameName.IndexOf(": "));
                result = PerformScraperSearch(platform, gameName);
            }

            // If still not found but contains "\\0", parse out everything behind it.
            if (result == null && gameName.Contains("\\0"))
            {
                gameName = gameName.Substring(0, gameName.IndexOf("\\0"));
                result = PerformScraperSearch(platform, gameName);
            }

            // If still not found and contains " vs ", try changing that to " vs. ".
            if (result == null && gameName.Contains(" vs "))
            {
                gameName = gameName.Replace(" vs ", " vs. ");
                result = PerformScraperSearch(platform, gameName);
            }

            // If we still didn't find anything, try the filename.
            if (result == null)
            {
                gameName = Path.GetFileNameWithoutExtension(filename);
                result = PerformScraperSearch(platform, gameName);
            }

            return null;
        }

        private string GetGameName(Platform platform, string filename)
        {
            string initialGameName = Path.GetFileNameWithoutExtension(filename).ToLower();
            string gameName = initialGameName;

            // For arcade games, we need to perform a filename to game name conversion.
            if (platform == Platform.Arcade || platform == Platform.NeoGeo)
            {
                // Try to parse out name in FBA game list.
                string fbaLine = Properties.Resources.gamelist.FindFirstBetween("\r\n| " + gameName + "\t", "\r\n");
                if (!String.IsNullOrEmpty(fbaLine))
                {
                    gameName = fbaLine.Parse("|", false)[2].Trim();
                }
                // Else try mamedb.com.
                else
                {
                    try
                    {
                        string html = WebHelper.DownloadSource("http://www.mamedb.com/game/" + initialGameName);
                        if (!String.IsNullOrEmpty(html))
                            gameName = html.FindFirstBetween("<h1>", "</h1>");
                    }
                    catch { }
                }
                //// If not found, try the Mame4All list.
                //else
                //{
                //    int endIndex = Properties.Resources.cheat_dat.IndexOf(" ]\n" + initialGameName + ":");
                //    if (endIndex > 0)
                //    {
                //        int startIndex = Properties.Resources.cheat_dat.LastIndexOf("\n; [", endIndex);
                //        if (startIndex > 0)
                //            gameName = Properties.Resources.cheat_dat.Substring(startIndex + 4, endIndex - startIndex - 4);
                //    }
                //    else
                //    {
                //        int startIndex = Properties.Resources.cheat_dat.IndexOf("]\n; " + initialGameName + " ");
                //        if (startIndex > 0)
                //        {
                //            endIndex = Properties.Resources.cheat_dat.IndexOf("\n", startIndex + 5);
                //            string noCheatLine = Properties.Resources.cheat_dat.Substring(startIndex, endIndex - startIndex);
                //            gameName = noCheatLine.FindFirstBetween("[", "]");
                //        }
                //    }
                //}

                // Clean.
                if (gameName.Contains(" / "))
                    gameName = gameName.Substring(0, gameName.IndexOf(" / "));
                if (gameName.Contains(" ("))
                    gameName = gameName.Substring(0, gameName.IndexOf(" ("));
                gameName = gameName.Trim();
            }

            return gameName;
        }   
    }
}
