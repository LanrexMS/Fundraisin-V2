using FNQHF_Fundraising_Engagement.Service;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FNQHF_Fundraising_Engagement.Plugins
{
    public class TotalNonRallyUpTransaction : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = serviceFactory.CreateOrganizationService(context.UserId);
            var tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            var fundraisingService = new FundraisingEngagementService(service, context, tracingService);

            fundraisingService.CalculateTotalTransactionForAllContacts();
        }
    }
}
