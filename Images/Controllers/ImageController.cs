using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Web.Http;
using Images.Models;

namespace Images.Controllers
{
    public class ImageController : ApiController
    {
        [HttpPost, Route("api/image/create")]
        public IHttpActionResult Create(MessageDetails details)
        {
            if (string.IsNullOrEmpty(details.Message))
            {
                return BadRequest("Needs a Message");    
            }
            if (string.IsNullOrEmpty(details.Target))
            {
                return BadRequest("Needs a Target");    
            }
            Bitmap bmp;
            if (!string.IsNullOrEmpty(details.Image))
            {
                bmp = GetBitmapFromBase64(details.Image);
            }
            else
            {
                var imagePath = System.Web.Hosting.HostingEnvironment.MapPath(@"~/App_Data/espionage.jpg");
                bmp = new Bitmap(Image.FromFile(imagePath));
            }
            var key = details.Target.ToLower();
            var message = Encryption.Encrypt(key + ":" + details.Message, key);
            var img = Steganography.Embed(message, bmp);
            var ms = new MemoryStream();
            img.Save(ms, ImageFormat.Png);
            var result = Convert.ToBase64String(ms.ToArray());
            return Ok(new CreatedImage { Image = result });
        }

        [HttpPost, Route("api/image/retrieve")]
        public IHttpActionResult Retrieve(ImageDetails details)
        {
            if (string.IsNullOrEmpty(details.Image))
            {
                return BadRequest("Needs an Image");
            }
            if (string.IsNullOrEmpty(details.Username))
            {
                return BadRequest("Needs a Username");
            }
            var bmp = GetBitmapFromBase64(details.Image);
            var extracted = Steganography.Extract(bmp);
            var key = details.Username.ToLower();
            var decrypted = Encryption.Decrypt(extracted, key);
            var message = string.Empty;
            var messageStart = key + ":";
            if (decrypted.StartsWith(messageStart))
            {
                message = decrypted.Replace(messageStart, string.Empty);
            }
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