using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.Security.Credentials;
using Windows.UI.ViewManagement;

namespace WallSetter.Helpers
{
    class HelperMethods
    {
        public static string GetValueFromVault(PasswordVault vault, string name)
        {
            var wpCredential = vault.RetrieveAll().FirstOrDefault(i => i.UserName == name);
            wpCredential?.RetrievePassword();
            return wpCredential?.Password;
        }

        public static bool ParseOrFalse(string input)
        {
            Boolean.TryParse(input, out bool ret);
            return ret;
        }

        public static string GetSearchQueryBasedOnTimeOfDay()
        {
            int hour = DateTime.Now.Hour;
            if (hour < 7 || hour > 22)
            {
                return "?" + new List<string> {"night", "space", "black"}.Shuffle().First();
            }

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


        public static string GetDefaultImageUrl(PasswordVault vault, string searchTerms)
        {
            var WidthAndHeight = GetValueFromVault(vault, "WidthAndHeight");

            var UseFeaturedImagesOnly = HelperMethods.ParseOrFalse(HelperMethods.GetValueFromVault(vault, "UseFeaturedImagesOnly")) ? "featured/" : "";

            string url = $"https://source.unsplash.com/{UseFeaturedImagesOnly}{WidthAndHeight}/";
            bool BaseImageOnTimeOfDay =
                HelperMethods.ParseOrFalse(HelperMethods.GetValueFromVault(vault, nameof(BaseImageOnTimeOfDay)));

            return url + HelperMethods.GetSearchQueryBasedOnTimeOfDay() ?? searchTerms ?? "";
        }
    }
}
