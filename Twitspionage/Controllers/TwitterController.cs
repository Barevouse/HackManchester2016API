using System.Collections.Generic;
using System.Configuration;
using System.Device.Location;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Script.Serialization;
using TweetSharp;
using Twitspionage.Models;

namespace Twitspionage.Controllers
{
    public class TwitterController : ApiController
    {
        [HttpGet]
        public IHttpActionResult GetFollowing(string token, string tokenSecret)
        {

            var settingsReader = new AppSettingsReader();
            var consumerKey = settingsReader.GetValue("TwitterKey", typeof(string)).ToString();
            var consumerSecret = settingsReader.GetValue("TwitterSecret", typeof(string)).ToString();
            var service = new TwitterService(consumerKey, consumerSecret);

            service.AuthenticateWith(token, tokenSecret);

            var following = service.ListFriends(new ListFriendsOptions());

            if (following == null) return Ok(new List<FollowingUser>());

            var users = following.Select(user => new FollowingUser
            {
                ScreenName = user.ScreenName, Name = user.Name, ProfileImageUrl = user.ProfileImageUrl
            }).ToList();

            return Ok(users);
        }

        [HttpGet]
        public IHttpActionResult GetFeedPath(string token, string tokenSecret, double latitude, double longitude,
            string screenname)
        {
            var settingsReader = new AppSettingsReader();
            var consumerKey = settingsReader.GetValue("TwitterKey", typeof(string)).ToString();
            var consumerSecret = settingsReader.GetValue("TwitterSecret", typeof(string)).ToString();
            var service = new TwitterService(consumerKey, consumerSecret);

            service.AuthenticateWith(token, tokenSecret);

            var response = service.ListTweetsOnUserTimeline(new ListTweetsOnUserTimelineOptions
            {
                ScreenName = screenname,
                Count = 5
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

                        embedded.FinalMystery = null;

                        encrypted = true;
                        statuses.Add(new Tweets
                        {
                            Text = twitterStatus.Text,
                            ScreenName = twitterStatus.User.ScreenName,
                            Name = twitterStatus.User.Name,
                            MediaUrls = urls,
                            ProfileImageUrl = twitterStatus.User.ProfileImageUrl,
                            DecryptedMessage = embedded
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

        [HttpPost]
        public IHttpActionResult GuessAnswer(GuessAnswer guessAnswer)
        {

            var settingsReader = new AppSettingsReader();
            var consumerKey = settingsReader.GetValue("TwitterKey", typeof(string)).ToString();
            var consumerSecret = settingsReader.GetValue("TwitterSecret", typeof(string)).ToString();
            var service = new TwitterService(consumerKey, consumerSecret);

            service.AuthenticateWith(guessAnswer.Token, guessAnswer.TokenSecret);

            var response = service.ListTweetsOnUserTimeline(new ListTweetsOnUserTimelineOptions
            {
                ScreenName = guessAnswer.Screenname
            }) ?? new List<TwitterStatus>();

            var statuses = new List<Tweets>();

            var missionName = string.Empty;

            var result = false;
            foreach (var twitterStatus in response)
            {
                if (twitterStatus.Entities == null) continue;
                var urls = twitterStatus.Entities.Media.Select(rawLinks => rawLinks.MediaUrl).ToList();
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

                        if (embedded.FinalMystery != null)
                        {
                            if(embedded.FinalMystery.ToLower().Equals(guessAnswer.Guess.ToLower()))
                            {
                                result = true;
                                missionName = embedded.Mystery;
                            }
                        } 
                    }
                }
            }
            if (result)
            {
                service.AuthenticateWith(guessAnswer.Token, guessAnswer.TokenSecret);

                service.SendTweet(new SendTweetOptions
                {
                    Status = $"@{guessAnswer.Screenname} mission '{missionName}' accomplished!",

                });

                return Ok(new GuessResult
                {
                    message = "Congratulations! You win!"
                });
            }

            return Ok(new GuessResult {message = "Incorrect, try again."});
        }
    }

    public class GuessAnswer
    {
        public string Token { get; set; }
        public string TokenSecret { get; set; }
        public string Screenname { get; set; }
        public string Guess { get; set; }
    }
}