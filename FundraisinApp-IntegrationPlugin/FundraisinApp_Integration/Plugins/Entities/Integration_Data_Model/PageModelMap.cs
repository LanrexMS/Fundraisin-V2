using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FundraisinApp_IntegrationPlugin.FundraisinApp_Integration.Plugins.Entities.Integration_Data_Model
{
    public class PageModelMap : ClassMap<PageModel>
    {
        public PageModelMap()
        {
            Map(m => m.Page_Id).Name("page_id");
            Map(m => m.Page_Name).Name("page_name");
            Map(m => m.Last_Updated).Name("last_updated");
            Map(m => m.Date_Created).Name("date_created");
        }
    }
}
