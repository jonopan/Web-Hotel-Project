using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace WebHotel.Models
{
    public class Customer
    {
        [Key, Required]
        [DataType(DataType.EmailAddress)]
        [EmailAddress]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Email { get; set; }

        [Display(Name = "Surname")]
        [Required]
        [RegularExpression(@"^[a-zA-Z-']{2,20}$", ErrorMessage = "Please enter surname that consist English letter, hyphen, and apostrophe and has length between 2-20 characters inclusive!")]
        [StringLength(20, MinimumLength = 2)]
        [DataType(DataType.Text)]
        public string Surname { get; set; }

        [Display(Name = "Given Name")]
        [Required]
        [RegularExpression(@"^[a-zA-Z-']{2,20}$", ErrorMessage = "Please enter given name that consist English letter, hyphen, and apostrophe and has length between 2-20 characters inclusive!")]
        [StringLength(20, MinimumLength = 2)]
        [DataType(DataType.Text)]
        public string GivenName { get; set; }

        [Display(Name = "Post Code")]
        [Required]
        [RegularExpression(@"^[0-9]{4}$", ErrorMessage = "Please enter the 4 digit postcode!")]
        [DataType(DataType.PostalCode)]
        public string PostCode { get; set; }

        // Navigation properties
        public ICollection<Booking> TheBookings { get; set; }
        public ICollection<CalStats> TheCalStats { get; set; }

    }
}
