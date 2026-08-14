using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FundraisinApp_IntegrationPlugin.FundraisinApp_Integration.Plugins.Entities.Integration_Data_Model
{
    public class WaveModel
    {
        public string wave_id { get; set; }
        public string event_id { get; set; }
        public string parent_id { get; set; }
        public string wave_name { get; set; }
        public string wave_code { get; set; }
        public string wave_time { get; set; }
        public string wave_date { get; set; }
        public string wave_price { get; set; }
        public string wave_limit { get; set; }
        public string wave_tag { get; set; }
        public string wave_description { get; set; }
        public string wave_image { get; set; }
        public string bib_colour { get; set; }
        public string bib_numbers_start { get; set; }
        public string wave_status { get; set; }
        public string sort_order { get; set; }
        public string crm_wave_id { get; set; }
        public string last_updated { get; set; }
        public string date_created { get; set; }
    }
}
