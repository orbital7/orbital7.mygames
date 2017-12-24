using Orbital7.MyGames;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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
                Type = NavigationTreeviewModelItemType.Platforms,
                FontWeight = FontWeights.Bold,
            };
            this.Items.Add(platformItem);

            foreach (var gameList in catalog.GameLists)
            {
                platformItem.Children.Add(new NavigationTreeviewModelItem()
                {
                    Text = gameList.PlatformFolderName,
                    Type = NavigationTreeviewModelItemType.Platform,
                    GameList = gameList,
                });
            }

            this.Items.Add(new NavigationTreeviewModelItem()
            {
                Text = "All Incomplete",
                Type = NavigationTreeviewModelItemType.IncompleteGames,
                FontWeight = FontWeights.Bold,
            });
        }

        void NotifiyPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
