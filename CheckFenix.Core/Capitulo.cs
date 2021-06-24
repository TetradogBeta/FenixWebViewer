using System;
using System.Xml;
using System.Linq;
using System.Collections.Generic;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using System.Drawing;
using Gabriel.Cat.S.Extension;
using System.IO;
using System.Net;

namespace CheckFenix.Core
{
    public static class HtmlDic
    {
        public const string URLANIMEFENIX = "https://www.animefenix.com/";
        public static string CacheFolder = "CacheHtml";
        public static TimeSpan TiempoMinimoRefresh = TimeSpan.FromMinutes(5);
        static SortedList<string, KeyValuePair<long, string>> DicHtml { get; set; } = new SortedList<string, KeyValuePair<long, string>>();
        public static HtmlDocument GetHtml(Uri url)
        {

            return new HtmlDocument().LoadString(GetString(url));
        }
        public static string GetString(Uri url)
        {
            if (DicHtml.ContainsKey(url.AbsoluteUri))
            {
                if (DateTime.Now - new DateTime(DicHtml[url.AbsoluteUri].Key) > TiempoMinimoRefresh)
                    DicHtml.Remove(url.AbsoluteUri);
            }
            if (!DicHtml.ContainsKey(url.AbsoluteUri))
            {
                DicHtml.Add(url.AbsoluteUri, new KeyValuePair<long, string>(DateTime.Now.Ticks, url.DownloadString()));
            }
            return DicHtml[url.AbsoluteUri].Value;
        }
        public static void SaveCache()
        {
            Uri urlAnimeFenix = new Uri(URLANIMEFENIX);

            foreach (var item in DicHtml)
            {
             File.WriteAllText(item.Value.Value,System.IO.Path.Combine(CacheFolder, new Uri(item.Key).MakeRelativeUri(urlAnimeFenix).ToString() + ".html"));
            }
        }
    }
    public class Capitulo
    {
        public static string CacheFolder = "CacheCapitulos";
        static SortedList<string, Bitmap> DicImagenes { get; set; } = new SortedList<string, Bitmap>();

        Serie parent;
        public Capitulo() { }
        public Capitulo(HtmlNode nodeDiv)
        {
            HtmlNode nodeLink = nodeDiv.ChildNodes[1].ChildNodes[1];
            Pagina = new Uri(nodeLink.Attributes["href"].Value);
            Name = nodeLink.Attributes["title"].Value;
            Picture = new Uri(nodeLink.ChildNodes[1].Attributes["src"].Value);
        }
        public string Name { get; set; }
        public Uri Picture { get; set; }
        public Bitmap Image
        {
            get
            {
                if (!DicImagenes.ContainsKey(Picture.AbsoluteUri))
                    DicImagenes.Add(Picture.AbsoluteUri, Picture.GetBitmap().Escala(0.25f));
                return DicImagenes[Picture.AbsoluteUri];
            }
        }
        public Uri Pagina { get; set; }
        public Serie Parent
        {
            get
            {
                string urlParent;
                if (Equals(parent, default(Serie)))
                {
                    //cargo la serie
                    urlParent = Pagina.AbsoluteUri;
                    urlParent = urlParent.Replace("/ver/", "/");
                    urlParent = urlParent.Remove(urlParent.LastIndexOf('-'));

                    parent = Serie.FromUrl(new Uri(urlParent));
                }
                return parent;
            }
        }
        public IEnumerable<string> GetLinks()
        {

            string url;
            string html = HtmlDic.GetString(Pagina);
            Regex regex = new Regex(@"(?<=<iframe[^>]*?)(?:\s*width=[""'](?<width>[^""']+)[""']|\s*height=[""'](?<height>[^'""]+)[""']|\s*src=[""'](?<src>[^'""]+[""']))+[^>]*?>");
            Match match = regex.Match(html);


            while (match.Success)
            {
                url = HtmlNode.CreateNode("<iframe " + match.Value).Attributes["src"].Value;
                yield return url;
                match = match.NextMatch();
            }

        }
        public bool AbrirLink()
        {
            string url = GetLinks().Where(l => l.Contains("mega.nz")).FirstOrDefault();
            if (string.IsNullOrEmpty(url))
            {
                url = GetLinks().FirstOrDefault();
            }
            if (!string.IsNullOrEmpty(url))
            {
                new Uri(url).Abrir();
            }
            return !string.IsNullOrEmpty(url);
        }

        public static IEnumerable<Capitulo> GetCapitulosActuales(string urlFenix)
        {
            return GetCapitulos(new HtmlDocument().LoadUrl(urlFenix).DocumentNode);
        }
        public static IEnumerable<Capitulo> GetCapitulos(HtmlNode nodePagina)
        {
            return nodePagina.GetByClass("capitulos-grid").First().GetByClass("item").Select(c => new Capitulo(c));

        }
        public static Capitulo FromUrl(Uri urlVisionado)
        {
            Capitulo capitulo = new Capitulo() { Pagina = urlVisionado };
            HtmlDocument docUrl = HtmlDic.GetHtml(urlVisionado);
            HtmlNode nodoName = docUrl.GetByTagName("h1").FirstOrDefault();


            capitulo.Name = nodoName.InnerText;
            capitulo.Picture = capitulo.Parent.Picture;

            return capitulo;
        }
        public static void SaveCache()
        {
            Uri urlAnimeFenix = new Uri(HtmlDic.URLANIMEFENIX);

            foreach (var item in DicImagenes)
            {
                item.Value.Save(System.IO.Path.Combine(CacheFolder, new Uri(item.Key).MakeRelativeUri(urlAnimeFenix).ToString() + ".jpg"), System.Drawing.Imaging.ImageFormat.Jpeg);
            }
        }
    }
}
