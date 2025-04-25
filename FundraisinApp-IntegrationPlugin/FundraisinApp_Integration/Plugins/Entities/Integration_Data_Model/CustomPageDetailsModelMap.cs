using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FundraisinApp_IntegrationPlugin.FundraisinApp_Integration.Plugins.Entities.Integration_Data_Model
{
    public class CustomPageDetailsModelMap : ClassMap<CustomPageDetailsModel>
    {
        public CustomPageDetailsModelMap()
        {
            // Mapping CustomPageDetailsModel properties to CSV column names
            Map(m => m.history_id).Name("history_id");
            Map(m => m.member_id).Name("member_id");
            Map(m => m.m_fname).Name("m_fname");
            Map(m => m.m_lname).Name("m_lname");
            Map(m => m.total_raised).Name("total_raised");
            Map(m => m.m_target).Name("m_target");
            Map(m => m.blog_title).Name("blog_title");
            Map(m => m.blog_content).Name("blog_content");
        }
    }
}
