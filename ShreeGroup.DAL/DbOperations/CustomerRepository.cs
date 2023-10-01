using ShreeGroup.DAL.Database;
using ShreeGroup.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ShreeGroup.DAL.DbOperations
{
    public class CustomerRepository
    {

        public Customer UpdateCustomerPaymentFlag(string aadharCard , string orderId,bool isPaymentDone)
        {
            using (var context = new ShreeGroupUmrethEntities())
            {
                Customer customer = context.Customer.Where(x => x.AadharCard == aadharCard).FirstOrDefault();
                customer.IsPaymentDone = isPaymentDone;
                customer.TransactionId = orderId;

                context.SaveChanges();

                return customer;
            }
        }

        public Customer UpdateCustomerdetail(RegistrationModel registration)
        {
            using (var context = new ShreeGroupUmrethEntities())
            {
                Customer customer = context.Customer.Where(x => x.AadharCard == registration.AadharCard).FirstOrDefault();
                customer.District = registration.District;

                context.SaveChanges();

                return customer;
            }
        }

        public int AddCustomer(RegistrationModel model)
        {
            using (var context = new ShreeGroupUmrethEntities())
            {

                Customer customer = new Customer()
                {
                    FirstName = model.FirstName,
                    MiddleName = model.MiddleName,
                    LastName = model.LastName,
                    Email = model.Email,
                    Address = model.Address,
                    PinCode = model.PinCode,
                    City = model.City,
                    District = model.District,
                    PhoneNumber = model.PhoneNumber,
                    DateOfBirth = model.DateOfBirth.ToString(),
                    Age = model.Age,
                    ProfilePicture = model.ProfilePicture,
                    AadharCard = model.AadharCard,
                    RegistrationCode = model.RegistrationCode,
                    QRCode = model.QRCode,
                    UrlCode = model.UrlCode,
                    AadharCardImage = model.AadharCardImage,
                    BloodGroup = model.BloodGroup,
                    DateAdded = System.DateTime.Now,
                    IsPaymentDone = model.IsPaymentDone,
                    //TransactionId =model.TransactionId
                };

                context.Customer.Add(customer);
                context.SaveChanges();
                
                return customer.Id;
            }
        }

        public string GetMaxUniqueId()
        {
            using (var context = new ShreeGroupUmrethEntities())
            {
                var customerCount = context.Customer.Count();
                if (customerCount > 0)
                {
                    var maxId = context.Customer.OrderByDescending(x=>x.Id).Select(x=>x.District).FirstOrDefault();
                    var nextMaxId = Convert.ToInt64(maxId) + 1;
                    return nextMaxId.ToString();
                }
                else
                {
                    return "1";
                }
            }   
        }
        public bool isAadharCardAlreadyExists(string adharCard)
        {
            using (var context = new ShreeGroupUmrethEntities())
            {
                var isAadharCardAlreadyExists = context.Customer.Any(x => x.AadharCard == adharCard);
                if (isAadharCardAlreadyExists)
                {
                    return true;
                }
                return false;
            }
        }

        public List<RegistrationModel> GetAllCustomer(string searching)
        {
            using (var context = new ShreeGroupUmrethEntities())
            {
                var result = context.Customer.Where(x=>x.AadharCard == searching && x.IsPaymentDone == true).Select(x => new RegistrationModel()
                {
                    Id = x.Id,
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    AadharCard = x.AadharCard,
                    Email = x.Email,
                    PhoneNumber = x.PhoneNumber,
                    RegistrationCode = x.RegistrationCode,
                    Age = x.Age,
                    UrlCode=x.UrlCode,
                    QRCode=x.QRCode
                }).ToList();

                return result;

            }
        }

        public List<RegisterCustomer> GetAllCustomerAdmin(string searching,string passType)
        {
            using (var context = new ShreeGroupUmrethEntities())
            {
                var result = context.Customer.Where(x => (x.AadharCard == searching || x.PhoneNumber.Contains(searching)) && x.District.Contains(passType) && x.IsPaymentDone == true).Select(x => new RegisterCustomer()
                {
                    Id = x.Id,
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    AadharCard = x.AadharCard,
                    Email = x.Email,
                    PhoneNumber = x.PhoneNumber,
                    RegistrationCode = x.RegistrationCode,
                    Age = x.Age,
                    UrlCode = x.UrlCode,
                    QRCode = x.QRCode,
                    IsVaccinated =x.isVaccinated,
                    District = x.District   
                }).ToList();

                return result;

            }
        }


        public List<RegisterCustomer> GetAllCustomer()
        {
            using (var context = new ShreeGroupUmrethEntities())
            {
                var result = context.Customer.Where(x=> ( x.isVaccinated == null || x.isVaccinated == false ) &&  x.IsPaymentDone == true ).Select(x => new RegisterCustomer()
                {
                    Id = x.Id,
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    AadharCard = x.AadharCard,
                    Email = x.Email,
                    PhoneNumber = x.PhoneNumber,
                    RegistrationCode = x.RegistrationCode,
                    Age = x.Age,
                    UrlCode = x.UrlCode,
                    QRCode = x.QRCode,
                    IsVaccinated = x.isVaccinated,
                    District = x.District
                }).OrderByDescending(x=>x.Id).ToList();

                return result;

            }
        }

        public List<GeneralSeatBookingModel> GetGeneralSeatBookings()
        {
            using (var context = new ShreeGroupUmrethEntities())
            {
                var result = context.GeneralSeatBooking.Where(x => x.IsPaid == true).Select(x => new GeneralSeatBookingModel()
                {
                    Id = x.Id,
                    FirstName = x.FirstName,
                    BookingId = x.BookingId,
                    MobileNumber = x.MobileNumber,
                    Quantity = x.Quantity,
                    TotalAmount = x.TotalAmount
                }).OrderByDescending(x => x.Id).ToList();

                return result;

            }
        }

        public List<RegisterCustomer> GetFreeAdminCustomer()
        {
            using (var context = new ShreeGroupUmrethEntities())
            {
                var result = context.Customer.Where(x => x.isVaccinated == true && x.IsPaymentDone == false).Select(x => new RegisterCustomer()
                {
                    Id = x.Id,
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    AadharCard = x.AadharCard,
                    Email = x.Email,
                    PhoneNumber = x.PhoneNumber,
                    RegistrationCode = x.RegistrationCode,
                    Age = x.Age,
                    UrlCode = x.UrlCode,
                    QRCode = x.QRCode,
                    IsVaccinated = x.isVaccinated
                }).OrderByDescending(x => x.Id).ToList();

                return result;

            }
        }


        public RegistrationModel GetCustomer(string id)
        {
            using (var context = new ShreeGroupUmrethEntities())
            {
                var result = context.Customer.Where(x => x.UrlCode == id).Select(x => new RegistrationModel()
                {
                    Id = x.Id,
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    AadharCard = x.AadharCard,
                    Email = x.Email,
                    PhoneNumber = x.PhoneNumber,
                    RegistrationCode = x.RegistrationCode,
                    Age = x.Age,
                    ProfilePicture=x.ProfilePicture,
                    QRCode =x.QRCode,
                    UrlCode=x.UrlCode,
                    BloodGroup = x.BloodGroup,
                    IsVaccinated = x.isVaccinated == null || x.isVaccinated == false ? false : true,
                    IsPaymentDone = x.IsPaymentDone == null || x.IsPaymentDone == false ? false : true
                }).FirstOrDefault();

                return result;

            }
        }

        public RegistrationModel GetCustomerByAadharCard(string aadaharCard)
        {
            using (var context = new ShreeGroupUmrethEntities())
            {
                var result = context.Customer.Where(x => x.AadharCard == aadaharCard).Select(x => new RegistrationModel()
                {
                    Id = x.Id,
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    AadharCard = x.AadharCard,
                    Email = x.Email,
                    PhoneNumber = x.PhoneNumber,
                    RegistrationCode = x.RegistrationCode,
                    Age = x.Age,
                    ProfilePicture = x.ProfilePicture,
                    QRCode = x.QRCode,
                    UrlCode = x.UrlCode,
                    BloodGroup = x.BloodGroup,
                    IsVaccinated = x.isVaccinated == null || x.isVaccinated == false ? false : true,
                    IsPaymentDone = x.IsPaymentDone == null || x.IsPaymentDone == false ? false : true,
                    City=x.City,
                    District= x.District,
                    PinCode = x.PinCode
                }).FirstOrDefault();

                return result;

            }
        }

        public int AddCustomer(RegisterCustomer model)
        {
            using (var context = new ShreeGroupUmrethEntities())
            {

                Customer customer = new Customer()
                {
                    FirstName = model.FirstName,
                    MiddleName = model.MiddleName,
                    LastName = model.LastName,
                    Email = model.Email,
                    Address = model.Address,
                    PinCode = model.PinCode,
                    City = model.City,
                    District = model.District,
                    PhoneNumber = model.PhoneNumber,
                    DateOfBirth = model.DateOfBirth.ToString(),
                    Age = model.Age,
                    ProfilePicture = model.ProfilePicture,
                    AadharCard = model.AadharCard,
                    RegistrationCode = model.RegistrationCode,
                    QRCode = model.QRCode,
                    UrlCode = model.UrlCode,
                    AadharCardImage = model.AadharCardImage,
                    BloodGroup = model.BloodGroup,
                    DateAdded = System.DateTime.Now,
                    isVaccinated = model.IsVaccinated,
                    IsPaymentDone = !model.IsVaccinated

                };

                context.Customer.Add(customer);
                context.SaveChanges();

                return customer.Id;
            }
        }

        public int AddCashCustomer(RegisterCashCustomer model)
        {
            using (var context = new ShreeGroupUmrethEntities())
            {

                Customer customer = new Customer()
                {
                    FirstName = model.FirstName,
                    MiddleName = model.MiddleName,
                    LastName = model.LastName,
                    Email = (bool)model.IsVaccinated ? "Cash" : "Default",
                    PhoneNumber = model.PhoneNumber,
                    ProfilePicture = model.ProfilePicture,
                    AadharCard = model.AadharCard,
                    RegistrationCode = model.RegistrationCode,
                    QRCode = model.QRCode,
                    UrlCode = model.UrlCode,
                    AadharCardImage = model.AadharCardImage,
                    DateAdded = System.DateTime.Now,
                    BloodGroup = "A+",
                    isVaccinated = false,
                    IsPaymentDone = true
                };

                context.Customer.Add(customer);
                context.SaveChanges();
                
                return customer.Id;
            }
        }


        public int GetTotalCustomerCount()
        {
            using (var context = new ShreeGroupUmrethEntities())
            {
                var result = context.Customer.Where(x=>x.IsPaymentDone == true || x.isVaccinated == true).Count();

                return result;

            }
        }

    }
}
