using Razorpay.Api;
using RestSharp;
using ShreeGroup.DAL.DbOperations;
using ShreeGroup.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace ShreeGroup.Controllers
{
    public class GeneralSeatPassController : Controller
    {
        public readonly int generalPassAmount = Int32.Parse(System.Configuration.ConfigurationManager.AppSettings["GeneralPassSeating"]);
        private static Random random = new Random();


        private string _key = "";
        private string _secret = "";
        private string _SMSkey = "";
        private string _SMSsecret = "";
        private decimal _Amount;
        private bool smsEnable = false;


        GeneralSeatBookingRepository repository = null;
        KeyRepository key_repository = null;

        public GeneralSeatPassController()
        {
            repository = new GeneralSeatBookingRepository();
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


        // GET: GeneralSeatPass
        public ActionResult SaveGeneralPass()
        {
            var generalPassModel = new GeneralSeatBookingModel();
            generalPassModel.TotalAmount = generalPassAmount;
            ViewBag.GeneralPassAmount = generalPassAmount;
            return View(generalPassModel);
        }

        // GET: GeneralSeatPass/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // POST: GeneralSeatPass/Create
        [HttpPost]
        public ActionResult SaveGeneralPass(GeneralSeatBookingModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    //var isAadharCardAlreadyExists = repository.isAadharCardAlreadyExists(registration.AadharCard);
                    //if (isAadharCardAlreadyExists)
                    //{
                    //    ModelState.AddModelError("AadharCard", "User with this Adahar Number already exists");
                    //    return View();
                    //}
                    var id = repository.AddGeneralPassSeat(model);
                    model.Id = id;
                    //model.BookingId = "BSGU00" + id;
                    return RedirectToAction("Payment", "GeneralSeatPass", new RouteValueDictionary(model));
                }
                return View();
            }
            catch(Exception ex)
            {
                return View();
            }
        }

        public ViewResult Payment(GeneralSeatBookingModel registration)
        {
            try
            {
                //RegistrationModel registration = (RegistrationModel)TempData["paymentregistration"];
                if (registration != null)
                {
                    OrderModel order = new OrderModel()
                    {
                        OrderAmount = (decimal)registration.TotalAmount,
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
                        ProfileContact = registration.MobileNumber,
                        ProfileEmail = "GeneralPass",
                        AadharCard = registration.Id.ToString(),
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
            //Here aadharCard means Primary key Id 
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
                var id = Int64.Parse(AadharCard);
                if (validSignature)
                {
                    var seatDetail = repository.UpdateSeatPaymentFlag(id, orderId, true);
                    //var referenceId = customerDetail.District.Substring(0, 1) + "-" + customerDetail.Id.ToString();

                    if (smsEnable)
                    {
                        var fullName = seatDetail.FirstName;
                        sendSMSWhatsapp(seatDetail.MobileNumber, fullName, seatDetail.BookingId, seatDetail.TotalAmount.ToString(), seatDetail.Quantity.ToString());
                        sendSMS(seatDetail.MobileNumber, fullName, seatDetail.BookingId, seatDetail.TotalAmount.ToString(),seatDetail.Quantity.ToString());
                    }
                    ViewBag.UniqueId = seatDetail.BookingId;
                    return View("SeatSuccess");
                }
                else
                {
                    var customerid = repository.UpdateSeatPaymentFlag(id, orderId, false);
                    return View("SeatFail");
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


        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public string sendSMSWhatsapp(string to, string fullName, string Id, string amount, string Quantity)
        {

            //string msg = "Successfully register for Shree Group Yashotsav Garba 2023\n"
            //  + "Ref No :" + Id + "\n" + "Aadhar: " + aadharcardNumber + "\n"
            //  + "\nCollect physical pass from office\n"
            //  + "Helpline: 9723555960";
            //var msg = "Test1";
            string msg = "Your General Pass Details:\n"
              + "Ref No :" + Id + "\n" + "Quanity:" + Quantity + "\n"
              + "\nCollect pass from Shree Group office\n"
              + "Office Address : \n Shree Group Umreth\n Vraj Arcade, Nr. Champaklal Ghabhawala Gate\n Opp.Tarunimata Temple, Umreth"
              + "Helpline: 9723555960";

            var jsonUrl = "https://login.bulksmsgateway.in/textmobilesmsapi.php?user=shreegroup&password=Yash6595@&mobile=" + to + "&message=" + msg + "&type=203";
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


        public string sendSMS(string to, string fullName, string Id, string amount, string Quantity)
        {

            //string _SMSkey = "gyg8zeu70ogeydt";
            //string _SMSsecret = "gsa7x0l6b8mfxn4";
            //string msg = "Registration has been successfully completed for Shree Group Yashotsav Garba 2023.\n\n" + "Open Pass Link\n" + QRCodelink;
            //string msg = "Registration has been successfully completed for Shree Group Yashotsav Garba 2023.\n\n"
            //    + "PLease Collect your physical pass after 10th October from office Address \n"
            //    + "Reference No. :- " + Id + "\n" + "Aadhar No. :- " + aadharcardNumber + "\n"
            //    + "Name :- " + fullName
            //    +"\n Shree Group Umreth\n Vraj Arcade, Nr. Champaklal Ghabhawala Gate\n Opp.Tarunimata Temple, Umreth";
            string msg = "Your General Pass Details:\n"
              + "Ref No :" + Id + "\n" + "Quanity:" + Quantity + "\n"
              + "\nCollect pass from Shree Group office\n"
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
    }
}
