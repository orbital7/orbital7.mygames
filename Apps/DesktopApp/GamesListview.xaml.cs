using Orbital7.MyGames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DesktopApp
{
    /// <summary>
    /// Interaction logic for GamesListview.xaml
    /// </summary>
    public partial class GamesListview : UserControl
    {
        private Game _selected = null;

        public Game SelectedGame { get; }

        public GamesListview()
        {
            InitializeComponent();
        }

        public void Load(List<Game> games)
        {
            Clear();

            if (games != null)
            {
                foreach (Game game in (from x in games orderby x.Name, x.GamePath select x).ToList())
                {
                    var item = new GamesListviewItem(game);
                    item.Margin = new Thickness(0, 0, 0, 15);
                    stackPanel.Children.Add(item);
                }
            }
        }

        public void Clear()
        {
            stackPanel.Children.Clear();     
        }
    }
}
