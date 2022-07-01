using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LockChests.Utilities
{
    internal static class Json
    {
        private static JsonSerializerSettings _serializerSettings = new JsonSerializerSettings()
        {
            Formatting = Formatting.Indented,
            ObjectCreationHandling = ObjectCreationHandling.Replace
        };

        public static T? Read<T>(string json) => JsonConvert.DeserializeObject<T>(json);

        public static string Write<T>(T obj) => JsonConvert.SerializeObject(obj, _serializerSettings);
    }
}
