using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace WebHotel.Models.RoomsViewModels
{
    public class SearchRooms
    {
        public int BedCount { get; set; }

        [Display(Name = "Check In")]
        [Required(ErrorMessage = "Please enter the check in date")]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}")]
        [DataType(DataType.Date)]
        public DateTime CheckIn { get; set; }

        [Display(Name = "Check Out")]
        [Required(ErrorMessage = "Please enter the check out date")]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}")]
        [DataType(DataType.Date)]
        public DateTime CheckOut { get; set; }

    }
}
