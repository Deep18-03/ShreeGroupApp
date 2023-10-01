using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShreeGroup.Models
{
    public class GeneralSeatBookingModel
    {
        public long Id { get; set; }
        [Display(Name = "Full Name")]
        [Required]
        public string FirstName { get; set; }
        [Display(Name = "Last Name")]
        //[Required]
        public string LastName { get; set; }
        [Required]
        public Nullable<int> Quantity { get; set; }
        public Nullable<int> TotalAmount { get; set; }
        public Nullable<bool> IsPaid { get; set; }
        public string BookingId { get; set; }
        public string TransactionId { get; set; }
        public Nullable<System.DateTime> InsertDate { get; set; }
        public Nullable<System.DateTime> UpdateDate { get; set; }
        [Display(Name = "Phone Number")]
        [Required]
        public string MobileNumber { get; set; }
    }
}
