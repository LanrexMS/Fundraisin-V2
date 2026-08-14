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
    public class EventProduct : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {

            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = serviceFactory.CreateOrganizationService(context.UserId);
            var tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            var fundraisingService = new FundraisingService(service, context, tracingService);

            tracingService.Trace(
     $"EventProduct Plugin FIRED. Message={context.MessageName}, PrimaryEntity={context.PrimaryEntityName}");

            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {    
                var targetEntity = (Entity)context.InputParameters["Target"];

                if (targetEntity.LogicalName == LRx_Product.EntityLogicalName)
                {
                    var productID = targetEntity.Id;

                    LRx_Product productRecord = new LRx_Product();

                    switch (context.MessageName)
                    {
                        case "Create":
                            fundraisingService.UpdateEventProductRevenue(productID, productRecord);
                            break;
                        case "Update":
                            fundraisingService.UpdateEventProductRevenue(productID, productRecord);
                            break;
                        default:
                            break;
                    }
                }
            }
            else if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is EntityReference && context.MessageName == "Delete")
            {
                // Handle logic on transaction deletion
                tracingService.Trace("Enters in delete plugin");
                var preImageEntity = context.PreEntityImages["ProductPreImage"];

                if (context.PreEntityImages != null && context.PreEntityImages.Contains("ProductPreImage"))
                {
                    var preImage = preImageEntity.ToEntity<LRx_Product>();

                    if (preImage != null)
                    {
                        // Initialize a new instance of the registration record
                        LRx_Product productRecord = new LRx_Product();

                        productRecord.LRx_Event = preImage.LRx_Event;
                        productRecord.LRx_EventProduct = preImage.LRx_EventProduct;

                        fundraisingService.UpdateEventProductRevenue(Guid.Empty, productRecord);
                    }
                }
            }
        }
    }
}
