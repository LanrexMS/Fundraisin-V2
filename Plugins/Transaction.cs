using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrmEarlyBound;
using Fundraising_Engagement.Plugins.Service;
using System.IdentityModel.Metadata;


namespace Fundraising_Engagement.Plugins
{
    public class Transaction : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = serviceFactory.CreateOrganizationService(context.UserId);
            var tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));


            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                var targetEntity = (Entity)context.InputParameters["Target"];

                if (targetEntity.LogicalName == MsnFp_Transaction.EntityLogicalName)
                {
                  
                    MsnFp_Transaction transaction = targetEntity.ToEntity<MsnFp_Transaction>();

                    var fundraisingService = new FundraisingService(service, context, tracingService);

                    switch (context.MessageName)
                    {
                        case "Create":
                            fundraisingService.YearlyGiving(transaction);
                            fundraisingService.CampaignPerformanceTransaction(transaction);
                            break;
                        case "Update":
                            fundraisingService.YearlyGiving(transaction);
                            fundraisingService.CampaignPerformanceTransaction(transaction);
                            break;
                        default:
                            break;
                    }
                }
            }
        }

    }
}