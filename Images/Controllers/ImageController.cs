using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;

namespace Images.Controllers
{
    public class ImageController : ApiController
    {
        public class CreateImage
        {
            public string Username { get; set; }
            public string Message { get; set; }
        }

        [HttpPost]
        public HttpResponseMessage Create(CreateImage details)
        {
            var imagePath = System.Web.Hosting.HostingEnvironment.MapPath(@"~/App_Data/espionage.jpg");
            var bmp = new Bitmap(Image.FromFile(imagePath));

            using (Graphics graphics = Graphics.FromImage(bmp))
            {
                using (Font arial = new Font("Arial", 10))
                {
                    graphics.DrawString(details.Username, arial, Brushes.Red, new PointF(10, 10));
                    graphics.DrawString(details.Message, arial, Brushes.DarkRed, new PointF(10, 20));
                }
            }

            var img = Image.FromHbitmap(bmp.GetHbitmap());
            var ms = new MemoryStream();
            img.Save(ms, ImageFormat.Png);
            var result = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(ms.ToArray())
            };
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
            return result;
        }       
    }
}