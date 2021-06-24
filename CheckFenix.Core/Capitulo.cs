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
    public class Capitulo
    {
        public static string CacheFolder = "CacheCapitulos";
        static SortedList<string, Bitmap> DicImagenes { get; set; }

        Serie parent;
        static Capitulo()
        {
            DicImagenes = new SortedList<string, Bitmap>();
            if (!Directory.Exists(CacheFolder))
                Directory.CreateDirectory(CacheFolder);
            else
            {
                //cargo el cache!
                foreach (string item in Directory.GetFiles(CacheFolder))
                    DicImagenes.Add(Path.GetFileName(item), new Bitmap(item));
            }
        }
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
                string url = Path.GetFileName(Picture.AbsoluteUri);
                if (!DicImagenes.ContainsKey(url))
                    DicImagenes.Add(url, Picture.GetBitmap().Escala(0.25f));
                return DicImagenes[url];
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
        public IEnumerable<Comentario> GetComentarios(IReadComentario reader)
        {

            return Comentario.GetComentarios(reader,Pagina);
        }
        public IEnumerable<string> GetLinks()
        {

            string url;
            string html = HtmlDic.GetStringCapitulo(Pagina);
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
            HtmlDocument docUrl = HtmlDic.GetHtmlCapitulo(urlVisionado);
            HtmlNode nodoName = docUrl.GetByTagName("h1").FirstOrDefault();


            capitulo.Name = nodoName.InnerText;
            capitulo.Picture = capitulo.Parent.Picture;

            return capitulo;
        }
        public static void SaveCache()
        {
            string path;
            foreach (var item in DicImagenes)
            {
                try
                {
                    path = Path.Combine(CacheFolder, item.Key);
                    if (!File.Exists(path))
                        item.Value.Save(path);
                }
                catch {
                    System.Diagnostics.Debugger.Break();
                }
            }
        }
    }
}
