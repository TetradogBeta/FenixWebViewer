using System;
using System.Xml;
using System.Linq;
using System.Collections.Generic;
using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace CheckFenix.Core
{
    public class Capitulo
    {
        Serie parent;
        public Capitulo() { }
        public Capitulo(HtmlNode nodeDiv)
        {
            HtmlNode nodeLink = nodeDiv.ChildNodes[1].ChildNodes[1];
            Pagina =new Uri( nodeLink.Attributes["href"].Value);
            Name = nodeLink.Attributes["title"].Value;
            Picture = new Uri(nodeLink.ChildNodes[1].Attributes["src"].Value);
        }
        public string Name { get; set; }
        public Uri Picture { get; set; }
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
            string html = Pagina.DownloadString();
            Regex regex=new Regex(@"(?<=<iframe[^>]*?)(?:\s*width=[""'](?<width>[^""']+)[""']|\s*height=[""'](?<height>[^'""]+)[""']|\s*src=[""'](?<src>[^'""]+[""']))+[^>]*?>");
            Match match = regex.Match(html);

            while (match.Success)
            {
                yield return HtmlNode.CreateNode("<iframe "+match.Value).Attributes["src"].Value;
                match = match.NextMatch();
            }

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
            HtmlDocument docUrl = new HtmlDocument().LoadUrl(urlVisionado);
            HtmlNode nodoName = docUrl.GetByTagName("h1").FirstOrDefault();


            capitulo.Name = nodoName.InnerText;
            capitulo.Picture = capitulo.Parent.Picture;

            return capitulo;
        }
    }
}
