using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantReviewApp.Models
{
    public class Restaurant
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [NotMapped]
        public double DisplayRating { get; set; }


        [StringLength(255)]
        public string Description { get; set; }

        [Required]
        public string Location { get; set; }

        [Range(1, 5)]
        public double AverageRating { get; set; }

        public List<Review> Reviews { get; set; } = new List<Review>();

        [Required]
        public string Category { get; set; } = string.Empty;

    }
}
