using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FundraisinApp_IntegrationPlugin.FundraisinApp_Integration.Plugins.Entities.Integration_Data_Model
{
    public class CustomPageDetailsModel
    {
        public string history_id { get; set; }
        public string member_id { get; set; }
        public string m_fname { get; set; }
        public string m_lname { get; set; }
        public string total_raised { get; set; }
        public string m_target { get; set; }
        public string blog_title { get; set; }
        public string blog_content { get; set; }
    }
}
