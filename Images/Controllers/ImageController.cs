using System;
using System.Device.Location;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Web.Http;
using System.Web.Script.Serialization;
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
            if (details.Latitude == null || details.Longitude == null)
            {
                return BadRequest("Needs a Latitude and Longitude");    
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
            var embedded = new EmbeddedDetails
            {
                Message = details.Message,
                Latitude = details.Latitude.Value,
                Longitude = details.Longitude.Value
            };
            var json = new JavaScriptSerializer().Serialize(embedded);
            var message = Encryption.Encrypt(json);
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
            if (details.Latitude == null || details.Longitude == null)
            {
                return BadRequest("Needs a Latitude and Longitude");
            }
            var bmp = GetBitmapFromBase64(details.Image);
            var extracted = Steganography.Extract(bmp);
            var decrypted = Encryption.Decrypt(extracted);
            var embedded = new JavaScriptSerializer().Deserialize<EmbeddedDetails>(decrypted);
            var imageLocation = new GeoCoordinate(embedded.Latitude, embedded.Longitude);
            var userLocation = new GeoCoordinate(details.Latitude.Value, details.Longitude.Value);
            return Ok(GeoLocation.WithinRadius(imageLocation, userLocation) ? embedded : new EmbeddedDetails());
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