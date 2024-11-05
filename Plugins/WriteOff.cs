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
    public class WriteOff : IPlugin
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

                if (targetEntity.LogicalName == LRx_WriteOff.EntityLogicalName)
                {

                    LRx_WriteOff writeOff = targetEntity.ToEntity<LRx_WriteOff>();

                    

                    switch (context.MessageName)
                    {
                        case "Create":
                            fundraisingService.WriteOff(writeOff.Id);
                            break;
                        case "Update":
                            fundraisingService.WriteOff(writeOff.Id);
                            break;
                        default:
                            break;
                    }
                }
            }
            else if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is EntityReference && context.MessageName == "Delete")
            {
                // Handle logic on writeoff deletion
                var preImageEntity = context.PreEntityImages["WriteOffPreImage"];
                

                if (context.PreEntityImages != null && context.PreEntityImages.Contains("WriteOffPreImage"))
                {
                    var preImage = preImageEntity.ToEntity<LRx_WriteOff>();
                    //donorCommitment Writeoff amount
                    if (preImage.LRx_MsnFp_DonorCommitment != null && preImage.LRx_MsnFp_DonorCommitment.Id != Guid.Empty)
                    {
                        fundraisingService.WriteOffRecalculation(preImage.LRx_MsnFp_DonorCommitment.Id);
                    }


                }
            }

        }
    }
}
