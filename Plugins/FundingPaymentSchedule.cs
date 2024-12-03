using CrmEarlyBound;
using DataverseModel;
using Fundraising_Engagement.Plugins.Service;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fundraising_Engagement.Plugins.Plugins
{
    public class FundingPaymentSchedule : IPlugin
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

                if (targetEntity.LogicalName == LRx_FundingPaymentSchedule.EntityLogicalName)
                {
                    var fundingPaymentScheduleID = targetEntity.Id;

                    LRx_FundingPaymentSchedule fundingPaymentScheduleRecord = new LRx_FundingPaymentSchedule();

                    switch (context.MessageName)
                    {
                        case "Create":
                            fundraisingService.ComputeDonorCommitmentPaid(fundingPaymentScheduleID, fundingPaymentScheduleRecord);
                            break;
                        case "Update":
                            fundraisingService.ComputeDonorCommitmentPaid(fundingPaymentScheduleID, fundingPaymentScheduleRecord);
                            break;
                        default:
                            break;
                    }
                }
            }
            else if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is EntityReference && context.MessageName == "Delete")
            {
                var preImageEntity = context.PreEntityImages["FundingPaymentSchedulePreImage"];

                if (context.PreEntityImages != null && context.PreEntityImages.Contains("FundingPaymentSchedulePreImage"))
                {
                    var preImage = preImageEntity.ToEntity<LRx_FundingPaymentSchedule>();

                    LRx_FundingPaymentSchedule fundingPaymentScheduleRecord = new LRx_FundingPaymentSchedule();

                    fundingPaymentScheduleRecord.LRx_FundingAgreement = preImage.LRx_FundingAgreement;

                    fundraisingService.ComputeDonorCommitmentPaid(Guid.Empty, fundingPaymentScheduleRecord);
                }
            }
        }
    }
}
