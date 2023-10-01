using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShreeGroup.Models
{
    public class UserModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Please enter user name")]
        public string UserName { get; set; }
        [Required(ErrorMessage = "Please enter password")]
        public string Password { get; set; }
    }
}
