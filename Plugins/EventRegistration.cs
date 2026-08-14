using CrmEarlyBound;
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
            var fundraisingService = new FundraisingService(service, context, tracingService);
            tracingService.Trace(
     $"Registrations Plugin FIRED. Message={context.MessageName}, PrimaryEntity={context.PrimaryEntityName}");

            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {    
                var targetEntity = (Entity)context.InputParameters["Target"];
                LRx_Registrations registrationRecord = new LRx_Registrations();

                if (targetEntity.LogicalName == LRx_Registrations.EntityLogicalName)
                {
                    var registrationID = targetEntity.Id;
                    
                    switch (context.MessageName)
                    {
                        case "Create":
                            fundraisingService.UpdateEventRegistrationRevenue(registrationID, registrationRecord);
                            break;
                        case "Update":
                            fundraisingService.UpdateEventRegistrationRevenue(registrationID, registrationRecord);
                            break;
                        default:
                            break;
                    }
                }
            }
            else if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is EntityReference && context.MessageName == "Delete")
            {
                // Handle logic on transaction deletion
                if (context.PreEntityImages == null ||
     !context.PreEntityImages.Contains("RegistrationPreImage"))
                {
                    throw new InvalidPluginExecutionException("RegistrationPreImage is missing.");
                }

                var preImageEntity = context.PreEntityImages["RegistrationPreImage"];
                var preImage = preImageEntity.ToEntity<LRx_Registrations>();

                if (preImage == null)
                {
                    throw new InvalidPluginExecutionException("RegistrationPreImage is null.");
                }

                LRx_Registrations registrationRecord = new LRx_Registrations
                {
                    LRx_Event = preImage.LRx_Event,
                    LRx_EventTicket = preImage.LRx_EventTicket,
                    LRx_EventTable = preImage.LRx_EventTable,
                    LRx_EventTeam = preImage.LRx_EventTeam
                };

                fundraisingService.UpdateEventRegistrationRevenue(Guid.Empty, registrationRecord);
            }
        }
    }
}
