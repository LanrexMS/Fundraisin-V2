using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FundraisinApp_IntegrationPlugin.FundraisinApp_Integration.Plugins.Entities.Integration_Data_Model
{
    public class PromoCodeModel
    {
        public string total_records { get; set; }
        public string promo_id { get; set; }
        public string promo_code { get; set; }
        public string promo_code_type { get; set; }
        public string promo_type { get; set; }
        public string promo_payment_type { get; set; }
        public string event_id { get; set; }
        public string member_id { get; set; }
        public string team_id { get; set; }
        public string promo_value { get; set; }
        public string promo_value_type { get; set; }
        public string promo_entries_limit { get; set; }
        public string promo_status { get; set; }
        public string date_created { get; set; }
    }
}
