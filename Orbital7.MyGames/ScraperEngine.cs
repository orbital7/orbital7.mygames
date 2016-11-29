using Orbital7.Extensions.Windows;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml;

namespace Orbital7.MyGames
{
    public class ScraperEngine
    {
        private delegate object PerformScraperSearch(Platform platform, string gameName);

        public static List<Scraper> GatherScrapers()
        {
            List<Scraper> scrapers = ReflectionHelper.GetTypeInstances<Scraper>(typeof(Scraper),
                FileSystemHelper.GetExecutingAssemblyFolder());
            return (from x in scrapers orderby x.Priority descending select x).ToList();
        }

        public Game SearchExact(Scraper scraper, Platform platform, string query, string filename)
        {
            var game = (Game)ExecuteSearch(platform, query, scraper.SearchExact);
            UpdateGameResult(game, scraper, platform, filename);
            return game;
        }

        public List<Game> Search(Scraper scraper, Platform platform, string query, string filename)
        {
            var games = (List<Game>)ExecuteSearch(platform, query, scraper.Search);
            foreach (var game in games)
                UpdateGameResult(game, scraper, platform, filename);
            return games;
        }

        private static void UpdateGameResult(Game game, Scraper scraper, Platform platform, string filename)
        {
            if (game != null)
            {
                game.Platform = platform;
                game.Source = scraper.SourceName;
                game.UpdateFilename(filename);
            }
        }

        private object ExecuteSearch(Platform platform, string query, PerformScraperSearch PerformScraperSearch)
        {
            object result = null;

            // Search.
            result = PerformScraperSearch(platform, query);

            // If not found, try converting " - " to ": ".
            if (result == null && query.Contains(" - "))
            {
                int index = query.IndexOf(" - ");
                query = query.Substring(0, index) + ": " + query.Substring(index + 3, query.Length - index - 3);
                result = PerformScraperSearch(platform, query);
            }

            // If still not found, but we contain ": ", parse everything behind that out.
            if (result == null && query.Contains(": "))
            {
                query = query.Substring(0, query.IndexOf(": "));
                result = PerformScraperSearch(platform, query);
            }

            // If still not found but contains "\\0", parse out everything behind it.
            if (result == null && query.Contains("\\0"))
            {
                query = query.Substring(0, query.IndexOf("\\0"));
                result = PerformScraperSearch(platform, query);
            }

            // If still not found and contains " vs ", try changing that to " vs. ".
            if (result == null && query.Contains(" vs "))
            {
                query = query.Replace(" vs ", " vs. ");
                result = PerformScraperSearch(platform, query);
            }

            return result;
        }

        public static string GetGameName(Platform platform, string query)
        {
            // In most cases, we're given a filename as the input query, so we need to trim off the file
            // extension; one problem here is that the Path.GetFileNameWithoutExtension() function will
            // trim off anything to the right of the last period, which is an issue for game names with 
            // containing periods when not specifying a file extension.
            string initialGameName = query.ToLower(); //Path.GetFileNameWithoutExtension(query).ToLower();
            var extensions = GameList.GetPlatformFileExtensions(platform);
            foreach (var extension in extensions)
                initialGameName = initialGameName.PruneEnd(extension.ToLower());
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
                    bool found = false;

                    try
                    {
                        string html = WebHelper.DownloadSource("http://www.mamedb.com/game/" + initialGameName);
                        if (!String.IsNullOrEmpty(html))
                        {
                            gameName = html.FindFirstBetween("<h1>", "</h1>");
                            found = true;
                        }
                    }
                    catch { }

                    // If not found, try the Mame4All list.
                    if (!found)
                    {
                        int endIndex = Properties.Resources.cheat_dat.IndexOf(" ]\n" + initialGameName + ":");
                        if (endIndex > 0)
                        {
                            int startIndex = Properties.Resources.cheat_dat.LastIndexOf("\n; [", endIndex);
                            if (startIndex > 0)
                                gameName = Properties.Resources.cheat_dat.Substring(startIndex + 4, endIndex - startIndex - 4);
                        }
                        else
                        {
                            int startIndex = Properties.Resources.cheat_dat.IndexOf("]\n; " + initialGameName + " ");
                            if (startIndex > 0)
                            {
                                endIndex = Properties.Resources.cheat_dat.IndexOf("\n", startIndex + 5);
                                string noCheatLine = Properties.Resources.cheat_dat.Substring(startIndex, endIndex - startIndex);
                                gameName = noCheatLine.FindFirstBetween("[", "]");
                            }
                        }
                    }
                }

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
