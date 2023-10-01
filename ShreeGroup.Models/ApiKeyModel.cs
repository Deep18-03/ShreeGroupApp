using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShreeGroup.Models
{
    public class ApiKeyModel
    {
        public int Id { get; set; }
        public string RazorKey { get; set; }
        public string RazorSecret { get; set; }
        public string SMSKey { get; set; }
        public string SMSSecret { get; set; }
        public string SMSMessage { get; set; }
        public decimal Amount { get; set; }
        public int? APINumber { get; set; }
    }
}
