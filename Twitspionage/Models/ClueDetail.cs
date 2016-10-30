using Hammock.Attributes.Validation;

namespace Twitspionage.Models
{
    public class ClueDetail
    {
        [Required]
        public string Clue { get; set; }
        [Required]
        public double? Latitude { get; set; }
        [Required]
        public double? Longitude { get; set; }
    }
}