using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FundraisinApp_IntegrationPlugin.FundraisinApp_Integration.Plugins.Entities.Integration_Data_Model
{
    public class EventTableModel
    {
        public string table_id { get; set; }
        public string event_id { get; set; }
        public string member_id { get; set; }
        public string history_id { get; set; }
        public string ticket_id { get; set; }
        public string option_id { get; set; }
        public string table_number { get; set; }
        public string table_name { get; set; }
        public string number_seats { get; set; }
        public string table_price { get; set; }
        public string table_image { get; set; }
        public string table_status { get; set; }
        public string last_updated { get; set; }
        public string date_created { get; set; }
    }
}
