using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FundraisinApp_IntegrationPlugin.FundraisinApp_Integration.Plugins.Entities.Integration_Data_Model
{
    public class EventModelMap : ClassMap<EventModel>
    {
        public EventModelMap()
        {
            // Mapping the EventModel properties to the respective CSV column names
            Map(m => m.EventId).Name("event_id");
            Map(m => m.EventName).Name("event_name");
            Map(m => m.EventType).Name("event_type");
            Map(m => m.EventFee).Name("event_fee");
            Map(m => m.EventAboutInfo).Name("event_about_info");
            Map(m => m.EventWaiver).Name("event_waiver");
            Map(m => m.EventTarget).Name("event_target");
            // Add additional fields as needed...
        }
    }
}
