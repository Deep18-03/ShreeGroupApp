using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ShreeGroup.Models
{
    public class RazorPayOptionsModel
    {
        public string Key { get; set; }
        public decimal AmountInSubUnits { get; set; }
        public string Currency { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageLogUrl { get; set; }
        public string OrderId { get; set; }
        public string ProfileName { get; set; }
        public string ProfileContact { get; set; }
        public string ProfileEmail { get; set; }
        public Dictionary<string, string> Notes { get; set; }

        public decimal Amount { get; set; }

        public int Id { get; set; }

       
        public string FirstName { get; set; }

        public string MiddleName { get; set; }

       
        public string LastName { get; set; }

        public string Address { get; set; }

        public string City { get; set; }

        
        public string District { get; set; }

      
        public string PinCode { get; set; }

       
        public string PhoneNumber { get; set; }

       
        public string Email { get; set; }

      
        public DateTime DateOfBirth { get; set; }

        public int? Age { get; set; }

        public string ProfilePicture { get; set; }

        public string AadharCard { get; set; }

        public HttpPostedFileBase ImageFile { get; set; }

        public string QRCode { get; set; }

        public string RegistrationCode { get; set; }
    }
}