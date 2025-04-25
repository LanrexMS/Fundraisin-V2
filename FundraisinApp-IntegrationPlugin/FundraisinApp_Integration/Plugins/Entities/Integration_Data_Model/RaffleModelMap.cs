using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FundraisinApp_IntegrationPlugin.FundraisinApp_Integration.Plugins.Entities.Integration_Data_Model
{
    public class RaffleModelMap : ClassMap<RaffleModel>
    {
        public RaffleModelMap()
        {
            Map(m => m.total_records).Name("total_records");
            Map(m => m.raffle_id).Name("raffle_id");
            Map(m => m.charity_id).Name("charity_id");
            Map(m => m.vip_id).Name("vip_id");
            Map(m => m.raffle_name).Name("raffle_name");
            Map(m => m.raffle_key).Name("raffle_key");
            Map(m => m.raffle_code).Name("raffle_code");
            Map(m => m.ticket_start).Name("ticket_start");
            Map(m => m.ticket_prefix).Name("ticket_prefix");
            Map(m => m.entries_closed).Name("entries_closed");
            Map(m => m.raffle_end_date).Name("raffle_end_date");
            Map(m => m.raffle_end_time).Name("raffle_end_time");
            Map(m => m.number_tickets).Name("number_tickets");
            Map(m => m.max_tickets).Name("max_tickets");
            Map(m => m.min_tickets).Name("min_tickets");
            Map(m => m.has_tax).Name("has_tax");
            Map(m => m.allow_single_tickets).Name("allow_single_tickets");
            Map(m => m.ticket_price).Name("ticket_price");
            Map(m => m.ticket_image).Name("ticket_image");
            Map(m => m.raffle_short_desc).Name("raffle_short_desc");
            Map(m => m.raffle_closed_message).Name("raffle_closed_message");
            Map(m => m.raffle_thumbnail).Name("raffle_thumbnail");
            Map(m => m.send_confirmation_email).Name("send_confirmation_email");
            Map(m => m.confirmation_email_template_id).Name("confirmation_email_template_id");
            Map(m => m.confirmation_email_subject).Name("confirmation_email_subject");
            Map(m => m.confirmation_email_body).Name("confirmation_email_body");
            Map(m => m.redirect_page).Name("redirect_page");
            Map(m => m.redirect_url).Name("redirect_url");
            Map(m => m.event_confirmation_html).Name("event_confirmation_html");
            Map(m => m.crm_raffle_id).Name("crm_raffle_id");
            Map(m => m.sort_order).Name("sort_order");
            Map(m => m.raffle_status).Name("raffle_status");
            Map(m => m.last_updated).Name("last_updated");
            Map(m => m.date_created).Name("date_created");
        }
    }
}
