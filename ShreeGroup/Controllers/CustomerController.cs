using QRCoder;
using ShreeGroup.DAL.DbOperations;
using System;
using System.Drawing;
using System.IO;
using System.Web.Mvc;

namespace ShreeGroup.Controllers
{
    public class CustomerController : Controller
    {
        CustomerRepository repository = null;

        public CustomerController()
        {
            repository = new CustomerRepository();
        }
        // GET: Customer
        public ActionResult GetAllPassRecords(string searching)
        {
            try
            {
                var result = repository.GetAllCustomer(searching);
                return View(result);
            }
            catch (Exception ex)
            {
                return View("Error");
            }
        }

        public ActionResult Details(string id)
        {
            if (id != null)
            {
                var customer = repository.GetCustomer(id);
                if(customer.IsPaymentDone == true)
                {
                    QRCodeGenerator QrGenerator = new QRCodeGenerator();
                    QRCodeData QrCodeInfo = QrGenerator.CreateQrCode(customer.QRCode, QRCodeGenerator.ECCLevel.Q);
                    QRCode QrCode = new QRCode(QrCodeInfo);
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (Bitmap bitmap = QrCode.GetGraphic(20))
                        {
                            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                            ViewBag.QRCodeDetailPass = "data:image/png;base64," + Convert.ToBase64String(ms.ToArray());
                        }
                    }

                    var profilePictureServerPath = Path.Combine(Server.MapPath(customer.ProfilePicture));
                    var profilePicture64 = ImageBase64(profilePictureServerPath);
                    ViewBag.imagepng = profilePicture64;
                    return View(customer);
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            return View("Error");
        }

        public ActionResult ShowAadharCard(string id)
        {
            string aadharcardPath = "~/Assets/Images/UploadedImages/AadharCard/" + id + ".jpeg";
            var aadharCardServerPath = Path.Combine(Server.MapPath(aadharcardPath));
            var aadharcardPic64 = ImageBase64(aadharCardServerPath);
            ViewBag.aadharjepg = aadharcardPic64;

            return View();
        }

        public string ImageBase64(string path)
        {
            byte[] imageArray = System.IO.File.ReadAllBytes(path);
            string base64ImageRepresentation = Convert.ToBase64String(imageArray);
            var imagePhotoBase64 = "data:image/png;base64," + base64ImageRepresentation;
            return imagePhotoBase64;
        }
    }
}