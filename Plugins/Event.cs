using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataverseModel;
using Fundraising_Engagement.Plugins.Service;
using Microsoft.Xrm.Sdk;


namespace Fundraising_Engagement.Plugins.Plugins
{
    public class EventRollup : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var context = (IPluginExecutionContext)serviceProvider.GetService(
                typeof(IPluginExecutionContext));

            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(
                typeof(IOrganizationServiceFactory));

            var service = serviceFactory.CreateOrganizationService(context.UserId);

            var tracingService = (ITracingService)serviceProvider.GetService(
                typeof(ITracingService));

            var fundraisingService = new FundraisingService(
                service,
                context,
                tracingService);

            tracingService.Trace(
                $"Event Rollup Plugin FIRED. Message={context.MessageName}, PrimaryEntity={context.PrimaryEntityName}");

            // CREATE / UPDATE
            if (context.InputParameters.Contains("Target") &&
                context.InputParameters["Target"] is Entity targetEntity)
            {
                if (targetEntity.LogicalName != LRx_Event.EntityLogicalName)
                {
                    return;
                }

                if (context.MessageName == "Create")
                {
                    if (targetEntity.Id != Guid.Empty)
                    {
                        fundraisingService.UpdateCampaignEventTotals(
                            targetEntity.Id,
                            Guid.Empty);

                        fundraisingService.UpdateAppealEventTotals(
                            targetEntity.Id,
                            Guid.Empty);

                        fundraisingService.UpdatePackageEventTotals(
                            targetEntity.Id,
                        Guid.Empty);
                    }
                }
                else if (context.MessageName == "Update")
                {
                    Guid oldCampaignId = Guid.Empty;
                    Guid oldAppealId = Guid.Empty;
                    Guid oldPackageId = Guid.Empty;

                    // Get old Campaign from Pre Image
                    if (context.PreEntityImages.Contains("EventPreImage"))
                    {
                        var preImage = context.PreEntityImages["EventPreImage"]
                            .ToEntity<LRx_Event>();

                        if (preImage.LRx_Campaign != null)
                        {
                            oldCampaignId = preImage.LRx_Campaign.Id;
                        }

                        if (preImage.LRx_SiFund_Appeal != null)
                        {
                            oldAppealId = preImage.LRx_SiFund_Appeal.Id;
                        }

                        if (preImage.LRx_SiFund_Package != null)
                        {
                            oldPackageId = preImage.LRx_SiFund_Package.Id;
                        }
                    }

                    fundraisingService.UpdateCampaignEventTotals(
                        targetEntity.Id,
                        oldCampaignId);

                    fundraisingService.UpdateAppealEventTotals(
                     targetEntity.Id,
                    oldAppealId);

                    fundraisingService.UpdatePackageEventTotals(
                    targetEntity.Id,
                    oldPackageId);
                }
            }

            // DELETE
            else if (context.InputParameters.Contains("Target") &&
                     context.InputParameters["Target"] is EntityReference &&
                     context.MessageName == "Delete")
            {
                if (!context.PreEntityImages.Contains("EventPreImage"))
                {
                    tracingService.Trace("EventPreImage not found.");
                    return;
                }

                var preImage = context.PreEntityImages["EventPreImage"]
                    .ToEntity<LRx_Event>();

                if (preImage == null)
                {
                    tracingService.Trace("EventPreImage is null.");
                    return;
                }

                Guid oldCampaignId = Guid.Empty;
                Guid oldAppealId = Guid.Empty;
                Guid oldPackageId = Guid.Empty;

                if (preImage.LRx_Campaign != null)
                {
                    oldCampaignId = preImage.LRx_Campaign.Id;
                }

                if (preImage.LRx_SiFund_Appeal != null)
                {
                    oldAppealId = preImage.LRx_SiFund_Appeal.Id;
                }

                if (preImage.LRx_SiFund_Package != null)
                {
                    oldPackageId = preImage.LRx_SiFund_Package.Id;
                }

                fundraisingService.UpdateCampaignEventTotals(
                    Guid.Empty,
                    oldCampaignId);

                fundraisingService.UpdateAppealEventTotals(
                Guid.Empty,
                oldAppealId);

                fundraisingService.UpdatePackageEventTotals(
                    Guid.Empty,
                    oldPackageId);
            }
        }
    }
}
