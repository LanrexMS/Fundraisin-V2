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
                var preImageEntity = context.PreEntityImages["RegistrationPreImage"];


                if (context.PreEntityImages != null && context.PreEntityImages.Contains("RegistrationPreImage"))
                {
                    var preImage = preImageEntity.ToEntity<LRx_Registrations>();

                    if (preImage != null)
                    {
                        // Initialize a new instance of the registration record
                        LRx_Registrations registrationRecord = new LRx_Registrations();

                        // Assign fields from preImage to registrationRecord
                        registrationRecord.LRx_Event = preImage.LRx_Event;
                        registrationRecord.LRx_EventTicket = preImage.LRx_EventTicket;
                        registrationRecord.LRx_EventTable = preImage.LRx_EventTable;
                        registrationRecord.LRx_EventTeam = preImage.LRx_EventTeam;

                        fundraisingService.UpdateEventRegistrationRevenue(Guid.Empty, registrationRecord);
                    }
                }
            }
        }
    }
}
