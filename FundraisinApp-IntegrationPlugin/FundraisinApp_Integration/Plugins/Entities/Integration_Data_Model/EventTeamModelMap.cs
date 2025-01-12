using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FundraisinApp_IntegrationPlugin.FundraisinApp_Integration.Plugins.Entities.Integration_Data_Model
{
    public class EventTeamModelMap : ClassMap<EventTeamModel>
    {
        public EventTeamModelMap()
        {
            // Mapping EventTeamModel properties to respective CSV column names
            Map(m => m.total_records).Name("total_records");
            Map(m => m.team_id).Name("team_id");
            Map(m => m.event_id).Name("event_id");
            Map(m => m.org_id).Name("org_id");
            Map(m => m.history_id).Name("history_id");
            Map(m => m.charity_id).Name("charity_id");
            Map(m => m.captain_id).Name("captain_id");
            Map(m => m.bib_number).Name("bib_number");
            Map(m => m.t_name).Name("t_name");
            Map(m => m.t_url).Name("t_url");
            Map(m => m.t_target).Name("t_target");
            Map(m => m.t_target_local).Name("t_target_local");
            Map(m => m.t_target_distance).Name("t_target_distance");
            Map(m => m.t_target_steps).Name("t_target_steps");
            Map(m => m.t_target_duration).Name("t_target_duration");
            Map(m => m.t_name_alt).Name("t_name_alt");
            Map(m => m.is_featured).Name("is_featured");
            Map(m => m.t_comments).Name("t_comments");
            Map(m => m.t_codeblock).Name("t_codeblock");
            Map(m => m.t_codeblock_mobile).Name("t_codeblock_mobile");
            Map(m => m.t_page_title).Name("t_page_title");
            Map(m => m.t_company_name).Name("t_company_name");
            Map(m => m.t_company_website).Name("t_company_website");
            Map(m => m.t_industry).Name("t_industry");
            Map(m => m.t_industry_other).Name("t_industry_other");
            Map(m => m.t_photo).Name("t_photo");
            Map(m => m.t_coverphoto).Name("t_coverphoto");
            Map(m => m.t_background).Name("t_background");
            Map(m => m.t_alert_donation).Name("t_alert_donation");
            Map(m => m.t_alert_member).Name("t_alert_member");
            Map(m => m.t_alert_goal).Name("t_alert_goal");
            Map(m => m.t_goal_sent).Name("t_goal_sent");
            Map(m => m.t_gallery_title).Name("t_gallery_title");
            Map(m => m.t_gallery_copy).Name("t_gallery_copy");
            Map(m => m.t_thanks_message).Name("t_thanks_message");
            Map(m => m.t_address_unit).Name("t_address_unit");
            Map(m => m.t_address_number).Name("t_address_number");
            Map(m => m.t_address_street).Name("t_address_street");
            Map(m => m.t_address_2).Name("t_address_2");
            Map(m => m.t_address_suburb).Name("t_address_suburb");
            Map(m => m.t_address_pcode).Name("t_address_pcode");
            Map(m => m.t_address_state).Name("t_address_state");
            Map(m => m.t_address_country).Name("t_address_country");
            Map(m => m.t_event_date).Name("t_event_date");
            Map(m => m.t_kw_address).Name("t_kw_address");
            Map(m => m.t_event_time).Name("t_event_time");
            Map(m => m.t_lat).Name("t_lat");
            Map(m => m.t_lng).Name("t_lng");
            Map(m => m.related_teams).Name("related_teams");
            Map(m => m.t_public).Name("t_public");
            Map(m => m.in_search).Name("in_search");
            Map(m => m.t_passcode).Name("t_passcode");
            Map(m => m.admin_tags).Name("admin_tags");
            Map(m => m.has_vacancy).Name("has_vacancy");
            Map(m => m.vacancy_description).Name("vacancy_description");
            Map(m => m.total_raised).Name("total_raised");
            Map(m => m.total_raised_local).Name("total_raised_local");
            Map(m => m.total_steps).Name("total_steps");
            Map(m => m.total_distance).Name("total_distance");
            Map(m => m.total_duration).Name("total_duration");
            Map(m => m.crm_team_id).Name("crm_team_id");
            Map(m => m.t_status).Name("t_status");
            Map(m => m.last_updated).Name("last_updated");
            Map(m => m.date_created).Name("date_created");
        }
    }
}
