using System;
using DataverseModel;
using Fundraising_Engagement.Plugins.Service;
using Microsoft.Xrm.Sdk;

namespace Fundraising_Engagement.Plugins.Plugins
{
    public class TicketHolder : IPlugin
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
                $"Ticket Holder Rollup Plugin FIRED. Message={context.MessageName}, " +
                $"PrimaryEntity={context.PrimaryEntityName}");

            // CREATE / UPDATE
            if (context.InputParameters.Contains("Target") &&
                context.InputParameters["Target"] is Entity targetEntity)
            {
                if (targetEntity.LogicalName != lrx_TicketHolders.EntityLogicalName)
                {
                    return;
                }

                if (context.MessageName == "Create")
                {
                    if (targetEntity.Id != Guid.Empty)
                    {
                        fundraisingService.UpdateRegistrationTicketHolderCount(
                            targetEntity.Id,
                            Guid.Empty);
                    }
                }
                else if (context.MessageName == "Update")
                {
                    Guid oldRegistrationId = Guid.Empty;

                    // Get old Registration from Pre Image
                    if (context.PreEntityImages.Contains("TicketHolderPreImage"))
                    {
                        var preImage = context.PreEntityImages["TicketHolderPreImage"]
                            .ToEntity<lrx_TicketHolders>();

                        if (preImage.lrx_ParentRegistration != null)
                        {
                            oldRegistrationId =
                                preImage.lrx_ParentRegistration.Id;
                        }
                    }

                    fundraisingService.UpdateRegistrationTicketHolderCount(
                        targetEntity.Id,
                        oldRegistrationId);
                }
            }

            // DELETE
            else if (context.InputParameters.Contains("Target") &&
                     context.InputParameters["Target"] is EntityReference &&
                     context.MessageName == "Delete")
            {
                if (!context.PreEntityImages.Contains("TicketHolderPreImage"))
                {
                    tracingService.Trace(
                        "TicketHolderPreImage not found.");

                    return;
                }

                var preImage = context.PreEntityImages["TicketHolderPreImage"]
                    .ToEntity<lrx_TicketHolders>();

                if (preImage == null)
                {
                    tracingService.Trace(
                        "TicketHolderPreImage is null.");

                    return;
                }

                Guid oldRegistrationId = Guid.Empty;

                if (preImage.lrx_ParentRegistration != null)
                {
                    oldRegistrationId =
                        preImage.lrx_ParentRegistration.Id;
                }

                fundraisingService.UpdateRegistrationTicketHolderCount(
                    Guid.Empty,
                    oldRegistrationId);
            }
        }
    }
}