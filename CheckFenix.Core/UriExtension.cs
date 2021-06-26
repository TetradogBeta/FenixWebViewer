using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Gabriel.Cat.S.Extension;

namespace CheckFenix.Core
{
    public static class UriExtension
    {
        static Semaphore smDownloading = new Semaphore(1, 1);
        public static string DownloadString(this Uri url)
        {
            string html;
            WebClient wbClient;
            try
            {
                smDownloading.WaitOne();
                wbClient = new WebClient();
                wbClient.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
                html =wbClient.DownloadString(url);
            }
            catch
            {
                throw;
            }
            finally
            {
                smDownloading.Release();
            }
            return html;
        }
        public static bool? IsOk(this Uri url)
        {
            string html;
            bool? isOk = default;

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
