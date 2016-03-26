using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Divvvv
{
    public static class Stuff
    {
        public static TSource MaxBy<TSource>(this IEnumerable<TSource> source, Func<TSource, IComparable> selector)
        {
            TSource max = default(TSource);
            foreach (TSource i in source)
                if (selector(i).CompareTo(max) > 0)
                    max = i;
            return max;
        }

        public static T? ParseOrNull<T>(this string s) where T : struct
        {
            T n = default(T);
            if ((bool?) typeof (T).GetMethod("TryParse", new[] {typeof (string), typeof (T)})?.Invoke(null, new object[] {s, n}) ?? false)
                return null;
            return n;

        }

        public static bool IsInt(this string s)
        {
            int n;
            return int.TryParse(s, out n);
        }

        public static string[] Split(this string s, string separator) => s.Split(new[] {separator}, StringSplitOptions.None);

        public static string Unescape(this string s) => Regex.Unescape(s);

        public static string NullIfEmpty(this string s) => s == "" ? null : s;

        public static string ReMatch(this string s, string re, int groupIdx = 1) => Regex.Match(s, re).Groups[groupIdx].Value;

        public static IEnumerable<string> ReMatches(this string s, string re, int groupIdx = 1) => 
            Regex.Matches(s, re).Cast<Match>().Select(m => m.Groups[groupIdx].Value);

        public static string JsonValue(string json, string key) => json.ReMatch($"\"{key}\":" + "(\")?(.*?)(?(1)\")[,}]", 2);

        public static string GetHtmlPage(string url)
        {
            try
            {
                using (var client = new WebClient())
                {
                    client.Encoding = System.Text.Encoding.UTF8;
                    client.Headers[HttpRequestHeader.Accept] = "text/html, image/png, image/jpeg, image/gif, */*;q=0.1";
                    client.Headers[HttpRequestHeader.UserAgent] = "Mozilla/5.0 (X11; CrOS x86_64 7428.0.2015) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/46.0.2483.0 Safari/537.36";
                    return client.DownloadString(url);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine(url);
                return "";
            }
        }

        public static async Task<string> GetHtmlPageAsync(string url)
        {
            try
            {
                using (var client = new WebClient())
                {
                    client.Encoding = System.Text.Encoding.UTF8;
                    client.Headers[HttpRequestHeader.Accept] = "text/html, image/png, image/jpeg, image/gif, */*;q=0.1";
                    client.Headers[HttpRequestHeader.UserAgent] = "Mozilla/5.0 (X11; CrOS x86_64 7428.0.2015) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/46.0.2483.0 Safari/537.36";
                    return await client.DownloadStringTaskAsync(url);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine(url);
                return "";
            }
        }
    }

    public class Json
    {
        private readonly string _json;
        public Json(string s)
        {
            _json = s;
        }

        public string this[string key] => Value(key);

        public string Value(string key) => Stuff.JsonValue(_json, key);

        public static implicit operator Json(string s) => new Json(s);
    }

    // just because
    public class XmlNode
    {
        private readonly string _xml;
        private string _innerString = null;
        public XmlNode(string s)
        {
            _xml = s;
            Tag = s.ReMatch("<([^? ]+?)[ >]");
        }

        public string Tag { get; }

        public string InnerString => 
            _innerString ?? (_innerString = _xml.ReMatch($@"<{Tag}.*?>\s*([\s\S]*?)\s*</{Tag}>"));

        public IEnumerable<XmlNode> Nodes => 
            InnerString.ReMatches(@"<([^? ]+).*?>[\s\S]*?</\1>", 0).Select(s => new XmlNode(s));  

        public string this[string attribute] => _xml.ReMatch($"<{Tag}.*?{attribute}=\"(.*?)\".*?>");

        public static implicit operator XmlNode(string s) => new XmlNode(s);
    }
}