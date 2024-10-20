using DataverseModel;
using Fundraising_Engagement.Plugins.Service;
using Microsoft.Xrm.Sdk;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fundraising_Engagement.Plugins
{
    public class EventRegistration : IPlugin
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

                if (targetEntity.LogicalName == LRx_Registrations.EntityLogicalName)
                {
                    var registrationID = targetEntity.Id;
                    var fundraisingService = new FundraisingService(service, context, tracingService);
                    tracingService.Trace("RegGUID: " + registrationID);
                    
                    switch (context.MessageName)
                    {
                        case "Create":
                            fundraisingService.UpdateEventRegistrationRevenue(registrationID);
                            break;
                        case "Update":
                            fundraisingService.UpdateEventRegistrationRevenue(registrationID);
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }
}
