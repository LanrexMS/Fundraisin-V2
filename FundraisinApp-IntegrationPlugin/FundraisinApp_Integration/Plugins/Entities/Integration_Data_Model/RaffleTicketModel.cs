using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FundraisinApp_IntegrationPlugin.FundraisinApp_Integration.Plugins.Entities.Integration_Data_Model
{
    public class RaffleTicketModel
    {
        public string total_records { get; set; }
        public string option_id { get; set; }
        public string raffle_id { get; set; }
        public string vip_id { get; set; }
        public string option_name { get; set; }
        public string option_tickets { get; set; }
        public string option_price { get; set; }
        public string option_description { get; set; }
        public string sort_order { get; set; }
        public string last_updated { get; set; }
        public string date_created { get; set; }
    }
}
