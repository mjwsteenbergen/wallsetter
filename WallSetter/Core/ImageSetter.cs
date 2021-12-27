using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using wallsetter.Models;
using Windows.Storage;
using Windows.System.UserProfile;

namespace wallsetter.Core
{
    public class ImageSetter
    {

        public static async Task UpdateImages(Settings settings)
        {
            await Clean();
            Image<Rgba32> originalImage = null;
            
            if(settings.Wallpaper.IsEnabled)
            {
                var wallpaperImage = await ImageDownloader.DownloadNewImage(settings.Wallpaper, settings.ScreenSize);

                if (settings.SyncWithOther && settings.Lockscreen.IsEnabled)
                {
                    originalImage = wallpaperImage.Clone();
                }

                StorageFile file = await Save(ApplyEffects(wallpaperImage, settings.Wallpaper.ImageEffects, settings.Wallpaper.UnsplashSettings.ChangeOnTimeOfDay), "wallpaper.jpg");
                await UserProfilePersonalizationSettings.Current.TrySetWallpaperImageAsync(file);
            }

            if (settings.Lockscreen.IsEnabled)
            {
                Image<Rgba32> lockscreenimage = null;

                if (settings.SyncWithOther && settings.Wallpaper.IsEnabled)
                {
                    lockscreenimage = originalImage;
                }
                else
                {
                    lockscreenimage = await ImageDownloader.DownloadNewImage(settings.Lockscreen, settings.ScreenSize);
                }

                StorageFile file = await Save(ApplyEffects(lockscreenimage, settings.Lockscreen.ImageEffects, settings.Lockscreen.UnsplashSettings.ChangeOnTimeOfDay), "lockscreen.jpg");
                await UserProfilePersonalizationSettings.Current.TrySetLockScreenImageAsync(file);
            }

        }

        private static Image<Rgba32> ApplyEffects(Image<Rgba32> image, ImageEffects settings, bool changeOnTimeOfDay)
        {
            if (settings.Blur)
            {
                image.Mutate(i => i.GaussianBlur(10));
            }

            if (settings.BlacknWhite)
            {
                image.Mutate(i => i.Grayscale(GrayscaleMode.Bt601));
            }

            if (settings.Glow)
            {
                image.Mutate(i => i.Glow(GraphicsOptions.Default, Rgba32.Azure));
            }

            if (settings.OilPaint)
            {
                image.Mutate(i => i.OilPaint());
            }

            if (settings.Polaroid)
            {
                image.Mutate(i => i.Polaroid());
            }

            if (settings.Sepia)
            {
                image.Mutate(i => i.Sepia());
            }

            int hour = DateTime.Now.Hour;
            if (changeOnTimeOfDay && (hour < 8 || hour > 21))
            {
                image.Mutate(i => i.Brightness(0.3f));
            }

            return image;
        }

        private async static Task<StorageFile> Save(Image<Rgba32> image, string fileName)
        {
            StorageFile destinationFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(
                fileName, CreationCollisionOption.GenerateUniqueName);
            using (var ms = new MemoryStream())
            {
                image.Save(ms, new JpegEncoder());
                await FileIO.WriteBufferAsync(destinationFile, ms.GetWindowsRuntimeBuffer());
            }
            return destinationFile;
        }

        public static async Task Clean()
        {
            try
            {
                foreach (StorageFile file in await ApplicationData.Current.LocalFolder.GetFilesAsync())
                {
                    await file.DeleteAsync();
                }
            }
            catch (FileLoadException) { }
        }
    }
}
