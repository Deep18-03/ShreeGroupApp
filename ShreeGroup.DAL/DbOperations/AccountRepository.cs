using ShreeGroup.DAL.Database;
using ShreeGroup.Models;
using System.Linq;

namespace ShreeGroup.DAL.DbOperations
{
    public class AccountRepository
    {
        public string AddUser(UserModel model)
        {
            using (var context = new ShreeGroupUmrethEntities())
            {
                User user = new User()
                {
                    UserName = model.UserName,
                    Password = model.Password
                };

                context.User.Add(user);
                context.SaveChanges();
                return "SuccessFully added new user";
            }
        }

        public bool LoginUser(UserModel model)
        {
            using (var context = new ShreeGroupUmrethEntities())
            {
                bool isValid = context.User.Any(x => x.UserName == model.UserName && x.Password == model.Password);
                if (isValid)
                {
                    return true;
                }
                return false;
            }
        }
    }
}
