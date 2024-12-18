using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FundraisinApp_IntegrationPlugin.FundraisinApp_Integration.Plugins.Entities.Integration_Data_Model
{
    public class TicketHolderModelMap : ClassMap<TicketHolderModel>
    {
        public TicketHolderModelMap()
        {
            Map(m => m.total_records).Name("total_records");
            Map(m => m.guest_id).Name("guest_id");
            Map(m => m.member_id).Name("member_id");
            Map(m => m.event_id).Name("event_id");
            Map(m => m.charity_id).Name("charity_id");
            Map(m => m.history_id).Name("history_id");
            Map(m => m.ticket_id).Name("ticket_id");
            Map(m => m.wave_id).Name("wave_id");
            Map(m => m.table_id).Name("table_id");
            Map(m => m.related_member_id).Name("related_member_id");
            Map(m => m.related_history_id).Name("related_history_id");
            Map(m => m.bib_number).Name("bib_number");
            Map(m => m.g_fname).Name("g_fname");
            Map(m => m.g_lname).Name("g_lname");
            Map(m => m.g_lname_prefix).Name("g_lname_prefix");
            Map(m => m.g_email).Name("g_email");
            Map(m => m.g_phone_suffix).Name("g_phone_suffix");
            Map(m => m.g_phone).Name("g_phone");
            Map(m => m.g_notes).Name("g_notes");
            Map(m => m.g_company).Name("g_company");
            Map(m => m.g_gender).Name("g_gender");
            Map(m => m.g_dob).Name("g_dob");
            Map(m => m.g_shirt_size).Name("g_shirt_size");
            Map(m => m.g_emergency_contact).Name("g_emergency_contact");
            Map(m => m.g_emergency_contact_alt).Name("g_emergency_contact_alt");
            Map(m => m.g_emergency_phone).Name("g_emergency_phone");
            Map(m => m.g_emergency_contact_type).Name("g_emergency_contact_type");
            Map(m => m.g_address_unit).Name("g_address_unit");
            Map(m => m.g_address_number).Name("g_address_number");
            Map(m => m.g_address_street).Name("g_address_street");
            Map(m => m.g_address_2).Name("g_address_2");
            Map(m => m.g_address_suburb).Name("g_address_suburb");
            Map(m => m.g_address_pcode).Name("g_address_pcode");
            Map(m => m.g_address_state).Name("g_address_state");
            Map(m => m.g_address_country).Name("g_address_country");
            Map(m => m.g_kw_address).Name("g_kw_address");
            Map(m => m.is_attending).Name("is_attending");
            Map(m => m.date_attending).Name("date_attending");
            Map(m => m.crm_guest_id).Name("crm_guest_id");
            Map(m => m.ecrm_customer_id).Name("ecrm_customer_id");
            Map(m => m.ecrm_last_synced_date).Name("ecrm_last_synced_date");
            Map(m => m.last_updated).Name("last_updated");
            Map(m => m.date_created).Name("date_created");
        }
    }
}
