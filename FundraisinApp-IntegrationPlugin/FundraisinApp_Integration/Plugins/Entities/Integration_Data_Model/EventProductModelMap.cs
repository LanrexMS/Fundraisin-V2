using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FundraisinApp_IntegrationPlugin.FundraisinApp_Integration.Plugins.Entities.Integration_Data_Model
{
    public class EventProductModelMap : ClassMap<EventProductModel>
    {
        public EventProductModelMap()
        {
            Map(m => m.Product_Id).Name("product_id");
            Map(m => m.Event_Id).Name("event_id");
            Map(m => m.Shop_Product_Id).Name("shop_product_id");
            Map(m => m.Product_Price).Name("product_price");
            Map(m => m.Is_Free).Name("is_free");
            Map(m => m.Is_Mandatory).Name("is_mandatory");
            Map(m => m.Sort_Order).Name("sort_order");
            Map(m => m.Product_Status).Name("product_status");
            Map(m => m.Last_Updated).Name("last_updated");
            Map(m => m.Date_Created).Name("date_created");
        }
    }
}
