using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Security.Credentials;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using TraktApiSharp.Exceptions;
using Wallsetter;
using Control = Windows.UI.Xaml.Controls.Control;

namespace WallSetter.Views
{
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        private TraktApi trakt;
        private PasswordVault vault;
        List<Control> controls = new List<Control>();

        public MainPage()
        {
            InitializeComponent();
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

        private void FrameworkElement_OnLoaded(object sender, RoutedEventArgs e)
        {
            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;
            vault = new PasswordVault();

            controls.Add(unsplashLs);
            controls.Add(unsplashWp);
            controls.Add(traktId);
            controls.Add(traktSecret);
            controls.Add(traktRedirectUrl);
            controls.Add(fanartId);
            controls.Add(fanartSecret);
            controls.Add(LockscreenShowsToggle);

            var logins = vault.RetrieveAll();
            UpdateEnable(false);

            foreach (PasswordCredential credential in logins)
            {
                credential.RetrievePassword();
                if (credential.UserName == "LockscreenShowsToggle")
                {
                    bool enableTrakt = bool.Parse(credential.Password);
                    UpdateEnable(enableTrakt);
                    LockscreenShowsToggle.IsOn = enableTrakt;
                }
                foreach (Control control in controls)
                {
                    if(control.Name != credential.UserName)
                        continue;

                    if (control is PasswordBox)
                    {
                        (control as PasswordBox).Password = credential.Password;
                    }
                    if (control is TextBox)
                    {
                        (control as TextBox).Text = credential.Password;
                    }

                }
            }
            trakt = new TraktApi(traktRedirectUrl.Text, traktId.Password, traktSecret.Password);
        }

        private void UpdateEnable(bool enableTrakt)
        {
            traktId.IsEnabled = enableTrakt;
            traktSecret.IsEnabled = enableTrakt;
            traktAcessCode.IsEnabled = enableTrakt;
            fanartId.IsEnabled = enableTrakt;
            fanartSecret.IsEnabled = enableTrakt;
            ConnectToTrakt.IsEnabled = enableTrakt;
        }

        private async void ChangeWallpaper(object sender, RoutedEventArgs e)
        {
            await Wallpaper.SetNewWallpaper();
        }

        private async void ConnectToTrakt_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await trakt.RequestAuth();
            }
            catch (Exception exception)
            {
                await new MessageDialog(exception.Message).ShowAsync();
            }
        }

        private async void DeleteData(object sender, RoutedEventArgs e)
        {
            await ApplicationData.Current.ClearAsync();
            vault.RetrieveAll().ToList().ForEach(i => vault.Remove(i));
        }

        private async void Codebox_Changed(object sender, RoutedEventArgs routedEventArgs)
        {
            if ((sender as Control)?.Name == "traktAcessCode")
            {
                var res = await trakt.Connect(traktAcessCode.Password);
                vault.Add(new PasswordCredential("wallsetter", "traktRefreshToken", res.RefreshToken));
                vault.Add(new PasswordCredential("wallsetter", "traktAccessToken", res.AccessToken));
                return;
            }

            await Set(sender);
        }


        private async Task Set(object sender)
        {
            if (controls.Any(i => i.Name == (sender as Control)?.Name))
            {
                if (sender is TextBox || sender is PasswordBox)
                {
                    string name;
                    string text;
                    if (sender is TextBox)
                    {
                        TextBox textBox = (sender as TextBox);
                        name = textBox.Name;
                        text = textBox.Text;
                    }
                    else
                    {
                        PasswordBox textBox = (sender as PasswordBox);
                        name = textBox.Name;
                        text = textBox.Password;
                    }
                    if (text == "")
                    {
                        var cred = vault.RetrieveAll().FirstOrDefault(i => i.UserName == name);
                        if (cred != null)
                        {
                            vault.Remove(cred);
                        }
                    }
                    else
                    {
                        vault.Add(new PasswordCredential("wallsetter", name, text));
                    }
                }
                if (sender is ToggleSwitch)
                {
                    vault.Add(new PasswordCredential("wallsetter", (sender as ToggleSwitch).Name, (sender as ToggleSwitch).IsOn.ToString()));
                }
                trakt = new TraktApi(traktRedirectUrl.Text, traktId.Password, traktSecret.Password);
            }
            else
            {
                await new MessageDialog("This type I do not know how to adapt to").ShowAsync();
            }
        }


        private async void ChangeLockscreen(object sender, RoutedEventArgs e)
        {
            await Lockscreen.SetNew();
        }

        private async void LockScreenShowsEnabled(object sender, RoutedEventArgs routedEventArgs)
        {
            UpdateEnable((sender as ToggleSwitch).IsOn);
            await Set(sender);
        }
    }

    internal static class Shuffler
    {
        private static Random rng = new Random();

        public static void Shuffle<T>(this IList<T> list)
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
        }
    }
}
