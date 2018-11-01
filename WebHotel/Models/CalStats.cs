using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebHotel.Models
{
    public class CalStats
    {
        public int ID { get; set; }

        [Display(Name = "Post Code")]
        public string PostCode { get; set; }

        [Display(Name = "Number of Customers")]
        public int NumberOfCustomers { get; set; }

        [Display(Name = "Room ID")]
        public string RoomID { get; set; }

        [Display(Name = "Number of Bookings")]
        public int NumberOfBookings { get; set; }
    }
}
