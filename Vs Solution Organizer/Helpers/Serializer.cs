using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Vs_Solution_Organizer.Model;
using Windows.Storage;

namespace Vs_Solution_Organizer.Helpers
{
    class Serializer
    {
        public static T Deserialize<T>(string json)
        {
            var _Bytes = Encoding.Unicode.GetBytes(json);
            using (MemoryStream _Stream = new MemoryStream(_Bytes))
            {
                var _Serializer = new DataContractJsonSerializer(typeof(T));
                return (T)_Serializer.ReadObject(_Stream);
            }
        }

        public static string Serialize(object instance)
        {
            using (MemoryStream _Stream = new MemoryStream())
            {
                var _Serializer = new DataContractJsonSerializer(instance.GetType());
                _Serializer.WriteObject(_Stream, instance);
                _Stream.Position = 0;
                using (StreamReader _Reader = new StreamReader(_Stream))
                { return _Reader.ReadToEnd(); }
            }
        }

        public static async void SaveConfiguration(object configuration)
        {
            var jsonContent = Serialize(configuration);
            byte[] jsonBytes = System.Text.Encoding.UTF8.GetBytes(jsonContent);
            StorageFile file = await Windows.Storage.ApplicationData.Current.LocalFolder.CreateFileAsync("solutionMapper.json", CreationCollisionOption.ReplaceExisting);
            using (var stream = await file.OpenStreamForWriteAsync())
            {
                stream.Write(jsonBytes, 0, jsonBytes.Length);
            }
        }

        public static async Task<T> LoadConfiguration<T>()
        {
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            try
            {
                StorageFile configFile = await localFolder.GetFileAsync("solutionMapper.json");
            }
            catch (Exception e)
            {
                return default(T);
            }
            
            Stream stream = await localFolder.OpenStreamForReadAsync("solutionMapper.json");
            string textFile;
            using (StreamReader reader = new StreamReader(stream))
            {
                textFile = await reader.ReadToEndAsync();
            }
            return Deserialize<T>(textFile);
        }



    }
}
