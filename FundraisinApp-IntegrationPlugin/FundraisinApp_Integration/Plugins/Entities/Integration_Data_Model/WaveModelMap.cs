using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FundraisinApp_IntegrationPlugin.FundraisinApp_Integration.Plugins.Entities.Integration_Data_Model
{
    public class WaveModelMap : ClassMap<WaveModel>
    {
        public WaveModelMap() {
            Map(m => m.wave_id).Name("wave_id");
            Map(m => m.event_id).Name("event_id");
            Map(m => m.parent_id).Name("parent_id");
            Map(m => m.wave_name).Name("wave_name");
            Map(m => m.wave_code).Name("wave_code");
            Map(m => m.wave_time).Name("wave_time");
            Map(m => m.wave_date).Name("wave_date");
            Map(m => m.wave_price).Name("wave_price");
            Map(m => m.wave_limit).Name("wave_limit");
            Map(m => m.wave_tag).Name("wave_tag");
            Map(m => m.wave_description).Name("wave_description");
            Map(m => m.wave_image).Name("wave_image");
            Map(m => m.bib_colour).Name("bib_colour");
            Map(m => m.bib_numbers_start).Name("bib_numbers_start");
            Map(m => m.wave_status).Name("wave_status");
            Map(m => m.sort_order).Name("sort_order");
            Map(m => m.crm_wave_id).Name("crm_wave_id");
            Map(m => m.last_updated).Name("last_updated");
            Map(m => m.date_created).Name("date_created");
        }   
    }
}
