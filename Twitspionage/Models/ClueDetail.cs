using System.Collections.Generic;
using Hammock.Attributes.Validation;

namespace Twitspionage.Models
{
    public class MysteryDetail
    {
        public IEnumerable<ClueDetail> Clues { get; set; }
        public string Name { get; set; }

        public MysteryDetail()
        {
            Clues = new List<ClueDetail> { new ClueDetail() };
        }
    }
    public class ClueDetail
    {
        public string Clue { get; set; }
        public string Mystery { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}