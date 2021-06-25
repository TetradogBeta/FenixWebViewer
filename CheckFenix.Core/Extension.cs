using CheckFenix.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using System.Text;

namespace Gabriel.Cat.S.Extension
{
    public static class UriExtension
    {
        public static Bitmap GetBitmap(this Uri url)
        {
            System.Net.WebRequest request =System.Net.WebRequest.Create( url.AbsoluteUri);
            System.Net.WebResponse response = request.GetResponse();
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
