using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tax_Liability_Forecast_App.Models
{
    public class Client
    {
        private int clientID;
        private string name;

        public int ClientID { get { return clientID; } set { clientID = value; } }
        public string Name { get { return name; } set { name = value; } }

        //public int ClientID { get; set; }
        //public string Name { get; set; }
    }
}
