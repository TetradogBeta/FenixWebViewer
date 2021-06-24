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
        public static string CacheFolder = "CacheCapitulos";
        public static TimeSpan TiempoMinimoRefresh { get; set; }
        static SortedList<string, KeyValuePair<long, string>> DicHtmlSerie { get; set; }
        static SortedList<string, string> DicHtmlCapitulo { get; set; }
        static Semaphore smDic = new Semaphore(1, 1);
        static HtmlDic()
        {
            DicHtmlSerie = new SortedList<string, KeyValuePair<long, string>>();
            DicHtmlCapitulo = new SortedList<string,  string>();
            TiempoMinimoRefresh = TimeSpan.FromMinutes(5);
            if (!Directory.Exists(CacheFolder))
                Directory.CreateDirectory(CacheFolder);
            else
            {
                foreach(string item in Directory.GetFiles(CacheFolder))
                {
                    DicHtmlCapitulo.Add(Path.GetFileName(item),File.ReadAllText(item));
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
            try
            {
                smDic.WaitOne();
                if (DicHtmlSerie.ContainsKey(url.AbsoluteUri))
                {
                    if (DateTime.Now - new DateTime(DicHtmlSerie[url.AbsoluteUri].Key) > TiempoMinimoRefresh)
                        DicHtmlSerie.Remove(url.AbsoluteUri);
                }
                if (!DicHtmlSerie.ContainsKey(url.AbsoluteUri))
                {
                    DicHtmlSerie.Add(url.AbsoluteUri, new KeyValuePair<long, string>(DateTime.Now.Ticks, url.DownloadString()));
                }
                html = DicHtmlSerie[url.AbsoluteUri].Value;
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

            smDic.WaitOne();
            if (!DicHtmlCapitulo.ContainsKey(url.AbsoluteUri))
            {
                DicHtmlCapitulo.Add(url.AbsoluteUri, url.DownloadString());
            }
            html= DicHtmlCapitulo[url.AbsoluteUri];
            smDic.Release();
            return html;
        }
        public static void SaveCache()
        {
            string path;
            foreach (var item in DicHtmlCapitulo)
            {
                try
                {
                    path = Path.Combine(CacheFolder, item.Key);
                    if (!File.Exists(path))
                        File.WriteAllText(path, item.Value);
                }
                catch
                {
                    System.Diagnostics.Debugger.Break();
                }
            }
        }
    }
}
