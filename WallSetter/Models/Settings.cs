using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wallsetter.Helpers;
using Windows.Graphics.Display;
using Windows.UI.ViewManagement;

namespace wallsetter.Models
{
    public class Settings
    {
        public ImageSettings Lockscreen { get; set; }

        public ImageSettings Wallpaper { get; set; }

        public string ScreenSize { get; set; }
        public bool SyncWithOther { get; set; }
        public uint RunEvery { get; set; }

        public async static Task<Settings> GetSettings()
        {
            Settings @default = GetDefaultSettings();
            return await new Memory().ReadFile("settings.json", @default);
        }

        internal static Settings GetDefaultSettings()
        {
            return new Settings
            {
                RunEvery = 15,
                ScreenSize = "",
                SyncWithOther = true,
                Lockscreen = new ImageSettings
                {
                    ChosenSource = ChosenSource.unsplash,
                    IsEnabled = false,
                    ImageUrl = "",
                    UnsplashSettings = new UnsplashSettings
                    {
                        ChangeOnTimeOfDay = true,
                        OnlyUseFeatured = true,
                        SearchKeywords = "",
                    },
                    ImageEffects = new ImageEffects
                    {
                        BlacknWhite = true,
                        Blur = false,
                        Glow = false,
                        OilPaint = false,
                        Polaroid = false,
                        Sepia = false
                    }
                },
                Wallpaper = new ImageSettings
                {
                    ChosenSource = ChosenSource.unsplash,
                    IsEnabled = false,
                    ImageUrl = "",
                    UnsplashSettings = new UnsplashSettings
                    {
                        ChangeOnTimeOfDay = true,
                        OnlyUseFeatured = true,
                        SearchKeywords = "",
                    },
                    ImageEffects = new ImageEffects
                    {
                        BlacknWhite = false,
                        Blur = false,
                        Glow = false,
                        OilPaint = false,
                        Polaroid = false,
                        Sepia = false
                    }
                }
            };
        }

        public static async Task Save(Settings settings)
        {
            await new Memory().WriteFile("settings.json", settings);
        }
    }

    public class ImageSettings
    {
        public bool IsEnabled { get; set; }

        public ChosenSource ChosenSource { get; set; }

        public UnsplashSettings UnsplashSettings { get; set; }

        public string ImageUrl { get; set; }

        public ImageEffects ImageEffects { get; set; }
    }

    public enum ChosenSource
    {
        unsplash, url, tvshow
    }

    public class UnsplashSettings
    {
        public string SearchKeywords { get; set; }
        public bool OnlyUseFeatured { get; set; }
        public bool ChangeOnTimeOfDay { get; set; }
    }

    public class ImageEffects
    {
        public bool BlacknWhite { get; set; }
        public bool Blur { get; set; }

        public bool Glow { get; set; }
        public bool OilPaint { get; set; }

        public bool Polaroid { get; set; }
        public bool Sepia { get; set; }

    }
}
