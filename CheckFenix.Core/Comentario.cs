﻿using Gabriel.Cat.S.Extension;
using Gabriel.Cat.S.Utilitats;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CheckFenix.Core
{
    public class Comentario
    {
        static LlistaOrdenada<string, Bitmap> DicPic = new LlistaOrdenada<string, Bitmap>();
        public Comentario() { }
        public Comentario(HtmlNode nodo)
        {
            Picture =new Uri( nodo.GetByClass("avatar").First().GetByTagName("img").First().Attributes["src"].Value);
            Name = nodo.GetByClass("author").First().GetByTagName("a").First().InnerText;
            Mensaje = nodo.GetByClass("post-message").First().GetByTagName("p").First().InnerText;
        }
        public Uri Picture { get; set; }

        public string Name { get; set; }
        public string Mensaje { get; set; }

        public Bitmap GetImage()
        {

            Task<Bitmap> tBmp;
            string name = Path.GetFileName(Picture.AbsoluteUri);
            if (!DicPic.ContainsKey(name))
            {
                tBmp = Picture.GetBitmap();
                tBmp.Wait();
                DicPic.Add(name, tBmp.Result);
            }
            return DicPic[name];


        }
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