using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrmEarlyBound;
using Fundraising_Engagement.Plugins.Service;
using System.IdentityModel.Metadata;


namespace Fundraising_Engagement.Plugins
{
    public class Transaction : IPlugin
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

                if (targetEntity.LogicalName == MsnFp_Transaction.EntityLogicalName)
                {
                  
                    MsnFp_Transaction transaction = targetEntity.ToEntity<MsnFp_Transaction>();

                    MsnFp_Transaction transactionrecord = new MsnFp_Transaction();

                    switch (context.MessageName)
                    {
                        case "Create":
                            fundraisingService.CheckPledgeMatch(transaction.Id, "transaction");
                            fundraisingService.AutoCompleteCashTransactions(transaction);
                            fundraisingService.AutoPopulateRefundAmounts(transaction);
                            fundraisingService.YearlyGiving(transaction);
                            fundraisingService.UpdateLatestTransaction(transaction, transactionrecord);
                            fundraisingService.CampaignPerformanceTransaction(transaction);
                            fundraisingService.DonorCommitmentPaid(transaction);
                            break;
                        case "Update":
                            //Plugin step should only trigger on update of statuscode (for refunds)
                            fundraisingService.YearlyGiving(transaction);
                            fundraisingService.UpdateLatestTransaction(transaction, transactionrecord);
                            fundraisingService.DonorCommitmentPaid(transaction);
                            fundraisingService.CampaignPerformanceTransaction(transaction);
                            break;
                        default:
                            break;
                    }
                }
            }
            else if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is EntityReference && context.MessageName=="Delete")
            {
                // Handle logic on transaction deletion
                var preImageEntity = context.PreEntityImages["TransactionPreImage"];
                

                if (context.PreEntityImages != null && context.PreEntityImages.Contains("TransactionPreImage"))
                {
                    var preImage = preImageEntity.ToEntity<MsnFp_Transaction>();

                    //Yearly Giving
                    if (preImage.SiFund_Donor != null && preImage.SiFund_Donor.Id != Guid.Empty)
                    {
                        fundraisingService.YearlyGivingRecalculation(preImage.SiFund_Donor.Id, preImage.SiFund_Donor.LogicalName);
                    }

                    //Latest Transaction
                    if (preImage.SiFund_Donor != null && preImage.SiFund_Donor.Id != Guid.Empty)
                    {
                        fundraisingService.LastestTransactionRecalculation(preImage.SiFund_Donor.Id, preImage.SiFund_Donor.LogicalName);
                    }

                    //Donor CommitmentPaid Amount
                    if(preImage.SiFund_RelatedDonorCommitment != null && (preImage.SiFund_RelatedDonorCommitment.Id != Guid.Empty))
                    {
                        fundraisingService.DonorCommitmentPaidRecalculation(preImage.SiFund_RelatedDonorCommitment.Id);
                    }

                    //Campaign Performance - Campaign
                    if (preImage.LRx_Campaign != null && preImage.LRx_Campaign.Id != Guid.Empty)
                    {
                        fundraisingService.DonationsRollup(Campaign.EntityLogicalName, preImage.LRx_Campaign.Id, MsnFp_Transaction.Fields.LRx_Campaign);
                    }

                    //Campaign Performance - Appeal
                    if (preImage.SiFund_Appeal != null && preImage.SiFund_Appeal.Id != Guid.Empty)
                    {
                        fundraisingService.DonationsRollup(SiFund_Appeal.EntityLogicalName, preImage.SiFund_Appeal.Id, MsnFp_Transaction.Fields.SiFund_Appeal);
                    }

                    //Campaign Performance - Package
                    if (preImage.SiFund_Package != null && preImage.SiFund_Package.Id != Guid.Empty)
                    {
                        fundraisingService.DonationsRollup(SiFund_Package.EntityLogicalName, preImage.SiFund_Package.Id, MsnFp_Transaction.Fields.SiFund_Package);

                    }

                    MsnFp_Transaction transactionrecord = new MsnFp_Transaction();
                    transactionrecord.SiFund_Donor = preImage.SiFund_Donor;
                    transactionrecord.LRx_Event = preImage.LRx_Event;
                    transactionrecord.LRx_EventTeam = preImage.LRx_EventTeam;
                    transactionrecord.Id = Guid.Empty;
                    fundraisingService.UpdateLatestTransaction(transactionrecord, transactionrecord);
                }
            }
        }

    }
}