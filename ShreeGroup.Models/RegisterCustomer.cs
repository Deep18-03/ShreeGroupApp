using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ShreeGroup.Models
{
    public class RegisterCustomer
    {

        public int Id { get; set; }

        [Required(ErrorMessage = "Please enter first name")]
        [DataType(DataType.Text)]
        [MaxLength(50, ErrorMessage = "Maximum text limit is 50")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        //[Required(ErrorMessage = "Please enter middle name")]
        [MaxLength(50, ErrorMessage = "Maximum text limit is 50")]
        [Display(Name = "Middle Name")]
        public string MiddleName { get; set; }

        [Required(ErrorMessage = "Please enter last name")]
        [MaxLength(50, ErrorMessage = "Maximum text limit is 50")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Please enter address")]
        [MaxLength(200, ErrorMessage = "Maximum text limit is 200")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Please enter city")]
        [MaxLength(20, ErrorMessage = "Maximum text limit is 20")]
        public string City { get; set; }

        //[Required(ErrorMessage = "Please enter district")]
        [MaxLength(20, ErrorMessage = "Maximum text limit is 20")]
        public string District { get; set; }

        [Required(ErrorMessage = "Please enter pincode")]
        [Display(Name = "PinCode")]
        [RegularExpression(@"^(\d{6})$", ErrorMessage = "Please enter correct pincode")]
        public string PinCode { get; set; }

        [Required(ErrorMessage = "Please enter phone number")]
        [Display(Name = "Phone Number")]
        [RegularExpression(@"^(\d{10})$", ErrorMessage = "Please enter valid mobile number")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Please enter email address")]
        [MaxLength(50, ErrorMessage = "Maximum text limit is 50")]
        [RegularExpression(@"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$", ErrorMessage = "Email address is not valid")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please select date of birth")]
        [Display(Name = "Date Of Birth")]
        [DataType(DataType.Date), DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime DateOfBirth { get; set; }

        //[Required(ErrorMessage = "Please enter your age")]
        [Range(2, 100, ErrorMessage = "Enter valid age")]
        public int? Age { get; set; }

        [Display(Name = "Upload Profile Picture (Passport size)")]
        public string ProfilePicture { get; set; }

        [Required(ErrorMessage = "Please enter adharcard number")]
        //[RegularExpression("^[0-9]{4}[ -]?[0-9]{4}[ -]?[0-9]{4}$", ErrorMessage = "Please enter valid adharcard number")]
        [RegularExpression(@"^(\d{12})$", ErrorMessage = "Please enter valid adharcard number")]
        [Display(Name = "Aadharcard Number")]
        public string AadharCard { get; set; }

        //[Required(ErrorMessage = "Please select file.")]
        public HttpPostedFileBase ImageFile { get; set; }

        public string QRCode { get; set; }

        public string RegistrationCode { get; set; }

        public int? UrlId { get; set; }

        public string UrlCode { get; set; }

        [Display(Name = "Upload AddharCard Image")]
        public string AadharCardImage { get; set; }

        //[Required(ErrorMessage = "Please select file.")]
        //[RegularExpression(@"([a-zA-Z0-9\s_\\.\-:])+(.png|.jpg|.gif)$", ErrorMessage = "Only Image files allowed.")]
        public HttpPostedFileBase AadharImageFile { get; set; }

        [Required(ErrorMessage = "Please Select your blood group")]
        [Display(Name = "Blood Group")]
        public string BloodGroup { get; set; }

        public string TransactionId { get; set; }

        [Display(Name = "By Admin Free Pass")]
        public bool? IsVaccinated { get; set; }
    }
}
