using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Linq;
using System.Net;
using HtmlAgilityPack;

namespace CheckFenix.Core
{
    public static class UriExtension
    {
        public static string DownloadString(this Uri url)
        {
            return HtmlDocumentExtension.DownloadString(url.AbsoluteUri);
        }
    }
        public static class HtmlDocumentExtension
    {
            public static string DownloadString(string url)
        {
            return new WebClient().DownloadString(url);
        }
        public static HtmlDocument LoadUrl(this HtmlDocument document, string url)
        {
            return document.LoadString(DownloadString(url));
        }
        public static HtmlDocument LoadString(this HtmlDocument document,string htmlDoc)
        {

            document.LoadHtml(htmlDoc);
            return document;
        }
        public static IEnumerable<HtmlNode> GetByClass(this HtmlDocument document,string clase)
        {
            
            for(int i = 0; i < document.DocumentNode.ChildNodes.Count; i++)
            {
                for (int j = 0; j < document.DocumentNode.ChildNodes[i].ChildNodes.Count; j++)
                {
                    foreach (HtmlNode node in document.DocumentNode.ChildNodes[i].ChildNodes[j].GetByClass(clase))
                    {
                        yield return node;
                    }
                }

            }
        }
        public static IEnumerable<HtmlNode> GetByClass(this HtmlNode parentNode, string clase)
        {

            if (parentNode.GetClasses().Any(c=>c.Equals(clase)))
                yield return parentNode;
            for (int i = 0; i < parentNode.ChildNodes.Count; i++)
            {
                foreach (HtmlNode node in parentNode.ChildNodes[i].GetByClass(clase))
                {
                    yield return node;
                }

            }
        }

        public static IEnumerable<HtmlNode> GetByTagName(this HtmlDocument document, string tagName)
        {

            for (int i = 0; i < document.DocumentNode.ChildNodes.Count; i++)
            {
                for (int j = 0; j < document.DocumentNode.ChildNodes[i].ChildNodes.Count; j++)
                {
                    foreach (HtmlNode node in document.DocumentNode.ChildNodes[i].ChildNodes[j].GetByTagName(tagName))
                    {
                        yield return node;
                    }
                }

            }
        }
        public static IEnumerable<HtmlNode> GetByTagName(this HtmlNode parentNode, string tagName)
        {

            if (parentNode.Name.Equals(tagName))
                yield return parentNode;
            for (int i = 0; i < parentNode.ChildNodes.Count; i++)
            {
                foreach (HtmlNode node in parentNode.ChildNodes[i].GetByClass(tagName))
                {
                    yield return node;
                }

            }
        }
    }
}
