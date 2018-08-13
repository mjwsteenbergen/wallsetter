using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Web.Http;
using FanArtPortable;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using WallSetter.Views;

namespace WallSetter
{
    public class FanArt
    {
        private string key;
        private string client_code;

        public FanArt(string key, string client_code)
        {
            this.key = key;
            this.client_code = client_code;
        }

        public async Task<string> GetImage(string id)
        {
            HttpClient client = new HttpClient();
            var res = await client.GetAsync(new Uri("http://webservice.fanart.tv/v3/tv/" + id + "&api_key=" + client_code, UriKind.Absolute));
            var s = ShowImage.FromJson(await res.Content.ReadAsStringAsync());
            s.Showbackground.Shuffle();
            return s.Showbackground[0].Url;
        }
    }

    public partial class ShowImage
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("thetvdb_id")]
        public string ThetvdbId { get; set; }

        [JsonProperty("clearlogo")]
        public Characterart[] Clearlogo { get; set; }

        [JsonProperty("hdtvlogo")]
        public Characterart[] Hdtvlogo { get; set; }

        [JsonProperty("seasonposter")]
        public Characterart[] Seasonposter { get; set; }

        [JsonProperty("tvposter")]
        public Characterart[] Tvposter { get; set; }

        [JsonProperty("tvbanner")]
        public Characterart[] Tvbanner { get; set; }

        [JsonProperty("tvthumb")]
        public Characterart[] Tvthumb { get; set; }

        [JsonProperty("showbackground")]
        public Characterart[] Showbackground { get; set; }

        [JsonProperty("clearart")]
        public Characterart[] Clearart { get; set; }

        [JsonProperty("hdclearart")]
        public Characterart[] Hdclearart { get; set; }

        [JsonProperty("seasonthumb")]
        public Characterart[] Seasonthumb { get; set; }

        [JsonProperty("seasonbanner")]
        public Characterart[] Seasonbanner { get; set; }

        [JsonProperty("characterart")]
        public Characterart[] Characterart { get; set; }
    }

    public partial class Characterart
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("lang")]
        public Lang Lang { get; set; }

        [JsonProperty("likes")]
        public int Likes { get; set; }

        [JsonProperty("season")]
        public string Season { get; set; }
    }

    public enum Lang { Ar, Empty, En, Es, Fr, He, It, Ru, The00,
        De, Hu
    };

    public partial class ShowImage
    {
        public static ShowImage FromJson(string json) => JsonConvert.DeserializeObject<ShowImage>(json, Converter.Settings);
    }

    static class LangExtensions
    {
        public static Lang? ValueForString(string str)
        {
            switch (str)
            {
                case "ar": return Lang.Ar;
                case "": return Lang.Empty;
                case "en": return Lang.En;
                case "es": return Lang.Es;
                case "fr": return Lang.Fr;
                case "he": return Lang.He;
                case "it": return Lang.It;
                case "ru": return Lang.Ru;
                case "hu": return Lang.Hu;
                case "de": return Lang.De;
                case "00": return Lang.The00;
                default: return Lang.Empty;
            }
        }

        public static Lang ReadJson(JsonReader reader, JsonSerializer serializer)
        {
            var str = serializer.Deserialize<string>(reader);
            var maybeValue = ValueForString(str);
            if (maybeValue.HasValue) return maybeValue.Value;
            throw new Exception("Unknown enum case " + str);
        }

        public static void WriteJson(this Lang value, JsonWriter writer, JsonSerializer serializer)
        {
            switch (value)
            {
                case Lang.Ar:
                    serializer.Serialize(writer, "ar");
                    break;
                case Lang.Empty:
                    serializer.Serialize(writer, "");
                    break;
                case Lang.En:
                    serializer.Serialize(writer, "en");
                    break;
                case Lang.Es:
                    serializer.Serialize(writer, "es");
                    break;
                case Lang.De:
                    serializer.Serialize(writer, "de");
                    break;
                case Lang.Fr:
                    serializer.Serialize(writer, "fr");
                    break;
                case Lang.He:
                    serializer.Serialize(writer, "he");
                    break;
                case Lang.It:
                    serializer.Serialize(writer, "it");
                    break;
                case Lang.Ru:
                    serializer.Serialize(writer, "ru");
                    break;
                case Lang.The00:
                    serializer.Serialize(writer, "00");
                    break;
            }
        }
    }

    public static class Serialize
    {
        public static string ToJson(this ShowImage self) => JsonConvert.SerializeObject(self, Converter.Settings);
    }

    internal class Converter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(Lang) || t == typeof(Lang?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (t == typeof(Lang))
                return LangExtensions.ReadJson(reader, serializer);
            if (t == typeof(Lang?))
            {
                if (reader.TokenType == JsonToken.Null) return null;
                return LangExtensions.ReadJson(reader, serializer);
            }
            throw new Exception("Unknown type");
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var t = value.GetType();
            if (t == typeof(Lang))
            {
                ((Lang)value).WriteJson(writer, serializer);
                return;
            }
            throw new Exception("Unknown type");
        }

        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters = {
                new Converter(),
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }
}
