using Gabriel.Cat.S.Extension;
using Gabriel.Cat.S.Utilitats;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CheckFenix.Core
{
    public class Capitulo
    {
        public static string CacheFolder = "CacheCapitulos";

        List<string> links;
        Uri paginaSerie;
        Serie serie;
        int numero;
        private Uri picture;


        public Capitulo() { numero = -1; }
        public Capitulo(Uri pagina) : this()
        {
            Pagina = pagina;
        }

        public string Name { get; set; }
        public Uri Pagina { get; set; }
        public int Numero
        {
            get
            {
                if (numero < 0)
                {
                    numero = int.Parse(Pagina.AbsoluteUri.Substring(Pagina.AbsoluteUri.LastIndexOf('-') + 1));
                }
                return numero;
            }
        }
        public Uri Picture
        {
            get
            {
                HtmlDocument htmlDocument;
                if (Equals(picture, default))
                {
                    htmlDocument = new HtmlDocument().LoadUrlCloudflare(Pagina);
                    picture = new Uri(htmlDocument.GetByClass("is-2by4").First().GetByTagName("img").First().Attributes["src"].Value);
                }
                return picture;
            }
            set => picture = value;
        }


        public Uri PaginaSerie
        {
            get
            {
                if (Equals(paginaSerie, default))
                {
                    paginaSerie = new Uri(Pagina.AbsoluteUri.Remove(Pagina.AbsoluteUri.LastIndexOf('-')).Replace("/ver/", "/"));
                }
                return paginaSerie;

            }
            set => paginaSerie = value;
        }
        public Serie Serie
        {
            get
            {
                if (Equals(serie, default))
                {
                    serie = new Serie(PaginaSerie);
                }
                return serie;
            }
            set => serie = value;
        }
        public bool EsElUltimo => Equals(Pagina, Serie.PaginaUltimo);
        public List<string> Links
        {
            get
            {
                if (Equals(links, default) || links.Count == 0)
                    Reload();
                return links;
            }
            set => links = value;
        }
        public Bitmap GetImage()
        {
            Bitmap imgCapitulo;
            string fileName = Path.GetFileName(Picture.AbsoluteUri);
            string pathFile = Path.Combine(CacheFolder, fileName);

            if (!Directory.Exists(CacheFolder))
                Directory.CreateDirectory(CacheFolder);

            if (!File.Exists(pathFile))
            {

                imgCapitulo = Picture.GetBitmapCloudflare();
                imgCapitulo = imgCapitulo.Escala(0.35f);
                try
                {
                    imgCapitulo.Save(pathFile);
                }
                catch { }
            }
            else imgCapitulo = new Bitmap(pathFile);

            return imgCapitulo;
        }

        public void Reload()
        {
            string url;
            string html;
            Regex regex;
            Match match, matchUrl;
            try
            {
                html = Pagina.DownloadCloudflare();


            }
            catch
            {
                html = string.Empty;
            }
            regex = new Regex(@"(?<=<iframe[^>]*?)(?:\s*width=[""'](?<width>[^""']+)[""']|\s*height=[""'](?<height>[^'""]+)[""']|\s*src=[""'](?<src>[^'""]+[""']))+[^>]*?>");
            match = regex.Match(html);
            links = new List<string>();

            while (match.Success)
            {
                url = HtmlNode.CreateNode("<iframe " + match.Value).Attributes["src"].Value.Replace("&amp;", "&");
                try
                {
                    html = new Uri(url).DownloadCloudflare();
                    matchUrl = regex.Match(html);
                    if (matchUrl.Success)
                    {
                        url = HtmlNode.CreateNode("<iframe " + matchUrl.Value).Attributes["src"].Value.Replace("&amp;", "&");

                        links.Add(url);
                    }
                }
                catch
                {

                }

                match = match.NextMatch();


            }

        }
        public bool AbrirLink()
        {
            string url;


            url = GetLinkMega();

            if (!string.IsNullOrEmpty(url))
            {
                new Uri(url).Abrir();
            }
            else
            {
                url = Links.FirstOrDefault();
                if (!string.IsNullOrEmpty(url))
                {
                    new Uri(url).Abrir();
                }
            }
            return !string.IsNullOrEmpty(url);
        }
        public string GetLinkMega()
        {
            return Links.Where(l => l.Contains("mega")).Select(l => l.Replace("embed", "file")).FirstOrDefault();
        }
        public override string ToString()
        {
            return Name;
        }

        public static IEnumerable<Capitulo> GetCapitulosHome(string urlFenix)
        {
            HtmlDocument tDoc = new HtmlDocument().LoadUrlCloudflare(urlFenix);

            return GetCapitulosHome(tDoc.DocumentNode);
        }

        public static IEnumerable<Capitulo> GetCapitulosHome(HtmlNode nodePagina)
        {
            return nodePagina.GetByClass("capitulos-grid").First().GetByClass("item").Select(nodeDiv =>
            {

                HtmlNode nodeLink;
                Capitulo capitulo = new Capitulo();
                nodeLink = nodeDiv.ChildNodes[1].ChildNodes[1];
                capitulo.Pagina = new Uri(nodeLink.Attributes["href"].Value);
                capitulo.Name = nodeLink.Attributes["title"].Value;
                capitulo.Picture = new Uri(nodeLink.ChildNodes[1].Attributes["src"].Value);

                return capitulo;


            });

        }




    }
}
