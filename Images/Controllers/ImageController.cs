using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Results;
using Images.Models;

namespace Images.Controllers
{
    public class ImageController : ApiController
    {
        [HttpPost, Route("api/image/create")]
        public IHttpActionResult Create(MessageDetails details)
        {
            Bitmap bmp;
            if (details.Image != null)
            {
                bmp = GetBitmapFromBase64(details.Image);
            }
            else
            {
                var imagePath = System.Web.Hosting.HostingEnvironment.MapPath(@"~/App_Data/espionage.jpg");
                bmp = new Bitmap(Image.FromFile(imagePath));
            }
            var img = Steganography.Embed(details.Target + ":" + details.Message, bmp);
            var ms = new MemoryStream();
            img.Save(ms, ImageFormat.Png);
            var result = Convert.ToBase64String(ms.ToArray());
            return Ok(new CreatedImage { Image = result });
        }

        [HttpPost, Route("api/image/retrieve")]
        public IHttpActionResult Retrieve(CreatedImage details)
        {
            var bmp = GetBitmapFromBase64(details.Image);
            var message = Steganography.Extract(bmp);
            return Ok(new RetrievedMessage { Message = message });
        }

        private static Bitmap GetBitmapFromBase64(string image)
        {
            var bytes = Convert.FromBase64String(image);
            var ms = new MemoryStream(bytes);
            var bmp = new Bitmap(Image.FromStream(ms));
            return bmp;
        }
    }
}