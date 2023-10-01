using ShreeGroup.DAL.Database;
using ShreeGroup.Models;
using System.Linq;

namespace ShreeGroup.DAL.DbOperations
{
    public class KeyRepository
    {
        public ApiKeyModel GetApiDetails()
        {
            using (var context = new ShreeGroupUmrethEntities())
            {
                var result = context.APIDetails.Where(x => x.APINumber == 100).Select(x => new ApiKeyModel()
                {
                    Id = x.Id,
                    APINumber = x.APINumber,
                    Amount=x.Amount,
                    RazorKey=x.RazorKey,
                    RazorSecret=x.RazorSecret,
                    SMSKey=x.SMSKey,
                    SMSSecret=x.SMSSecret,
                    SMSMessage=x.SMSMessage
                }).FirstOrDefault();

                return result;

            }
        }
    }
}
