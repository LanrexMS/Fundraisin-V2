using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FundraisinApp_IntegrationPlugin.FundraisinApp_Integration.Plugins.Entities.Integration_Data_Model
{
    public class SaleItemModel
    {
        public string total_records { get; set; }
        public string id { get; set; }
        public string sale_id { get; set; }
        public string product_id { get; set; }
        public string option_id { get; set; }
        public string quantity { get; set; }
        public string recipient_first_name { get; set; }
        public string recipient_last_name { get; set; }
        public string recipient_email { get; set; }
        public string recipient_comments { get; set; }
        public string recipient_address { get; set; }
        public string unit_cost { get; set; }
        public string product_size { get; set; }
        public string product_colour { get; set; }
        public string product_option { get; set; }
        public string dynamic_pdf_attachment_url { get; set; }
        public string card_option { get; set; }
        public string date_created { get; set; }
    }

}
