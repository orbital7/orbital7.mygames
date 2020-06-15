using Orbital7.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Orbital7.MyGames
{
    [XmlRoot("gameList")]
    public class GameList : CollectionBase
    {
        public const string ImagesFolderName = "images";
        public const string GameConfigsFolderName = "gameconfigs";
        public const string SaveStatesFolderName = "savestates";

        [XmlIgnore]
        public IAccessProvider AccessProvider { get; private set; }

        [XmlIgnore]
        public string PlatformFolderPath { get; set; }

        [XmlIgnore]
        public Platform Platform { get; set; }

        [XmlElement("game")]
        public Game this[int index]
        {
            get { return this.InnerList[index] as Game; }
            set { this.InnerList[index] = value; }
        }

        public string FilePath
        {
            get { return GetFilePath(this.PlatformFolderPath); }
        }

        public string ImagesFolderPath
        {
            get { return Path.Combine(this.PlatformFolderPath, ImagesFolderName); }
        }

        public string PlatformFolderName
        {
            get { return Path.GetFileName(this.PlatformFolderPath); }
        }

        public GameList()
        {

        }

        public static string GetFilePath(string folderPath)
        {
            return Path.Combine(folderPath, "gamelist.xml");
        }

        internal static async Task<GameList> LoadAsync(IAccessProvider accessProvider, string folderPath)
        {
            GameList gameList = null;

            // Validate.
            var platform = GetPlatform(Path.GetFileName(folderPath));
            if (!platform.HasValue)
                throw new Exception("Unable to determine platform from folder " + folderPath);

            // Load.
            string filePath = GetFilePath(folderPath);
            if (File.Exists(filePath))
                gameList = SerializationHelper.DeserializeFromXml<GameList>(File.ReadAllText(filePath).Replace(
                    "<game ", "<Game ").Replace("</game>", "</Game>").Replace("<game>", "<Game>"));   // TODO: Fix.
            else
                gameList = new GameList();

            // Update.
            gameList.AccessProvider = accessProvider;
            gameList.PlatformFolderPath = folderPath;
            gameList.Platform = platform.Value;

            // Initialize.
            await gameList.InitializeAsync();
                        
            return gameList;
        }

        public Game Add(Game game)
        {
            if (game != null)
                this.InnerList.Add(game);

            return game;
        }

        public void AddRange(ICollection games)
        {
            if (games != null)
                this.InnerList.AddRange(games);
        }

        public void Remove(Game game)
        {
            if (game != null)
                this.InnerList.Remove(game);
        }

        public bool Contains(string filename)
        {
            return (from Game x in this.InnerList
                    where x.GamePath.ToLower() == "./" + filename.ToLower()
                    select x.ID).Count() > 0;
        }

        public static Platform? GetPlatform(string folderName)
        {
            switch (folderName.ToLower())
            {
                case "3do":
                    return Platform.The3DOCompany_3DO;

                case "amstradcpc":
                    return Platform.Amstrad_CPC;

                case "arcade":
                case "fba":
                case "mame-libretro":
                case "mame-mame4all":
                    return Platform.Arcade;

                case "atari2600":
                    return Platform.Atari_2600;

                case "atari800":
                case "atari5200":
                    return Platform.Atari_5200;

                case "atari7800":
                    return Platform.Atari_7800;

                case "atarijaguar":
                    return Platform.Atari_Jaguar;

                case "atarilynx":
                    return Platform.Atari_Lynx;

                case "coleco":
                    return Platform.Colecovision;

                case "c64":
                    return Platform.Commodore_64;

                case "dreamcast":
                    return Platform.Sega_Dreamcast;
                
                case "fds":
                    return Platform.Famicom_Disk_System;

                case "gamegear":
                    return Platform.Sega_Game_Gear;

                case "gb":
                    return Platform.Nintendo_Gameboy;

                case "gba":
                    return Platform.Nintendo_Gameboy_Advance;

                case "gbc":
                    return Platform.Nintendo_Gameboy_Color;

                case "genesis":
                    return Platform.Sega_Genesis;

                case "macintosh":
                    return Platform.Apple_Mac_OS;

                case "mastersystem":
                    return Platform.Sega_Master_System;

                case "megadrive":
                    return Platform.Sega_Mega_Drive;

                case "msx":
                    return Platform.MSX;

                case "n64":
                    return Platform.Nintendo_64;

                case "nds":
                    return Platform.Nintendo_DS;

                case "neogeo":
                    return Platform.NeoGeo;

                case "nes":
                    return Platform.Nintendo_NES;

                case "ngp":
                    return Platform.NeoGeo_Pocket;

                case "ngpc":
                    return Platform.NeoGeo_Pocket_Color;

                case "pc":
                    return Platform.PC;

                case "pcecd":
                    return Platform.NEC_TurboGrafx_CD;

                case "pcengine":
                    return Platform.NEC_TurboGrafx_16;

                case "psp":
                    return Platform.Sony_PSP;

                case "psx":
                    return Platform.Sony_Playstation;

                case "saturn":
                    return Platform.Sega_Saturn;

                case "sega32x":
                    return Platform.Sega_32X;

                case "segacd":
                    return Platform.Sega_CD;

                case "sg-1000":
                    return Platform.Sega_SG_1000;

                case "snes":
                    return Platform.Nintendo_SNES;

                case "vectrex":
                    return Platform.Vectrex;

                case "virtualboy":
                    return Platform.Nintendo_Virtual_Boy;

                case "zxspectrum":
                    return Platform.ZX_Spectrum;

                default:
                    return null;
            }
        }

        public static List<string> GetPlatformFileExtensions(Platform platform)
        {
            switch (platform)
            {
                case Platform.The3DOCompany_3DO:
                    return new List<string>() { ".iso" };

                case Platform.Amstrad_CPC:
                    return new List<string>() { ".dsk", ".cpc" };

                case Platform.Arcade:
                case Platform.NeoGeo:
                    return new List<string>() { ".fba", ".zip" };

                case Platform.Atari_2600:
                    return new List<string>() { ".bin", ".a26", ".rom", ".zip", ".gz" };

                case Platform.Atari_5200:
                    return new List<string>() { ".a52", ".bas", ".bin", ".xex", ".atr", ".xfd", ".dcm", ".atr.gz", ".xfd.gz" };

                case Platform.Atari_7800:
                    return new List<string>() { ".a78", ".bin" };

                case Platform.Atari_Jaguar:
                    return new List<string>() { ".j64", ".jag" };

                case Platform.Atari_Lynx:
                    return new List<string>() { ".lnx" };

                case Platform.Colecovision:
                    return new List<string>() { ".bin", ".col", ".rom", ".zip" };

                case Platform.Commodore_64:
                    return new List<string>() { ".crt", ".d64", ".g64", ".t64", ".tap", ".x64" };

                case Platform.Sega_Dreamcast:
                    return new List<string>() { ".cdi", ".gdi" };

                case Platform.Famicom_Disk_System:
                    return new List<string>() { ".fds" };

                case Platform.Sega_Game_Gear:
                    return new List<string>() { ".gg" };

                case Platform.Nintendo_Gameboy:
                    return new List<string>() { ".gb" };

                case Platform.Nintendo_Gameboy_Advance:
                    return new List<string>() { ".gba" };

                case Platform.Nintendo_Gameboy_Color:
                    return new List<string>() { ".gbc" };

                case Platform.Sega_Genesis:
                case Platform.Sega_Mega_Drive:
                    return new List<string>() { ".smd", ".bin", ".md" };

                case Platform.Apple_Mac_OS:
                    return new List<string>() { ".img", ".rom" };

                case Platform.Sega_Master_System:
                    return new List<string>() { ".sms" };

                case Platform.MSX:
                    return new List<string>() { ".rom", ".mx1", ".mx2", ".col", ".dsk" };

                case Platform.Nintendo_64:
                    return new List<string>() { ".z64", ".n64", ".v64" };

                case Platform.Nintendo_DS:
                    return new List<string>() { ".nds", ".bin" };

                case Platform.Nintendo_NES:
                    return new List<string>() { ".zip", ".nes", ".smc", ".sfc", ".fig", ".swc", ".mgd", ".fds" };

                case Platform.NeoGeo_Pocket:
                    return new List<string>() { ".ngp" };

                case Platform.NeoGeo_Pocket_Color:
                    return new List<string>() { ".ngc" };

                case Platform.NEC_TurboGrafx_CD:
                    return new List<string>() { ".cue" };

                case Platform.NEC_TurboGrafx_16:
                    return new List<string>() { ".pce", ".zip" };

                case Platform.PC:
                    return new List<string>() { ".com", ".sh", ".bat", ".exe" };

                case Platform.Sony_PSP:
                    return new List<string>() { ".cso", ".iso", ".pbp" };

                case Platform.Sony_Playstation:
                    return new List<string>() { ".cue", ".cbn", ".img", ".iso", ".m3u", ".mdf", ".pbp", ".toc", ".z", ".znx" };

                case Platform.Sega_32X:
                    return new List<string>() { ".32x", ".smd", ".bin", ".md", };

                case Platform.Sega_CD:
                    return new List<string>() { ".iso", ".cue" };

                case Platform.Sega_Saturn:
                    return new List<string>() { ".bin", ".iso", ".mdf" };

                case Platform.Sega_SG_1000:
                    return new List<string>() { ".sg", ".zip" };

                case Platform.Nintendo_SNES:
                    return new List<string>() { ".zip", ".smc", ".sfc", ".fig", ".swc" };

                case Platform.Vectrex:
                    return new List<string>() { ".vec", ".gam", ".bin" };

                case Platform.Nintendo_Virtual_Boy:
                    return new List<string>() { ".vb" };

                case Platform.ZX_Spectrum:
                    return new List<string>() { ".sna", ".szx", ".z80", ".tap", ".tzx", ".gz", ".udi", ".mgt", ".img", ".trd", ".scl", ".dsk" };
            }

            return null;
        }

        internal async Task InitializeAsync()
        {
            foreach (Game game in this.InnerList)
                await game.InitializeAsync(this);
        }

        public override string ToString()
        {
            return this.Platform.ToDisplayString();
        }
    }
}
