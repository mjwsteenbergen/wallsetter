using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using wallsetter.Core;
using wallsetter.Models;
using Windows.ApplicationModel.Core;
using Windows.Graphics.Display;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls;

namespace wallsetter.Views
{
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        Settings Settings = Settings.GetDefaultSettings();

        public bool WallpaperUnsplashSelected
        {
            get => Settings.Wallpaper.ChosenSource == ChosenSource.unsplash;
            set
            {
                if (value)
                {
                    Settings.Wallpaper.ChosenSource = ChosenSource.unsplash;
                    SettingsHaveChanged();
                }
            }
        }
        public bool LockscreenUnsplashSelected
        {
            get => Settings.Lockscreen.ChosenSource == ChosenSource.unsplash;
            set
            {
                if (value)
                {
                    Settings.Lockscreen.ChosenSource = ChosenSource.unsplash;
                    SettingsHaveChanged();
                }
            }
        }
        public bool WallpaperTvShowSelected
        {
            get => Settings.Wallpaper.ChosenSource == ChosenSource.tvshow;
            set
            {
                if (value)
                {
                    Settings.Wallpaper.ChosenSource = ChosenSource.tvshow;
                    SettingsHaveChanged();
                }
            }
        }
        public bool LockscreenTvShowSelected
        {
            get => Settings.Lockscreen.ChosenSource == ChosenSource.tvshow;
            set
            {
                if (value)
                {
                    Settings.Lockscreen.ChosenSource = ChosenSource.tvshow;
                    SettingsHaveChanged();
                }
            }
        }
        public bool WallpaperUrlSelected
        {
            get => Settings.Wallpaper.ChosenSource == ChosenSource.url;
            set
            {
                if (value)
                {
                    Settings.Wallpaper.ChosenSource = ChosenSource.url;
                    SettingsHaveChanged();
                }
            }
        }
        public bool LockscreenUrlSelected
        {
            get => Settings.Lockscreen.ChosenSource == ChosenSource.url;
            set
            {
                if (value)
                {
                    Settings.Lockscreen.ChosenSource = ChosenSource.url;
                    SettingsHaveChanged();
                }
            }
        }

        public MainPage()
        {
            InitializeComponent();
            Page_Loaded();
            App.Current.Suspending += async (a, b) =>
            {
                var def = b.SuspendingOperation.GetDeferral();
                await Settings.Save(Settings);
                def.Complete();
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Set<T>(ref T storage, T value, [CallerMemberName]string propertyName = null)
        {
            if (Equals(storage, value))
            {
                return;
            }

            storage = value;
            OnPropertyChanged(propertyName);
        }

        private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private async void Page_Loaded()
        {
            // Remove top bar
            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;
            var currentView = SystemNavigationManager.GetForCurrentView();
            currentView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;

            //GetScreensize
            var bounds = ApplicationView.GetForCurrentView().VisibleBounds;
            var scaleFactor = DisplayInformation.GetForCurrentView();

            Settings = await Settings.GetSettings();

            SetTimeSpanOptions(Settings);

            var windowSize = $"{scaleFactor.ScreenWidthInRawPixels}x{scaleFactor.ScreenHeightInRawPixels}";
            Settings.ScreenSize = windowSize;
            SettingsHaveChanged();
        }

        private void SetTimeSpanOptions(Settings settings)
        {
            TimeSpanOptions.Items.Clear();

            GetPossibleTimeSpanOptions().ForEach(i => TimeSpanOptions.Items.Add(Format(i)));

            TimeSpanOptions.SelectedItem = Format(TimeSpan.FromMinutes(settings.RunEvery));
        }

        private string Format(TimeSpan i)
        {
            if(i.Days >= 1) {
                return i.Days > 1 ? i.Days + " days" : "1 day";
            } else if (i.Hours >= 1)
            {
                return i.Hours > 1 ? i.Hours + " hours" : "1 hour";
            } else
            {
                return i.Minutes > 1 ? i.Minutes + " minutes" : "1 minute";
            }
        }

        private List<TimeSpan> GetPossibleTimeSpanOptions ()
        {
            return new List<TimeSpan>()
            {
                TimeSpan.FromMinutes(15),
                TimeSpan.FromMinutes(30),
                TimeSpan.FromHours(1),
                TimeSpan.FromHours(2),
                TimeSpan.FromHours(12),
                TimeSpan.FromDays(1),
            };
        }

        private async void UpdateWallpapers(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            await Settings.Save(Settings);
            await ImageSetter.UpdateImages(Settings);
        }

        private void SettingsHaveChanged()
        {
            OnPropertyChanged(nameof(Settings));
            OnPropertyChanged(nameof(LockscreenTvShowSelected));
            OnPropertyChanged(nameof(LockscreenUnsplashSelected));
            OnPropertyChanged(nameof(LockscreenUrlSelected));
            OnPropertyChanged(nameof(WallpaperTvShowSelected));
            OnPropertyChanged(nameof(WallpaperUnsplashSelected));
            OnPropertyChanged(nameof(WallpaperUrlSelected));

        }

        private void Page_Unloaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
        }

        private void ToggleSwitch_Toggled(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            //OnPropertyChanged(nameof(LockscreenTvShowSelected));
            //OnPropertyChanged(nameof(LockscreenUnsplashSelected));
            //OnPropertyChanged(nameof(LockscreenUrlSelected));
            //OnPropertyChanged(nameof(WallpaperTvShowSelected));
            //OnPropertyChanged(nameof(WallpaperUnsplashSelected));
            //OnPropertyChanged(nameof(WallpaperUrlSelected));
        }

        private void TimeSpanOptions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selected = (string) e.AddedItems[0];
            Settings.RunEvery = Convert.ToUInt32(GetPossibleTimeSpanOptions().Find(i => Format(i) == selected).TotalMinutes);
        }
    }
}
