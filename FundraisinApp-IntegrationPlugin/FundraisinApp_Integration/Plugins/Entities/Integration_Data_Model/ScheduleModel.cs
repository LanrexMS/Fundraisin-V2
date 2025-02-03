using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FundraisinApp_IntegrationPlugin.FundraisinApp_Integration.Plugins.Entities.Integration_Data_Model
{
    public class ScheduleModel
    {
        public string total_records { get; set; }
        public string ScheduleId { get; set; }
        public string donation_id { get; set; }
        public string d_amount { get; set; }
        public string d_fee { get; set; }
        public string donation_frequency { get; set; }
        public string donation_period { get; set; }
        public string donation_day { get; set; }
        public string start_date { get; set; }
        public string card_expiry { get; set; }
        public string last_attempt_date { get; set; }
        public string donation_status { get; set; }
        public string crm_schedule_id { get; set; }
        public string last_updated { get; set; }
        public string date_created { get; set; }
    }
}
