using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FundraisinApp_IntegrationPlugin.FundraisinApp_Integration.Plugins.Entities.Integration_Data_Model
{
    public class ParticipantEventModelMap : ClassMap<ParticipantEventModel>
    {
        public ParticipantEventModelMap()
        {
            // Mapping ParticipantEventModel properties to respective CSV column names
            Map(m => m.Total_Records).Name("total_records");
            Map(m => m.History_Id).Name("history_id");
            Map(m => m.Member_Id).Name("member_id");
            Map(m => m.Event_Id).Name("event_id");
            Map(m => m.Org_Id).Name("org_id");
            Map(m => m.Team_Id).Name("team_id");
            Map(m => m.Charity_Id).Name("charity_id");
            Map(m => m.Group_Id).Name("group_id");
            Map(m => m.Table_Id).Name("table_id");
            Map(m => m.Wave_Id).Name("wave_id");
            Map(m => m.Ticket_Id).Name("ticket_id");
            Map(m => m.Invited_Member_Id).Name("invited_member_id");
            Map(m => m.Paid_Member_Id).Name("paid_member_id");
            Map(m => m.Managed_Member_Id).Name("managed_member_id");
            Map(m => m.History_Type).Name("history_type");
            Map(m => m.Member_Type).Name("member_type");
            Map(m => m.Bib_Number).Name("bib_number");
            Map(m => m.Seat_Number).Name("seat_number");
            Map(m => m.Is_Active).Name("is_active");
            Map(m => m.Promo_Id).Name("promo_id");
            Map(m => m.Is_Paid).Name("is_paid");
            Map(m => m.Total_Paid_Entry).Name("total_paid_entry");
        }
    }

}
