﻿using Gabriel.Cat.S.Extension;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace CheckFenix.Core
{
    public class Comentario
    {
        static SortedList<string, Bitmap> DicPic = new SortedList<string, Bitmap>();
        public Comentario() { }
        public Comentario(HtmlNode nodo)
        {
            Picture =new Uri( nodo.GetByClass("avatar").First().GetByTagName("img").First().Attributes["src"].Value);
            Name = nodo.GetByClass("author").First().GetByTagName("a").First().InnerText;
            Mensaje = nodo.GetByClass("post-message").First().GetByTagName("p").First().InnerText;
        }
        public Uri Picture { get; set; }
        public Bitmap Image
        {
            get {

                string name = Path.GetFileName(Picture.AbsoluteUri);
                if (!DicPic.ContainsKey(name))
                {
                    DicPic.Add(name, Picture.GetBitmap());
                }
                return DicPic[name];
            
            }
        }
        public string Name { get; set; }
        public string Mensaje { get; set; }

        public static IEnumerable<Comentario> GetComentarios(IReadComentario reader,Uri page)
        {
            // post-list
            HtmlNode posts;
            reader.Load(page);
            reader.Call("$('#showComments').click()");
            posts = reader.GetDocument().GetElementbyId("post-list");//no existe hasta que se haga click en  $("#showComments")
            return Equals(posts,default(HtmlNode))?new Comentario[0]: posts.ChildNodes.Select(p => new Comentario(p));
        }
    }
}