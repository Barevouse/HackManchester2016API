using System.Collections.Generic;

namespace Twitspionage.Models
{
    public class Tweets
    {
        public string ScreenName { get; set; }
        public string Name { get; set; }
        public string Text { get; set; }
        public string ProfileImageUrl { get; set; }
        public List<string> MediaUrls { get; set; }
        public EmbeddedDetails DecryptedMessage { get; set; }
    }
}