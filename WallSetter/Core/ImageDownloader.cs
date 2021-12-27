using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using wallsetter.Models;
using Windows.Web.Http;

namespace wallsetter.Core
{
    public static class ImageDownloader
    {
        public static async Task<Image<Rgba32>> DownloadNewImage(ImageSettings settings, string screensize)
        {
            var url = GenerateUrl(settings, screensize);
            HttpClient client = new HttpClient();

            var buffer = await client.GetBufferAsync(new Uri(url));

            return Image.Load(buffer.AsStream());
        }

        private static string GenerateUrl(ImageSettings settings, string screensize)
        {
            switch(settings.ChosenSource)
            {
                case ChosenSource.tvshow:
                    return GetUrlForTVShow();
                case ChosenSource.url:
                    return string.IsNullOrEmpty(settings.ImageUrl) ? GetUrlForUnsplash(settings.UnsplashSettings, screensize) : settings.ImageUrl;
                case ChosenSource.unsplash:
                    return GetUrlForUnsplash(settings.UnsplashSettings, screensize);
                default:
                    throw new Exception("Unknown imagesource");
            }
        }

        private static string GetUrlForUnsplash(UnsplashSettings settings, string screensize)
        {
            var UseFeaturedImagesOnly = settings.OnlyUseFeatured ? "featured/" : "";

            string url = $"https://source.unsplash.com/{UseFeaturedImagesOnly}{screensize}/";

            return url + (GetSearchQueryBasedOnTimeOfDay(settings.ChangeOnTimeOfDay) ?? $"?{settings.SearchKeywords}" ?? "");
        }

        private static string GetSearchQueryBasedOnTimeOfDay(bool BaseImageOnTimeOfDay)
        {
            if (!BaseImageOnTimeOfDay)
            {
                return null;
            }

            //int hour = DateTime.Now.Hour;
            //if (hour < 8 || hour > 21)
            //{
            //    return "?" + new List<string> { "night", "space", "star", "galaxy", "astrophotography", "milky way" }.Shuffle().First();
            //}

            int hour = DateTime.Now.Hour;
            if (hour == 8)
            {
                return "?sunrise";
            }

            if (hour == 21)
            {
                return "?sunset";
            }

            return null;
        }

        private static string GetUrlForTVShow()
        {
            throw new NotImplementedException();
        }
    }

    internal static class Shuffler
    {
        private static Random rng = new Random();

        public static IList<T> Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
            return list;
        }
    }
}
