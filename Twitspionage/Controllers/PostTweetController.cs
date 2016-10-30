using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
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

            ViewBag.Title = "Create Mission";
            return View(new MysteryDetail());
        }

        [HttpPost]
        public ActionResult Index(MysteryDetail mysteryDetail)
        {
            if(string.IsNullOrWhiteSpace(mysteryDetail.Name))
            {
                return Error(mysteryDetail, "Please ensure all details are filled in.");
            }
            var details = mysteryDetail.Clues as IList<ClueDetail> ?? mysteryDetail.Clues.ToList();
            if (details.Any(clueDetail => string.IsNullOrWhiteSpace(clueDetail.Clue) || clueDetail.Latitude == null || clueDetail.Longitude == null))
            {
                return Error(mysteryDetail, "Please ensure all details are filled in.");
            }

            var service = CreateTwitterService();

            var token = TempData["token"];
            var tokenSecret = TempData["tokenSecret"];

            service.AuthenticateWith(token.ToString(), tokenSecret.ToString());
            
            foreach (var clueDetail in details)
            {
                var bmp = RandomImage.GetImage();
                var colour = bmp.GetPixel(0, 0);
                bmp.MakeTransparent(colour);

                var embedded = new EmbeddedDetails
                {
                    Mystery = mysteryDetail.Name,
                    Clue = clueDetail.Clue,
                    Latitude = clueDetail.Latitude.Value,
                    Longitude = clueDetail.Longitude.Value,
                    Message = clueDetail.Message
                };
                var json = new JavaScriptSerializer().Serialize(embedded);
                var message = Encryption.Encrypt(json);
                var img = Steganography.Embed(message, bmp);
                var ms = new MemoryStream();
                img.Save(ms, ImageFormat.Png);
                ms.Seek(0, SeekOrigin.Begin);
                service.SendTweetWithMedia(new SendTweetWithMediaOptions
                {
                    Status = "#twitspionage clue",
                    Images = new Dictionary<string, Stream> { { "Twitspionage clue", ms } }
                });

                if (service.Response.StatusCode == HttpStatusCode.OK && service.Response.Error == null) continue;

                return Error(mysteryDetail, "Error tweeting clues. Please check your Twitter account and repost any that are missing.");
            }
            return View("Success");
        }

        private ActionResult Error(MysteryDetail mysteryDetail, string errorMessage)
        {
            ViewBag.Error = errorMessage;
            return View("Index", mysteryDetail);
        }

        public ViewResult NewClueDetail()
        {
            return View("_Tweet", new ClueDetail());
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