using Orbital7.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orbital7.MyGames
{
    [TypeConverter(typeof(EnumDisplayTypeConverter))]
    public enum Platform
    {
        [Display(Name = "3DO")]
        The3DOCompany_3DO,

        [Display(Name = "Amstrad CPC")]
        Amstrad_CPC,

        Arcade,

        [Display(Name = "Atari 2600")]
        Atari_2600,

        [Display(Name = "Atari 5200")]
        Atari_5200,

        [Display(Name = "Atari 7800")]
        Atari_7800,

        [Display(Name = "Atari Jaguar")]
        Atari_Jaguar,

        [Display(Name = "Atari Jaguar CD")]
        Atari_Jaguar_CD,

        [Display(Name = "Atari Lynx")]
        Atari_Lynx,

        [Display(Name = "Atari XE")]
        Atari_XE,

        Colecovision,

        [Display(Name = "Commodore 64")]
        Commodore_64,

        [Display(Name = "Famicom Disk System")]
        Famicom_Disk_System,

        Intellivision,

        [Display(Name = "Mac OS")]
        Apple_Mac_OS,

        [Display(Name = "Microsoft Xbox")]
        Microsoft_Xbox,

        [Display(Name = "Microsoft Xbox 360")]
        Microsoft_Xbox360,

        MSX,

        [Display(Name = "Neo Geo")]
        NeoGeo,

        [Display(Name = "Neo Geo Pocket")]
        NeoGeo_Pocket,

        [Display(Name = "Neo Geo Pocket Color")]
        NeoGeo_Pocket_Color,

        [Display(Name = "Nintendo 64")]
        Nintendo_64,

        [Display(Name = "Nintendo DS")]
        Nintendo_DS,

        [Display(Name = "Nintendo NES")]
        Nintendo_NES,

        [Display(Name = "Nintendo SNES")]
        Nintendo_SNES,

        [Display(Name = "Nintendo Gameboy")]
        Nintendo_Gameboy,

        [Display(Name = "Nintendo Gameboy Advance")]
        Nintendo_Gameboy_Advance,

        [Display(Name = "Nintendo Gameboy Color")]
        Nintendo_Gameboy_Color,

        [Display(Name = "Nintendo GameCube")]
        Nintendo_GameCube,

        [Display(Name = "Nintendo Virtual Boy")]
        Nintendo_Virtual_Boy,

        [Display(Name = "Nintendo Wii")]
        Nintendo_Wii,

        [Display(Name = "Nintendo Wii U")]
        Nintendo_Wii_U,

        PC,

        [Display(Name = "Sega 32X")]
        Sega_32X,

        [Display(Name = "Sega CD")]
        Sega_CD,

        [Display(Name = "Sega Dreamcast")]
        Sega_Dreamcast,

        [Display(Name = "Sega Game Gear")]
        Sega_Game_Gear,

        [Display(Name = "Sega Genesis")]
        Sega_Genesis,

        [Display(Name = "Sega Master System")]
        Sega_Master_System,

        [Display(Name = "Sega Mega Drive")]
        Sega_Mega_Drive,

        [Display(Name = "Sega Saturn")]
        Sega_Saturn,

        [Display(Name = "Sega SG 1000")]
        Sega_SG_1000,

        [Display(Name = "Sony Playstation")]
        Sony_Playstation,

        [Display(Name = "Sony Playstation 2")]
        Sony_Playstation_2,

        [Display(Name = "Sony Playstation 3")]
        Sony_Playstation_3,

        [Display(Name = "Sony Playstation Vita")]
        Sony_Playstation_Vita,

        [Display(Name = "Sony PSP")]
        Sony_PSP,

        [Display(Name = "TurboGrafx 16")]
        NEC_TurboGrafx_16,

        [Display(Name = "TurboGrafx CD")]
        NEC_TurboGrafx_CD,

        Vectrex,

        [Display(Name = "ZX Spectrum")]
        ZX_Spectrum,
    }
}
