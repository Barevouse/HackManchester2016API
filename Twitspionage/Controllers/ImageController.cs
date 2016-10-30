using System;
using System.Device.Location;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Web.Http;
using System.Web.Script.Serialization;
using Twitspionage.Models;

namespace Twitspionage.Controllers
{
    public class ImageController : ApiController
    {
        [HttpPost, Route("api/image/create")]
        public IHttpActionResult Create(ClueDetail detail)
        {
            if (string.IsNullOrEmpty(detail.Clue))
            {
                return BadRequest("Needs a Clue");    
            }
            if (detail.Latitude == null || detail.Longitude == null)
            {
                return BadRequest("Needs a Latitude and Longitude");    
            }
            var bmp = RandomImage.GetImage();

            var embedded = new EmbeddedDetails
            {
                Message = detail.Clue,
                Latitude = detail.Latitude.Value,
                Longitude = detail.Longitude.Value
            };
            var json = new JavaScriptSerializer().Serialize(embedded);
            var message = Encryption.Encrypt(json);
            var img = Steganography.Embed(message, bmp);
            var result = GetBase64FromBitmap(img);
            return Ok(new CreatedImage { Image = result });
        }

        [HttpPost, Route("api/image/retrieve")]
        public IHttpActionResult Retrieve(ImageDetail detail)
        {
            if (string.IsNullOrEmpty(detail.Image))
            {
                return BadRequest("Needs an Image");
            }
            if (detail.Latitude == null || detail.Longitude == null)
            {
                return BadRequest("Needs a Latitude and Longitude");
            }
            var bmp = GetBitmapFromBase64(detail.Image);
            var extracted = Steganography.Extract(bmp);
            var decrypted = Encryption.Decrypt(extracted);
            var embedded = new JavaScriptSerializer().Deserialize<EmbeddedDetails>(decrypted);
            if (embedded == null) return Ok(new EmbeddedDetails());
            var imageLocation = new GeoCoordinate(embedded.Latitude, embedded.Longitude);
            var userLocation = new GeoCoordinate(detail.Latitude.Value, detail.Longitude.Value);
            var withinRadius = GeoLocation.WithinRadius(imageLocation, userLocation);
            var content = withinRadius ? embedded : new EmbeddedDetails();
            return Ok(content);
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