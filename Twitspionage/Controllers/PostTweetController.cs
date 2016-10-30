using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Hammock.Authentication.OAuth;
using Images.Models;
using TweetSharp;

namespace Twitspionage.Controllers
{
    public class PostTweetController : Controller
    {
        private const string ConsumerKey = "lsoMiOYqptZ6MdxxTiM1sIsc7";
        private const string ConsumerSecret = "7x15u25SsTNKhXDG5hRrChV2P3zl3RzC0SxJPs6BMiBKzG1nzi";

        [HttpGet]
        public ActionResult Index(string oauth_token, string oauth_verifier)
        {
            var service = new TwitterService(ConsumerKey, ConsumerSecret);

            if (string.IsNullOrEmpty(oauth_token) || string.IsNullOrEmpty(oauth_verifier))
            {
                var requestToken = service.GetRequestToken(Request.Url.AbsoluteUri);

                var uri = service.GetAuthenticationUrl(requestToken);

                return Redirect(uri.ToString());
            }

            var token = TempData["token"];
            var tokenSecret = TempData["tokenSecret"];

            OAuthAccessToken accessToken;
            if (token == null || token.ToString().Equals("?")|| tokenSecret == null || token.ToString().Equals("?"))
            {
                accessToken = service.GetAccessToken(new OAuthRequestToken {Token = oauth_token}, oauth_verifier);
            }
            else
            {
                accessToken = new OAuthAccessToken { Token = token.ToString(), TokenSecret = tokenSecret.ToString() };
            }

            service.AuthenticateWith(accessToken.Token, accessToken.TokenSecret);

            TempData["token"] = accessToken.Token;
            TempData["tokenSecret"] = accessToken.TokenSecret;

            var response = service.ListTweetsOnUserTimeline(new ListTweetsOnUserTimelineOptions
            {
                ScreenName = "Barevouse",
                Count = 10
            }) ?? new List<TwitterStatus>();

            var profile = service.VerifyCredentials(new VerifyCredentialsOptions());
            
            if(profile == null) return RedirectToAction("Index", "PostTweet", new {});

            ViewData["Username"] = profile.ScreenName;

            return View();
        }

        [HttpPost]
        public ActionResult Index(MessageDetails messageDetails)
        {

            var service = new TwitterService(ConsumerKey, ConsumerSecret);

            var image = EncryptionService.GetImage(messageDetails);
            var ms = new MemoryStream();
            image.Save(ms, ImageFormat.Png);
            ms.Seek(0, SeekOrigin.Begin);

            var token = TempData["token"];
            var tokenSecret = TempData["tokenSecret"];

            service.AuthenticateWith(token.ToString(), tokenSecret.ToString());

            service.SendTweetWithMedia(new SendTweetWithMediaOptions
            {
                Status = "Twitspionage Clue",
                Images = new Dictionary<string, Stream> { { "Clue", ms } }
            });

            return View("Success");
        }
    }
}