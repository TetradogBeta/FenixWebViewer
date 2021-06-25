using System;
using System.Net;
using Gabriel.Cat.S.Extension;

namespace CheckFenix.Core
{
    public static class UriExtension
    {
        public static string DownloadString(this Uri url)
        {
            return new WebClient().DownloadString(url);
        }
        public static bool? IsOk(this Uri url)
        {
            string html;
            bool isOk = default;

            switch (url.Host)
            {
                //case "mega.nz"://la string está cuando acaba de cargar al JS
                //    isOk = !html.Contains("file-removed-heading");
                //    break;

                case "femax20.com":
                    html = url.DownloadString();
                    isOk = !html.Contains("Sorry this video is unavailable");
                    break;
                //case "ok.ru"://se necesita cargar el js
                //    isOk = !html.Contains("The video is blocked");
                //    break;
                case "www.yourupload.com":
                    isOk = url.GetStatusCode() == HttpStatusCode.OK;
                    break;

            }



            return isOk;
        }
    }
}
