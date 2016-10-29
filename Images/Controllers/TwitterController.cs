using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Controllers;
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
    }

    public class TweetImage
    {
        public string ImageContent { get; set; }
        public string Status { get; set; }
        public string Token { get; set; }
        public string TokenSecret { get; set; }
    }
}
