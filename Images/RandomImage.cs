using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

namespace Images
{
    public class RandomImage
    {
        public static Bitmap GetImage()
        {
            using (WebClient client = new WebClient())
            {
                var image = client.DownloadData("https://unsplash.it/400/300/?random");
                var ms = new MemoryStream(image);
                var bmp = new Bitmap(Image.FromStream(ms));
                return bmp;
            }
        }
    }
}