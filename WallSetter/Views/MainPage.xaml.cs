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
using WallSetter.Helpers;
using Control = Windows.UI.Xaml.Controls.Control;

namespace WallSetter.Views
{
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        private TraktApi trakt;
        private PasswordVault vault;

        public string unsplashLs
        {
            get => GetFromDictionary(nameof(unsplashLs));
            set
            {
                secrets[nameof(unsplashLs)] = value;
                WritePasswords();
            }
        }
        public string unsplashWp
        {
            get => GetFromDictionary(nameof(unsplashWp));
            set
            {
                secrets[nameof(unsplashWp)] = value;
                WritePasswords();
            }
        }
        public string traktId
        {
            get => GetFromDictionary(nameof(traktId));
            set
            {
                secrets[nameof(traktId)] = value;
                WritePasswords();
            }
        }
        public string traktSecret
        {
            get => GetFromDictionary(nameof(traktSecret));
            set
            {
                secrets[nameof(traktSecret)] = value;
                WritePasswords();
            }
        }
        public string traktRedirectUrl
        {
            get => GetFromDictionary(nameof(traktRedirectUrl));
            set
            {
                secrets[nameof(traktRedirectUrl)] = value;
                WritePasswords();
            }
        }
        public string fanartId
        {
            get => GetFromDictionary(nameof(fanartId));
            set
            {
                secrets[nameof(fanartId)] = value;
                WritePasswords();
            }
        }
        public string fanartSecret
        {
            get => GetFromDictionary(nameof(fanartSecret));
            set
            {
                secrets[nameof(fanartSecret)] = value;
                WritePasswords();
            }
        }
        public bool LockscreenShowsToggle
        {
            get => HelperMethods.ParseOrFalse(GetFromDictionary(nameof(LockscreenShowsToggle)));
            set
            {
                secrets[nameof(LockscreenShowsToggle)] = value.ToString();
                WritePasswords();
            }
        }
        public bool BaseImageOnTimeOfDay
        {
            get => HelperMethods.ParseOrFalse(GetFromDictionary(nameof(BaseImageOnTimeOfDay)));
            set
            {
                secrets[nameof(BaseImageOnTimeOfDay)] = value.ToString();
                WritePasswords();
            }
        }

        Dictionary<string, string> secrets = new Dictionary<string, string>(); 

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

        private void WritePasswords()
        {
            foreach (KeyValuePair<string, string> secret in secrets)
            {
                if (secret.Value == "")
                {
                    var cred = vault.RetrieveAll().FirstOrDefault(i => i.UserName == secret.Key);
                    if (cred != null)
                    {
                        vault.Remove(cred);
                    }
                }
                else
                {
                    vault.Add(new PasswordCredential("wallsetter", secret.Key, secret.Value));
                }
            }
        }

        private string GetFromDictionary(string name)
        {
            if (secrets == null)
            {
                return "";
            }

            return secrets.FirstOrDefault(i => i.Key == name).Value ?? "";
        }

        private void FrameworkElement_OnLoaded(object sender, RoutedEventArgs e)
        {
            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;
            vault = new PasswordVault();

            var logins = vault.RetrieveAll();

            foreach (PasswordCredential credential in logins)
            {
                credential.RetrievePassword();
                secrets.Add(credential.UserName, credential.Password);
                OnPropertyChanged(credential.UserName);
            }
            trakt = new TraktApi(traktRedirectUrl, traktId, traktSecret);
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

        private async void ChangeLockscreen(object sender, RoutedEventArgs e)
        {
            await Lockscreen.SetNew();
        }

        private void OnToggle(object sender, RoutedEventArgs e)
        {
            ToggleSwitch ts = (sender as ToggleSwitch);
            if (ts.FocusState == FocusState.Unfocused)
            {
                return;
            }
            LockscreenShowsToggle = ts.IsOn;
            OnPropertyChanged(nameof(LockscreenShowsToggle));
        }

        private void OnToggleBoTOD(object sender, RoutedEventArgs e)
        {
            ToggleSwitch ts = (sender as ToggleSwitch);
            if (ts.FocusState == FocusState.Unfocused)
            {
                return;
            }
            BaseImageOnTimeOfDay = ts.IsOn;
            OnPropertyChanged(nameof(BaseImageOnTimeOfDay));
        }
    }
}
