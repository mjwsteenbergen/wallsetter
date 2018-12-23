using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace wallsetter.Helpers
{
    public class Memory
    {
        public readonly StorageFolder BaseUrl;

        public StorageFolder ApplicationDataPath => BaseUrl ?? ApplicationData.Current.RoamingFolder;

        public Memory() { }

        public Memory(StorageFolder BaseUrl)
        {
            this.BaseUrl = BaseUrl;
        }

        public async Task<T> ReadFile<T>(string filename)
        {
            string text = await ReadFile(filename);

            if (text == "")
            {
                throw new FileNotFoundException();
            }

            if (typeof(T) == typeof(string))
            {
                return (T)(object)text;
            }

            return JsonConvert.DeserializeObject<T>(text);
        }

        public async Task<T> ReadFile<T>(string filename, T @default)
        {
            try
            {
                return await ReadFile<T>(filename);
            }
            catch (FileNotFoundException)
            {
                await WriteFile(filename, @default);
                return @default;
            }
        }

        public async Task<string> ReadFile(string filename)
        {
            StorageFile file = await ApplicationDataPath.GetFileAsync(filename);
            return await FileIO.ReadTextAsync(file);
        }

        public async Task WriteFile(string filename, object obj)
        {
            string str = null;
            if (obj is string)
            {
                str = obj as string;
            }

            try
            {
                await ApplicationDataPath.CreateFileAsync(filename, CreationCollisionOption.FailIfExists);
            } catch { }
            StorageFile file = await ApplicationDataPath.GetFileAsync(filename);
            await FileIO.WriteTextAsync(file, str ?? JsonConvert.SerializeObject(obj, Formatting.Indented));
        }
    }
}
