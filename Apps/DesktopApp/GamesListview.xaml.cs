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
    public delegate void GamesListviewSelectionChangedHandler();

    /// <summary>
    /// Interaction logic for GamesListview.xaml
    /// </summary>
    public partial class GamesListview : UserControl
    {
        public event GamesListviewSelectionChangedHandler SelectionChanged;

        public bool AllowSelection { get; set; }

        public bool AllowEditing { get; set; }

        private List<Game> Games { get; set; }

        public GamesListviewItem SelectedItem { get; private set; }

        public Game SelectedGame
        {
            get { return this.SelectedItem?.Game; }
        }

        public GamesListview()
        {
            InitializeComponent();
        }

        public void Load(List<Game> games)
        {
            if (games != null)
                this.Games = (from x in games orderby x.Name, x.GamePath select x).ToList();
            else
                this.Games = null;

            Update();
        }

        public void Update()
        {
            Clear();

            if (this.Games != null)
            {
                foreach (Game game in this.Games)
                {
                    var item = new GamesListviewItem(this, game, this.AllowSelection, this.AllowEditing);
                    item.Margin = new Thickness(0, 10, 0, 5);
                    stackPanel.Children.Add(item);
                }
            }
        }
        
        public void Clear()
        {
            stackPanel.Children.Clear();     
        }

        public void Select(GamesListviewItem item)
        {
            if (this.AllowSelection)
            {
                foreach (GamesListviewItem child in stackPanel.Children)
                    child.IsSelected = false;
                item.IsSelected = true;
                this.SelectedItem = item;
                this.SelectionChanged?.Invoke();
            }
        }
    }
}
