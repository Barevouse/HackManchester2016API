using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using TweetSharp;

namespace Images.Controllers
{
    public class TwitterController : ApiController
    {
        public IHttpActionResult SendTweet(TweetThing tweet)
        {
            var twitter = new TwitterService();

            var bytes = Convert.FromBase64String(tweet.ImageContent);
            var ms = new MemoryStream(bytes, 0, bytes.Length);

            var dictionary = new Dictionary<string, Stream>();
            dictionary.Add("test", ms);

            twitter.BeginSendTweetWithMedia(new SendTweetWithMediaOptions
            {
                Status = tweet.Status,
                Images = dictionary
            });

            return Ok();
        }
    }

    public class TweetThing
    {
        public string ImageContent { get; set; }
        public string Status { get; set; }
    }
}
