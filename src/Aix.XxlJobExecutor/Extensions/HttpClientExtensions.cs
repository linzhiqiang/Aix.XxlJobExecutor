using Aix.XxlJobExecutor.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Aix.XxlJobExecutor
{
    public static class HttpClientExtensions
    {
        public static async Task<HttpResponseMessage> SendAsync(this HttpClient client, string url, HttpMethod httpMethod, HttpContent httpContent, IDictionary<string, string> headers)
        {
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage(httpMethod, url);
            httpRequestMessage.Content = httpContent;
            AddHead(httpRequestMessage, headers);
            return await client.SendAsync(httpRequestMessage);
        }

        #region get

        public static Task<T> GetAsync<T>(this HttpClient client, string url)
        {
            return GetAsync<T>(client,url, null, null);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="requestParams">支持三种形式：对象或字典或 a=1&b=2</param>
        /// <param name="headers"></param>
        /// <returns></returns>
        public static async Task<T> GetAsync<T>(this HttpClient client, string url, object requestParams, IDictionary<string, string> headers = null)
        {
            url = AddUrlParams(url, requestParams);
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, url);
            AddHead(httpRequestMessage, headers);

            var response = await client.SendAsync(httpRequestMessage);
            if (typeof(T) == typeof(byte[]))
            {
                var bytes = await response.Content.ReadAsByteArrayAsync();
                return (T)Convert.ChangeType(bytes, typeof(T));
            }

            return FromJson<T>(await response.Content.ReadAsStringAsync());
        }

        #endregion

        #region post



        public static async Task<T> PostAsync<T>(this HttpClient client, string url, HttpContent httpContent, IDictionary<string, string> headers = null)
        {
            var response = await client.SendAsync(url, HttpMethod.Post, httpContent, headers);
            if (typeof(T) == typeof(byte[]))
            {
                var bytes = await response.Content.ReadAsByteArrayAsync();
                return (T)Convert.ChangeType(bytes, typeof(T));
            }

            return FromJson<T>(await response.Content.ReadAsStringAsync());
        }

        public static async Task<T> PostJsonAsync<T>(this HttpClient client, string url, object requestParams, IDictionary<string, string> headers = null)
        {
            string requestString = ToJson(requestParams);
            var httpContent = new StringContent(requestString, Encoding.UTF8, "application/json");
            //var httpContent = new StringContent(requestString, Encoding.UTF8);
            //content.Headers.ContentType = new MediaTypeHeaderValue("application/json");//application/json; charset=UTF-8
            return await PostAsync<T>(client, url, httpContent, headers);
        }

        public static async Task<T> PostUrlEncodedAsync<T>(this HttpClient client, string url, object requestParams, IDictionary<string, string> headers = null)
        {
            var keyValues = WebParamsUtils.ToKeyValuePairs(requestParams);
            IDictionary<string, string> encodedParms = new Dictionary<string, string>();
            foreach (var item in keyValues)
            {
                encodedParms.Add(item.Key, item.Value?.ToString());
            }
            var content = new FormUrlEncodedContent(encodedParms);
            //等价于
            //var body = WebUtils.BuildQuery(keyValues,false);
            //var content = new StringContent(body, Encoding.UTF8, "application/x-www-form-urlencoded");
            //ByteArrayContent
            return await PostAsync<T>(client, url, content, headers);
        }

        public static async Task<T> PostFileAsync<T>(this HttpClient client, string url, object requestParams, byte[] datas, string fileName, IDictionary<string, string> headers = null)
        {
            var keyValues = WebParamsUtils.ToKeyValuePairs(requestParams);
            MultipartFormDataContent content = new MultipartFormDataContent();
            foreach (var item in keyValues)
            {
                content.Add(new StringContent(item.Value?.ToString() ?? string.Empty), item.Key);
            }

            var fileContent = new ByteArrayContent(datas);
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");// = MediaTypeHeaderValue.Parse("audio/mp3");
            content.Add(fileContent, "file", fileName);

            return await PostAsync<T>(client,url, content, headers);
        }

        #endregion

        #region private

        private static HttpRequestMessage AddHead(HttpRequestMessage httpRequestMessage, IDictionary<string, string> headers)
        {
            if (headers == null) return httpRequestMessage;

            foreach (var item in headers)
            {
                httpRequestMessage.Headers.Add(item.Key, item.Value);
            }
            return httpRequestMessage;
        }

        private static string DictToStr(IEnumerable<KeyValuePair<string, object>> dict, string str_join, bool isUrlEncode)
        {
            if (dict == null || dict.Count() == 0) return string.Empty;

            str_join = str_join == null ? "&" : str_join;
            StringBuilder result = new StringBuilder();
            object value;
            int i = 0;
            foreach (KeyValuePair<string, object> kv in dict)
            {
                value = isUrlEncode == true ? HttpUtility.UrlEncode(kv.Value?.ToString() ?? "", Encoding.UTF8) : kv.Value; // Uri.EscapeDataString  HttpUtility.UrlEncode(kv.Value, Encoding.UTF8)
                result.AppendFormat("{0}{1}={2}", i > 0 ? str_join : "", kv.Key, value);
                i++;
            }
            return result.ToString();

        }

        private static string AddUrlParams(string url, object requestParams)
        {
            if (requestParams == null) return url;
            string query = "";
            if (requestParams is string)
            {
                query = requestParams.ToString();
            }
            else
            {
                var keyValues = WebParamsUtils.ToKeyValuePairs(requestParams);
                query = DictToStr(keyValues, "&", true);
            }

            if (url.IndexOf("?") > 0)
            {
                url = url + '&' + query;
            }
            else
            {
                url = url + '?' + query;
            }
            return url;
        }

        private static T FromJson<T>(string value)
        {
            if (typeof(T) == typeof(string))
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            return JsonUtils.FromJson<T>(value);
        }

        private static string ToJson(object value)
        {
            return JsonUtils.ToJson(value);
        }

        #endregion

    }
}
