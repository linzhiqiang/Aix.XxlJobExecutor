using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Aix.XxlJobExecutor.Utils
{
    internal static class WebParamsUtils
    {
        static IDictionary<string, object> EmptyKeyValuePairs = new Dictionary<string, object>();

        public static IDictionary<string, object> ToKeyValuePairs(object obj)
        {
            if (obj == null)
                //return new List<KeyValuePair<string, object>>();
                return EmptyKeyValuePairs;

            return
                obj is string s ? StringToKV(s) :
                obj is IEnumerable e ? CollectionToKV(e) :
                ObjectToKV(obj);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="ignoreEmptyValue">忽略参数值为空的参数   true=忽略</param>
        /// <returns></returns>
        public static string BuildQuery(IEnumerable<KeyValuePair<string, object>> parameters, bool ignoreEmptyValue)
        {
            StringBuilder postData = new StringBuilder();
            bool hasParam = false;
            IEnumerator<KeyValuePair<string, object>> dem = parameters.GetEnumerator();
            while (dem.MoveNext())
            {
                string name = dem.Current.Key;
                string value = dem.Current.Value != null ? dem.Current.Value.ToString() : string.Empty;
                if (string.IsNullOrEmpty(name)) continue;

                if (!string.IsNullOrEmpty(value) || ignoreEmptyValue == false)
                {
                    if (hasParam)
                    {
                        postData.Append("&");
                    }

                    postData.Append(name);
                    postData.Append("=");
                    postData.Append(Uri.EscapeDataString(value ?? string.Empty));
                    hasParam = true;
                }
            }

            return postData.ToString();
        }

        public static string BuildQuery(IEnumerable<KeyValuePair<string, object>> parameters)
        {
            if (parameters == null || parameters.Count() == 0) return string.Empty;

            StringBuilder result = new StringBuilder();
            object value;
            int i = 0;
            foreach (KeyValuePair<string, object> kv in parameters)
            {
                value = HttpUtility.UrlEncode(kv.Value?.ToString() ?? "", Encoding.UTF8); // Uri.EscapeDataString  HttpUtility.UrlEncode(kv.Value, Encoding.UTF8)
                result.AppendFormat("{0}{1}={2}", i > 0 ? "&" : "", kv.Key, value);
                i++;
            }
            return result.ToString();
        }

        #region private 
        private static IDictionary<string, object> StringToKV(string query)
        {
            var result = new Dictionary<string, object>();
            query = query?.TrimStart('?');
            if (string.IsNullOrEmpty(query)) return result;

            foreach (var item in query.Split('&'))
            {
                var nameValue = SplitOnFirstOccurence(item, '=');
                var key = nameValue[0];
                var value = nameValue.Length > 1 ? nameValue[1] : null;

                result.Add(key, value);
            }

            return result;
        }

        private static string[] SplitOnFirstOccurence(string s, char separator)
        {
            // Needed because full PCL profile doesn't support Split(char[], int) (#119)
            if (string.IsNullOrEmpty(s))
                return new[] { s };

            var i = s.IndexOf(separator);
            if (i == -1)
                return new[] { s };

            return new[] { s.Substring(0, i), s.Substring(i + 1) };
        }

        private static IDictionary<string, object> CollectionToKV(IEnumerable col)
        {
            // Accepts KeyValuePairs or any arbitrary types that contain a property called "Key" or "Name" and a property called "Value".
            var result = new Dictionary<string, object>();
            foreach (var item in col)
            {
                if (item == null)
                    continue;

                string key;
                object val;

                var type = item.GetType();

                var keyProp = type.GetRuntimeProperty("Key") ?? type.GetRuntimeProperty("key") ?? type.GetRuntimeProperty("Name") ?? type.GetRuntimeProperty("name");
                var valProp = type.GetRuntimeProperty("Value") ?? type.GetRuntimeProperty("value");

                //var keyProp = type.GetProperty("Key") ?? type.GetProperty("key") ?? type.GetProperty("Name") ?? type.GetProperty("name");
                //var valProp = type.GetProperty("Value") ?? type.GetProperty("value");


                if (keyProp != null && valProp != null)
                {
                    key = keyProp.GetValue(item, null)?.ToString();
                    val = valProp.GetValue(item, null);
                }
                else
                {
                    key = item?.ToString();
                    val = null;
                }

                if (key != null)
                    // yield return new KeyValuePair<string, object>(key, val);
                    result.Add(key, val);
            }
            return result;
        }

        private static IDictionary<string, object> ObjectToKV(object obj)
        {
            var kvs = from prop in obj.GetType().GetRuntimeProperties()
                      let getter = prop.GetMethod
                      where getter?.IsPublic == true
                      let val = getter.Invoke(obj, null)
                      select new KeyValuePair<string, object>(prop.Name, val);

            return ToDictionary(kvs);

            //return from prop in obj.GetType().GetProperties()
            //       let getter = prop.GetGetMethod(false)
            //       where getter != null
            //       let val = getter.Invoke(obj, null)
            //       select new KeyValuePair<string, object>(prop.Name, val);

        }

        private static Dictionary<K, V> ToDictionary<K, V>(IEnumerable<KeyValuePair<K, V>> keyValues)
        {
            var result = new Dictionary<K, V>();
            foreach (var item in keyValues)
            {
                result.Add(item.Key, item.Value);
            }
            return result;
        }

        #endregion
    }
}
