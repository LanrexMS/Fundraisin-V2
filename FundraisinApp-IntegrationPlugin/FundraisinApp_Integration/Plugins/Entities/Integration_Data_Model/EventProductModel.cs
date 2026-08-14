using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FundraisinApp_IntegrationPlugin.FundraisinApp_Integration.Plugins.Entities.Integration_Data_Model
{
    public class EventProductModel
    {
        public string Product_Id { get; set; }
        public string Event_Id { get; set; }
        public string Shop_Product_Id { get; set; }
        public string Product_Price { get; set; }
        public string Is_Free { get; set; }
        public string Is_Mandatory { get; set; }
        public string Sort_Order { get; set; }
        public string Product_Status { get; set; }
        public string Last_Updated { get; set; }
        public string Date_Created { get; set; }
    }
}
