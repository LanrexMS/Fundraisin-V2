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
    public class EventTable : IPlugin
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

                if (targetEntity.LogicalName == LRx_EventTable.EntityLogicalName)
                {
                    var tableID = targetEntity.Id;
                    
                    LRx_EventTable eventTableRecord = new LRx_EventTable();

                    switch (context.MessageName)
                    {
                        case "Create":
                            fundraisingService.UpdateEventTableRevenue(tableID, eventTableRecord);
                            break;
                        case "Update":
                            fundraisingService.UpdateEventTableRevenue(tableID, eventTableRecord);
                            break;
                        default:
                            break;
                    }
                }
            }
            else if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is EntityReference && context.MessageName == "Delete")
            {
                // Handle logic on transaction deletion
                var preImageEntity = context.PreEntityImages["TablePreImage"];

                if (context.PreEntityImages != null && context.PreEntityImages.Contains("TablePreImage"))
                {
                    var preImage = preImageEntity.ToEntity<LRx_EventTable>();

                    if (preImage != null)
                    {
                        // Initialize a new instance of the registration record
                        LRx_EventTable eventTableRecord = new LRx_EventTable();

                        eventTableRecord.LRx_Event = preImage.LRx_Event;
                        eventTableRecord.LRx_EventTableId = preImage.LRx_EventTableId;

                        fundraisingService.UpdateEventTableRevenue(Guid.Empty, eventTableRecord);
                    }

                }
            }
        }
    }
}
