using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Twitspionage.Models
{
    public class MysteryDetail
    {
        public IEnumerable<ClueDetail> Clues { get; set; }
        [Display(Name = "Mission Name")]
        public string Name { get; set; }

        public MysteryDetail()
        {
            Clues = new List<ClueDetail> { new ClueDetail() };
        }
    }

    public class ClueDetail
    {
        [Display(Name = "Mission Name")]
        public string Mystery { get; set; }
        [Display(Name = "Hint about next location")]
        public string Clue { get; set; }
        [Display(Name = "Part of message for this clue")]
        public string Message { get; set; }
        [Display(Name = "Latitude")]
        public double? Latitude { get; set; }
        [Display(Name = "Longitude")]
        public double? Longitude { get; set; }
    }
}