using ShreeGroup.DAL.DbOperations;
using ShreeGroup.Models;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ShreeGroup.Controllers
{
    [Authorize]
    public class RegisterUserController : Controller
    {

        CustomerRepository repository = null;
        private static Random random = new Random();

        public RegisterUserController()
        {
            repository = new CustomerRepository();
        }

        // GET: RegisterUser
        public ActionResult Index()
        {
            ViewBag.TotalAllCount = repository.GetTotalCustomerCount();
            return View();
        }

        public ActionResult GetCustomerList(string searching,string passType)
        {
            try
            {
                var result = new List<RegisterCustomer>();
                if (string.IsNullOrWhiteSpace(searching) && string.IsNullOrWhiteSpace(passType))
                {
                    result = repository.GetAllCustomer();
                }
                else
                {
                    result = repository.GetAllCustomerAdmin(searching,passType);
                }
                ViewBag.TotalCount = result.Count();
                return View(result);

            }
            catch (Exception ex)
            {
                return View("Error");
            }
        }

        public ActionResult GetAdminFreeCustomerList()
        {
            try
            {
                var result = new List<RegisterCustomer>();
                 result = repository.GetFreeAdminCustomer();
               
                ViewBag.TotalAdminCount = result.Count();
                return View(result);

            }
            catch (Exception ex)
            {
                return View("Error");
            }
        }

        public ActionResult GetBookingSeatList()
        {
            try
            {
                var result = new List<GeneralSeatBookingModel>();
                result = repository.GetGeneralSeatBookings();

                ViewBag.TotalSeatCount = result.Count();
                return View(result);

            }
            catch (Exception ex)
            {
                return View("Error");
            }
        }

        public ActionResult RegisterCashCustomer()
        {
            return View();
        }

        [HttpPost]
        public  ActionResult RegisterCashCustomer(RegisterCashCustomer registrationModel)
        {
            if (ModelState.IsValid)
            {
                var isAadharCardAlreadyExists = repository.isAadharCardAlreadyExists(registrationModel.AadharCard);
                if (isAadharCardAlreadyExists)
                {
                    ModelState.AddModelError("AadharCard", "User with this Adahar Number already exists");
                    return View();
                }
                string qrCodeUrl = "http://shreegroupumreth.com/Customer/Details/";

                var ImageProfileFile = registrationModel.ImageFile;
                //string ImageProfileFileName = Path.GetFileNameWithoutExtension(ImageProfileFile.FileName);
                string ImageProfileExtension = ".jpeg";
                string NewProfileFileName = registrationModel.AadharCard.Substring(0, 4) + "_" + registrationModel.FirstName + DateTime.Now.ToString("yymmssfff") + ImageProfileExtension;
                registrationModel.ProfilePicture = "~/Assets/Images/UploadedImages/ProfilePicture/" + NewProfileFileName;
                var NewFileNamePath = Path.Combine(Server.MapPath("~/Assets/Images/UploadedImages/ProfilePicture/"), NewProfileFileName);

                System.Drawing.Image image = System.Drawing.Image.FromStream(ImageProfileFile.InputStream);
                var jpgEncoder = GetEncoder(ImageFormat.Jpeg);
                var qualityEncoder = System.Drawing.Imaging.Encoder.Quality;
                var encoderParameters = new EncoderParameters(1);
                encoderParameters.Param[0] = new EncoderParameter(qualityEncoder, 50L);
                image.Save(NewFileNamePath, jpgEncoder, encoderParameters);


                var ImageAadharFile = registrationModel.AadharImageFile;
                //string ImageProfileFileName = Path.GetFileNameWithoutExtension(ImageProfileFile.FileName);
                string ImageAadharExtension = ".jpeg";
                string NewAadharFileName = registrationModel.AadharCard + ImageAadharExtension;
                registrationModel.AadharCardImage = "~/Assets/Images/UploadedImages/AadharCard/" + NewAadharFileName;
                var NewAadharFileNamePath = Path.Combine(Server.MapPath("~/Assets/Images/UploadedImages/AadharCard/"), NewAadharFileName);

                System.Drawing.Image aadharImage = System.Drawing.Image.FromStream(ImageAadharFile.InputStream);
                var jpgEncoderAadhar = GetEncoder(ImageFormat.Jpeg);
                var qualityEncoderAadhar = System.Drawing.Imaging.Encoder.Quality;
                var encoderParametersAadhar = new EncoderParameters(1);
                encoderParametersAadhar.Param[0] = new EncoderParameter(qualityEncoderAadhar, 50L);
                aadharImage.Save(NewAadharFileNamePath, jpgEncoderAadhar, encoderParametersAadhar);

                registrationModel.UrlCode = registrationModel.AadharCard.Substring(0, 4) + RandomString(4);
                registrationModel.QRCode = qrCodeUrl + registrationModel.UrlCode;
                registrationModel.RegistrationCode = registrationModel.AadharCard + registrationModel.FirstName;
                //registrationModel.Age = get_age(Convert.ToDateTime(registrationModel.DateOfBirth));

                var resultId = repository.AddCashCustomer(registrationModel);
                ViewBag.AdminId = resultId;
                if (resultId != 0)
                {
                    return View("AdminSuccess");
                }
                return View("AdminFail");
            }
            return View();
        }

        public ActionResult RegisterationCustomer()
        {
            return View();
        }

        [HttpPost]
        public ActionResult RegisterationCustomer(RegisterCustomer registrationModel)
        {
            if (ModelState.IsValid)
            {
                var isAadharCardAlreadyExists = repository.isAadharCardAlreadyExists(registrationModel.AadharCard);
                if (isAadharCardAlreadyExists)
                {
                    ModelState.AddModelError("AadharCard", "User with this Adahar Number already exists");
                    return View();
                }
                string qrCodeUrl = "http://shreegroupumreth.com/Customer/Details/";

                var ImageProfileFile = registrationModel.ImageFile;
                //string ImageProfileFileName = Path.GetFileNameWithoutExtension(ImageProfileFile.FileName);
                string ImageProfileExtension = ".jpeg";
                string NewProfileFileName = registrationModel.AadharCard.Substring(0, 4) + "_" + registrationModel.FirstName + DateTime.Now.ToString("yymmssfff") + ImageProfileExtension;
                registrationModel.ProfilePicture = "~/Assets/Images/UploadedImages/ProfilePicture/" + NewProfileFileName;
                var NewFileNamePath = Path.Combine(Server.MapPath("~/Assets/Images/UploadedImages/ProfilePicture/"), NewProfileFileName);

                System.Drawing.Image image = System.Drawing.Image.FromStream(ImageProfileFile.InputStream);
                var jpgEncoder = GetEncoder(ImageFormat.Jpeg);
                var qualityEncoder = System.Drawing.Imaging.Encoder.Quality;
                var encoderParameters = new EncoderParameters(1);
                encoderParameters.Param[0] = new EncoderParameter(qualityEncoder, 50L);
                image.Save(NewFileNamePath, jpgEncoder, encoderParameters);


                var ImageAadharFile = registrationModel.AadharImageFile;
                //string ImageProfileFileName = Path.GetFileNameWithoutExtension(ImageProfileFile.FileName);
                string ImageAadharExtension = ".jpeg";
                string NewAadharFileName = registrationModel.AadharCard + ImageAadharExtension;
                registrationModel.AadharCardImage = "~/Assets/Images/UploadedImages/AadharCard/" + NewAadharFileName;
                var NewAadharFileNamePath = Path.Combine(Server.MapPath("~/Assets/Images/UploadedImages/AadharCard/"), NewAadharFileName);

                System.Drawing.Image aadharImage = System.Drawing.Image.FromStream(ImageAadharFile.InputStream);
                var jpgEncoderAadhar = GetEncoder(ImageFormat.Jpeg);
                var qualityEncoderAadhar = System.Drawing.Imaging.Encoder.Quality;
                var encoderParametersAadhar = new EncoderParameters(1);
                encoderParametersAadhar.Param[0] = new EncoderParameter(qualityEncoderAadhar, 50L);
                aadharImage.Save(NewAadharFileNamePath, jpgEncoderAadhar, encoderParametersAadhar);

                registrationModel.UrlCode = registrationModel.AadharCard.Substring(0, 4) + RandomString(4);
                registrationModel.QRCode = qrCodeUrl + registrationModel.UrlCode;
                registrationModel.RegistrationCode = registrationModel.AadharCard + registrationModel.FirstName;
                registrationModel.Age = get_age(Convert.ToDateTime(registrationModel.DateOfBirth));

                var result = repository.AddCustomer(registrationModel);
                ViewBag.AdminId = result;
                if (result != 0)
                {
                    return View("AdminSuccess");
                }
                return View("AdminFail");
            }
            return View();
        }

        public int get_age(DateTime dob)
        {
            int age = 0;
            age = DateTime.Now.Subtract(dob).Days;
            age = age / 365;
            return age;
        }


        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private ImageCodecInfo GetEncoder(ImageFormat format)
        {
            var codecs = ImageCodecInfo.GetImageDecoders();
            foreach (var codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }

            return null;
        }
    }
}