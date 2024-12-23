using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FundraisinApp_IntegrationPlugin.FundraisinApp_Integration.Plugins.Entities.Integration_Data_Model
{
    public class ProductModelMap : ClassMap<ProductModel>
    {
        public ProductModelMap()
        {
            Map(m => m.total_records).Name("total_records");
            Map(m => m.product_id).Name("product_id");
            Map(m => m.category_id).Name("category_id");
            Map(m => m.charity_id).Name("charity_id");
            Map(m => m.product_name).Name("product_name");
            Map(m => m.product_type).Name("product_type");
            Map(m => m.product_tags).Name("product_tags");
            Map(m => m.product_slug).Name("product_slug");
            Map(m => m.product_category).Name("product_category");
            Map(m => m.product_code).Name("product_code");
            Map(m => m.product_image).Name("product_image");
            Map(m => m.product_image_large).Name("product_image_large");
            Map(m => m.product_cost).Name("product_cost");
            Map(m => m.product_price).Name("product_price");
            Map(m => m.is_donation).Name("is_donation");
            Map(m => m.product_attachment).Name("product_attachment");
            Map(m => m.product_attachment_name).Name("product_attachment_name");
            Map(m => m.send_email).Name("send_email");
            Map(m => m.product_email_template_id).Name("product_email_template_id");
            Map(m => m.product_email_subject).Name("product_email_subject");
            Map(m => m.product_email_body).Name("product_email_body");
            Map(m => m.ecard_attachment).Name("ecard_attachment");
            Map(m => m.ecard_attachment_name).Name("ecard_attachment_name");
            Map(m => m.card_option_email_card).Name("card_option_email_card");
            Map(m => m.card_option_no_card).Name("card_option_no_card");
            Map(m => m.card_option_download_card_email_required).Name("card_option_download_card_email_required");
            Map(m => m.card_option_download_card).Name("card_option_download_card");
            Map(m => m.card_option_send_later).Name("card_option_send_later");
            Map(m => m.ecard_send_email).Name("ecard_send_email");
            Map(m => m.ecard_email_template_id).Name("ecard_email_template_id");
            Map(m => m.ecard_email_subject).Name("ecard_email_subject");
            Map(m => m.ecard_email_body).Name("ecard_email_body");
            Map(m => m.product_description).Name("product_description");
            Map(m => m.product_description_long).Name("product_description_long");
            Map(m => m.product_delivery_price).Name("product_delivery_price");
            Map(m => m.free_delivery).Name("free_delivery");
            Map(m => m.delivery_type).Name("delivery_type");
            Map(m => m.product_stock).Name("product_stock");
            Map(m => m.min_buy_limit).Name("min_buy_limit");
            Map(m => m.max_buy_limit).Name("max_buy_limit");
            Map(m => m.gst_free).Name("gst_free");
            Map(m => m.product_status).Name("product_status");
            Map(m => m.linked_to).Name("linked_to");
            Map(m => m.is_featured).Name("is_featured");
            Map(m => m.sort_order).Name("sort_order");
            Map(m => m.crm_product_id).Name("crm_product_id");
            Map(m => m.shopify_product_id).Name("shopify_product_id");
            Map(m => m.dynamic_pdf_attachment).Name("dynamic_pdf_attachment");
            Map(m => m.last_updated).Name("last_updated");
            Map(m => m.date_created).Name("date_created");
        }
    }
}
