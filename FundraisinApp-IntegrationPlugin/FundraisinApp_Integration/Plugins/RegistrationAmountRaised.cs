using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrmEarlyBound;
using Fundraising_Integration.Plugins.Service;

namespace FundraisinApp_Integration.Plugins
{
    public class RegistrationAmountRaised : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = serviceFactory.CreateOrganizationService(context.UserId);
            var tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            var fundraisingService = new FundraisingService(service, context, tracingService);

            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                var targetEntity = (Entity)context.InputParameters["Target"];

                if (targetEntity.LogicalName == MsnFp_Transaction.EntityLogicalName)
                {

                    MsnFp_Transaction transaction = targetEntity.ToEntity<MsnFp_Transaction>();

                    switch (context.MessageName)
                    {
                        case "Create":
                            fundraisingService.UpdateRegistrationAmountRaised(transaction);
                            break;
                        case "Update":
                            fundraisingService.UpdateRegistrationAmountRaised(transaction);
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }
}
