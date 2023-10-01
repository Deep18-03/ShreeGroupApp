using Newtonsoft.Json;
using OpenHtmlToPdf;
using QRCoder;
using Razorpay.Api;
using RestSharp;
using ShreeGroup.CustomerFilters;
using ShreeGroup.DAL.DbOperations;
using ShreeGroup.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;

namespace ShreeGroup.Controllers
{
    public class PaymentController : Controller
    {
        //Razor pay key and secret
        
        private string _key = "";
        private string _secret = "";
        private string _SMSkey = "";
        private string _SMSsecret = "";
        private decimal _Amount;
        private const string qrCodeUrl = "http://shreegroupumreth.com/Customer/Details/";
        private bool smsEnable = false;

        CustomerRepository repository = null;
        KeyRepository key_repository = null;
        private static Random random = new Random();

        public PaymentController()
        {
            repository = new CustomerRepository();
            key_repository = new KeyRepository();
            ApiKey();
        }

        public string ApiKey()
        {
            var keydetails = key_repository.GetApiDetails();
            smsEnable = !string.IsNullOrEmpty(keydetails.SMSMessage);
            //Razor keys
            _key = keydetails.RazorKey;
            _secret = keydetails.RazorSecret;

            //Sms
            _SMSkey = keydetails.SMSKey;
            _SMSsecret = keydetails.SMSSecret;

            //Amount for pass
            _Amount = keydetails.Amount;


            return "Success";
        }
        
        public ActionResult BeforeRegistrationPass(string garbaPassType = "SILVER")
        {
            var model = new RegistrationModel();
            //sendSMS("7600288440", "Shrey Thakar", "S-57", "432126965310");
            model.District = garbaPassType;
            return View(model);
        }



        [HttpPost]
        public ActionResult BeforeRegistration(string aadharCard,string district)
        {
            if (ModelState.IsValid)
            {
                var isAadharCardAlreadyExists = repository.isAadharCardAlreadyExists(aadharCard);
                if (isAadharCardAlreadyExists)
                {
                    return RedirectToAction("UpdateRegistration", "Payment", new { @aadharCard  = aadharCard, @garbaPassType = district });
                }
                return RedirectToAction("Registration", "Payment", new { @aadharCard = aadharCard , @garbaPassType = district });
            }
            return View();
        }

        public ActionResult UpdateRegistration(string aadharCard,string garbaPassType)
        {
            //Amount to register
            RegistrationModel registrationModel = repository.GetCustomerByAadharCard(aadharCard);
            if (garbaPassType == "SILVER")
            {
                registrationModel.Amount = _Amount;
                registrationModel.District = "SILVER";
            }
            if(garbaPassType == "GOLD")
            {
                registrationModel.Amount = _Amount + 300;
                registrationModel.District = "GOLD";
            }
            TempData["generatepass"] = registrationModel;
            return View(registrationModel);
        }

        [HttpPost]
        public ActionResult UpdateRegistration(RegistrationModel registration)
        {
            repository.UpdateCustomerdetail(registration);
            return RedirectToAction("Payment", "Payment", new RouteValueDictionary(registration));
        }

        /// <summary>
        /// Registrarion
        /// </summary>
        /// <returns></returns>
        public ActionResult Registration(string aadharCard,string garbaPassType)
        {
            //Amount to register
            var model = new RegistrationModel() { Amount = _Amount };
            if (garbaPassType == "SILVER")
            {
                model.Amount = _Amount;
            }
            if (garbaPassType == "GOLD")
            {
                model.Amount = _Amount + 300;
            }
            model.AadharCard = aadharCard;
            model.District = garbaPassType;
            return View(model);
        }

        [HttpPost]
        public ActionResult Registration(RegistrationModel registration)
        {
            if (ModelState.IsValid)
            {
                var isAadharCardAlreadyExists = repository.isAadharCardAlreadyExists(registration.AadharCard);
                if (isAadharCardAlreadyExists)
                {
                    ModelState.AddModelError("AadharCard", "User with this Adahar Number already exists");
                    return View();
                }
                SaveCustomerData(registration,false);
                //TempData["paymentregistration"] = registration;
                //TempData["formregistration"] = registration;
                TempData["generatepass"] = registration;

                return RedirectToAction("Payment", "Payment", new RouteValueDictionary(registration));
            }
            return View();
        }


        public ViewResult Payment(RegistrationModel registration)
        {
            try
            {
                //RegistrationModel registration = (RegistrationModel)TempData["paymentregistration"];
                if (registration != null)
                {
                    OrderModel order = new OrderModel()
                    {
                        OrderAmount = registration.Amount,
                        Currency = "INR",
                        Payment_Capture = 1,    // 0 - Manual capture, 1 - Auto capture
                        Notes = new Dictionary<string, string>()
                {
                    { "note 1", "first note while creating order" }, { "note 2", "you can add max 15 notes" },
                    { "note for account 1", "this is a linked note for account 1" }, { "note 2 for second transfer", "it's another note for 2nd account" }
                }
                    };
                    var orderId = CreateOrder(order);
                    RazorPayOptionsModel razorPayOptions = new RazorPayOptionsModel()
                    {
                        Key = _key,
                        AmountInSubUnits = order.OrderAmountInSubUnits,
                        Currency = order.Currency,
                        Name = "Shree Group",
                        Description = "for garba registration",
                        ImageLogUrl = "",
                        OrderId = orderId,
                        ProfileName = registration.FirstName,
                        ProfileContact = registration.PhoneNumber,
                        ProfileEmail = registration.Email,
                        AadharCard = registration.AadharCard,
                        Notes = new Dictionary<string, string>()
                {
                    { "note 1", "this is a payment note" }, { "note 2", "here also, you can add max 15 notes" }
                }
                    };
                    return View(razorPayOptions);
                }
                else
                {
                    return View("Error");
                }

            }
            catch (Exception ex)
            {
                return View("Error");
            }
        }

        private string CreateOrder(OrderModel order)
        {
            try
            {
                RazorpayClient client = new RazorpayClient(_key, _secret);
                Dictionary<string, object> options = new Dictionary<string, object>();
                options.Add("amount", order.OrderAmountInSubUnits);
                options.Add("currency", order.Currency);
                options.Add("payment_capture", order.Payment_Capture);
                options.Add("notes", order.Notes);

                Order orderResponse = client.Order.Create(options);
                var orderId = orderResponse.Attributes["id"].ToString();
                return orderId;
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        /// <summary>
        /// After payment 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ViewResult AfterPayment(string AadharCard)
        {
            try
            {
                //RegistrationModel registration = (RegistrationModel)TempData["formregistration"];
                //if (registration != null)
                //{
                    var paymentStatus = Request.Form["paymentstatus"].ToString();
                    if (paymentStatus == "Fail")
                        return View("Fail");

                    var orderId = Request.Form["orderid"].ToString();
                    var paymentId = Request.Form["paymentid"].ToString();
                    var signature = Request.Form["signature"].ToString();

                    var validSignature = CompareSignatures(orderId, paymentId, signature);
                    if (validSignature)
                    {
                        //var qrCodeLink = CustomerData(registration, orderId);
                        
                        
                        //create method which take aadhar card as a paramter  and order id and update it 
                        var customerDetail = repository.UpdateCustomerPaymentFlag(AadharCard, orderId,true);
                        var referenceId = customerDetail.District.Substring(0,1) + "-" + customerDetail.Id.ToString();

                        if (smsEnable)
                            {
                                var fullName = customerDetail.FirstName + " " + customerDetail.LastName;
                                sendSMSWhatsapp(customerDetail.PhoneNumber, fullName, referenceId, customerDetail.AadharCard);
                                sendSMS(customerDetail.PhoneNumber, fullName, referenceId, customerDetail.AadharCard);
                            }
                            ViewBag.UniqueId = referenceId;
                            return View("Success");
                        }
                        else
                        {
                            var customerid = repository.UpdateCustomerPaymentFlag(AadharCard, orderId,false);
                            return View("Fail");
                        }
                //}
                //else
                //{
                //    return View("Error");
                //}
            }
            catch (Exception ex)
            {
                return View("Error");
            }
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

        private int SaveCustomerData(RegistrationModel registrationModel, bool isPaymentDone)
        {

            //string qrCodeUrl = "http://shreegroupumreth.com/Customer/Details/";

            //string fileName = Path.GetFileNameWithoutExtension(registrationModel.ImageFile.FileName);
            //string extension = Path.GetExtension(registrationModel.ImageFile.FileName);
            //string fileName = registrationModel.AadharCard.Substring(0, 4) + "_" + registrationModel.FirstName + DateTime.Now.ToString("yymmssfff") + extension;
            //registrationModel.ProfilePicture = "~/Assets/Images/UploadedImages/ProfilePicture/" + fileName;
            //fileName = Path.Combine(Server.MapPath("~/Assets/Images/UploadedImages/ProfilePicture/"), fileName);
            //registrationModel.ImageFile.SaveAs(fileName);

            //Using system.drawing
            var ImageProfileFile = registrationModel.ImageFile;
            //string ImageProfileFileName = Path.GetFileNameWithoutExtension(ImageProfileFile.FileName);
            string ImageProfileExtension = ".jpeg";
            string NewProfileFileName = registrationModel.AadharCard.Substring(0, 4) + "_" + registrationModel.FirstName + DateTime.Now.ToString("yymmssfff") + ImageProfileExtension;
            registrationModel.ProfilePicture = "~/Assets/Images/UploadedImages/ProfilePicture/" + NewProfileFileName;
            var NewFileNamePath = Path.Combine(Server.MapPath("~/Assets/Images/UploadedImages/ProfilePicture/"), NewProfileFileName);

            //string fullPath = Server.MapPath("~/Assets/Images/UploadedImages/ProfilePicture/");
            //var file = registrationModel.ImageFile;
            System.Drawing.Image image = System.Drawing.Image.FromStream(ImageProfileFile.InputStream);
            var jpgEncoder = GetEncoder(ImageFormat.Jpeg);
            var qualityEncoder = System.Drawing.Imaging.Encoder.Quality;
            var encoderParameters = new EncoderParameters(1);
            encoderParameters.Param[0] = new EncoderParameter(qualityEncoder, 50L);
            image.Save(NewFileNamePath, jpgEncoder, encoderParameters);



            //AaharCard Upload 
            //string adharFileName = Path.GetFileNameWithoutExtension(registrationModel.AadharImageFile.FileName);
            //string aadharExtension = Path.GetExtension(registrationModel.AadharImageFile.FileName);
            //string aadharFileName = registrationModel.AadharCard + aadharExtension;
            //registrationModel.AadharCardImage = "~/Assets/Images/UploadedImages/AadharCard/" + aadharFileName;
            //aadharFileName = Path.Combine(Server.MapPath("~/Assets/Images/UploadedImages/AadharCard/"), aadharFileName);
            //registrationModel.AadharImageFile.SaveAs(aadharFileName);

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
            //registrationModel.TransactionId = orderId;
            registrationModel.Age = get_age(Convert.ToDateTime(registrationModel.DateOfBirth));
            registrationModel.IsPaymentDone = isPaymentDone;
            //registrationModel.District = repository.GetMaxUniqueId();

            var id = repository.AddCustomer(registrationModel);

            return id;

        }

        /// <summary>
        /// Save Customer data
        /// </summary>
        /// <param name="registrationModel"></param>
        /// <returns></returns>
        private string CustomerData(RegistrationModel registrationModel, string orderId)
        {

            //string qrCodeUrl = "http://shreegroupumreth.com/Customer/Details/";

            //string fileName = Path.GetFileNameWithoutExtension(registrationModel.ImageFile.FileName);
            //string extension = Path.GetExtension(registrationModel.ImageFile.FileName);
            //string fileName = registrationModel.AadharCard.Substring(0, 4) + "_" + registrationModel.FirstName + DateTime.Now.ToString("yymmssfff") + extension;
            //registrationModel.ProfilePicture = "~/Assets/Images/UploadedImages/ProfilePicture/" + fileName;
            //fileName = Path.Combine(Server.MapPath("~/Assets/Images/UploadedImages/ProfilePicture/"), fileName);
            //registrationModel.ImageFile.SaveAs(fileName);

            //Using system.drawing
            var ImageProfileFile = registrationModel.ImageFile;
            //string ImageProfileFileName = Path.GetFileNameWithoutExtension(ImageProfileFile.FileName);
            string ImageProfileExtension = ".jpeg";
            string NewProfileFileName = registrationModel.AadharCard.Substring(0, 4) + "_" + registrationModel.FirstName + DateTime.Now.ToString("yymmssfff") + ImageProfileExtension;
            registrationModel.ProfilePicture = "~/Assets/Images/UploadedImages/ProfilePicture/" + NewProfileFileName;
            var NewFileNamePath = Path.Combine(Server.MapPath("~/Assets/Images/UploadedImages/ProfilePicture/"), NewProfileFileName);

            //string fullPath = Server.MapPath("~/Assets/Images/UploadedImages/ProfilePicture/");
            //var file = registrationModel.ImageFile;
            System.Drawing.Image image = System.Drawing.Image.FromStream(ImageProfileFile.InputStream);
            var jpgEncoder = GetEncoder(ImageFormat.Jpeg);
            var qualityEncoder = System.Drawing.Imaging.Encoder.Quality;
            var encoderParameters = new EncoderParameters(1);
            encoderParameters.Param[0] = new EncoderParameter(qualityEncoder, 50L);
            image.Save(NewFileNamePath, jpgEncoder, encoderParameters);



            //AaharCard Upload 
            //string adharFileName = Path.GetFileNameWithoutExtension(registrationModel.AadharImageFile.FileName);
            //string aadharExtension = Path.GetExtension(registrationModel.AadharImageFile.FileName);
            //string aadharFileName = registrationModel.AadharCard + aadharExtension;
            //registrationModel.AadharCardImage = "~/Assets/Images/UploadedImages/AadharCard/" + aadharFileName;
            //aadharFileName = Path.Combine(Server.MapPath("~/Assets/Images/UploadedImages/AadharCard/"), aadharFileName);
            //registrationModel.AadharImageFile.SaveAs(aadharFileName);

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
            registrationModel.TransactionId = orderId;
            registrationModel.Age = get_age(Convert.ToDateTime(registrationModel.DateOfBirth));


            int id = repository.AddCustomer(registrationModel);

            return registrationModel.QRCode;

        }

        public int get_age(DateTime dob)
        {
            int age = 0;
            age = DateTime.Now.Subtract(dob).Days;
            age = age / 365;
            return age;
        }

        /// <summary>
        /// Random string
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private bool CompareSignatures(string orderId, string paymentId, string razorPaySignature)
        {
            var text = orderId + "|" + paymentId;
            var secret = _secret;
            var generatedSignature = CalculateSHA256(text, secret);
            if (generatedSignature == razorPaySignature)
                return true;
            else
                return false;
        }

        private string CalculateSHA256(string text, string secret)
        {
            string result = "";
            var enc = Encoding.Default;
            byte[]
            baText2BeHashed = enc.GetBytes(text),
            baSalt = enc.GetBytes(secret);
            System.Security.Cryptography.HMACSHA256 hasher = new HMACSHA256(baSalt);
            byte[] baHashedText = hasher.ComputeHash(baText2BeHashed);
            result = string.Join("", baHashedText.ToList().Select(b => b.ToString("x2")).ToArray());
            return result;
        }

        /// <summary>
        /// Customer pass
        /// </summary>
        /// <returns></returns>
        [PreventFromUrl]
        public ActionResult CustomerPass()
        {
            RegistrationModel registration = (RegistrationModel)TempData["generatepass"];
            if (registration != null)
            {
                ViewBag.FirstName = registration.FirstName;
                ViewBag.LastName = registration.LastName;
                ViewBag.PhoneNumber = registration.PhoneNumber;
                ViewBag.AadharCard = registration.AadharCard;
                ViewBag.ProfilePicture = registration.ProfilePicture;
                ViewBag.Age = registration.Age;
                ViewBag.BloodGroup = registration.BloodGroup;
                ViewBag.QRCode = registration.QRCode;
                QRCodeGenerator QrGenerator = new QRCodeGenerator();
                QRCodeData QrCodeInfo = QrGenerator.CreateQrCode(registration.QRCode, QRCodeGenerator.ECCLevel.Q);
                QRCode QrCode = new QRCode(QrCodeInfo);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (Bitmap bitmap = QrCode.GetGraphic(20))
                    {
                        bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                        ViewBag.QRCodeImagePass = "data:image/png;base64," + Convert.ToBase64String(ms.ToArray());
                    }
                }
                return View();
            }
            else
            {
                return View("Error");
            }

        }

        /// <summary>
        /// Generate pass 
        /// </summary>
        /// <param name="registration"></param>
        /// <returns></returns>
        public ActionResult GeneratePass(RegistrationModel registration)
        {
            if (registration.QRCode != null)
            {
                QRCodeGenerator QrGenerator = new QRCodeGenerator();
                QRCodeData QrCodeInfo = QrGenerator.CreateQrCode(registration.QRCode, QRCodeGenerator.ECCLevel.Q);
                QRCode QrCode = new QRCode(QrCodeInfo);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (Bitmap bitmap = QrCode.GetGraphic(20))
                    {
                        bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                        ViewBag.QRCodeImage = "data:image/png;base64," + Convert.ToBase64String(ms.ToArray());
                    }
                }

                var profilePictureServerPath = Path.Combine(Server.MapPath(registration.ProfilePicture));
                var profilePicture64 = ImageBase64(profilePictureServerPath);
                ViewBag.imagepng = profilePicture64;

                return View(registration);
            }
            else
            {
                return View("Error");
            }

        }

        public string sendSMS(string to, string fullName,string Id,string aadharcardNumber)
        {

            //string _SMSkey = "gyg8zeu70ogeydt";
            //string _SMSsecret = "gsa7x0l6b8mfxn4";
            //string msg = "Registration has been successfully completed for Shree Group Yashotsav Garba 2023.\n\n" + "Open Pass Link\n" + QRCodelink;
            //string msg = "Registration has been successfully completed for Shree Group Yashotsav Garba 2023.\n\n"
            //    + "PLease Collect your physical pass after 10th October from office Address \n"
            //    + "Reference No. :- " + Id + "\n" + "Aadhar No. :- " + aadharcardNumber + "\n"
            //    + "Name :- " + fullName
            //    +"\n Shree Group Umreth\n Vraj Arcade, Nr. Champaklal Ghabhawala Gate\n Opp.Tarunimata Temple, Umreth";
            string msg = "Successfully register for Shree Group Yashotsav Garba 2023\n"
              + "Ref No :" + Id + "\n" + "Aadhar: " + aadharcardNumber + "\n"
              + "\nCollect physical pass from office\n"
              + "Helpline: 9723555960";
            var client = new RestSharp.RestClient("https://www.thetexting.com/rest/sms/json/message");

            string sendSMSTo = "91" + to;
            var request = new RestRequest("/send/", Method.Post);
            request.AddHeader("content-type", "application/x-www-form-urlencoded");
            request.AddHeader("cache-control", "no-cache");
            request.AddParameter("application/x-www-form-urlencoded", ParameterType.RequestBody);
            request.AddParameter("api_secret", _SMSsecret);
            request.AddParameter("api_key", _SMSkey);
            request.AddParameter("from", "test");
            request.AddParameter("to", sendSMSTo);
            request.AddParameter("text", msg);

            var response = client.Execute(request);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return "Success";
            }
            return "Fail";
        }

        //twilio message

        //public string TwilioSMS()
        //{
        //    string accountSid = "AC29203ac8be0a5394854af271b7fbfbd0";
        //    string authToken = "d90de28e8d65aa2c1864ddfaa764bb30";
        //    TwilioClient.Init(accountSid, authToken);

        //    var message = MessageResource.Create(
        //        body: "Join Earth's mightiest heroes. Like Kevin Bacon.",
        //        from: new Twilio.Types.PhoneNumber("+15738792535"),
        //        to: new Twilio.Types.PhoneNumber("+918153024214")
        //    );


        //    //var message2 = MessageResource.Create(
        //    //    from: new Twilio.Types.PhoneNumber("whatsapp:+15738792535"),
        //    //    body: "Hello, there!",
        //    //    to: new Twilio.Types.PhoneNumber("whatsapp:+918153024214")
        //    //);

        //    return message.To;
        //}
        /// <summary>
        /// Generate Pdf
        /// </summary>
        /// <param name="registration"></param>
        /// <returns></returns>
    //    [HttpPost]
    //    public string GeneratePdf(RegistrationModel registration)
    //    {
    //        var firstName = registration.FirstName;
    //        var lastName = registration.LastName;
    //        int? age = registration.Age;
    //        var phoneNumber = registration.PhoneNumber;
    //        var aadharCardNumber = registration.AadharCard;

    //        //QRCode
    //        var QRCodeImage = QRCodeGeneration(registration.QRCode);

    //        //Image server path
    //        var profilePictureServerPath = Path.Combine(Server.MapPath(registration.ProfilePicture));
    //        var bgPassImageServerPath = Path.Combine(Server.MapPath("~/Assets/Images/download-pass-bg.png"));
    //        var shreeGroupLogoServerPath = Path.Combine(Server.MapPath("~/Assets/Images/SHREE GROUP logo.png"));
    //        var utsavLogoServerPath = Path.Combine(Server.MapPath("~/Assets/Images/utsav.png"));

    //        //Base64
    //        var profilePicture64 = ImageBase64(profilePictureServerPath);

    //        var bgPass64 = ImageBase64(bgPassImageServerPath);
    //        var shreeGroupLogo64 = ImageBase64(shreeGroupLogoServerPath);
    //        var utsavLogo64 = ImageBase64(utsavLogoServerPath);

    //        //HTML 
    //        var htmlPDF = string.Format(@"<!DOCTYPE HTML PUBLIC ""-//W3C//DTD HTML 4.01//EN""      ""http://www.w3.org/TR/html4/strict.dtd"">
    //        <html>
    //        <head>
    //        <title>Shree Group Umreth</title>
    //        <link rel=""stylesheet"" type=""text/css"" href=""style.css"">
    //        </head>
    //        <body>

    //            <div class=""sub-page"">
    //        <br />
    //         <img src=""{9}"" style=""margin: 500; width: 560px; height: 660px;z-index: -1; position:absolute; left: 600px;top:0px;"" />
    //         <div style="" margin: 500; width: 580px; height: 680px; box-shadow: 0px 0px 35px #888888; margin-top: 70px; position:absolute; left: 600px;top:0px;"">
    //            <div style=""float:right; width: 40%; height: 100%;"">
    //                <img src=""{0}"" alt=""Profile Image"" style=""width: 200px; height: 200px; margin-left: 0px; border-radius: 10px; margin-top:25px;"" />
    //                <div style=""margin-top: 30px; margin-left:-15px;"">
    //                    <div>
    //                        <label style=""font-size: 22px; font-weight: 500; color: #324055; display: inline; padding: 0.2em 0.6em 0.3em;""
    //                               for=""firstname"">First Name</label> &nbsp;
    //                        <span style=""width:10px !important;"">
    //                            <input type=""text"" id=""LastName"" name=""LastName"" value=""{1}"" readonly style=""text-decoration: none; border: none; background: none; outline: none; padding: 0.2em 0.6em 0.3em; font-weight: bold; width: 170px; font-size: 24px;"">
    //                        </span>
    //                    </div>
    //                    <div>
    //                        <label style=""font-size: 22px; font-weight: 500; color: #324055; display: inline; padding: 0.2em 0.6em 0.3em;""
    //                               for=""lastname"">Last Name</label> &nbsp;
    //                        <span style=""width:10px !important;"">
    //                            <input type=""text"" id=""LastName"" name=""LastName"" value=""{2}"" readonly style=""text-decoration: none; border: none; background: none; outline: none; padding: 0.2em 0.6em 0.3em; font-weight: bold; width: 170px; font-size: 24px;"">
    //                        </span>
    //                    </div>
    //                    <div>
    //                        <label style=""font-size: 22px; font-weight: 500; color: #324055; display: inline; padding: 0.2em 0.6em 0.3em;""
    //                               for=""bloodgroup"">Age</label> &nbsp;
    //                        <br />
    //                        <span style=""width:10px !important;"">
    //                            <input type=""text"" id=""Age"" name=""Age"" value=""{3}"" readonly style=""text-decoration: none; border: none; background: none; outline: none; padding: 0.2em 0.6em 0.3em; font-weight: bold; width: 170px; font-size: 24px;"">
    //                        </span>
    //                    </div>
    //                    <div>
    //                        <label style=""font-size: 22px; font-weight: 500; color: #324055; display: inline; padding: 0.2em 0.6em 0.3em;""
    //                               for=""phonenumber"">Phone Number</label> &nbsp;
    //                        <span style=""width:10px !important;"">
    //                            <input type=""text"" id=""PhoneNumber"" name=""PhoneNumber"" value=""{4}"" readonly style=""text-decoration: none; border: none; background: none; outline: none; padding: 0.2em 0.6em 0.3em; font-weight: bold; width: 170px; font-size: 24px;"">
    //                        </span>
    //                    </div>
    //                    <div>
    //                        <label style=""font-size: 22px; font-weight: 500; color: #324055; display: inline; padding: 0.2em 0.6em 0.3em;""
    //                               for=""aadharcard"">Aadhar Card</label> &nbsp;
    //                        <span style=""width:10px !important;"">
    //                            <input type=""text"" id=""AadharCard"" name=""AadharCard"" value=""{5}"" readonly style=""text-decoration: none; border: none; background: none; outline: none; padding: 0.2em 0.6em 0.3em; font-weight: bold; width: 170px; font-size: 24px;"">
    //                        </span>
    //                    </div>
    //                </div>
    //                <div style="" font-size: 14px !important; font-weight: bold; margin-top: 15px;""> <span style=""font-size: 14px !important; margin-top: 15px;"">From:</span> 26-09-2022 <span style="" font-size: 14px !important; margin-top: 15px;""><br /><br />To:</span> 4-10-2022</div>
    //            </div>
    //            <div style=""float: left; width: 58%; height: 100%;"">
    //                <div>
    //                    <img style="" width: 250px; position: relative; right: -50px; height: 250px; bottom: 30px;"" src=""{6}"" alt=""Logo"" />
    //                    <br />
    //                    <img style=""width: 270px; height: 150px; position: relative; right: -30px; bottom:45px;"" src=""{7}"" alt=""Logo"" />
    //                </div>

    //                <div class=""content"">
    //                    <img src=""{8}"" width=""240px"" height=""240px;"" style=""position: relative; right: -30px; bottom: 30px; "" />
    //                </div>
    //                <div style=""margin-left:30px;""><span style=""font-weight:bold;color:black !important;"">Venue:</span> SNDT Ground, Umreth</div>
    //            </div>
    //        </div>

    //    </div>
    //</div>
            
           
    //        </body>
    //        ", profilePicture64, firstName, lastName, age, phoneNumber, aadharCardNumber, shreeGroupLogo64, utsavLogo64, QRCodeImage, bgPassImageServerPath);


    //        //PDF generation code
    //        var pdf = OpenHtmlToPdf.Pdf
    //          .From(htmlPDF)
    //          .OfSize(PaperSize.A4)
    //          .WithMargins(0.Centimeters())
    //          .Landscape()
    //          .Content();


    //        string extension = ".pdf";
    //        string pdfFileName = registration.AadharCard + extension;
    //        var pdfDownloadServerPath = Path.Combine(Server.MapPath("~/Assets/Images/UploadedImages/DownloadedPass/"), pdfFileName);

    //        //This will save a pdf file in server
    //        System.IO.File.WriteAllBytes(pdfDownloadServerPath, pdf);

    //        //Download pass at cleint side
    //        var clientDownloadResult = DownloadPDFClientSide(pdfDownloadServerPath);

    //        return "Success";
    //    }

        [HttpPost]
        public string GeneratePdf(RegistrationModel registration)
        {
            var firstName = registration.FirstName;
            var lastName = registration.LastName;
            var bloodgroup = registration.BloodGroup;
            var phoneNumber = registration.PhoneNumber;
            var aadharCardNumber = registration.AadharCard;

            //QRCode
            var QRCodeImage = QRCodeGeneration(registration.QRCode);

            //Image server path
            var profilePictureServerPath = Path.Combine(Server.MapPath(registration.ProfilePicture));
            var bgPassImageServerPath =registration.IsVaccinated == true ? Path.Combine(Server.MapPath("~/Assets/Images/pass-background-admin-free.jpg"))  : Path.Combine(Server.MapPath("~/Assets/Images/pass-background-image.jpg"));
            var heartImagePath = Path.Combine(Server.MapPath("~/Assets/Images/heart.png"));
            var userImagePath = Path.Combine(Server.MapPath("~/Assets/Images/User.png"));
            var phoneImagePath = Path.Combine(Server.MapPath("~/Assets/Images/phone.png"));
            var noteImagePath = Path.Combine(Server.MapPath("~/Assets/Images/note.png"));


            //var shreeGroupLogoServerPath = Path.Combine(Server.MapPath("~/Assets/Images/SHREE GROUP logo.png"));
            //var utsavLogoServerPath = Path.Combine(Server.MapPath("~/Assets/Images/utsav.png"));

            //Base64
            var profilePicture64 = ImageBase64(profilePictureServerPath);

            var bgPass64 = ImageBase64(bgPassImageServerPath);

            var heartsymbol = ImageBase64(heartImagePath);
            var usersymbol = ImageBase64(userImagePath);
            var phonesymbol = ImageBase64(phoneImagePath);
            var notesymbol = ImageBase64(noteImagePath);

            //HTML 
            var htmlPDF = string.Format(@"<!DOCTYPE HTML PUBLIC ""-//W3C//DTD HTML 4.01//EN""      ""http://www.w3.org/TR/html4/strict.dtd"">
            <html>
            <head>
            <title>Shree Group Umreth</title>
            <link rel=""stylesheet"" type=""text/css"" href=""style.css"">
            </head>
            <body>
     <div style="" position:absolute; top:100px; left:250px;margin: auto; display: flex; width: 540px; height: 780px; box-shadow: 0px 0px 35px #888888; border-radius: 10px;"">
        <div style=""position:relative; width:540px; height:780px;"">
            <img src=""{0}"" style=""width: 540px; height:780px;"" />
            <div style=""position:relative; width:540px; top:-590px; height:350px;"">
                <img src=""{1}"" alt=""Profile Image""  style="" display: block; width: 200px; height: 200px; border-radius: 10px; position: absolute; left: 25px;""/>
                <div style=""z-index: 1; position:relative; left:315px; width:200px; height:200px; overflow:hidden;"">
                    <img src=""{2}"" style=""width:235px; height:235px; margin:-17px 0px 0px -18px;"" />
             </div>
                <div style="" text-align:center; font-size: 24px !important; color: black !important; font-weight: 600; font-family: 'Times New Roman', Times, serif; text-transform: uppercase; width: 100%; margin-top: 15px;"">
                    <div>
                       <img src=""{9}"" style=""width: 18px; height:18px;"" />   {3} {4}
                    </div>
                    <br />
                    <div>
                        <img src=""{11}"" style=""width: 18px; height:18px;"" />   +91 {5}
                    </div>
                    <br />
                    <div>
                        <img src=""{8}"" style=""width: 18px; height:18px;"" /> {6} (Blood group)
                    </div>
                    <br />
                    <div>
                        <img src=""{10}"" style=""width: 18px; height:18px;"" />  {7}
                    </div>
                </div>
            </div>
        </div>
    </div>
            </body>
            ", bgPass64, profilePicture64, QRCodeImage, firstName, lastName, phoneNumber, bloodgroup, aadharCardNumber, heartsymbol, usersymbol, notesymbol, phonesymbol);


            //PDF generation code
            var pdf = OpenHtmlToPdf.Pdf
              .From(htmlPDF)
              .OfSize(PaperSize.A4)
              .WithMargins(0.Centimeters())
              .Landscape()
              .Content();


            string extension = ".pdf";
            string pdfFileName = registration.AadharCard + extension;
            var pdfDownloadServerPath = Path.Combine(Server.MapPath("~/Assets/Images/UploadedImages/DownloadedPass/"), pdfFileName);

            //This will save a pdf file in server
            System.IO.File.WriteAllBytes(pdfDownloadServerPath, pdf);

            //Download pass at cleint side
            var clientDownloadResult = DownloadPDFClientSide(pdfDownloadServerPath);

            return "Success";
        }




        public string DownloadPDFClientSide(string clientPath)
        {
            string name = Path.GetFileName(clientPath); //get file name
            string ext = Path.GetExtension(clientPath); //get file extension
            string type = "";

            if (ext != null)
            {
                switch (ext.ToLower())
                {
                    case ".htm":
                    case ".html":
                        type = "text/HTML";
                        break;

                    case ".txt":
                        type = "text/plain";
                        break;

                    case ".GIF":
                        type = "image/GIF";
                        break;

                    case ".pdf":
                        type = "Application/pdf";
                        break;

                    case ".doc":
                    case ".rtf":
                        type = "Application/msword";
                        break;
                }
            }


            Response.AppendHeader("content-disposition", "attachment; filename=" + name);
            if (type != "")
                Response.ContentType = type;
            Response.WriteFile(clientPath);
            Response.End();

            return "Succsessfully dowmlaoded pass at cliet side";
        }

        public string ImageBase64(string path)
        {
            byte[] imageArray = System.IO.File.ReadAllBytes(path);
            string base64ImageRepresentation = Convert.ToBase64String(imageArray);
            var imagePhotoBase64 = "data:image/png;base64," + base64ImageRepresentation;
            return imagePhotoBase64;
        }

        public string QRCodeGeneration(string QRCodestring)
        {
            string QRCodeImage = "";
            QRCodeGenerator QrGenerator = new QRCodeGenerator();
            QRCodeData QrCodeInfo = QrGenerator.CreateQrCode(QRCodestring, QRCodeGenerator.ECCLevel.Q);
            QRCode QrCode = new QRCode(QrCodeInfo);
            using (MemoryStream ms = new MemoryStream())
            {
                using (Bitmap bitmap = QrCode.GetGraphic(20))
                {
                    bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    QRCodeImage = "data:image/png;base64," + Convert.ToBase64String(ms.ToArray());
                }
            }
            return QRCodeImage;
        }

        //public string sendSMSWhatsapp()
        //{

        //    //string json = @"{
        //    //    ""messaging_product"": ""whatsapp"",
        //    //    ""to"": ""918153024214"",
        //    //    ""type"": ""template"",
        //    //    ""template"": {
        //    //        ""name"": ""hello_world"",
        //    //        ""language"": {
        //    //            ""code"": ""en_US""
        //    //        }
        //    //    }
        //    //}";

        //    string recipientPhoneNumber = "918153024214";
        //    string referenceId = "1234";

        //    // Create the JSON object
        //    var jsonBody = new
        //    {
        //        messaging_product = "whatsapp",
        //        recipient_type = "individual",
        //        to = recipientPhoneNumber,
        //        type = "text",
        //        text = new
        //        {
        //            preview_url = false,
        //            body = $"Shree Group Umreth {referenceId}"
        //        }
        //    };

        //    // Serialize the JSON object to a string
        //    string jsonString = JsonConvert.SerializeObject(jsonBody);

        //    //string msg = "Successfully register for Shree Group Yashotsav Garba 2023\n"
        //    //  + "Ref No :" + Id + "\n" + "Aadhar: " + aadharcardNumber + "\n"
        //    //  + "\nCollect physical pass from office\n"
        //    //  + "Helpline: 9723555960";
        //    var client = new RestSharp.RestClient("https://graph.facebook.com/v17.0/102288765958485/messages");

        //    //string sendSMSTo = "91" + to;
        //    var request = new RestRequest();
        //    request.Method = Method.Post;
        //    //var request = new RestRequest(Method.Post);
        //    request.AddHeader("content-type", "application/json");
        //    request.AddHeader("Authorization", "Bearer EAAD3bHg8dCkBOZBCvkXi1TEjvKdbQp400lBtYQ7IZAd0YMy5BWEZB0Lc7TO1iMI8SwoFacut6a8b3W1cvwGi5keFZAiP6j7db9HftZCN2QAW44wqTPWjiJeXbv73BdmLGnY2ef6fAPRE1srqhRRa8dRer6PybefXZAkpS8DZCCczzicci4lqwpw9SsdJUDK77sd");

        //    request.AddParameter("application/json", jsonString, ParameterType.RequestBody);

        //    //request.AddParameter("application/x-www-form-urlencoded", ParameterType.RequestBody);
        //    //request.AddParameter("api_secret", _SMSsecret);
        //    //request.AddParameter("api_key", _SMSkey);
        //    //request.AddParameter("from", "test");
        //    //request.AddParameter("to", sendSMSTo);
        //    //request.AddParameter("text", msg);

        //    var response = client.Execute(request);
        //    if (response.StatusCode == System.Net.HttpStatusCode.OK)
        //    {
        //        return "Success";
        //    }
        //    return "Fail";
        //}
        public string sendSMSWhatsapp(string to, string fullName, string Id, string aadharcardNumber)
        {

            //string msg = "Successfully register for Shree Group Yashotsav Garba 2023\n"
            //  + "Ref No :" + Id + "\n" + "Aadhar: " + aadharcardNumber + "\n"
            //  + "\nCollect physical pass from office\n"
            //  + "Helpline: 9723555960";
            //var msg = "Test1";
            string msg = "Registration has been successfully completed for Shree Group Yashotsav Garba 2023.\n\n"
                + "PLease Collect your physical pass after 10th October from office Address \n"
                + "Reference No. :- " + Id + "\n" + "Aadhar No. :- " + aadharcardNumber + "\n"
                + "Name :- " + fullName
                + "Office Address : \n Shree Group Umreth\n Vraj Arcade, Nr. Champaklal Ghabhawala Gate\n Opp.Tarunimata Temple, Umreth";

            var jsonUrl = "https://login.bulksmsgateway.in/textmobilesmsapi.php?user=shreegroup&password=Yash6595@&mobile="+to+"&message="+msg+"&type=203";
            var client = new RestSharp.RestClient(jsonUrl);

            //string sendSMSTo = "91" + to;
            var request = new RestRequest();
            request.Method = Method.Post;

            //request.AddHeader("content-type", "application/json");

            ////request.AddParameter("application/x-www-form-urlencoded", ParameterType.RequestBody);
            //request.AddParameter("user", "shreegroup");
            //request.AddParameter("password", "Yash6595@");
            //request.AddParameter("mobile", "9033767091");
            //request.AddParameter("message", "Shree Group Umreth");
            //request.AddParameter("type", 203);

            var response = client.Execute(request);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return "Success";
            }
            return "Fail";
        }

    }
}