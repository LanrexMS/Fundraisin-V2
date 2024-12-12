using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FundraisinApp_IntegrationPlugin.FundraisinApp_Integration.Plugins.Entities.Integration_Data_Model
{
    public class TicketsModel
    {
        public string total_records { get; set; }
        public string ticket_id { get; set; }
        public string event_id { get; set; }
        public string member_type_id { get; set; }
        public string ticket_name { get; set; }
        public string ticket_code { get; set; }
        public string ticket_category { get; set; }
        public string ticket_date { get; set; }
        public string ticket_price { get; set; }
        public string ticket_price_early { get; set; }
        public string ticket_early_start { get; set; }
        public string ticket_early_end { get; set; }
        public string ticket_limit { get; set; }
        public string is_table { get; set; }
        public string allow_guests { get; set; }
        public string is_private { get; set; }
        public string num_tickets { get; set; }
        public string min_buy_limit { get; set; }
        public string max_buy_limit { get; set; }
        public string ticket_description { get; set; }
        public string ticket_image { get; set; }
        public string ticket_line_item { get; set; }
        public string is_earlybird { get; set; }
        public string ticket_available_start { get; set; }
        public string ticket_available_end { get; set; }
        public string ticket_status { get; set; }
        public string sort_order { get; set; }
        public string crm_ticket_id { get; set; }
        public string last_updated { get; set; }
        public string date_created { get; set; }        
    }
}
