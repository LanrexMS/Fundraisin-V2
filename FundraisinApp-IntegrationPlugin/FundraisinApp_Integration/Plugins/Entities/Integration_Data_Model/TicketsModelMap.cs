using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FundraisinApp_IntegrationPlugin.FundraisinApp_Integration.Plugins.Entities.Integration_Data_Model
{
    public class TicketsModelMap : ClassMap<TicketsModel>
    {
        public TicketsModelMap()
        {
            Map(m => m.total_records).Name("total_records");
            Map(m => m.ticket_id).Name("ticket_id");
            Map(m => m.event_id).Name("event_id");
            Map(m => m.member_type_id).Name("member_type_id");
            Map(m => m.ticket_name).Name("ticket_name");
            Map(m => m.ticket_code).Name("ticket_code");
            Map(m => m.ticket_category).Name("ticket_category");
            Map(m => m.ticket_date).Name("ticket_date");
            Map(m => m.ticket_price).Name("ticket_price");
            Map(m => m.ticket_price_early).Name("ticket_price_early");
            Map(m => m.ticket_early_start).Name("ticket_early_start");
            Map(m => m.ticket_early_end).Name("ticket_early_end");
            Map(m => m.ticket_limit).Name("ticket_limit");
            Map(m => m.is_table).Name("is_table");
            Map(m => m.allow_guests).Name("allow_guests");
            Map(m => m.is_private).Name("is_private");
            Map(m => m.num_tickets).Name("num_tickets");
            Map(m => m.min_buy_limit).Name("min_buy_limit");
            Map(m => m.max_buy_limit).Name("max_buy_limit");
            Map(m => m.ticket_description).Name("ticket_description");
            Map(m => m.ticket_image).Name("ticket_image");
            Map(m => m.ticket_line_item).Name("ticket_line_item");
            Map(m => m.is_earlybird).Name("is_earlybird");
            Map(m => m.ticket_available_start).Name("ticket_available_start");
            Map(m => m.ticket_available_end).Name("ticket_available_end");
            Map(m => m.ticket_status).Name("ticket_status");
            Map(m => m.sort_order).Name("sort_order");
            Map(m => m.crm_ticket_id).Name("crm_ticket_id");
            Map(m => m.last_updated).Name("last_updated");
            Map(m => m.date_created).Name("date_created");
        }
    }
}
