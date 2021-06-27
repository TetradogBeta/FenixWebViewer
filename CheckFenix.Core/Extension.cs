using CheckFenix.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Gabriel.Cat.S.Extension
{
    public static class UriExtension
    {
        public static async Task<Bitmap> GetBitmapAsnyc(this Uri url)
        {
            System.Net.WebRequest request =System.Net.WebRequest.Create( url.AbsoluteUri);
            System.Net.WebResponse response =await request.GetResponseAsync();
            System.IO.Stream responseStream = response.GetResponseStream();
           return new Bitmap(responseStream);
        }
        public static HttpStatusCode GetStatusCode(this Uri url)
        {
            HttpStatusCode response;
            HttpWebResponse httpRes;
            HttpWebRequest httpReq;
            try
            {
                httpReq = (HttpWebRequest)WebRequest.Create(url.AbsoluteUri);

                httpReq.AllowAutoRedirect = false;
                httpRes = (HttpWebResponse)httpReq.GetResponse();

                response = httpRes.StatusCode;
                // Close the response.
                httpRes.Close();
            }
            catch
            {
                response = HttpStatusCode.NotFound;
            }

            return response;
        }

    }

}
