using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebHotel.Models
{
    public class Room
    {
        [Key]
        public int ID { get; set; }

        [Display(Name = "Level")]
        [Required]
        [RegularExpression(@"^[G|1-3]{1}$", ErrorMessage = "Please enter the level G or 1 or 2 or 3!")]
        public string Level { get; set; }

        [Display(Name = "BedCount")]
        [Required]
        [RegularExpression(@"^[1-3]{1}$", ErrorMessage = "Please enter the number of bed in the room can only be 1 or 2 or 3!")]
        public int BedCount { get; set; }

        [Display(Name = "Price")]
        [Range(50, 300)]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        // Navigation properties
        public ICollection<Booking> TheBookings { get; set; }
    }
}
