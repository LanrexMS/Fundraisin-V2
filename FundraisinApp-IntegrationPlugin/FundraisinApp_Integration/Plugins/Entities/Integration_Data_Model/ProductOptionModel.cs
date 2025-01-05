using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FundraisinApp_IntegrationPlugin.FundraisinApp_Integration.Plugins.Entities.Integration_Data_Model
{
    public class ProductOptionModel
    {
        public string total_records { get; set; }
        public string option_id { get; set; }
        public string product_id { get; set; }
        public string option_name { get; set; }
        public string option_type { get; set; }
        public string option_code { get; set; }
        public string option_price { get; set; }
        public string option_image { get; set; }
        public string option_image_large { get; set; }
        public string option_stock { get; set; }
        public string option_delivery { get; set; }
        public string sort_order { get; set; }
        public string option_status { get; set; }
        public string shopify_option_id { get; set; }
        public string last_updated { get; set; }
        public string date_created { get; set; }
    }
}
