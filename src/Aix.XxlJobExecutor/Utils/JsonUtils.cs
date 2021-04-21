using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Aix.XxlJobExecutor.Utils
{
    internal static class JsonUtils
    {
        private static JsonSerializerSettings _jsonSerializer = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };

        public static string ToJson(object obj)
        {
            if (obj == null) return string.Empty;
            if (obj is string || obj.GetType().IsValueType)
            {
                return obj.ToString();
            }
            return JsonConvert.SerializeObject(obj);
        }

        public static T FromJson<T>(string str)
        {
            return (T)FromJson(str, typeof(T));
        }

        public static object FromJson(string str, Type type)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return null;
            }

            if (type == typeof(string))
            {
                return str;
            }
            if (type.IsValueType)
            {
                return Convert.ChangeType(str, type);
            }

            return JsonConvert.DeserializeObject(str, type, _jsonSerializer);
        }

        public static Dictionary<string, T> FromJsonToDict<T>(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return new Dictionary<string, T>();
            }
            var jsonDict = JsonConvert.DeserializeObject<Dictionary<string, T>>(str);
            if (jsonDict == null) jsonDict = new Dictionary<string, T>();
            return jsonDict;
        }
    }
}
