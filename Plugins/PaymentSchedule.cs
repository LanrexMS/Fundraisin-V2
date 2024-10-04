using CrmEarlyBound;
using Fundraising_Engagement.Plugins.Service;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fundraising_Engagement.Plugins.Plugins
{
    public class PaymentSchedule : IPlugin
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

                if (targetEntity.LogicalName == MsnFp_PaymentSchedule.EntityLogicalName)
                {

                    MsnFp_PaymentSchedule paymentSchedule = targetEntity.ToEntity<MsnFp_PaymentSchedule>();

                    var fundraisingService = new FundraisingService(service, context, tracingService);

                    switch (context.MessageName)
                    {
                        case "Create":
                            fundraisingService.CreatePledgeCommitments(paymentSchedule);
                            break;
                        case "Update":
                            fundraisingService.CreatePledgeCommitments(paymentSchedule);
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }
}
