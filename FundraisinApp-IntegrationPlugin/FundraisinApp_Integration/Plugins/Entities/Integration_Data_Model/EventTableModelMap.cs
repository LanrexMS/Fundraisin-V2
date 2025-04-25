using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FundraisinApp_IntegrationPlugin.FundraisinApp_Integration.Plugins.Entities.Integration_Data_Model
{
    public class EventTableModelMap : ClassMap<EventTableModel>
    {
        public EventTableModelMap()
        {
            // Mapping the EventTableModel properties to the respective CSV column names
            Map(m => m.table_id).Name("table_id");
            Map(m => m.event_id).Name("event_id");
            Map(m => m.member_id).Name("member_id");
            Map(m => m.history_id).Name("history_id");
            Map(m => m.ticket_id).Name("ticket_id");
            Map(m => m.option_id).Name("option_id");
            Map(m => m.table_number).Name("table_number");
            Map(m => m.table_name).Name("table_name");
            Map(m => m.number_seats).Name("number_seats");
            Map(m => m.table_price).Name("table_price");
            Map(m => m.table_image).Name("table_image");
            Map(m => m.table_status).Name("table_status");
            Map(m => m.last_updated).Name("last_updated");
            Map(m => m.date_created).Name("date_created");
        }
    }
}
