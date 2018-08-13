using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Credentials;

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

            return "";
        }
    }
}
