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
    public class DonorCommitment : IPlugin
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

                if (targetEntity.LogicalName == MsnFp_DonorCommitment.EntityLogicalName)
                {
                    var donorCommitmentId = targetEntity.Id;

                    var fundraisingService = new FundraisingService(service, context, tracingService);

                    switch (context.MessageName)
                    {
                        case "Create":
                            fundraisingService.CampaignPerformanceDonorCommitment(donorCommitmentId);
                            break;
                        case "Update":

                            // Handle logic when lookup fields are updated (with previous value) 
                            if (context.PreEntityImages != null && context.PreEntityImages.Contains("DonorCommitmentPreImage"))
                            {
                                // Retrieve the pre-image as an Entity
                                var preImageEntity = context.PreEntityImages["DonorCommitmentPreImage"];

                              
                                var preImage = preImageEntity.ToEntity<MsnFp_DonorCommitment>();

                                if (preImage.SiFund_Appeal != null)
                                {
                                    var siFundAppeal = preImage.SiFund_Appeal;
                                    fundraisingService.PledgesRollup(SiFund_Appeal.EntityLogicalName, siFundAppeal.Id, MsnFp_DonorCommitment.Fields.SiFund_Appeal);

                                }

                                if(preImage.SiFund_Package != null)
                                {
                                    var siFundPackage = preImage.SiFund_Package;
                                    fundraisingService.PledgesRollup(SiFund_Package.EntityLogicalName, siFundPackage.Id, MsnFp_DonorCommitment.Fields.SiFund_Package);
                                }

                                if(preImage.LRx_Campaign!= null)
                                {
                                    var lRxCampaign = preImage.LRx_Campaign;
                                    fundraisingService.PledgesRollup(Campaign.EntityLogicalName, lRxCampaign.Id, MsnFp_DonorCommitment.Fields.LRx_Campaign);
                                }
                            }

                            fundraisingService.CampaignPerformanceDonorCommitment(donorCommitmentId);
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }
}
