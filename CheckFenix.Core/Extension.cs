using System;
using System.Collections.Generic;
using System.Drawing;
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
    }

}
