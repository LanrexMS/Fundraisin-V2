using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FundraisinApp_IntegrationPlugin.FundraisinApp_Integration.Plugins.Entities.Integration_Data_Model
{
    public class ParticipantOptionModelMap : ClassMap<ParticipantOptionModel>
    {
        public ParticipantOptionModelMap()
        {
            Map(m => m.id).Name("id");
            Map(m => m.option_type).Name("option_type");
            Map(m => m.event_id).Name("event_id");
            Map(m => m.member_id).Name("member_id");
            Map(m => m.history_id).Name("history_id");
            Map(m => m.product_id).Name("product_id");
            Map(m => m.shop_product_id).Name("shop_product_id");
            Map(m => m.option_id).Name("option_id");
            Map(m => m.ticket_id).Name("ticket_id");
            Map(m => m.wave_id).Name("wave_id");
            Map(m => m.option_num).Name("option_num");
            Map(m => m.option_textfield).Name("option_textfield");
            Map(m => m.option_cost).Name("option_cost");
            Map(m => m.date_created).Name("date_created");
        }
    }
}
