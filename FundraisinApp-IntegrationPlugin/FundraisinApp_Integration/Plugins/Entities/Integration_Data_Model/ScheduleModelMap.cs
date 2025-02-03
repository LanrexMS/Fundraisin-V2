using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FundraisinApp_IntegrationPlugin.FundraisinApp_Integration.Plugins.Entities.Integration_Data_Model
{
    public class ScheduleModelMap : ClassMap<ScheduleModel>
    {
        public ScheduleModelMap()
        {
            Map(m => m.total_records).Name("total_records");
            Map(m => m.ScheduleId).Name("id");
            Map(m => m.donation_id).Name("donation_id");
            Map(m => m.d_amount).Name("d_amount");
            Map(m => m.d_fee).Name("d_fee");
            Map(m => m.donation_frequency).Name("donation_frequency");
            Map(m => m.donation_period).Name("donation_period");
            Map(m => m.donation_day).Name("donation_day");
            Map(m => m.start_date).Name("start_date");
            Map(m => m.card_expiry).Name("card_expiry");
            Map(m => m.last_attempt_date).Name("last_attempt_date");
            Map(m => m.donation_status).Name("donation_status");
            Map(m => m.crm_schedule_id).Name("crm_schedule_id");
            Map(m => m.last_updated).Name("last_updated");
            Map(m => m.date_created).Name("date_created");
        }
    }
}
