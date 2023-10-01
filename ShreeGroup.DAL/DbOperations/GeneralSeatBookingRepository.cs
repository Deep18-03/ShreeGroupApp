using ShreeGroup.DAL.Database;
using ShreeGroup.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShreeGroup.DAL.DbOperations
{
    public class GeneralSeatBookingRepository
    {
        public long AddGeneralPassSeat(GeneralSeatBookingModel model)
        {
            using (var context = new ShreeGroupUmrethEntities())
            {

                GeneralSeatBooking seat = new GeneralSeatBooking()
                {
                    FirstName = model.FirstName,
                    //LastName = model.LastName,
                    MobileNumber = model.MobileNumber,
                    IsPaid = false,
                    InsertDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time")),
                    TotalAmount = model.TotalAmount,
                    Quantity = model.Quantity
                    //TransactionId =model.TransactionId
                };

                context.GeneralSeatBooking.Add(seat);
                context.SaveChanges();

                return seat.Id;
            }
        }

        public GeneralSeatBooking UpdateSeatPaymentFlag(long id, string orderId, bool isPaymentDone)
        {
            using (var context = new ShreeGroupUmrethEntities())
            {
                GeneralSeatBooking seat = context.GeneralSeatBooking.Where(x => x.Id == id).FirstOrDefault();
                seat.IsPaid = isPaymentDone;
                seat.BookingId = "BSGU00" + id;
                seat.UpdateDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
                seat.TransactionId = orderId;

                context.SaveChanges();

                return seat;
            }
        }
    }
}
