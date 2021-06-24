using System;
using System.Collections.Generic;
using HtmlAgilityPack;
using System.IO;
using System.Threading;

namespace CheckFenix.Core
{
    public static class HtmlDic
    {
        public const string URLANIMEFENIX = "https://www.animefenix.com/";
        public static string CacheFolder = "CacheHtml";
        public static TimeSpan TiempoMinimoRefresh { get; set; }
        static SortedList<string, KeyValuePair<long, string>> DicHtmlSerie { get; set; }
        static SortedList<string, string> DicHtmlCapitulo { get; set; }
        static Semaphore smDic = new Semaphore(1, 1);
        static HtmlDic()
        {
            DicHtmlSerie = new SortedList<string, KeyValuePair<long, string>>();
            DicHtmlCapitulo = new SortedList<string,  string>();
            TiempoMinimoRefresh = TimeSpan.FromMinutes(5);
            if (Directory.Exists(CacheFolder))
            {
                foreach(string item in Directory.GetFiles(CacheFolder))
                {
                    DicHtmlCapitulo.Add(Path.GetFileNameWithoutExtension(item),File.ReadAllText(item));
                }
            }

        }
        public static HtmlDocument GetHtmlSerie(Uri url)
        {

            return new HtmlDocument().LoadString(GetStringSerie(url));
        }
        public static HtmlDocument GetHtmlCapitulo(Uri url)
        {

            return new HtmlDocument().LoadString(GetStringCapitulo(url));
        }
        public static string GetStringSerie(Uri url)
        {
            string html;
            string key = url.AbsoluteUri.Substring(url.AbsoluteUri.LastIndexOf('/') + 1);
            try
            {
                smDic.WaitOne();
                if (DicHtmlSerie.ContainsKey(key))
                {
                    if (DateTime.Now - new DateTime(DicHtmlSerie[key].Key) > TiempoMinimoRefresh)
                        DicHtmlSerie.Remove(key);
                }
                if (!DicHtmlSerie.ContainsKey(key))
                {
                    DicHtmlSerie.Add(key, new KeyValuePair<long, string>(DateTime.Now.Ticks, url.DownloadString()));
                }
                html = DicHtmlSerie[key].Value;
            }
            catch
            {
                throw;
            }
            finally
            {
                smDic.Release();
            }
            return html;
        }
        public static string GetStringCapitulo(Uri url)
        {
            string html;
            string key = url.AbsoluteUri.Substring(url.AbsoluteUri.LastIndexOf('/') + 1);
            smDic.WaitOne();
            if (!DicHtmlCapitulo.ContainsKey(key))
            {
                DicHtmlCapitulo.Add(key, url.DownloadString());
            }
            html= DicHtmlCapitulo[key];
            smDic.Release();
            return html;
        }
        public static void SaveCache()
        {
            string path;
            if (DicHtmlCapitulo.Count > 0 && !Directory.Exists(CacheFolder))
                Directory.CreateDirectory(CacheFolder);
            else if (DicHtmlCapitulo.Count == 0 && Directory.Exists(CacheFolder))
                Directory.Delete(CacheFolder);
            foreach (var item in DicHtmlCapitulo)
            {
                try
                {
                    path = Path.Combine(CacheFolder, item.Key);
                    if (!File.Exists(path))
                        File.WriteAllText($"{path}.html",item.Value);
                }
                catch
                {
                    System.Diagnostics.Debugger.Break();
                }
            }
        }
        public static void Clear()
        {
            smDic.WaitOne();
            DicHtmlCapitulo.Clear();
            smDic.Release();
        }
    }
}
