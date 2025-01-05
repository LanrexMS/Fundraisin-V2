using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FundraisinApp_IntegrationPlugin.FundraisinApp_Integration.Plugins.Entities.Integration_Data_Model
{
    public class ProductOptionModelMap : ClassMap<ProductOptionModel>
    {
        public ProductOptionModelMap()
        {
            Map(m => m.total_records).Name("total_records");
            Map(m => m.option_id).Name("option_id");
            Map(m => m.product_id).Name("product_id");
            Map(m => m.option_name).Name("option_name");
            Map(m => m.option_type).Name("option_type");
            Map(m => m.option_code).Name("option_code");
            Map(m => m.option_price).Name("option_price");
            Map(m => m.option_image).Name("option_image");
            Map(m => m.option_image_large).Name("option_image_large");
            Map(m => m.option_stock).Name("option_stock");
            Map(m => m.option_delivery).Name("option_delivery");
            Map(m => m.sort_order).Name("sort_order");
            Map(m => m.option_status).Name("option_status");
            Map(m => m.shopify_option_id).Name("shopify_option_id");
            Map(m => m.last_updated).Name("last_updated");
            Map(m => m.date_created).Name("date_created");
        }
    }
}
