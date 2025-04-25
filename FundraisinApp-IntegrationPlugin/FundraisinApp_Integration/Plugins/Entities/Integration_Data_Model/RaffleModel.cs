using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FundraisinApp_IntegrationPlugin.FundraisinApp_Integration.Plugins.Entities.Integration_Data_Model
{
    public class RaffleModel
    {
        public string total_records { get; set; }
        public string raffle_id { get; set; }
        public string charity_id { get; set; }
        public string vip_id { get; set; }
        public string raffle_name { get; set; }
        public string raffle_key { get; set; }
        public string raffle_code { get; set; }
        public string ticket_start { get; set; }
        public string ticket_prefix { get; set; }
        public string entries_closed { get; set; }
        public string raffle_end_date { get; set; }
        public string raffle_end_time { get; set; }
        public string number_tickets { get; set; }
        public string max_tickets { get; set; }
        public string min_tickets { get; set; }
        public string has_tax { get; set; }
        public string allow_single_tickets { get; set; }
        public string ticket_price { get; set; }
        public string ticket_image { get; set; }
        public string raffle_short_desc { get; set; }
        public string raffle_closed_message { get; set; }
        public string raffle_thumbnail { get; set; }
        public string send_confirmation_email { get; set; }
        public string confirmation_email_template_id { get; set; }
        public string confirmation_email_subject { get; set; }
        public string confirmation_email_body { get; set; }
        public string redirect_page { get; set; }
        public string redirect_url { get; set; }
        public string event_confirmation_html { get; set; }
        public string crm_raffle_id { get; set; }
        public string sort_order { get; set; }
        public string raffle_status { get; set; }
        public string last_updated { get; set; }
        public string date_created { get; set; }
    }
}
