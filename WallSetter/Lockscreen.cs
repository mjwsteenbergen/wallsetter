using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Credentials;
using TraktApiSharp.Objects.Get.Calendars;
using Wallsetter;
using WallSetter.Helpers;
using WallSetter.Views;

namespace WallSetter
{
    class Lockscreen
    {
        public static async Task SetNew()
        {
            PasswordVault vault = new PasswordVault();

            var lsSetting = vault.RetrieveAll().FirstOrDefault(i => i.UserName == "LockscreenShowsToggle");
            var enableTrakt = false;
            if (lsSetting != null)
            {
                lsSetting.RetrievePassword();
                enableTrakt = bool.Parse(lsSetting.Password);
            }

            if (enableTrakt)
            {
                try
                {
                    //Trakt
                    var traktId = vault.FindAllByUserName("traktId")[0];
                    var traktSecret = vault.FindAllByUserName("traktSecret")[0];
                    var traktRefreshToken = vault.FindAllByUserName("traktRefreshToken")[0];
                    var traktAccessToken = vault.FindAllByUserName("traktAccessToken")[0];
                    var traktRedirectUrl = vault.FindAllByUserName("traktRedirectUrl")[0];
                    traktId.RetrievePassword();
                    traktSecret.RetrievePassword();
                    traktRefreshToken.RetrievePassword();
                    traktAccessToken.RetrievePassword();
                    traktRedirectUrl.RetrievePassword();

                    //FanArt
                    var fanartId = vault.FindAllByUserName("fanartId")[0];
                    var fanartSecret = vault.FindAllByUserName("fanartSecret")[0];
                    fanartId.RetrievePassword();
                    fanartSecret.RetrievePassword();



                    TraktApi trakt = new TraktApi(traktRedirectUrl.Password, traktId.Password, traktSecret.Password, traktAccessToken.Password, traktRefreshToken.Password);

                    var newEpisodes = (await trakt.GetLatestShow()).ToList();
                    List<TraktCalendarShow> l2 = new List<TraktCalendarShow>();
                    foreach (TraktCalendarShow show in newEpisodes)
                    {
                        if (!await trakt.IsWatched(show.Episode))
                        {
                            l2.Add(show);
                        }
                    }

                    newEpisodes = l2;

                    if (newEpisodes.Count > 0)
                    {
                        newEpisodes.Shuffle();
                        var latest = newEpisodes.First();
                        var url = await new FanArt(fanartId.Password, fanartSecret.Password).GetImage(latest.Show.Ids.Tvdb.ToString());
                        await Wallpaper.Clean();
                        var file = await Wallpaper.DownloadWallpaper(url, DateTime.Now.Hour + "" + DateTime.Now.Millisecond + "-lock.jpg");
                        await Wallpaper.SetNewLockScreen(file);
                    }
                    else
                    {
                        await RegularLockscreen();
                    }
                }
                catch (Exception ex)
                {
                    await RegularLockscreen();
                }
            }
            else
            {
                await RegularLockscreen();
            }
        }

        private static async Task RegularLockscreen()
        {
            PasswordVault vault = new PasswordVault();

            var url = HelperMethods.GetValueFromVault(vault, "LockscreenUrl") ?? HelperMethods.GetDefaultImageUrl(vault, HelperMethods.GetValueFromVault(vault, "LockscreenSearchTags"));

            var file = await Wallpaper.DownloadWallpaper(url,
                DateTime.Now.Hour + "" + DateTime.Now.Millisecond + "-lock.jpg");
                await Wallpaper.SetNewLockScreen(file);
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
