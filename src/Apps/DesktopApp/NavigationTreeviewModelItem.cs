using Orbital7.MyGames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DesktopApp
{
    public enum NavigationTreeviewModelItemType
    {
        Platforms,

        Platform,

        IncompleteGames,
    }

    public class NavigationTreeviewModelItem
    {
        public string Text { get; set; }

        public NavigationTreeviewModelItemType Type { get; set; }

        public FontWeight FontWeight { get; set; } = FontWeights.Normal;

        public bool IsExpanded { get; set; } = true;

        public GameList GameList { get; set; }

        public List<NavigationTreeviewModelItem> Children { get; set; } = new List<NavigationTreeviewModelItem>();

        public override string ToString()
        {
            return this.Text;
        }
    }
}
