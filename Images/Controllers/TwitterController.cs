using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Controllers;
using Images.Models;
using TweetSharp;

namespace Images.Controllers
{
    public class TwitterController : ApiController
    {
        [HttpPost]
        public IHttpActionResult SendTweet(TweetImage tweet)
        {
            var twitter = new TwitterService("lsoMiOYqptZ6MdxxTiM1sIsc7", "7x15u25SsTNKhXDG5hRrChV2P3zl3RzC0SxJPs6BMiBKzG1nzi");

            var bytes = Convert.FromBase64String(tweet.ImageContent);
            var ms = new MemoryStream(bytes, 0, bytes.Length);

            var dictionary = new Dictionary<string, Stream>();
            dictionary.Add("test", ms);

            twitter.AuthenticateWith(tweet.Token, tweet.TokenSecret);


            twitter.SendTweetWithMedia(new SendTweetWithMediaOptions
            {
                Status = tweet.Status,
                Images = dictionary
            });

            return Ok();
        }

        [HttpGet]
        public IHttpActionResult GetFeed(string token, string tokenSecret)
        {
            TwitterService service = new TwitterService("lsoMiOYqptZ6MdxxTiM1sIsc7", "7x15u25SsTNKhXDG5hRrChV2P3zl3RzC0SxJPs6BMiBKzG1nzi");

            service.AuthenticateWith(token, tokenSecret);

            var response = service.ListTweetsOnHomeTimeline(new ListTweetsOnHomeTimelineOptions());

            return Ok(response);
        }

        [HttpGet]
        public IHttpActionResult GetFeedPath(string token, string tokenSecret, double longitude, double latitude)
        {
            TwitterService service = new TwitterService("lsoMiOYqptZ6MdxxTiM1sIsc7", "7x15u25SsTNKhXDG5hRrChV2P3zl3RzC0SxJPs6BMiBKzG1nzi");

            service.AuthenticateWith(token, tokenSecret);

            var response = service.ListTweetsOnUserTimeline(new ListTweetsOnUserTimelineOptions
            {
                ScreenName = "Twitspionage"
            });

            var statuses = new List<Tweets>();

            foreach (var twitterStatus in response)
            {
                var urls = twitterStatus.Entities.Media.Select(rawLinks => rawLinks.MediaUrl).ToList();
                var encrypted = false;
                foreach (var media in twitterStatus.Entities.Media)
                {
                    if (media.MediaType == TwitterMediaType.Photo)
                    {
                        using (WebClient client = new WebClient())
                        {
                            var image = client.DownloadData(media.MediaUrl);
                            var ms = new MemoryStream(image);
                            var bmp = new Bitmap(Image.FromStream(ms));

                            var decryptedMessage = Steganography.Extract(bmp);
                            encrypted = true;
                            statuses.Add(new Tweets
                            {
                                Text = twitterStatus.Text,
                                ScreenName = twitterStatus.User.ScreenName,
                                Name = twitterStatus.User.Name,
                                MediaUrls = urls,
                                ProfileImageUrl = twitterStatus.User.ProfileImageUrl,
                                decryptedMessage = decryptedMessage
                            });

                        }
                    }
                }
                if(!encrypted)
                statuses.Add(new Tweets
                {
                    Text = twitterStatus.Text,
                    ScreenName = twitterStatus.User.ScreenName,
                    Name = twitterStatus.User.Name,
                    MediaUrls = urls,
                    ProfileImageUrl = twitterStatus.User.ProfileImageUrl
                });

               }

            return Ok(response);
        }
    }

    public class Tweets
    {
        public string ScreenName { get; set; }
        public string Name { get; set; }
        public string Text { get; set; }
        public string ProfileImageUrl { get; set; }
        public List<string> MediaUrls { get; set; }
        public string decryptedMessage { get; set; }
    }
}
