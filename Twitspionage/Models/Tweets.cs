using System.Collections.Generic;

namespace Images.Models
{
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