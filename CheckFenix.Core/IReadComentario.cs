using HtmlAgilityPack;
using System;

namespace CheckFenix.Core
{
    public interface IReadComentario
    {
        void Load(Uri page);
        void Call(string js);
        HtmlDocument GetDocument();
    }
}