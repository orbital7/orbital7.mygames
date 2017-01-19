using System.IO;
using System.Threading.Tasks;

namespace Orbital7.MyGames.Devices
{
    public class DeviceSyncEngine
    {
        private Device Device { get; set; }

        private IAccessProvider AccessProvider { get; set; }

        public DeviceSyncEngine(Device device, IAccessProvider accessProvider)
        {
            this.Device = device;
            this.AccessProvider = accessProvider;
        }

        public async Task SyncGameSaveStatesAsync(Catalog catalog, bool includeGameConfigs)
        {
            var saveStateFileExtensions = Catalog.GetSaveStateFileExtensions();

            foreach (var gameList in catalog.GameLists)
            {
                string saveStatesFolderPath = await this.AccessProvider.EnsureFolderExistsAsync(
                    Path.Combine(gameList.PlatformFolderPath, GameList.SaveStatesFolderName));

                string gameConfigsFolderPath = await this.AccessProvider.EnsureFolderExistsAsync(
                    Path.Combine(gameList.PlatformFolderPath, GameList.GameConfigsFolderName));

                foreach (var game in gameList)
                {
                                                                                                                       
                }
            }
        }

        public void SyncGames(Catalog catalog)
        {

        }
    }
}
