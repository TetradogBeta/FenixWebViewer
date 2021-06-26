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
using Gabriel.Cat.S.Utilitats;

namespace CheckFenix.Core
{
    public class Capitulo
    {
        public static string CacheFolder = "CacheCapitulos";
        static LlistaOrdenada<string, Bitmap> DicImagenes { get; set; }
        static LlistaOrdenada<string, Capitulo> DicCapitulos { get; set; }
        Serie parent;
        static Capitulo()
        {
            DicImagenes = new LlistaOrdenada<string, Bitmap>();
            DicCapitulos = new LlistaOrdenada<string, Capitulo>();

            if (Directory.Exists(CacheFolder))
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
                    DicImagenes.Add(url, Picture.GetBitmap().Escala(0.35f));
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
            return Comentario.GetComentarios(reader, Pagina);
        }
        public IEnumerable<string> GetLinksFromHtml()
        {

            string url;
            string html;
            Regex regex;
            Match match;
            try
            {
                html = HtmlAndLinksDic.GetHtmlServer(Pagina);
            }
            catch
            {
                html = string.Empty;
            }
            regex = new Regex(@"(?<=<iframe[^>]*?)(?:\s*width=[""'](?<width>[^""']+)[""']|\s*height=[""'](?<height>[^'""]+)[""']|\s*src=[""'](?<src>[^'""]+[""']))+[^>]*?>");
            match = regex.Match(html);


            while (match.Success)
            {
                url = HtmlNode.CreateNode("<iframe " + match.Value).Attributes["src"].Value;
                match = match.NextMatch();

                yield return url;
            }


        }
        public bool AbrirLink(string serverPreference = "mega.nz")
        {
            string url = HtmlAndLinksDic.GetLinks(this).Where(l => l.Contains(serverPreference)).FirstOrDefault();
            if (string.IsNullOrEmpty(url))
            {
                url = HtmlAndLinksDic.GetLinks(this).FirstOrDefault();
            }
            if (!string.IsNullOrEmpty(url))
            {
                new Uri(url).Abrir();
            }
            return !string.IsNullOrEmpty(url);
        }
        public override string ToString()
        {
            return Name;
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
            Capitulo capitulo;
            HtmlDocument docUrl;
            HtmlNode nodoName;
            if (!DicCapitulos.ContainsKey(urlVisionado.AbsoluteUri))
            {
                capitulo = new Capitulo() { Pagina = urlVisionado };
                DicCapitulos.Add(urlVisionado.AbsoluteUri, capitulo);

                docUrl = new HtmlDocument().LoadString(HtmlAndLinksDic.GetHtml(urlVisionado));
                nodoName = docUrl.GetByTagName("h1").FirstOrDefault();

                if (!Equals(nodoName, default(HtmlNode)))
                    capitulo.Name = nodoName.InnerText;

                if (Equals(capitulo.Parent.Total, int.Parse(urlVisionado.AbsoluteUri.Substring(urlVisionado.AbsoluteUri.LastIndexOf("-")+1))))
                    capitulo.Picture = capitulo.Parent.Picture;
                else
                    capitulo.Picture = capitulo.Parent.UltimoOrDefault.Picture;

            }
            return DicCapitulos[urlVisionado.AbsoluteUri];
        }
        public static void SaveCache()
        {
            string path;

            if (DicImagenes.Count > 0 && !Directory.Exists(CacheFolder))
                Directory.CreateDirectory(CacheFolder);
            else if (DicImagenes.Count == 0 && Directory.Exists(CacheFolder))
                Directory.Delete(CacheFolder);

            foreach (var item in DicImagenes)
            {
                try
                {
                    path = Path.Combine(CacheFolder, item.Key);
                    if (!File.Exists(path))
                        item.Value.Save(path);
                }
                catch
                {
                    System.Diagnostics.Debugger.Break();
                }
            }
        }
    }
}
