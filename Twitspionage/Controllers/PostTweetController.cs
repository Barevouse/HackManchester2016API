using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Hammock.Authentication.OAuth;
using TweetSharp;
using Twitspionage.Models;

namespace Twitspionage.Controllers
{
    public class PostTweetController : Controller
    {
        [HttpGet]
        public ActionResult Index(string oauth_token, string oauth_verifier)
        {
            var service = CreateTwitterService();

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

            var profile = service.VerifyCredentials(new VerifyCredentialsOptions());
            
            if(profile == null) return RedirectToAction("Index", "PostTweet", new {});

            ViewData["Username"] = profile.ScreenName;

            return View();
        }

        [HttpPost]
        public ActionResult Index(MessageDetails messageDetails)
        {

            var service = CreateTwitterService();

            var image = EncryptionService.GetImage(messageDetails);
            var ms = new MemoryStream();
            image.Save(ms, ImageFormat.Png);
            ms.Seek(0, SeekOrigin.Begin);

            var token = TempData["token"];
            var tokenSecret = TempData["tokenSecret"];

            service.AuthenticateWith(token.ToString(), tokenSecret.ToString());

            service.SendTweetWithMedia(new SendTweetWithMediaOptions
            {
                Status = "#twitspionage clue",
                Images = new Dictionary<string, Stream> { { "Twitspionage clue", ms } }
            });

            return service.Response.StatusCode == HttpStatusCode.OK && service.Response.Error != null
                ? View("Success")
                : View("Error");
        }

        private static TwitterService CreateTwitterService()
        {
            var settingsReader = new AppSettingsReader();
            var consumerKey = settingsReader.GetValue("TwitterKey", typeof(string)).ToString();
            var consumerSecret = settingsReader.GetValue("TwitterSecret", typeof(string)).ToString();
            return new TwitterService(consumerKey, consumerSecret);
        }
    }
}