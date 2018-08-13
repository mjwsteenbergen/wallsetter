using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using TraktApiSharp;
using TraktApiSharp.Authentication;
using TraktApiSharp.Enums;
using TraktApiSharp.Objects.Get.Calendars;
using TraktApiSharp.Objects.Get.Shows;
using TraktApiSharp.Objects.Get.Shows.Episodes;

namespace WallSetter
{
    public class TraktApi
    {
        TraktClient trakt;


        public TraktApi(string redirectUrl, string id, string secret)
        {
            trakt = new TraktApiSharp.TraktClient(id, secret);
            trakt.Authentication.RedirectUri = redirectUrl;
        }

        public TraktApi(string redirect, string id, string secret, string accessToken, string refreshtoken) : this(redirect, id, secret)
        {
            trakt.Authorization =
                TraktAuthorization.CreateWith(accessToken, refreshtoken);
        }

        public async Task RequestAuth()
        {
            var success = await Windows.System.Launcher.LaunchUriAsync(new Uri(trakt.OAuth.CreateAuthorizationUrl()));
        }

        public async Task<TraktAuthorization> Connect(string code)
        {
            var res = await trakt.OAuth.GetAuthorizationAsync(code);
            return res;
        }
        
        public async Task<IEnumerable<TraktCalendarShow>> GetLatestShow() => await trakt.Calendar.GetUserShowsAsync(DateTime.Now.AddDays(-1.0), 2);

        public async Task RefreshToken()
        {
            await trakt.Authentication.RefreshAuthorizationAsync();
        }

        public async Task<bool> IsWatched(TraktEpisode episode)
        {
            var items = (await trakt.Sync.GetWatchedHistoryAsync(TraktSyncItemType.Episode, episode.Ids.Trakt)).Items;
            return items.Any();
        }
    }
}
