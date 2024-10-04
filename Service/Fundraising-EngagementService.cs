using CrmEarlyBound;
using Fundraising_Engagement.Plugins.Plugins;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

namespace Fundraising_Engagement.Plugins.Service
{
    public class FundraisingService
    {
        private readonly IOrganizationService _service;
        private readonly IPluginExecutionContext _context;
        private readonly ITracingService _tracingService;


        public FundraisingService(IOrganizationService service, IPluginExecutionContext context, ITracingService tracingService)
        {
            _service = service;
            _context = context;
            _tracingService = tracingService;
        }

        public void AutoCompleteCashTransactions(MsnFp_Transaction transaction)
        {
            MsnFp_Transaction transactionrecord = (MsnFp_Transaction)RetrieveRecord(MsnFp_Transaction.EntityLogicalName, transaction.Id, MsnFp_Transaction.Fields.SiFund_PaymentTypeCode);

            if (transactionrecord.SiFund_PaymentTypeCode != null && transactionrecord.SiFund_PaymentTypeCode == SiFund_PaymentTypeCode.Cash)
            {
                var updateTransaction = new MsnFp_Transaction
                {
                    Id = transaction.Id,
                    StatusCode = MsnFp_Transaction_StatusCode.Completed
                };

                _service.Update(updateTransaction);
            }
        }


        public void YearlyGiving(MsnFp_Transaction transaction)
        {
            var fiscalYears = GetFiscalYears(4);

            //assign fiscal year date values
            DateTime startDateCurrentYear = fiscalYears["Year 0"].StartDate;
            DateTime endDateCurrentYear = fiscalYears["Year 0"].EndDate;
            DateTime startDateLastYear = fiscalYears["Year 1"].StartDate;
            DateTime endDateLastYear = fiscalYears["Year 1"].EndDate;
            DateTime startDateThirdYear = fiscalYears["Year 2"].StartDate;
            DateTime endDateThirdYear = fiscalYears["Year 2"].EndDate;
            DateTime startDateFourthYear = fiscalYears["Year 3"].StartDate;
            DateTime endDateFourthYear = fiscalYears["Year 3"].EndDate;
            DateTime startDateFifthYear = fiscalYears["Year 4"].StartDate;
            DateTime endDateFifthYear = fiscalYears["Year 4"].EndDate;

            MsnFp_Transaction transactionrecord = (MsnFp_Transaction)RetrieveRecord(MsnFp_Transaction.EntityLogicalName, transaction.Id, MsnFp_Transaction.Fields.SiFund_Donor);

            if(transactionrecord.SiFund_Donor !=null && transactionrecord.SiFund_Donor.Id != Guid.Empty)
            {

                Guid donorId = transactionrecord.SiFund_Donor.Id;

                ColumnSet filterFields = new ColumnSet(
                    MsnFp_Transaction.Fields.StatusCode, 
                    MsnFp_Transaction.Fields.MsnFp_Amount, 
                    MsnFp_Transaction.Fields.MsnFp_BookDate,
                    MsnFp_Transaction.Fields.SiFund_TypeCode
                 );

                // CurrentGiving Sum, All related transactions where statuscode = Completed, 
                var givingCriteria = new Dictionary<string, (ConditionOperator, object)>
                {
                    { MsnFp_Transaction.Fields.StatusCode, (ConditionOperator.Equal, (int)MsnFp_Transaction_StatusCode.Completed) },
                    { MsnFp_Transaction.Fields.SiFund_TypeCode,(ConditionOperator.Equal, (int)MsnFp_Transaction_SiFund_TypeCode.Donation)}
                };

                decimal currentYearGivingAmount = CalculateGivingRollup(Contact.EntityLogicalName, donorId, MsnFp_Transaction.EntityLogicalName, MsnFp_Transaction.Fields.SiFund_Donor, MsnFp_Transaction.Fields.MsnFp_Amount, 
                    filterFields, givingCriteria, startDateCurrentYear, endDateCurrentYear);

                decimal lastYearGivingAmount = CalculateGivingRollup(Contact.EntityLogicalName, donorId, MsnFp_Transaction.EntityLogicalName, MsnFp_Transaction.Fields.SiFund_Donor, MsnFp_Transaction.Fields.MsnFp_Amount,
                    filterFields, givingCriteria, startDateLastYear, endDateLastYear);

                decimal thirdYearGivingAmount = CalculateGivingRollup(Contact.EntityLogicalName, donorId, MsnFp_Transaction.EntityLogicalName, MsnFp_Transaction.Fields.SiFund_Donor, MsnFp_Transaction.Fields.MsnFp_Amount,
                    filterFields, givingCriteria, startDateThirdYear, endDateThirdYear);

                decimal fourthYearGivingAmount = CalculateGivingRollup(Contact.EntityLogicalName, donorId, MsnFp_Transaction.EntityLogicalName, MsnFp_Transaction.Fields.SiFund_Donor, MsnFp_Transaction.Fields.MsnFp_Amount,
                    filterFields, givingCriteria, startDateFourthYear, endDateFourthYear);

                decimal fifthYearGivingAmount = CalculateGivingRollup(Contact.EntityLogicalName, donorId, MsnFp_Transaction.EntityLogicalName, MsnFp_Transaction.Fields.SiFund_Donor, MsnFp_Transaction.Fields.MsnFp_Amount,
                    filterFields, givingCriteria, startDateFifthYear, endDateFifthYear);

                decimal lifetimeGivingAmount = CalculateGivingRollup(Contact.EntityLogicalName, donorId, MsnFp_Transaction.EntityLogicalName, MsnFp_Transaction.Fields.SiFund_Donor, MsnFp_Transaction.Fields.MsnFp_Amount, 
                    filterFields, givingCriteria);

                //Updateding contact/account with giving values
                if (transactionrecord.SiFund_Donor.LogicalName == Contact.EntityLogicalName)
                {
                    var parentContact = new Contact
                    {
                        Id = donorId,
                        LRx_CurrentYearGiving = new Money(currentYearGivingAmount),
                        LRx_LastYearsGiving = new Money(lastYearGivingAmount),
                        LRx_ThirdYearGiving = new Money(thirdYearGivingAmount),
                        LRx_FourthYearGiving= new Money(fourthYearGivingAmount),
                        LRx_FifthYearGiving= new Money(fifthYearGivingAmount),
                        LRx_LifetimeGivingSum = new Money(lifetimeGivingAmount)
                    };

                    _service.Update(parentContact);
                }
                else if (transactionrecord.SiFund_Donor.LogicalName == Account.EntityLogicalName)
                {
                    var parentAccount = new Account
                    {
                        Id = donorId,
                        LRx_Year0_Giving= new Money(currentYearGivingAmount),
                        LRx__Year1_Giving = new Money(lastYearGivingAmount),
                        LRx__Year2_Giving = new Money(thirdYearGivingAmount),
                        LRx__Year3_Giving = new Money(fourthYearGivingAmount),
                        LRx__Year4_Giving = new Money(fifthYearGivingAmount),
                        LRx_LifetimeGivingSum = new Money(lifetimeGivingAmount)
                    };

                    _service.Update(parentAccount);

                }
            }
           
        }

        

        public void UpdateLatestTransaction(MsnFp_Transaction transaction)
        {
            MsnFp_Transaction transactionrecord = (MsnFp_Transaction)RetrieveRecord(
               MsnFp_Transaction.EntityLogicalName,
               transaction.Id,
               MsnFp_Transaction.Fields.SiFund_Donor
           );

            ColumnSet filterFields = new ColumnSet(
             MsnFp_Transaction.Fields.MsnFp_BookDate);

            var donationCriteria = new Dictionary<string, (ConditionOperator, object)>
            {
                    { MsnFp_Transaction.Fields.StatusCode, (ConditionOperator.Equal, (int)MsnFp_Transaction_StatusCode.Completed) },
                    { MsnFp_Transaction.Fields.SiFund_TypeCode,(ConditionOperator.Equal, (int)MsnFp_Transaction_SiFund_TypeCode.Donation)}
            };


            if (transactionrecord.SiFund_Donor != null && (transactionrecord.SiFund_Donor.Id != Guid.Empty)){
                var donorId = transactionrecord.SiFund_Donor.Id;

                EntityCollection childRecords = RetrieveChildRecords(
                    MsnFp_Transaction.EntityLogicalName,
                    MsnFp_Transaction.Fields.SiFund_Donor,
                    donorId,
                    filterFields,
                    donationCriteria,
                    orderByField: MsnFp_Transaction.Fields.MsnFp_BookDate,
                    isAscending: false
                    );

                // Check if any child transactions found
                if (childRecords.Entities.Any())
                {
                   
                    var mostRecentTransaction = childRecords.Entities.FirstOrDefault();

                    DateTime mostRecentBookDate = mostRecentTransaction.GetAttributeValue<DateTime>(MsnFp_Transaction.Fields.MsnFp_BookDate);
                    
                    EntityReference mostRecentTransactionReference = new EntityReference(
                        MsnFp_Transaction.EntityLogicalName,
                        mostRecentTransaction.Id
                    );

                   
                    if (transactionrecord.SiFund_Donor.LogicalName == Contact.EntityLogicalName)
                    {
                        var parentContact = new Contact
                        {
                            Id = donorId,
                            LRx_LastTransactionDate = mostRecentBookDate, // Set the most recent MsnFp_BookDate
                            LRx_LastTransaction = mostRecentTransactionReference // Set the most recent transaction as a lookup field
                        };

                        // Update the contact record
                        _service.Update(parentContact);
                    }
                    else if (transactionrecord.SiFund_Donor.LogicalName == Account.EntityLogicalName)
                    {
                        var parentAccount = new Account
                        {
                            Id = donorId,
                            LRx_LastTransactionDate = mostRecentBookDate,
                            LRx_LastTransactionId= mostRecentTransactionReference

                        };

                        _service.Update(parentAccount);

                    }
                }

            };

        }

        public void DonorCommitmentPaid(MsnFp_Transaction transaction)
        {
            MsnFp_Transaction transactionrecord = (MsnFp_Transaction)RetrieveRecord(
              MsnFp_Transaction.EntityLogicalName,
              transaction.Id,
              MsnFp_Transaction.Fields.SiFund_RelatedDonorCommitment
            );

            ColumnSet filterFields = new ColumnSet(
                    MsnFp_Transaction.Fields.StatusCode,
                    MsnFp_Transaction.Fields.MsnFp_Amount,
                    MsnFp_Transaction.Fields.SiFund_TypeCode
             );

            var donationCriteria = new Dictionary<string, (ConditionOperator, object)>
            {
                    { MsnFp_Transaction.Fields.StatusCode, (ConditionOperator.Equal, (int)MsnFp_Transaction_StatusCode.Completed) },
                    { MsnFp_Transaction.Fields.SiFund_TypeCode,(ConditionOperator.Equal, (int)MsnFp_Transaction_SiFund_TypeCode.Donation)}
            };

            if(transactionrecord.SiFund_RelatedDonorCommitment != null && (transactionrecord.SiFund_RelatedDonorCommitment.Id != Guid.Empty))
            {
                var donorCommitmentId = transactionrecord.SiFund_RelatedDonorCommitment.Id;

                decimal donorCommitmentPaidAmount = CalculateGivingRollup(MsnFp_DonorCommitment.EntityLogicalName, donorCommitmentId, MsnFp_Transaction.EntityLogicalName, MsnFp_Transaction.Fields.SiFund_RelatedDonorCommitment, MsnFp_Transaction.Fields.MsnFp_Amount,
              filterFields, donationCriteria);

                var parentDonorCommitment = new MsnFp_DonorCommitment
                {
                    Id = donorCommitmentId,
                    LRx_TotalAmountPaid = new Money(donorCommitmentPaidAmount),

                };
                _service.Update(parentDonorCommitment);
            }

            
        }

        public void CampaignPerformanceTransaction(MsnFp_Transaction transaction)
        {

            MsnFp_Transaction transactionrecord = (MsnFp_Transaction)RetrieveRecord(
                MsnFp_Transaction.EntityLogicalName,
                transaction.Id,
                MsnFp_Transaction.Fields.LRx_Campaign,
                MsnFp_Transaction.Fields.SiFund_Appeal,
                MsnFp_Transaction.Fields.SiFund_Package
            );

            // Total Donations for Campaign
            if (transactionrecord.LRx_Campaign != null && transactionrecord.LRx_Campaign.Id != Guid.Empty)
            {
                DonationsRollup(Campaign.EntityLogicalName, transactionrecord.LRx_Campaign.Id, MsnFp_Transaction.Fields.LRx_Campaign);
            }

            // Total Donations for Appeal
            if (transactionrecord.SiFund_Appeal != null && transactionrecord.SiFund_Appeal.Id != Guid.Empty)
            {
                DonationsRollup(SiFund_Appeal.EntityLogicalName, transactionrecord.SiFund_Appeal.Id, MsnFp_Transaction.Fields.SiFund_Appeal);
            }

            // Total Donations for Package
            if (transactionrecord.SiFund_Package != null && transactionrecord.SiFund_Package.Id != Guid.Empty)
            {
                DonationsRollup(SiFund_Package.EntityLogicalName, transactionrecord.SiFund_Package.Id, MsnFp_Transaction.Fields.SiFund_Package);
               
            }

        }

        public void DonationsRollup(String entityLogicalName, Guid entityId, String parentFieldName)
        {
            ColumnSet filterFields = new ColumnSet(
                  MsnFp_Transaction.Fields.StatusCode,
                  MsnFp_Transaction.Fields.MsnFp_Amount,
                  MsnFp_Transaction.Fields.SiFund_TypeCode
            );

            var donationCriteria = new Dictionary<string, (ConditionOperator, object)>
            {
                    { MsnFp_Transaction.Fields.StatusCode, (ConditionOperator.Equal, (int)MsnFp_Transaction_StatusCode.Completed) },
                    { MsnFp_Transaction.Fields.SiFund_TypeCode,(ConditionOperator.Equal, (int)MsnFp_Transaction_SiFund_TypeCode.Donation)}
            };

            if(entityId != Guid.Empty)
            {
                decimal donationsAmount = CalculateGivingRollup(entityLogicalName, entityId, MsnFp_Transaction.EntityLogicalName, parentFieldName, MsnFp_Transaction.Fields.MsnFp_Amount,
                   filterFields, donationCriteria);

                decimal donationsCount = CalculateCount(entityLogicalName, entityId, MsnFp_Transaction.EntityLogicalName, parentFieldName,
                   filterFields, donationCriteria);


                if (entityLogicalName == Campaign.EntityLogicalName)
                {
                    var parentCampaign = new Campaign
                    {
                        Id = entityId,
                        LRx_TotalDonations = new Money(donationsAmount),
                        LRx_DonationCount = (int)donationsCount

                    };
                    _service.Update(parentCampaign);
                }

                if (entityLogicalName == SiFund_Package.EntityLogicalName)
                {
                    var parentPackage = new SiFund_Package
                    {
                        Id = entityId,
                        LRx_TotalDonations = new Money(donationsAmount),
                        LRx_DonationCount = (int)donationsCount

                    };

                    _service.Update(parentPackage);
                }

                if (entityLogicalName == SiFund_Appeal.EntityLogicalName)
                {
                    var parentAppeal = new SiFund_Appeal
                    {
                        Id = entityId,
                        LRx_TotalDonations = new Money(donationsAmount),
                        LRx_DonationCount = (int)donationsCount

                    };

                    _service.Update(parentAppeal);
                }
            }

        }



        public void CampaignPerformanceDonorCommitment(Guid donorCommitmentId)
        {

            MsnFp_DonorCommitment commitmentRecord = (MsnFp_DonorCommitment)RetrieveRecord(
                MsnFp_DonorCommitment.EntityLogicalName,
                donorCommitmentId,
                MsnFp_DonorCommitment.Fields.LRx_Campaign,
                MsnFp_DonorCommitment.Fields.SiFund_Appeal,
                MsnFp_DonorCommitment.Fields.SiFund_Package
            );

            // Total Pledges for Campaign
            if (commitmentRecord.LRx_Campaign != null && commitmentRecord.LRx_Campaign.Id != Guid.Empty)
            {
                PledgesRollup(Campaign.EntityLogicalName,commitmentRecord.LRx_Campaign.Id, MsnFp_DonorCommitment.Fields.LRx_Campaign);
            }

            // Total Pledges for Package
            if (commitmentRecord.SiFund_Package != null && commitmentRecord.SiFund_Package.Id != Guid.Empty)
            {
                PledgesRollup(SiFund_Package.EntityLogicalName, commitmentRecord.SiFund_Package.Id, MsnFp_DonorCommitment.Fields.SiFund_Package);
            }

            // Total Pledges for Appeal
            if (commitmentRecord.SiFund_Appeal != null && commitmentRecord.SiFund_Appeal.Id != Guid.Empty)
            {
                PledgesRollup(SiFund_Appeal.EntityLogicalName, commitmentRecord.SiFund_Appeal.Id, MsnFp_DonorCommitment.Fields.SiFund_Appeal);
            }

        }

        //Roll up for Campaign Pledges
        public void PledgesRollup(String entityLogicalName,Guid entityId, String parentFieldName)
        {

            ColumnSet filterFields = new ColumnSet(
                   MsnFp_DonorCommitment.Fields.MsnFp_TotalAmount,
                   MsnFp_DonorCommitment.Fields.LRx_TotalAmountBalance
             );

            var commitmentCriteria = new Dictionary<string, (ConditionOperator, object)>
            {
                //add filtering criteria for commitment if any   
            };

            if (entityId != Guid.Empty)
            {
                
                decimal pledgesAmount = CalculateGivingRollup(entityLogicalName, entityId, MsnFp_DonorCommitment.EntityLogicalName, parentFieldName, MsnFp_DonorCommitment.Fields.MsnFp_TotalAmount,
                   filterFields, commitmentCriteria);

                decimal pledgesCount = CalculateCount(entityLogicalName, entityId, MsnFp_DonorCommitment.EntityLogicalName, parentFieldName,
                   filterFields, commitmentCriteria);

                decimal pledgesBalance = CalculateGivingRollup(entityLogicalName, entityId, MsnFp_DonorCommitment.EntityLogicalName, parentFieldName, MsnFp_DonorCommitment.Fields.LRx_TotalAmountBalance,
                   filterFields, commitmentCriteria);


                if(entityLogicalName == Campaign.EntityLogicalName)
                {
                    var parentCampaign = new Campaign
                    {
                        Id = entityId,
                        LRx_TotalPledges = new Money(pledgesAmount),
                        LRx_PledgeCount = (int)pledgesCount,
                        LRx_TotalOutstandingPledges = new Money(pledgesBalance),
                    };
                    _service.Update(parentCampaign);
                }

                if (entityLogicalName == SiFund_Package.EntityLogicalName)
                {
                    var parentPackage = new SiFund_Package
                    {
                        Id = entityId,
                        LRx_TotalPledges = new Money(pledgesAmount),
                        LRx_PledgeCount = (int)pledgesCount,
                        LRx_TotalOutstandingPledges = new Money(pledgesBalance),

                    };

                    _service.Update(parentPackage);
                }

                if (entityLogicalName == SiFund_Appeal.EntityLogicalName)
                {
                    var parentAppeal = new SiFund_Appeal
                    {
                        Id = entityId,
                        LRx_TotalPledges = new Money(pledgesAmount),
                        LRx_PledgeCount = (int)pledgesCount,
                        LRx_TotalOutstandingPledges = new Money(pledgesBalance),

                    };

                    _service.Update(parentAppeal);
                }

            }
        }

        public void WriteOff(Guid writeOffId)
        {
            LRx_WriteOff writeOffRecord = (LRx_WriteOff)RetrieveRecord(
               LRx_WriteOff.EntityLogicalName,
               writeOffId,
               LRx_WriteOff.Fields.LRx_WriteOffAmount,
               LRx_WriteOff.Fields.LRx_MsnFp_DonorCommitment
           );

            ColumnSet filterFields = new ColumnSet(
                   LRx_WriteOff.Fields.LRx_WriteOffAmount
             );

            var writeOffCriteria = new Dictionary<string, (ConditionOperator, object)>
            {
                //add filtering criteria for writeoff if any   
            };

            // Total Writeoff for DonorCommitment
            if (writeOffRecord.LRx_MsnFp_DonorCommitment != null && writeOffRecord.LRx_MsnFp_DonorCommitment.Id != Guid.Empty)
            {
                Guid donorCommitmentId = writeOffRecord.LRx_MsnFp_DonorCommitment.Id;

                decimal writeOffAmount = CalculateGivingRollup(MsnFp_DonorCommitment.EntityLogicalName, donorCommitmentId, LRx_WriteOff.EntityLogicalName, LRx_WriteOff.Fields.LRx_MsnFp_DonorCommitment, LRx_WriteOff.Fields.LRx_WriteOffAmount,
                   filterFields, writeOffCriteria);

                var parentDonorCommitment = new MsnFp_DonorCommitment
                {
                    Id = donorCommitmentId,
                    LRx_TotalAmountWRiTenOff = new Money(writeOffAmount),
                  
                };
                _service.Update(parentDonorCommitment);
            }

        }

        public void Refund(Guid refundId)
        {
            LRx_Refund refundRecord = (LRx_Refund)RetrieveRecord(
               LRx_Refund.EntityLogicalName,
               refundId,
               LRx_Refund.Fields.LRx_Transaction,
               LRx_Refund.Fields.LRx_AmountReceiptAbleRefund,
               LRx_Refund.Fields.LRx_AmountNonreceiptAbleRefund,
               LRx_Refund.Fields.LRx_AmountMembershipRefund,
               LRx_Refund.Fields.LRx_AmountTaxRefunded
           );

            ColumnSet filterFields = new ColumnSet(
                   LRx_Refund.Fields.LRx_TotalAmountPaidRefund,
                   LRx_Refund.Fields.LRx_AmountNonreceiptAbleRefund,
                   LRx_Refund.Fields.LRx_AmountReceiptAbleRefund,
                   LRx_Refund.Fields.LRx_AmountMembershipRefund,
                   LRx_Refund.Fields.LRx_AmountTaxRefunded
             );

            var refundCriteria = new Dictionary<string, (ConditionOperator, object)>
            {
                //add filtering criteria for refunds if any   
            };

            // Total Writeoff for DonorCommitment
            if (refundRecord.LRx_Transaction != null && refundRecord.LRx_Transaction.Id != Guid.Empty)
            {
                Guid transactionRecordId = refundRecord.LRx_Transaction.Id;

                decimal totalAmountPaidRefund = CalculateGivingRollup(MsnFp_Transaction.EntityLogicalName, transactionRecordId,LRx_Refund.EntityLogicalName, LRx_Refund.Fields.LRx_Transaction, LRx_Refund.Fields.LRx_TotalAmountPaidRefund,
                   filterFields, refundCriteria);

                decimal totalAmountNonReceiptablRefund = CalculateGivingRollup(MsnFp_Transaction.EntityLogicalName, transactionRecordId, LRx_Refund.EntityLogicalName, LRx_Refund.Fields.LRx_Transaction, LRx_Refund.Fields.LRx_AmountNonreceiptAbleRefund,
                   filterFields, refundCriteria);

                decimal totalAmountReceiptableRefund = CalculateGivingRollup(MsnFp_Transaction.EntityLogicalName, transactionRecordId, LRx_Refund.EntityLogicalName, LRx_Refund.Fields.LRx_Transaction, LRx_Refund.Fields.LRx_AmountReceiptAbleRefund,
                   filterFields, refundCriteria);

                decimal totalAmountMembershipRefund = CalculateGivingRollup(MsnFp_Transaction.EntityLogicalName, transactionRecordId, LRx_Refund.EntityLogicalName, LRx_Refund.Fields.LRx_Transaction, LRx_Refund.Fields.LRx_AmountMembershipRefund,
                   filterFields, refundCriteria);

                decimal totalAmountTaxRefund = CalculateGivingRollup(MsnFp_Transaction.EntityLogicalName, transactionRecordId, LRx_Refund.EntityLogicalName, LRx_Refund.Fields.LRx_Transaction, LRx_Refund.Fields.LRx_AmountTaxRefunded,
                  filterFields, refundCriteria);

                MsnFp_Transaction transactionRecord = (MsnFp_Transaction)RetrieveRecord(
                       MsnFp_Transaction.EntityLogicalName,
                       transactionRecordId,
                       MsnFp_Transaction.Fields.MsnFp_Amount,
                       MsnFp_Transaction.Fields.SiFund_Amount_Receipted,
                       MsnFp_Transaction.Fields.SiFund_Amount_NonreceiptAble,
                       MsnFp_Transaction.Fields.LRx_AmountMembership,
                       MsnFp_Transaction.Fields.SiFund_Amount_Tax
                       
                );

                Money amount = transactionRecord.MsnFp_Amount;
                Money totalAmountRefunded = new Money(totalAmountPaidRefund);

                //perform new amounts for transaction
                decimal newAmountReceipted = (transactionRecord.SiFund_Amount_Receipted?.Value ?? 0m) - (refundRecord.LRx_AmountReceiptAbleRefund?.Value ?? 0m);
                decimal newAmountNonReceipted = (transactionRecord.SiFund_Amount_NonreceiptAble?.Value ?? 0m) - (refundRecord.LRx_AmountNonreceiptAbleRefund?.Value ?? 0m);
                decimal newAmountMembership = (transactionRecord.LRx_AmountMembership?.Value ?? 0m) - (refundRecord.LRx_AmountMembershipRefund?.Value ?? 0m);
                decimal newAmountTax = (transactionRecord.SiFund_Amount_Tax?.Value ?? 0m) - (refundRecord.LRx_AmountTaxRefunded?.Value ?? 0m);


                decimal refundedValue = totalAmountRefunded != null ? totalAmountRefunded.Value : 0m;
                decimal amountValue = amount != null ? amount.Value : 0m;

                // Set the status code based on the comparison.
                MsnFp_Transaction_StatusCode statusCode = refundedValue >= amountValue
                    ? MsnFp_Transaction_StatusCode.Refund
                    : MsnFp_Transaction_StatusCode.PartialRefund;


                var parentTransaction = new MsnFp_Transaction
                {
                    Id = transactionRecordId,
                    LRx_TotalAmountRefunded = new Money(totalAmountPaidRefund),
                    LRx_AmountRefunded= new Money(totalAmountReceiptableRefund),
                    LRx_AmountMembershipRefunded = new Money(totalAmountMembershipRefund),
                    LRx_AmountTaxRefunded= new Money(totalAmountTaxRefund),
                    LRx_AmountNonreceiptAbleRefunded= new Money(totalAmountNonReceiptablRefund),
                    SiFund_Amount_Receipted = new Money(newAmountReceipted),
                    SiFund_Amount_NonreceiptAble = new Money(newAmountNonReceipted),
                    LRx_AmountMembership = new Money(newAmountMembership),
                    SiFund_Amount_Tax= new Money(newAmountTax),
                    StatusCode = statusCode

                };
                _service.Update(parentTransaction);
            }

        }

        public void CreatePledgeCommitments(MsnFp_PaymentSchedule paymentSchedule)
        {
            MsnFp_PaymentSchedule paymentScheduleRecord = (MsnFp_PaymentSchedule)RetrieveRecord(
              MsnFp_PaymentSchedule.EntityLogicalName,
              paymentSchedule.Id,
              MsnFp_PaymentSchedule.Fields.MsnFp_FirstPaymentDate,
              MsnFp_PaymentSchedule.Fields.MsnFp_FrequencyInterval,
              MsnFp_PaymentSchedule.Fields.MsnFp_Frequency,
              MsnFp_PaymentSchedule.Fields.MsnFp_RecurringAmount,
              MsnFp_PaymentSchedule.Fields.SiFund_ScheduleTypeCode,
              MsnFp_PaymentSchedule.Fields.SiFund_Donor,
              MsnFp_PaymentSchedule.Fields.LRx_Campaign
          ); 

            //run code only for pledge schedule
            if (paymentScheduleRecord.SiFund_ScheduleTypeCode == MsnFp_PaymentSchedule_SiFund_ScheduleTypeCode.PledgeSchedule)
            {
                if( paymentScheduleRecord.MsnFp_FrequencyInterval!=null && paymentScheduleRecord.MsnFp_Frequency != null && paymentSchedule.MsnFp_RecurringAmount !=null)
                {

                    //set frequency interval variables
                    var startDate = paymentScheduleRecord.MsnFp_FirstPaymentDate ?? DateTime.Today;
                    var frequency = paymentScheduleRecord.MsnFp_Frequency;
                    int? intervals = paymentScheduleRecord.MsnFp_FrequencyInterval;
                    Money amount = paymentSchedule.MsnFp_RecurringAmount;

                    decimal commitmentAmount = (amount?.Value ?? 0m) / (intervals ?? 1);

                  
                    for (int i = 0; i < (intervals ?? 1); i++)
                    {
                        // Calculate the `msnfp_bookdate` based on frequency
                        var bookDate = startDate;

                        switch (frequency)
                        {
                            case MsnFp_PaymentSchedule_MsnFp_Frequency.Days:
                                bookDate = startDate.AddDays(i);
                                break;
                            case MsnFp_PaymentSchedule_MsnFp_Frequency.Weeks:
                                bookDate = startDate.AddDays(i * 7);
                                break;
                            case MsnFp_PaymentSchedule_MsnFp_Frequency.Months:
                                bookDate = startDate.AddMonths(i);
                                break;
                            case MsnFp_PaymentSchedule_MsnFp_Frequency.Years:
                                bookDate = startDate.AddYears(i);
                                break;
                            default:
                                throw new ArgumentException("Invalid frequency value");
                        }

                        // Create the child commitment record
                        var childCommitment = new MsnFp_DonorCommitment
                        {
                            SiFund_RelatedSchedule = paymentScheduleRecord.ToEntityReference(),
                            SiFund_Donor = paymentScheduleRecord.SiFund_Donor,
                            LRx_Campaign = paymentScheduleRecord.LRx_Campaign,
                            MsnFp_TotalAmount = new Money(commitmentAmount),
                            MsnFp_BookDate = bookDate 
                        };

                        _service.Create(childCommitment);
                    }
                }
            }

        }


        //-- START OF HELPER METHODS
        // Method to perform dynamic roll-up calculation for giving amounts
        public decimal CalculateGivingRollup(string parentEntityLogicalName, Guid parentId, string childEntityLogicalName, string childToParentLookupField,  string rollupField, ColumnSet filterFields,
            Dictionary<string, (ConditionOperator, object)> criteria, DateTime? startDate = null, DateTime? endDate = null)
        {
    
            QueryExpression query = new QueryExpression(childEntityLogicalName)
            {
                ColumnSet = filterFields, 
                Criteria = new FilterExpression
                {
                    Conditions =
                {
                    new ConditionExpression(childToParentLookupField, ConditionOperator.Equal, parentId) // Link to parent record
                }
                }
            };

            foreach (var criterion in criteria)
            {
                query.Criteria.AddCondition(new ConditionExpression(criterion.Key, criterion.Value.Item1, criterion.Value.Item2));
            }

            // Add date range condition for MsnFp_BookDate if startDate and endDate are provided
            if (startDate.HasValue && endDate.HasValue)
            {
                query.Criteria.AddCondition(new ConditionExpression(MsnFp_Transaction.Fields.MsnFp_BookDate, ConditionOperator.GreaterEqual, startDate.Value));
                query.Criteria.AddCondition(new ConditionExpression(MsnFp_Transaction.Fields.MsnFp_BookDate, ConditionOperator.LessEqual, endDate.Value));
            }

            // Retrieve the child records based on the query
            EntityCollection childRecords = _service.RetrieveMultiple(query);

            decimal total = 0; //default to 0 if no rollup child records found

            total = childRecords.Entities
                .Where(e => e.Contains(rollupField))
                .Sum(e => ((Money)e[rollupField]).Value);

            return total;
        }

        public decimal CalculateCount(string parentEntityLogicalName, Guid parentId, string childEntityLogicalName, string childToParentLookupField, ColumnSet filterFields,
            Dictionary<string, (ConditionOperator, object)> criteria, DateTime? startDate = null, DateTime? endDate = null)
        {

            QueryExpression query = new QueryExpression(childEntityLogicalName)
            {
                ColumnSet = filterFields,
                Criteria = new FilterExpression
                {
                    Conditions =
                {
                    new ConditionExpression(childToParentLookupField, ConditionOperator.Equal, parentId) // Link to parent record
                }
                }
            };

            foreach (var criterion in criteria)
            {
                query.Criteria.AddCondition(new ConditionExpression(criterion.Key, criterion.Value.Item1, criterion.Value.Item2));
            }

            // Add date range condition for MsnFp_BookDate if startDate and endDate are provided
            if (startDate.HasValue && endDate.HasValue)
            {
                query.Criteria.AddCondition(new ConditionExpression(MsnFp_Transaction.Fields.MsnFp_BookDate, ConditionOperator.GreaterEqual, startDate.Value));
                query.Criteria.AddCondition(new ConditionExpression(MsnFp_Transaction.Fields.MsnFp_BookDate, ConditionOperator.LessEqual, endDate.Value));
            }

            // Retrieve the child records based on the query
            EntityCollection childRecords = _service.RetrieveMultiple(query);

            decimal count = childRecords.Entities.Count;

            return count;
        }

        public EntityCollection RetrieveChildRecords(string childEntityLogicalName, string childToParentLookupField, Guid parentId, ColumnSet filterFields,
                Dictionary<string, (ConditionOperator, object)> criteria, string orderByField = null, bool isAscending = true)
        {
            QueryExpression query = new QueryExpression(childEntityLogicalName)
            {
                ColumnSet = filterFields,
                Criteria = new FilterExpression
                {
                    Conditions =
            {
                new ConditionExpression(childToParentLookupField, ConditionOperator.Equal, parentId) // Link to parent record
            }
                }
            };

            foreach (var criterion in criteria)
            {
                query.Criteria.AddCondition(new ConditionExpression(criterion.Key, criterion.Value.Item1, criterion.Value.Item2));
            }

            if (!string.IsNullOrEmpty(orderByField))
            {
                query.AddOrder(orderByField, isAscending ? OrderType.Ascending : OrderType.Descending);
            }

            return _service.RetrieveMultiple(query);
        }

        public static Dictionary<string, (DateTime StartDate, DateTime EndDate)> GetFiscalYears(int numberOfYears)
        {
            var fiscalYears = new Dictionary<string, (DateTime StartDate, DateTime EndDate)>();

            // Define the fiscal year start and end dates for each year
            for (int i = 0; i <= numberOfYears; i++)
            {
                // year of FY
                int fiscalYear = DateTime.Now.Year - i;

                // start and endDates of FY
                DateTime startDate = new DateTime(fiscalYear, 7, 1);
                DateTime endDate = new DateTime(fiscalYear + 1, 6, 30);

                // Add the fiscal year to the dictionary
                fiscalYears.Add($"Year {i}", (StartDate: startDate, EndDate: endDate));
            }

            return fiscalYears;
        }


        //retrieve single record
        public Entity RetrieveRecord(string entityName, Guid entityId, params string[] fieldsToRetrieve)
        {
            
            ColumnSet columns = new ColumnSet(fieldsToRetrieve);

            try
            {
                Entity record = _service.Retrieve(entityName, entityId, columns);
                return record;
            }
            catch (Exception ex)
            {
     
                throw new InvalidOperationException($"An error occurred while retrieving the record: {ex.Message}", ex);
            }
        }

    }
}
