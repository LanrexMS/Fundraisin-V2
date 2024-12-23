using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FundraisinApp_IntegrationPlugin.FundraisinApp_Integration.Plugins.Entities.Integration_Data_Model
{
    public class ProductModel
    {
        public string total_records { get; set; }
        public string product_id { get; set; }
        public string category_id { get; set; }
        public string charity_id { get; set; }
        public string product_name { get; set; }
        public string product_type { get; set; }
        public string product_tags { get; set; }
        public string product_slug { get; set; }
        public string product_category { get; set; }
        public string product_code { get; set; }
        public string product_image { get; set; }
        public string product_image_large { get; set; }
        public string product_cost { get; set; }
        public string product_price { get; set; }
        public string is_donation { get; set; }
        public string product_attachment { get; set; }
        public string product_attachment_name { get; set; }
        public string send_email { get; set; }
        public string product_email_template_id { get; set; }
        public string product_email_subject { get; set; }
        public string product_email_body { get; set; }
        public string ecard_attachment { get; set; }
        public string ecard_attachment_name { get; set; }
        public string card_option_email_card { get; set; }
        public string card_option_no_card { get; set; }
        public string card_option_download_card_email_required { get; set; }
        public string card_option_download_card { get; set; }
        public string card_option_send_later { get; set; }
        public string ecard_send_email { get; set; }
        public string ecard_email_template_id { get; set; }
        public string ecard_email_subject { get; set; }
        public string ecard_email_body { get; set; }
        public string product_description { get; set; }
        public string product_description_long { get; set; }
        public string product_delivery_price { get; set; }
        public string free_delivery { get; set; }
        public string delivery_type { get; set; }
        public string product_stock { get; set; }
        public string min_buy_limit { get; set; }
        public string max_buy_limit { get; set; }
        public string gst_free { get; set; }
        public string product_status { get; set; }
        public string linked_to { get; set; }
        public string is_featured { get; set; }
        public string sort_order { get; set; }
        public string crm_product_id { get; set; }
        public string shopify_product_id { get; set; }
        public string dynamic_pdf_attachment { get; set; }
        public string last_updated { get; set; }
        public string date_created { get; set; }
    }
}
