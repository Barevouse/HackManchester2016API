using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Hammock.Authentication.OAuth;
using TweetSharp;

namespace Twitspionage.Controllers
{
    public class PostTweetController : Controller
    {
        private const string ConsumerKey = "lsoMiOYqptZ6MdxxTiM1sIsc7";
        private const string ConsumerSecret = "7x15u25SsTNKhXDG5hRrChV2P3zl3RzC0SxJPs6BMiBKzG1nzi";

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
            if (token == null || tokenSecret == null)
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
            
            if(profile == null) return RedirectToAction("Index");

            ViewData["Username"] = profile.ScreenName;

            return View();
        }
    }
}