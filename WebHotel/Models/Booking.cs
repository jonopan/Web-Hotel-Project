using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebHotel.Models
{
    public class Booking
    {
        // primary key
        public int ID { get; set; }

        // foreign key
        public int RoomID { get; set; }

        //[Required]
        [DataType(DataType.EmailAddress)]
        [EmailAddress]
        // foreign key
        public string CustomerEmail { get; set; }

        [Display(Name = "CheckIn")]
        [Required(ErrorMessage = "Please enter the check in date")]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        [DataType(DataType.Date)]
        public DateTime CheckIn { get; set; }

        [Display(Name = "CheckOut")]
        [Required(ErrorMessage = "Please enter the check out date")]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        [DataType(DataType.Date)]
        public DateTime CheckOut { get; set; }

        public decimal Cost { get; set; }

        // Navigation properties
        public Room TheRoom { get; set; }
        public Customer TheCustomer { get; set; }

        public ICollection<CalStats> TheCalStats { get; set; }





    }
}
