using Orbital7.MyGames;
using System;
using System.Collections.Generic;
using System.Drawing;
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
    /// Interaction logic for EditGameView.xaml
    /// </summary>
    public partial class EditGameView : UserControl
    {
        public Game Game { get; private set; }

        public EditGameView()
        {
            InitializeComponent();
        }

        public void Load(Game game)
        {
            if (game != null)
            {
                panel.IsEnabled = true;
                this.Game = game;
                this.DataContext = game;
                image.Source = game.Image.ToImageSource();
                this.Foreground = System.Windows.Media.Brushes.White;
            }
            else
            {
                Clear();
            }
        }

        public void Clear()
        {
            panel.IsEnabled = false;
            this.Game = null;
            this.DataContext = null;
            image.Source = null;
            this.Foreground = System.Windows.Media.Brushes.DarkGray;
        }
    }
}
