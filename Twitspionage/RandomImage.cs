using System.Drawing;
using System.IO;
using System.Net;

namespace Twitspionage
{
    public class RandomImage
    {
        public static Bitmap GetImage()
        {
            using (WebClient client = new WebClient())
            {
                var image = client.DownloadData("https://unsplash.it/506/506/?random");
                var ms = new MemoryStream(image);
                var bmp = new Bitmap(Image.FromStream(ms));
                return bmp;
            }
        }
    }
}