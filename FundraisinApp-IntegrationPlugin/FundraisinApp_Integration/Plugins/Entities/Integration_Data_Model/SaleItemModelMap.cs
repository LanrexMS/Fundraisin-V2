using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FundraisinApp_IntegrationPlugin.FundraisinApp_Integration.Plugins.Entities.Integration_Data_Model
{
    public class SaleItemModelMap : ClassMap<SaleItemModel>
    {
        public SaleItemModelMap()
        {
            Map(m => m.total_records).Name("total_records");
            Map(m => m.id).Name("id");
            Map(m => m.sale_id).Name("sale_id");
            Map(m => m.product_id).Name("product_id");
            Map(m => m.option_id).Name("option_id");
            Map(m => m.quantity).Name("quantity");
            Map(m => m.recipient_first_name).Name("recipient_first_name");
            Map(m => m.recipient_last_name).Name("recipient_last_name");
            Map(m => m.recipient_email).Name("recipient_email");
            Map(m => m.recipient_comments).Name("recipient_comments");
            Map(m => m.recipient_address).Name("recipient_address");
            Map(m => m.unit_cost).Name("unit_cost");
            Map(m => m.product_size).Name("product_size");
            Map(m => m.product_colour).Name("product_colour");
            Map(m => m.product_option).Name("product_option");
            Map(m => m.dynamic_pdf_attachment_url).Name("dynamic_pdf_attachment_url");
            Map(m => m.card_option).Name("card_option");
            Map(m => m.date_created).Name("date_created");
        }
    }

}
