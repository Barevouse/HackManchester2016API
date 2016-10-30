using System.Collections.Generic;
using System.Device.Location;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Script.Serialization;
using Images.Models;
using TweetSharp;

namespace Images.Controllers
{
    public class TwitterController : ApiController
    {
        [HttpGet]
        public IHttpActionResult GetFeedPath(string token, string tokenSecret, double latitude, double longitude)
        {
            var service = new TwitterService("lsoMiOYqptZ6MdxxTiM1sIsc7",
                "7x15u25SsTNKhXDG5hRrChV2P3zl3RzC0SxJPs6BMiBKzG1nzi");

            service.AuthenticateWith(token, tokenSecret);

            var response = service.ListTweetsOnUserTimeline(new ListTweetsOnUserTimelineOptions
            {
                ScreenName = "Barevouse",
                Count = 10
            }) ?? new List<TwitterStatus>();
            
            var statuses = new List<Tweets>();

            foreach (var twitterStatus in response)
            {
                if (twitterStatus.Entities == null) continue;
                var urls = twitterStatus.Entities.Media.Select(rawLinks => rawLinks.MediaUrl).ToList();
                var encrypted = false;
                foreach (var media in twitterStatus.Entities.Media)
                {
                    if (media.MediaType != TwitterMediaType.Photo) continue;

                    using (var client = new WebClient())
                    {
                        var image = client.DownloadData(media.MediaUrl);
                        var ms = new MemoryStream(image);
                        var bmp = new Bitmap(Image.FromStream(ms));

                        var extracted = Steganography.Extract(bmp);
                        var decryptedMessage = Encryption.Decrypt(extracted);
                        if (string.IsNullOrEmpty(decryptedMessage)) continue;

                        var embedded = new JavaScriptSerializer().Deserialize<EmbeddedDetails>(decryptedMessage);

                        var currentLocation = new GeoCoordinate
                        {
                            Latitude = latitude,
                            Longitude = longitude
                        };
                        var requiredLocation = new GeoCoordinate
                        {
                            Latitude = embedded.Latitude,
                            Longitude = embedded.Longitude
                        };

                        if (!GeoLocation.WithinRadius(currentLocation, requiredLocation)) continue;

                        encrypted = true;
                        statuses.Add(new Tweets
                        {
                            Text = twitterStatus.Text,
                            ScreenName = twitterStatus.User.ScreenName,
                            Name = twitterStatus.User.Name,
                            MediaUrls = urls,
                            ProfileImageUrl = twitterStatus.User.ProfileImageUrl,
                            decryptedMessage = embedded
                        });
                    }
                }
                if (!encrypted)
                    statuses.Add(new Tweets
                    {
                        Text = twitterStatus.Text,
                        ScreenName = twitterStatus.User.ScreenName,
                        Name = twitterStatus.User.Name,
                        MediaUrls = urls,
                        ProfileImageUrl = twitterStatus.User.ProfileImageUrl
                    });
            }

            return Ok(statuses);
        }
    }
}