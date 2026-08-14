using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FundraisinApp_IntegrationPlugin.FundraisinApp_Integration.Plugins.Entities.Integration_Data_Model
{
    public class TicketHolderModel
    {
        public string total_records { get; set; }
        public string guest_id { get; set; }
        public string member_id { get; set; }
        public string event_id { get; set; }
        public string charity_id { get; set; }
        public string history_id { get; set; }
        public string ticket_id { get; set; }
        public string wave_id { get; set; }
        public string table_id { get; set; }
        public string related_member_id { get; set; }
        public string related_history_id { get; set; }
        public string bib_number { get; set; }
        public string g_fname { get; set; }
        public string g_lname { get; set; }
        public string g_lname_prefix { get; set; }
        public string g_email { get; set; }
        public string g_phone_suffix { get; set; }
        public string g_phone { get; set; }
        public string g_notes { get; set; }
        public string g_company { get; set; }
        public string g_gender { get; set; }
        public string g_dob { get; set; }
        public string g_shirt_size { get; set; }
        public string g_emergency_contact { get; set; }
        public string g_emergency_contact_alt { get; set; }
        public string g_emergency_phone { get; set; }
        public string g_emergency_contact_type { get; set; }
        public string g_address_unit { get; set; }
        public string g_address_number { get; set; }
        public string g_address_street { get; set; }
        public string g_address_2 { get; set; }
        public string g_address_suburb { get; set; }
        public string g_address_pcode { get; set; }
        public string g_address_state { get; set; }
        public string g_address_country { get; set; }
        public string g_kw_address { get; set; }
        public string is_attending { get; set; }
        public string date_attending { get; set; }
        public string crm_guest_id { get; set; }
        public string ecrm_customer_id { get; set; }
        public string ecrm_last_synced_date { get; set; }
        public string last_updated { get; set; }
        public string date_created { get; set; }

        public string g_guardian_fname { get; set; }
        public string g_guardian_lname { get; set; }
        public string g_guardian_phone { get; set; }
        public string g_guardian_email { get; set; }
        public string g_guardian_relationship { get; set; }
    }
}
