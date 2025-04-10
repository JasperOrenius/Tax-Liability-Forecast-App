using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tax_Liability_Forecast_App.Models
{
    public class Client
    {
        [Key]
        public Guid ClientID;
        public string Name;
        public string Email;
        public string PhoneNum;
    }
}
