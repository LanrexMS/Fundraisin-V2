using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FundraisinApp_IntegrationPlugin.FundraisinApp_Integration.Plugins.Entities.Integration_Data_Model
{
    public class PromoCodeModelMap : ClassMap<PromoCodeModel>
    {
        public PromoCodeModelMap()
        {
            Map(m => m.total_records).Name("total_records");
            Map(m => m.promo_id).Name("promo_id");
            Map(m => m.promo_code).Name("promo_code");
            Map(m => m.promo_code_type).Name("promo_code_type");
            Map(m => m.promo_type).Name("promo_type");
            Map(m => m.promo_payment_type).Name("promo_payment_type");
            Map(m => m.event_id).Name("event_id");
            Map(m => m.member_id).Name("member_id");
            Map(m => m.team_id).Name("team_id");
            Map(m => m.promo_value).Name("promo_value");
            Map(m => m.promo_value_type).Name("promo_value_type");
            Map(m => m.promo_entries_limit).Name("promo_entries_limit");
            Map(m => m.promo_status).Name("promo_status");
            Map(m => m.date_created).Name("date_created");
        }
    }
}
