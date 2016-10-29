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
            var imagePath = System.Web.Hosting.HostingEnvironment.MapPath(@"~/App_Data/espionage.jpg");
            var bmp = new Bitmap(Image.FromFile(imagePath));
            var img = Steganography.Embed(details.Username + ":" + details.Message, bmp);
            var ms = new MemoryStream();
            img.Save(ms, ImageFormat.Png);
            var result = Convert.ToBase64String(ms.ToArray());
            return Ok(new CreatedImage { Image = result });
        }

        [HttpPost, Route("api/image/retrieve")]
        public IHttpActionResult Retrieve(CreatedImage details)
        {
            var bytes = Convert.FromBase64String(details.Image);
            var ms = new MemoryStream(bytes);
            var bmp = new Bitmap(Image.FromStream(ms));
            var message = Steganography.Extract(bmp);
            return Ok(new RetrievedMessage { Message = message });
        }
    }
}