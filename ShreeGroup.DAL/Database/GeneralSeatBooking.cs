//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ShreeGroup.DAL.Database
{
    using System;
    using System.Collections.Generic;
    
    public partial class GeneralSeatBooking
    {
        public long Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Nullable<int> Quantity { get; set; }
        public Nullable<int> TotalAmount { get; set; }
        public Nullable<bool> IsPaid { get; set; }
        public string BookingId { get; set; }
        public string TransactionId { get; set; }
        public Nullable<System.DateTime> InsertDate { get; set; }
        public Nullable<System.DateTime> UpdateDate { get; set; }
        public string MobileNumber { get; set; }
    }
}
