using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FundraisinApp_IntegrationPlugin.FundraisinApp_Integration.Plugins.Entities.Integration_Data_Model
{
    public class RaffleTicketModelMap : ClassMap<RaffleTicketModel>
    {
        public RaffleTicketModelMap()
        {
            Map(m => m.total_records).Name("total_records");
            Map(m => m.option_id).Name("option_id");
            Map(m => m.raffle_id).Name("raffle_id");
            Map(m => m.vip_id).Name("vip_id");
            Map(m => m.option_name).Name("option_name");
            Map(m => m.option_tickets).Name("option_tickets");
            Map(m => m.option_price).Name("option_price");
            Map(m => m.option_description).Name("option_description");
            Map(m => m.sort_order).Name("sort_order");
            Map(m => m.last_updated).Name("last_updated");
            Map(m => m.date_created).Name("date_created");
        }
    }
}
