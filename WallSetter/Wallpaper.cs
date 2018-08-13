using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.UserDataTasks;
using Windows.Security.Credentials;
using Windows.Storage;
using Windows.System.UserProfile;
using Windows.Web.Http;

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
            var wpCredential = vault.RetrieveAll().FirstOrDefault(i => i.UserName == "unsplashWp");
            wpCredential?.RetrievePassword();
            var url = wpCredential?.Password ?? "https://source.unsplash.com/featured/1920x1080/";
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
