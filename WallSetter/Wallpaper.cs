using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.UserDataTasks;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.Security.Credentials;
using Windows.Storage;
using Windows.System.UserProfile;
using Windows.UI.ViewManagement;
using Windows.Web.Http;
using WallSetter.Helpers;
using WallSetter.Views;

namespace Wallsetter
{
    public class Wallpaper
    {
        public static async Task<StorageFile> DownloadWallpaper(string url, string fileName)
        {
            Uri uri = new Uri(url);

            StorageFile destinationFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(
                fileName, CreationCollisionOption.GenerateUniqueName);


            HttpClient client = new HttpClient();

            var buffer = await client.GetBufferAsync(uri);
            await Windows.Storage.FileIO.WriteBufferAsync(destinationFile, buffer);
            return destinationFile;
        }

        public static async Task SetWallpaper(StorageFile file)
        {
            var s = await UserProfilePersonalizationSettings.Current.TrySetWallpaperImageAsync(file);
        }

        public static async Task SetNewWallpaper()
        {
            await Clean();

            PasswordVault vault = new PasswordVault();

            var url = HelperMethods.GetValueFromVault(vault, "WallpaperUrl") ?? HelperMethods.GetDefaultImageUrl(vault, HelperMethods.GetValueFromVault(vault, "WallpaperSearchTags"));

            var storage = await DownloadWallpaper(url, DateTime.Today.ToString("t").Replace(':', '-') + ".jpg");
            await SetWallpaper(storage);
        }

        public static async Task Clean()
        {
            foreach (StorageFile file in await ApplicationData.Current.LocalFolder.GetFilesAsync())
            {
                await file.DeleteAsync();
            }
        }

        public static async Task SetNewLockScreen(StorageFile file)
        {
            var s = await UserProfilePersonalizationSettings.Current.TrySetLockScreenImageAsync(file);
        }
    }
}
