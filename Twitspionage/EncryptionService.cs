using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using Images;
using Images.Models;

namespace Twitspionage
{
    public static class EncryptionService
    {
        public static Bitmap GetImage(MessageDetails details)
        {
            var bmp = RandomImage.GetImage();
            var colour = bmp.GetPixel(0, 0);
            bmp.MakeTransparent(colour);

            var embedded = new EmbeddedDetails
            {
                Message = details.Message,
                Latitude = details.Latitude.Value,
                Longitude = details.Longitude.Value
            };
            var json = new JavaScriptSerializer().Serialize(embedded);
            var message = Encryption.Encrypt(json);
            var img = Steganography.Embed(message, bmp);

            return img;
        }
    }
}