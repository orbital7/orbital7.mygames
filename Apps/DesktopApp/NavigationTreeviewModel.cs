using Orbital7.MyGames;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DesktopApp
{
    public class NavigationTreeviewModel : INotifyPropertyChanged
    {
        private List<NavigationTreeviewModelItem> _items = new List<NavigationTreeviewModelItem>();

        public List<NavigationTreeviewModelItem> Items
        {
            get { return _items; }
            set
            {
                _items = value;
                NotifiyPropertyChanged("Items");
            }
        }

        public NavigationTreeviewModel(Catalog catalog)
        {
            var platformItem = new NavigationTreeviewModelItem()
            {
                Text = "Platforms",
                FontWeight = FontWeights.Bold,
            };

            foreach (var gameList in catalog.GameLists)
            {
                platformItem.Children.Add(new NavigationTreeviewModelItem()
                {
                    Text = Path.GetFileName(gameList.PlatformFolderPath),
                    GameList = gameList,
                });
            }

            this.Items.Add(platformItem);
        }

        void NotifiyPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
