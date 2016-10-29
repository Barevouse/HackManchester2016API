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
            var message = Encryption.Encrypt(details.Message);
            var img = Steganography.Embed(message, bmp);
            var result = GetBase64FromBitmap(img);
            return Ok(new CreatedImage { Image = result });
        }

        [HttpPost, Route("api/image/retrieve")]
        public IHttpActionResult Retrieve(ImageDetails details)
        {
            if (string.IsNullOrEmpty(details.Image))
            {
                return BadRequest("Needs an Image");
            }
            var bmp = GetBitmapFromBase64(details.Image);
            var extracted = Steganography.Extract(bmp);
            var decrypted = Encryption.Decrypt(extracted);
            return Ok(new RetrievedMessage { Message = decrypted });
        }

        private static Bitmap GetBitmapFromBase64(string image)
        {
            var bytes = Convert.FromBase64String(image);
            var ms = new MemoryStream(bytes);
            var bmp = new Bitmap(Image.FromStream(ms));
            return bmp;
        }

        private static string GetBase64FromBitmap(Image img)
        {
            var ms = new MemoryStream();
            img.Save(ms, ImageFormat.Png);
            var result = Convert.ToBase64String(ms.ToArray());
            return result;
        }
    }
}