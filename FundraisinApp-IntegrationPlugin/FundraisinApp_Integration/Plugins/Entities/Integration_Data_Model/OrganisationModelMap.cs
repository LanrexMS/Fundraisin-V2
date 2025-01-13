using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FundraisinApp_IntegrationPlugin.FundraisinApp_Integration.Plugins.Entities.Integration_Data_Model
{
    public class OrganisationModelMap : ClassMap<OrganisationModel>
    {
        public OrganisationModelMap()
        {
            // Mapping the OrganisationModel properties to the respective CSV column names
            Map(m => m.total_records).Name("total_records");
            Map(m => m.org_id).Name("org_id");
            Map(m => m.event_id).Name("event_id");
            Map(m => m.charity_id).Name("charity_id");
            Map(m => m.org_name).Name("org_name");
            Map(m => m.org_url).Name("org_url");
            Map(m => m.org_photo).Name("org_photo");
            Map(m => m.org_coverphoto).Name("org_coverphoto");
            Map(m => m.org_description).Name("org_description");
            Map(m => m.org_target).Name("org_target");
            Map(m => m.org_target_local).Name("org_target_local");
            Map(m => m.org_target_distance).Name("org_target_distance");
            Map(m => m.org_target_steps).Name("org_target_steps");
            Map(m => m.org_target_duration).Name("org_target_duration");
            Map(m => m.org_address_unit).Name("org_address_unit");
            Map(m => m.org_address_number).Name("org_address_number");
            Map(m => m.org_address_street).Name("org_address_street");
            Map(m => m.org_address_2).Name("org_address_2");
            Map(m => m.org_address_suburb).Name("org_address_suburb");
            Map(m => m.org_address_pcode).Name("org_address_pcode");
            Map(m => m.org_address_state).Name("org_address_state");
            Map(m => m.org_address_country).Name("org_address_country");
            Map(m => m.is_public).Name("is_public");
            Map(m => m.in_search).Name("in_search");
            Map(m => m.org_passcode).Name("org_passcode");
            Map(m => m.org_business_number).Name("org_business_number");
            Map(m => m.org_industry).Name("org_industry");
            Map(m => m.org_category).Name("org_category");
            Map(m => m.org_website).Name("org_website");
            Map(m => m.org_kw_address).Name("org_kw_address");
            Map(m => m.org_lat).Name("org_lat");
            Map(m => m.org_lng).Name("org_lng");
            Map(m => m.created_member_id).Name("created_member_id");
            Map(m => m.total_raised).Name("total_raised");
            Map(m => m.total_raised_local).Name("total_raised_local");
            Map(m => m.total_steps).Name("total_steps");
            Map(m => m.total_distance).Name("total_distance");
            Map(m => m.total_duration).Name("total_duration");
            Map(m => m.crm_org_id).Name("crm_org_id");
            Map(m => m.org_status).Name("org_status");
            Map(m => m.last_updated).Name("last_updated");
            Map(m => m.date_created).Name("date_created");
        }
    }

}
