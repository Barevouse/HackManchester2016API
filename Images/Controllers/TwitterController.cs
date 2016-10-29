using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Web.Http;
using TweetSharp;

namespace Images.Controllers
{
    public class TwitterController : ApiController
    {
        [HttpPost]
        public IHttpActionResult SendTweet(TweetThing tweet)
        {
            var twitter = new TwitterService();

            var bytes = Convert.FromBase64String(tweet.ImageContent);
            var ms = new MemoryStream(bytes, 0, bytes.Length);

            var dictionary = new Dictionary<string, Stream>();
            dictionary.Add("test", ms);

            var request = twitter.BeginSendTweetWithMedia(new SendTweetWithMediaOptions
            {
                Status = tweet.Status,
                Images = dictionary
            });

            twitter.EndSendTweetWithMedia(ExecuteAsync(this.ControllerContext, CancellationToken.None));

            return Ok();
        }
    }

    public class TweetThing
    {
        public string ImageContent { get; set; }
        public string Status { get; set; }
    }
}
