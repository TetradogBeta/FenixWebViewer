using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Gabriel.Cat.S.Extension;

namespace CheckFenix.Core
{
 
    public static class UriExtension
    {
        public static bool? IsOk(this Uri url)
        {
            Task<string> tDoc;
            Task<HttpStatusCode> tStatus;
            string html;
            bool? isOk = default;

            switch (url.Host)
            {
                //case "mega.nz"://la string está cuando acaba de cargar al JS
                //    isOk = !html.Contains("file-removed-heading");
                //    break;

                case "femax20.com":
                    tDoc = url.DownloadString();
                    tDoc.Wait();
                    html = tDoc.Result;
                    isOk = !html.Contains("Sorry this video is unavailable");
                    break;
                //case "ok.ru"://se necesita cargar el js
                //    isOk = !html.Contains("The video is blocked");
                //    break;
                //case "www.yourupload.com":
                //    tStatus = url.GetStatusCode();
                //    tStatus.Wait();
                //    isOk = tStatus.Result == HttpStatusCode.OK;
                //    break;

            }



            return isOk;
        }
    }
}
