using CrmEarlyBound;
using DataverseModel;
using Fundraising_Engagement.Plugins.Plugins;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.PluginTelemetry;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlTypes;
using System.IdentityModel.Metadata;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;

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

        public void YearlyGivingRecalculation(Guid donorId, string donorLogicalName)
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

            if (donorId != Guid.Empty)
            {

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
                if (donorLogicalName == Contact.EntityLogicalName)
                {
                    var parentContact = new Contact
                    {
                        Id = donorId,
                        LRx_CurrentYearGiving = new Money(currentYearGivingAmount),
                        LRx_LastYearsGiving = new Money(lastYearGivingAmount),
                        LRx_ThirdYearGiving = new Money(thirdYearGivingAmount),
                        LRx_FourthYearGiving = new Money(fourthYearGivingAmount),
                        LRx_FifthYearGiving = new Money(fifthYearGivingAmount),
                        LRx_LifetimeGivingSum = new Money(lifetimeGivingAmount)
                    };

                    _service.Update(parentContact);
                }
                else if (donorLogicalName == Account.EntityLogicalName)
                {
                    var parentAccount = new Account
                    {
                        Id = donorId,
                        LRx_Year0_Giving = new Money(currentYearGivingAmount),
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

            if (transactionrecord.SiFund_Donor != null && transactionrecord.SiFund_Donor.Id != Guid.Empty)
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
                        LRx_FourthYearGiving = new Money(fourthYearGivingAmount),
                        LRx_FifthYearGiving = new Money(fifthYearGivingAmount),
                        LRx_LifetimeGivingSum = new Money(lifetimeGivingAmount)
                    };

                    _service.Update(parentContact);
                }
                else if (transactionrecord.SiFund_Donor.LogicalName == Account.EntityLogicalName)
                {
                    var parentAccount = new Account
                    {
                        Id = donorId,
                        LRx_Year0_Giving = new Money(currentYearGivingAmount),
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

        public void LastestTransactionRecalculation(Guid donorId, string donorLogicalName)
        {
            ColumnSet filterFields = new ColumnSet(
             MsnFp_Transaction.Fields.MsnFp_BookDate);

            var donationCriteria = new Dictionary<string, (ConditionOperator, object)>
            {
                    { MsnFp_Transaction.Fields.StatusCode, (ConditionOperator.Equal, (int)MsnFp_Transaction_StatusCode.Completed) },
                    { MsnFp_Transaction.Fields.SiFund_TypeCode,(ConditionOperator.Equal, (int)MsnFp_Transaction_SiFund_TypeCode.Donation)}
            };


            if (donorId != Guid.Empty)
            {
                //var donorId = transactionrecord.SiFund_Donor.Id;

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


                    if (donorLogicalName == Contact.EntityLogicalName)
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
                    else if (donorLogicalName == Account.EntityLogicalName)
                    {
                        var parentAccount = new Account
                        {
                            Id = donorId,
                            LRx_LastTransactionDate = mostRecentBookDate,
                            LRx_LastTransactionId = mostRecentTransactionReference

                        };

                        _service.Update(parentAccount);

                    }
                }

            };
        }



        public void UpdateLatestTransaction(MsnFp_Transaction transaction, MsnFp_Transaction transactionrecord)
        {
            if (transaction.Id != Guid.Empty) {
               transactionrecord = (MsnFp_Transaction)RetrieveRecord(
                   MsnFp_Transaction.EntityLogicalName,
                   transaction.Id,
                   MsnFp_Transaction.Fields.SiFund_Donor,
                   MsnFp_Transaction.Fields.LRx_Event,
                   MsnFp_Transaction.Fields.LRx_EventTeam
               );
            }

            ColumnSet filterFields = new ColumnSet(
             MsnFp_Transaction.Fields.MsnFp_BookDate);

            var donationCriteria = new Dictionary<string, (ConditionOperator, object)>
            {
                    { MsnFp_Transaction.Fields.StatusCode, (ConditionOperator.Equal, (int)MsnFp_Transaction_StatusCode.Completed) },
                    { MsnFp_Transaction.Fields.SiFund_TypeCode,(ConditionOperator.Equal, (int)MsnFp_Transaction_SiFund_TypeCode.Donation)}
            };


            if (transactionrecord.SiFund_Donor != null && transactionrecord.SiFund_Donor.Id != Guid.Empty)
            {
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
                            LRx_LastTransactionId = mostRecentTransactionReference

                        };

                        _service.Update(parentAccount);

                    }
                }

            };

            if (transactionrecord.LRx_Event != null && transactionrecord.LRx_Event.Id != Guid.Empty)
            {
                QueryExpression query = new QueryExpression(MsnFp_Transaction.EntityLogicalName)
                {
                    ColumnSet = new ColumnSet(MsnFp_Transaction.Fields.MsnFp_Amount),
                    Criteria = new FilterExpression
                    {
                        Conditions =
                        {
                            // Add condition to filter transactions by the related LRx_Event
                            new ConditionExpression(MsnFp_Transaction.Fields.LRx_Event, ConditionOperator.Equal, transactionrecord.LRx_Event.Id),

                            // Filter by status code (Completed)
                            new ConditionExpression(MsnFp_Transaction.Fields.StatusCode, ConditionOperator.Equal, (int)MsnFp_Transaction_StatusCode.Completed),

                            // Filter by type code (Donation)
                            new ConditionExpression(MsnFp_Transaction.Fields.SiFund_TypeCode, ConditionOperator.Equal, (int)MsnFp_Transaction_SiFund_TypeCode.Donation)
                        }
                    }
                };

                EntityCollection donationRecords = _service.RetrieveMultiple(query);

                // Get the count of records and store it in the out parameter
                var donationCount = donationRecords.Entities.Count;

                // Sum up the values using LINQ for cleaner code
                decimal totalEventDonationRevenue = donationRecords.Entities
                    .Where(record => record.Contains(MsnFp_Transaction.Fields.MsnFp_Amount) && record[MsnFp_Transaction.Fields.MsnFp_Amount] != null)
                    .Sum(record =>
                    {
                        if (record[MsnFp_Transaction.Fields.MsnFp_Amount] is Money moneyValue)
                        {
                            return moneyValue.Value; // If it's of type Money, return the decimal value.
                        }
                        else if (record[MsnFp_Transaction.Fields.MsnFp_Amount] is int intValue)
                        {
                            return (decimal)intValue; // If it's an int, convert to decimal.
                        }
                        else
                        {
                            return 0m; // If it's neither Money nor int, return 0 or handle as appropriate.
                        }
                    });
                var parentEventDonation = new LRx_Event
                {
                    Id = transactionrecord.LRx_Event.Id,
                    LRx_TotalDonations = new Money(totalEventDonationRevenue),
                    LRx_Donations = (int)donationCount
                };
                _service.Update(parentEventDonation);
                
            }

            if (transactionrecord.LRx_EventTeam != null && transactionrecord.LRx_EventTeam.Id != Guid.Empty)
            {
                QueryExpression query = new QueryExpression(MsnFp_Transaction.EntityLogicalName)
                {
                    ColumnSet = new ColumnSet(MsnFp_Transaction.Fields.MsnFp_Amount),
                    Criteria = new FilterExpression
                    {
                        Conditions =
                        {
                            // Add condition to filter transactions by the related LRx_Event
                            new ConditionExpression(MsnFp_Transaction.Fields.LRx_EventTeam, ConditionOperator.Equal, transactionrecord.LRx_EventTeam.Id),

                            // Filter by status code (Completed)
                            new ConditionExpression(MsnFp_Transaction.Fields.StatusCode, ConditionOperator.Equal, (int)MsnFp_Transaction_StatusCode.Completed),

                            // Filter by type code (Donation)
                            new ConditionExpression(MsnFp_Transaction.Fields.SiFund_TypeCode, ConditionOperator.Equal, (int)MsnFp_Transaction_SiFund_TypeCode.Donation)
                        }
                    }
                };

                EntityCollection donationTeamRecords = _service.RetrieveMultiple(query);

                // Get the count of records and store it in the out parameter
                var donationCount = donationTeamRecords.Entities.Count;

                // Sum up the values using LINQ for cleaner code
                decimal totalTeamDonationRevenue = donationTeamRecords.Entities
                    .Where(record => record.Contains(MsnFp_Transaction.Fields.MsnFp_Amount) && record[MsnFp_Transaction.Fields.MsnFp_Amount] != null)
                    .Sum(record =>
                    {
                        if (record[MsnFp_Transaction.Fields.MsnFp_Amount] is Money moneyValue)
                        {
                            return moneyValue.Value; // If it's of type Money, return the decimal value.
                        }
                        else if (record[MsnFp_Transaction.Fields.MsnFp_Amount] is int intValue)
                        {
                            return (decimal)intValue; // If it's an int, convert to decimal.
                        }
                        else
                        {
                            return 0m; // If it's neither Money nor int, return 0 or handle as appropriate.
                        }
                    });
                var parentTeamDonation = new LRx_EventTeam
                {
                    Id = transactionrecord.LRx_EventTeam.Id,
                    LRx_Donations = new Money(totalTeamDonationRevenue)
                };
                _service.Update(parentTeamDonation);

            }
        }

        public void DonorCommitmentPaidRecalculation(Guid relatedDonorCommitment)
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

            if (relatedDonorCommitment != Guid.Empty)
            {
                var donorCommitmentId = relatedDonorCommitment;

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

            if (transactionrecord.SiFund_RelatedDonorCommitment != null && (transactionrecord.SiFund_RelatedDonorCommitment.Id != Guid.Empty))
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

            if (entityId != Guid.Empty)
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
                PledgesRollup(Campaign.EntityLogicalName, commitmentRecord.LRx_Campaign.Id, MsnFp_DonorCommitment.Fields.LRx_Campaign);
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
        public void PledgesRollup(String entityLogicalName, Guid entityId, String parentFieldName)
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


                if (entityLogicalName == Campaign.EntityLogicalName)
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

        public void WriteOffRecalculation(Guid donorCommitmentId)
        {
            ColumnSet filterFields = new ColumnSet(
                   LRx_WriteOff.Fields.LRx_WriteOffAmount
             );

            var writeOffCriteria = new Dictionary<string, (ConditionOperator, object)>
            {
                //add filtering criteria for writeoff if any   
            };

            // Total Writeoff for DonorCommitment
            if (donorCommitmentId != Guid.Empty)
            {
                
                decimal writeOffAmount = CalculateGivingRollup(MsnFp_DonorCommitment.EntityLogicalName, donorCommitmentId, LRx_WriteOff.EntityLogicalName, LRx_WriteOff.Fields.LRx_MsnFp_DonorCommitment, LRx_WriteOff.Fields.LRx_WriteOffAmount,
                   filterFields, writeOffCriteria);

                var parentDonorCommitment = new MsnFp_DonorCommitment
                {
                    Id = donorCommitmentId,
                    LRx_TotalAmountWRiTenOff = new Money(writeOffAmount),

                };
                _service.Update(parentDonorCommitment);
                CampaignPerformanceDonorCommitment(donorCommitmentId);
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
                CampaignPerformanceDonorCommitment(donorCommitmentId);
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

                decimal totalAmountPaidRefund = CalculateGivingRollup(MsnFp_Transaction.EntityLogicalName, transactionRecordId, LRx_Refund.EntityLogicalName, LRx_Refund.Fields.LRx_Transaction, LRx_Refund.Fields.LRx_TotalAmountPaidRefund,
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
                    LRx_AmountRefunded = new Money(totalAmountReceiptableRefund),
                    LRx_AmountMembershipRefunded = new Money(totalAmountMembershipRefund),
                    LRx_AmountTaxRefunded = new Money(totalAmountTaxRefund),
                    LRx_AmountNonreceiptAbleRefunded = new Money(totalAmountNonReceiptablRefund),
                    SiFund_Amount_Receipted = new Money(newAmountReceipted),
                    SiFund_Amount_NonreceiptAble = new Money(newAmountNonReceipted),
                    LRx_AmountMembership = new Money(newAmountMembership),
                    SiFund_Amount_Tax = new Money(newAmountTax),
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
                if (paymentScheduleRecord.MsnFp_FrequencyInterval != null && paymentScheduleRecord.MsnFp_Frequency != null && paymentSchedule.MsnFp_RecurringAmount != null)
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

        public void UpdateEventRegistrationRevenue(Guid registrationID, LRx_Registrations registrationRecord)
        {
            // Retrieve the registration record based on the registrationID
            if (registrationID != Guid.Empty) {
                registrationRecord = (LRx_Registrations)RetrieveRecord(
                    LRx_Registrations.EntityLogicalName,
                    registrationID,
                    LRx_Registrations.Fields.LRx_Event,
                    LRx_Registrations.Fields.LRx_EventTicket,
                    LRx_Registrations.Fields.LRx_EventTable,
                    LRx_Registrations.Fields.LRx_EventTeam
                );
            }
            
            // Check if the registration record and event are valid
            if (registrationRecord != null)
            {
                if (registrationRecord.LRx_Event != null &&
                    registrationRecord.LRx_Event.Id != Guid.Empty)
                {
                    decimal totalRegistrationRevenue = 0;
                    int registrationCount = 0;
                    // Calculate the total registration revenue and get the count of records
                    totalRegistrationRevenue = CalculateAmountRevenue(
                        LRx_Registrations.EntityLogicalName,
                        LRx_Registrations.Fields.LRx_PricePerRegistration,
                        LRx_Registrations.Fields.LRx_Event,
                        registrationRecord.LRx_Event.Id,
                        out registrationCount
                    );
                    var parentEvent = new LRx_Event
                    {
                        Id = registrationRecord.LRx_Event.Id,
                        LRx_TotalRegistrations = new Money(totalRegistrationRevenue),
                        LRx_Registrations = (int)registrationCount
                    };
                    _service.Update(parentEvent);
                }

                if (registrationRecord.LRx_EventTable != null &&
                    registrationRecord.LRx_EventTable.Id != Guid.Empty) 
                {
                    decimal totalRegistrationMemberPrice = 0;
                    int registrationMemberCount = 0;

                    totalRegistrationMemberPrice = CalculateAmountRevenue(
                        LRx_Registrations.EntityLogicalName,
                        LRx_Registrations.Fields.LRx_PricePerRegistration,
                        LRx_Registrations.Fields.LRx_EventTable,
                        registrationRecord.LRx_EventTable.Id,
                        out registrationMemberCount
                    );

                    var parentEventTable = new LRx_EventTable
                    {
                        Id = registrationRecord.LRx_EventTable.Id,
                        LRx_Members = (int)registrationMemberCount
                    };
                    _service.Update(parentEventTable);
                }

                if (registrationRecord.LRx_EventTeam != null &&
                    registrationRecord.LRx_EventTeam.Id != Guid.Empty) 
                {
                    decimal totalRegistrationTeamMemberPrice = 0;
                    int registrationTeamMemberCount = 0;

                    totalRegistrationTeamMemberPrice = CalculateAmountRevenue(
                        LRx_Registrations.EntityLogicalName,
                        LRx_Registrations.Fields.LRx_PricePerRegistration,
                        LRx_Registrations.Fields.LRx_EventTeam,
                        registrationRecord.LRx_EventTeam.Id,
                        out registrationTeamMemberCount
                    );

                    var parentEventTeamTable = new LRx_EventTeam
                    {
                        Id = registrationRecord.LRx_EventTeam.Id,
                        LRx_TotalRegistrant = (int)registrationTeamMemberCount
                    };
                    _service.Update(parentEventTeamTable);
                }             
            }

            LRx_EventTicket eventTicketRecord;

            if (registrationRecord.LRx_EventTicket != null &&
            registrationRecord.LRx_EventTicket.Id != Guid.Empty)
            {
                eventTicketRecord = (LRx_EventTicket)RetrieveRecord(
                    LRx_EventTicket.EntityLogicalName,
                    registrationRecord.LRx_EventTicket.Id,
                    LRx_EventTicket.Fields.LRx_TableTicket,
                    LRx_EventTicket.Fields.LRx_EventTicketId
                );
            }
            else
            {
                LRx_EventTable eventTable = (LRx_EventTable)RetrieveRecord(
                    LRx_EventTable.EntityLogicalName,
                    registrationRecord.LRx_EventTable.Id,
                    LRx_EventTable.Fields.LRx_EventTicket
                );

                eventTicketRecord = (LRx_EventTicket)RetrieveRecord(
                    LRx_EventTicket.EntityLogicalName,
                    eventTable.LRx_EventTicket.Id,
                    LRx_EventTicket.Fields.LRx_TableTicket,
                    LRx_EventTicket.Fields.LRx_EventTicketId
                );
            }


            decimal totalTicketRevenue = 0;
            int TicketCount = 0;
            bool isTableTicket = (bool)eventTicketRecord.LRx_TableTicket.Value;

            if (isTableTicket)
            { //calculate table revenue
                totalTicketRevenue = CalculateAmountRevenue(
                    LRx_EventTable.EntityLogicalName,
                    LRx_EventTable.Fields.LRx_PricePerTable,
                    LRx_EventTable.Fields.LRx_EventTicket,
                    eventTicketRecord.LRx_EventTicketId.Value,
                    out TicketCount
                );

                if (eventTicketRecord.LRx_EventTicketId != Guid.Empty && eventTicketRecord.LRx_EventTicketId.HasValue)
                {
                    var parentEventTicket = new LRx_EventTicket
                    {
                        Id = eventTicketRecord.LRx_EventTicketId.Value,
                        LRx_TotalRegistrationsOld = new Money(totalTicketRevenue),
                        LRx_TicketsOldCount = (int)TicketCount
                    };
                    _service.Update(parentEventTicket);
                }
            }
            else //calculate individual registrations
            {
                // Calculate the total registration revenue and get the count of records
                totalTicketRevenue = CalculateAmountRevenue(
                    LRx_Registrations.EntityLogicalName,
                    LRx_Registrations.Fields.LRx_PricePerRegistration,
                    LRx_Registrations.Fields.LRx_EventTicket,
                    registrationRecord.LRx_EventTicket.Id,
                    out TicketCount
                );

                var parentEventTicket = new LRx_EventTicket
                {
                    Id = registrationRecord.LRx_EventTicket.Id,
                    LRx_TotalRegistrationsOld = new Money(totalTicketRevenue),
                    LRx_TicketsOldCount = (int)TicketCount
                };
                _service.Update(parentEventTicket);
            }

            LRx_Event EventParentRecord = (LRx_Event)RetrieveRecord(
                LRx_Event.EntityLogicalName,
                registrationRecord.LRx_Event.Id,
                LRx_Event.Fields.LRx_Campaign,
                LRx_Event.Fields.LRx_SiFund_Appeal,
                LRx_Event.Fields.LRx_SiFund_Package
            );

            decimal totalCampaignRegistrationRevenue = 0;
            decimal totalCampaignRegistrationCount = 0;
            decimal totalAppealRegistrationRevenue = 0;
            decimal totalAppealRegistrationCount = 0;
            decimal totalPackageRegistrationRevenue = 0;
            decimal totalPackageRegistrationCount = 0;
            int tempHolder = 0;

            if (EventParentRecord.LRx_Campaign != null)
            {
                totalCampaignRegistrationRevenue = CalculateAmountRevenue(
                    LRx_Event.EntityLogicalName,
                    LRx_Event.Fields.LRx_TotalRegistrations,
                    LRx_Event.Fields.LRx_Campaign,
                    EventParentRecord.LRx_Campaign.Id,
                    out tempHolder
                );

                totalCampaignRegistrationCount = CalculateAmountRevenue(
                    LRx_Event.EntityLogicalName,
                    LRx_Event.Fields.LRx_Registrations,
                    LRx_Event.Fields.LRx_Campaign,
                    EventParentRecord.LRx_Campaign.Id,
                    out tempHolder
                );

                var parentCampaign = new Campaign
                {
                    Id = EventParentRecord.LRx_Campaign.Id,
                    LRx_TotalRegistrations = new Money(totalCampaignRegistrationRevenue),
                    LRx_RegistrationCount = (int)totalCampaignRegistrationCount
                };
                _service.Update(parentCampaign);
            }

            if (EventParentRecord.LRx_SiFund_Appeal != null)
            {
                totalAppealRegistrationRevenue = CalculateAmountRevenue(
                    LRx_Event.EntityLogicalName,
                    LRx_Event.Fields.LRx_TotalRegistrations,
                    LRx_Event.Fields.LRx_SiFund_Appeal,
                    EventParentRecord.LRx_SiFund_Appeal.Id,
                    out tempHolder
                );
                totalAppealRegistrationCount = CalculateAmountRevenue(
                    LRx_Event.EntityLogicalName,
                    LRx_Event.Fields.LRx_Registrations,
                    LRx_Event.Fields.LRx_SiFund_Appeal,
                    EventParentRecord.LRx_SiFund_Appeal.Id,
                    out tempHolder
                );
                var parentAppeal = new SiFund_Appeal
                {
                    Id = EventParentRecord.LRx_SiFund_Appeal.Id,
                    LRx_TotalRegistrations = new Money(totalAppealRegistrationRevenue),
                    LRx_RegistrationCount = (int)totalAppealRegistrationCount
                };
                _service.Update(parentAppeal);
            }

            if (EventParentRecord.LRx_SiFund_Package != null)
            {
                totalPackageRegistrationRevenue = CalculateAmountRevenue(
                    LRx_Event.EntityLogicalName,
                    LRx_Event.Fields.LRx_TotalRegistrations,
                    LRx_Event.Fields.LRx_SiFund_Package,
                    EventParentRecord.LRx_SiFund_Package.Id,
                    out tempHolder
                );
                totalPackageRegistrationCount = CalculateAmountRevenue(
                    LRx_Event.EntityLogicalName,
                    LRx_Event.Fields.LRx_Registrations,
                    LRx_Event.Fields.LRx_SiFund_Package,
                    EventParentRecord.LRx_SiFund_Package.Id,
                    out tempHolder
                );
                var parentPackage = new SiFund_Package
                {
                    Id = EventParentRecord.LRx_SiFund_Package.Id,
                    LRx_TotalRegistrations = new Money(totalPackageRegistrationRevenue),
                    LRx_RegistrationCount = (int)totalPackageRegistrationCount
                };
                _service.Update(parentPackage);
            }
        }

        public void UpdateEventProductRevenue(Guid productID, LRx_Product productRecord)
        {
            if (productID != Guid.Empty) {
                productRecord = (LRx_Product)RetrieveRecord(
                    LRx_Product.EntityLogicalName,
                    productID,
                    LRx_Product.Fields.LRx_Event,
                    LRx_Product.Fields.LRx_EventProduct
                );
            }
       
            if (productRecord != null &&
                productRecord.LRx_Event != null &&
                productRecord.LRx_Event.Id != Guid.Empty)
            {
                decimal totalProductRevenue = 0;
                int productCount = 0;

                totalProductRevenue = CalculateAmountRevenue(
                    LRx_Product.EntityLogicalName,
                    LRx_Product.Fields.LRx_ProductAmount,
                    LRx_Product.Fields.LRx_Event,
                    productRecord.LRx_Event.Id,
                    out productCount
                );
                var parentEvent = new LRx_Event
                {
                    Id = productRecord.LRx_Event.Id,
                    LRx_TotalProducts = new Money(totalProductRevenue),
                    LRx_Products = (int)productCount
                };
                _service.Update(parentEvent);
            }

            LRx_EventProduct eventProductRecord;

            eventProductRecord = (LRx_EventProduct)RetrieveRecord(
                LRx_EventProduct.EntityLogicalName,
                productRecord.LRx_EventProduct.Id,
                LRx_EventProduct.Fields.LRx_EventProductId
            );

            decimal totalEventProductRevenue = 0;
            decimal EventProductCount = 0;
            int tempHolder = 0;

            totalEventProductRevenue = CalculateAmountRevenue(
                    LRx_Product.EntityLogicalName,
                    LRx_Product.Fields.LRx_ProductAmount,
                    LRx_Product.Fields.LRx_EventProduct,
                    eventProductRecord.LRx_EventProductId.Value,
                    out tempHolder
             );

            EventProductCount = CalculateAmountRevenue(
                    LRx_Product.EntityLogicalName,
                    LRx_Product.Fields.LRx_Quantity,
                    LRx_Product.Fields.LRx_EventProduct,
                    eventProductRecord.LRx_EventProductId.Value,
                    out tempHolder
             );

            var parentEventProduct = new LRx_EventProduct
            {
                Id = eventProductRecord.LRx_EventProductId.Value,
                LRx_TotalProductsOld = new Money(totalEventProductRevenue),
                LRx_QuantitySold = (int)EventProductCount
            };
            _service.Update(parentEventProduct);

            LRx_Event EventParentRecord = (LRx_Event)RetrieveRecord(
                LRx_Event.EntityLogicalName,
                productRecord.LRx_Event.Id,
                LRx_Event.Fields.LRx_Campaign,
                LRx_Event.Fields.LRx_SiFund_Appeal,
                LRx_Event.Fields.LRx_SiFund_Package
            );

            decimal totalCampaignProductRevenue = 0;
            decimal totalCampaignProductCount = 0;
            decimal totalAppealProductRevenue = 0;
            decimal totalAppealProductCount = 0;
            decimal totalPackageProductRevenue = 0;
            decimal totalPackageProductCount = 0;

            if (EventParentRecord.LRx_Campaign != null)
            {
                totalCampaignProductRevenue = CalculateAmountRevenue(
                    LRx_Event.EntityLogicalName,
                    LRx_Event.Fields.LRx_TotalProducts,
                    LRx_Event.Fields.LRx_Campaign,
                    EventParentRecord.LRx_Campaign.Id,
                    out tempHolder
                );

                totalCampaignProductCount = CalculateAmountRevenue(
                    LRx_Event.EntityLogicalName,
                    LRx_Event.Fields.LRx_Products,
                    LRx_Event.Fields.LRx_Campaign,
                    EventParentRecord.LRx_Campaign.Id,
                    out tempHolder
                );

                var parentCampaign = new Campaign
                {
                    Id = EventParentRecord.LRx_Campaign.Id,
                    LRx_TotalProductsSold = new Money(totalCampaignProductRevenue),
                    LRx_ProductsOldCount = (int)totalCampaignProductCount
                };
                _service.Update(parentCampaign);
            }

            if (EventParentRecord.LRx_SiFund_Appeal != null)
            {
                totalAppealProductRevenue = CalculateAmountRevenue(
                    LRx_Event.EntityLogicalName,
                    LRx_Event.Fields.LRx_TotalProducts,
                    LRx_Event.Fields.LRx_SiFund_Appeal,
                    EventParentRecord.LRx_SiFund_Appeal.Id,
                    out tempHolder
                );

                totalAppealProductCount = CalculateAmountRevenue(
                    LRx_Event.EntityLogicalName,
                    LRx_Event.Fields.LRx_Products,
                    LRx_Event.Fields.LRx_SiFund_Appeal,
                    EventParentRecord.LRx_SiFund_Appeal.Id,
                    out tempHolder
                );

                var parentAppeal = new SiFund_Appeal
                {
                    Id = EventParentRecord.LRx_SiFund_Appeal.Id,
                    LRx_TotalProductsSold = new Money(totalAppealProductRevenue),
                    LRx_ProductsOldCount = (int)totalAppealProductCount
                };
                _service.Update(parentAppeal);
            }

            if (EventParentRecord.LRx_SiFund_Package != null)
            {
                totalPackageProductRevenue = CalculateAmountRevenue(
                    LRx_Event.EntityLogicalName,
                    LRx_Event.Fields.LRx_TotalProducts,
                    LRx_Event.Fields.LRx_SiFund_Package,
                    EventParentRecord.LRx_SiFund_Package.Id,
                    out tempHolder
                );

                totalPackageProductCount = CalculateAmountRevenue(
                    LRx_Event.EntityLogicalName,
                    LRx_Event.Fields.LRx_Products,
                    LRx_Event.Fields.LRx_SiFund_Package,
                    EventParentRecord.LRx_SiFund_Package.Id,
                    out tempHolder
                );

                var parentPackage = new SiFund_Package
                {
                    Id = EventParentRecord.LRx_SiFund_Package.Id,
                    LRx_TotalProductsSold = new Money(totalPackageProductRevenue),
                    LRx_ProductsOldCount = (int)totalPackageProductCount
                };
                _service.Update(parentPackage);
            }
        }

        public void UpdateEventSponsorRevenue(Guid sponsortID, LRx_Sponsorship sponsorRecord)
        {
            if (sponsortID != Guid.Empty) {
                sponsorRecord = (LRx_Sponsorship)RetrieveRecord(
                    LRx_Sponsorship.EntityLogicalName,
                    sponsortID,
                    LRx_Sponsorship.Fields.LRx_Event,
                    LRx_Sponsorship.Fields.LRx_EventSponsorship
                );
            }

            if (sponsorRecord != null &&
                sponsorRecord.LRx_Event != null &&
                sponsorRecord.LRx_Event.Id != Guid.Empty)
            {
                decimal totalSponsorRevenue = 0;
                int sponsorCount = 0;

                totalSponsorRevenue = CalculateAmountRevenue(
                    LRx_Sponsorship.EntityLogicalName,
                    LRx_Sponsorship.Fields.LRx_PricePerSponsorship,
                    LRx_Sponsorship.Fields.LRx_Event,
                    sponsorRecord.LRx_Event.Id,
                    out sponsorCount
                );
                var parentEvent = new LRx_Event
                {
                    Id = sponsorRecord.LRx_Event.Id,
                    LRx_TotalSponsorships = new Money(totalSponsorRevenue),
                    LRx_Sponsorships = (int)sponsorCount
                };
                _service.Update(parentEvent);
            }

            LRx_EventSponsorship eventSponsorRecord;

            eventSponsorRecord = (LRx_EventSponsorship)RetrieveRecord(
                LRx_EventSponsorship.EntityLogicalName,
                sponsorRecord.LRx_EventSponsorship.Id,
                LRx_EventSponsorship.Fields.LRx_EventSponsorshipId
            );

            decimal totalEventSponsorRevenue = 0;
            int EventSponsorCount = 0;

            totalEventSponsorRevenue = CalculateAmountRevenue(
                    LRx_Sponsorship.EntityLogicalName,
                    LRx_Sponsorship.Fields.LRx_PricePerSponsorship,
                    LRx_Sponsorship.Fields.LRx_EventSponsorship,
                    eventSponsorRecord.LRx_EventSponsorshipId.Value,
                    out EventSponsorCount
                );

            var parentEventSponsor = new LRx_EventSponsorship
            {
                Id = eventSponsorRecord.LRx_EventSponsorshipId.Value,
                LRx_TotalSponsorships = new Money(totalEventSponsorRevenue),
                LRx_SponsorshipsOld = (int)EventSponsorCount
            };
            _service.Update(parentEventSponsor);

            LRx_Event EventParentRecord = (LRx_Event)RetrieveRecord(
                LRx_Event.EntityLogicalName,
                sponsorRecord.LRx_Event.Id,
                LRx_Event.Fields.LRx_Campaign,
                LRx_Event.Fields.LRx_SiFund_Appeal,
                LRx_Event.Fields.LRx_SiFund_Package
            );

            decimal totalCampaignSponsorRevenue = 0;
            decimal totalCampaignSponsorCount = 0;
            decimal totalAppealSponsorRevenue = 0;
            decimal totalAppealSponsorCount = 0;
            decimal totalPackageSponsorRevenue = 0;
            decimal totalPackageSponsorCount = 0;
            int tempHolder = 0;

            if (EventParentRecord.LRx_Campaign != null)
            {
                totalCampaignSponsorRevenue = CalculateAmountRevenue(
                    LRx_Event.EntityLogicalName,
                    LRx_Event.Fields.LRx_TotalSponsorships,
                    LRx_Event.Fields.LRx_Campaign,
                    EventParentRecord.LRx_Campaign.Id,
                    out tempHolder
                );

                totalCampaignSponsorCount = CalculateAmountRevenue(
                    LRx_Event.EntityLogicalName,
                    LRx_Event.Fields.LRx_Sponsorships,
                    LRx_Event.Fields.LRx_Campaign,
                    EventParentRecord.LRx_Campaign.Id,
                    out tempHolder
                );

                var parentCampaign = new Campaign
                {
                    Id = EventParentRecord.LRx_Campaign.Id,
                    LRx_TotalSponsorship = new Money(totalCampaignSponsorRevenue),
                    LRx_SponsorshipCount = (int)totalCampaignSponsorCount
                };
                _service.Update(parentCampaign);
            }

            if (EventParentRecord.LRx_SiFund_Appeal != null)
            {
                totalAppealSponsorRevenue = CalculateAmountRevenue(
                    LRx_Event.EntityLogicalName,
                    LRx_Event.Fields.LRx_TotalSponsorships,
                    LRx_Event.Fields.LRx_SiFund_Appeal,
                    EventParentRecord.LRx_SiFund_Appeal.Id,
                    out tempHolder
                );

                totalAppealSponsorCount = CalculateAmountRevenue(
                    LRx_Event.EntityLogicalName,
                    LRx_Event.Fields.LRx_Sponsorships,
                    LRx_Event.Fields.LRx_SiFund_Appeal,
                    EventParentRecord.LRx_SiFund_Appeal.Id,
                    out tempHolder
                );

                var parentAppeal = new SiFund_Appeal
                {
                    Id = EventParentRecord.LRx_SiFund_Appeal.Id,
                    LRx_TotalSponsorship = new Money(totalAppealSponsorRevenue),
                    LRx_SponsorshipCount = (int)totalAppealSponsorCount
                };
                _service.Update(parentAppeal);
            }

            if ( EventParentRecord.LRx_SiFund_Package != null)
            {
                totalPackageSponsorRevenue = CalculateAmountRevenue(
                    LRx_Event.EntityLogicalName,
                    LRx_Event.Fields.LRx_TotalSponsorships,
                    LRx_Event.Fields.LRx_SiFund_Package,
                    EventParentRecord.LRx_SiFund_Package.Id,
                    out tempHolder
                );

                totalPackageSponsorCount = CalculateAmountRevenue(
                    LRx_Event.EntityLogicalName,
                    LRx_Event.Fields.LRx_Sponsorships,
                    LRx_Event.Fields.LRx_SiFund_Package,
                    EventParentRecord.LRx_SiFund_Package.Id,
                    out tempHolder
                );

                var parentPackage = new SiFund_Package
                {
                    Id = EventParentRecord.LRx_SiFund_Package.Id,
                    LRx_TotalSponsorship = new Money(totalPackageSponsorRevenue),
                    LRx_SponsorshipCount = (int)totalPackageSponsorCount
                };
                _service.Update(parentPackage);
            }
        }

        public void UpdateEventTableRevenue(Guid tableID, LRx_EventTable eventTableRecord)
        {
            if (tableID != Guid.Empty) {
                eventTableRecord = (LRx_EventTable)RetrieveRecord(
                    LRx_EventTable.EntityLogicalName,
                    tableID,
                    LRx_EventTable.Fields.LRx_Event,
                    LRx_EventTable.Fields.LRx_EventTableId
                );
            }
            
            if (eventTableRecord != null &&
                eventTableRecord.LRx_Event != null &&
                eventTableRecord.LRx_Event.Id != Guid.Empty)
            {
                decimal totalEventTableRevenue = 0;
                int eventTableCount = 0;

                totalEventTableRevenue = CalculateAmountRevenue(
                    LRx_EventTable.EntityLogicalName,
                    LRx_EventTable.Fields.LRx_PricePerTable,
                    LRx_EventTable.Fields.LRx_Event,
                    eventTableRecord.LRx_Event.Id,
                    out eventTableCount
                );
                var parentEvent = new LRx_Event
                {
                    Id = eventTableRecord.LRx_Event.Id,
                    LRx_TotalEventTables = new Money(totalEventTableRevenue),
                    LRx_EventTable = (int)eventTableCount
                };
                _service.Update(parentEvent);
            }

            LRx_EventTable eventTable = (LRx_EventTable)RetrieveRecord(
                LRx_EventTable.EntityLogicalName,
                eventTableRecord.LRx_EventTableId.Value,
                LRx_EventTable.Fields.LRx_EventTicket
            );

            var eventTicketRecord = (LRx_EventTicket)RetrieveRecord(
                LRx_EventTicket.EntityLogicalName,
                eventTable.LRx_EventTicket.Id,
                LRx_EventTicket.Fields.LRx_TableTicket,
                LRx_EventTicket.Fields.LRx_EventTicketId
            );

            decimal totalTicketRevenue = 0;
            int TicketCount = 0;

            totalTicketRevenue = CalculateAmountRevenue(
                    LRx_EventTable.EntityLogicalName,
                    LRx_EventTable.Fields.LRx_PricePerTable,
                    LRx_EventTable.Fields.LRx_EventTicket,
                    eventTicketRecord.LRx_EventTicketId.Value,
                    out TicketCount
                );

            if (eventTicketRecord.LRx_EventTicketId != Guid.Empty && eventTicketRecord.LRx_EventTicketId.HasValue)
            {
                var parentEventTicket = new LRx_EventTicket
                {
                    Id = eventTicketRecord.LRx_EventTicketId.Value,
                    LRx_TotalRegistrationsOld = new Money(totalTicketRevenue),
                    LRx_TicketsOldCount = (int)TicketCount
                };
                _service.Update(parentEventTicket);
            }

            LRx_Event EventParentRecord = (LRx_Event)RetrieveRecord(
                LRx_Event.EntityLogicalName,
                eventTableRecord.LRx_Event.Id,
                LRx_Event.Fields.LRx_Campaign,
                LRx_Event.Fields.LRx_SiFund_Appeal,
                LRx_Event.Fields.LRx_SiFund_Package
            );

            decimal totalCampaignTableRevenue = 0;
            decimal totalCampaignTableCount = 0;
            decimal totalAppealTableRevenue = 0;
            decimal totalAppealTableCount = 0;
            decimal totalPackageTableRevenue = 0;
            decimal totalPackageTableCount = 0;
            int tempHolder = 0;

            if (EventParentRecord.LRx_Campaign != null)
            {
                totalCampaignTableRevenue = CalculateAmountRevenue(
                    LRx_Event.EntityLogicalName,
                    LRx_Event.Fields.LRx_TotalEventTables,
                    LRx_Event.Fields.LRx_Campaign,
                    EventParentRecord.LRx_Campaign.Id,
                    out tempHolder
                );

                totalCampaignTableCount = CalculateAmountRevenue(
                    LRx_Event.EntityLogicalName,
                    LRx_Event.Fields.LRx_EventTable,
                    LRx_Event.Fields.LRx_Campaign,
                    EventParentRecord.LRx_Campaign.Id,
                    out tempHolder
                );

                var parentCampaign = new Campaign
                {
                    Id = EventParentRecord.LRx_Campaign.Id,
                    LRx_TotalEventTablesSold = new Money(totalCampaignTableRevenue),
                    LRx_EventTablesSoldCount = (int)totalCampaignTableCount
                };
                _service.Update(parentCampaign);
            }

            if (EventParentRecord.LRx_SiFund_Appeal != null)
            {
                totalAppealTableRevenue = CalculateAmountRevenue(
                    LRx_Event.EntityLogicalName,
                    LRx_Event.Fields.LRx_TotalEventTables,
                    LRx_Event.Fields.LRx_SiFund_Appeal,
                    EventParentRecord.LRx_SiFund_Appeal.Id,
                    out tempHolder
                );

                totalAppealTableCount = CalculateAmountRevenue(
                    LRx_Event.EntityLogicalName,
                    LRx_Event.Fields.LRx_EventTable,
                    LRx_Event.Fields.LRx_SiFund_Appeal,
                    EventParentRecord.LRx_SiFund_Appeal.Id,
                    out tempHolder
                );

                var parentAppeal = new SiFund_Appeal
                {
                    Id = EventParentRecord.LRx_SiFund_Appeal.Id,
                    LRx_TotalEventTablesSold = new Money(totalAppealTableRevenue),
                    LRx_EventTablesSoldCount = (int)totalAppealTableCount
                };
                _service.Update(parentAppeal);
            }

            if (EventParentRecord.LRx_SiFund_Package != null)
            {
                totalPackageTableRevenue = CalculateAmountRevenue(
                    LRx_Event.EntityLogicalName,
                    LRx_Event.Fields.LRx_TotalEventTables,
                    LRx_Event.Fields.LRx_SiFund_Package,
                    EventParentRecord.LRx_SiFund_Package.Id,
                    out tempHolder
                );

                totalPackageTableCount = CalculateAmountRevenue(
                    LRx_Event.EntityLogicalName,
                    LRx_Event.Fields.LRx_EventTable,
                    LRx_Event.Fields.LRx_SiFund_Package,
                    EventParentRecord.LRx_SiFund_Package.Id,
                    out tempHolder
                );

                var parentPackage = new SiFund_Package
                {
                    Id = EventParentRecord.LRx_SiFund_Package.Id,
                    LRx_TotalEventTablesSold = new Money(totalPackageTableRevenue),
                    LRx_EventTablesSoldCount = (int)totalPackageTableCount
                };
                _service.Update(parentPackage);
            }

        }

        public void UpdateFinancialSummary(Guid financialID, LRx_FinAnaCiaL financialRecord) 
        {
            if (financialID != Guid.Empty) {
                financialRecord = (LRx_FinAnaCiaL)RetrieveRecord(
                    LRx_FinAnaCiaL.EntityLogicalName,
                    financialID,
                    LRx_FinAnaCiaL.Fields.LRx_OpportunityToFinancial,
                    LRx_FinAnaCiaL.Fields.LRx_AssetType
                );
            }     

            if(financialRecord != null)
            {
                // Define a QueryExpression to fetch financial records based on related opportunity
                QueryExpression query = new QueryExpression(LRx_FinAnaCiaL.EntityLogicalName)
                {
                    ColumnSet = new ColumnSet(LRx_FinAnaCiaL.Fields.LRx_TotalAmount, LRx_FinAnaCiaL.Fields.LRx_AssetType),
                    Criteria = new FilterExpression
                    {
                        Conditions =
                        {
                            // Filter by related opportunity
                            new ConditionExpression(LRx_FinAnaCiaL.Fields.LRx_OpportunityToFinancial, ConditionOperator.Equal, financialRecord.LRx_OpportunityToFinancial.Id),
                        }
                    }
                };

                // Retrieve all financial records for the given opportunity
                EntityCollection financialRecords = _service.RetrieveMultiple(query);

                // Initialize totals for each asset type
                decimal totalRealEstateAmount = 0m;
                decimal totalBusinessAmount = 0m;
                decimal totalIncomeCompensationAmount = 0m;
                decimal totalSecuritiesAmount = 0m;
                decimal totalOtherAssetsAmount = 0m;

                // Sum the values by asset type
                foreach (var record in financialRecords.Entities)
                {
                    if (record.Contains(LRx_FinAnaCiaL.Fields.LRx_TotalAmount) && record[LRx_FinAnaCiaL.Fields.LRx_TotalAmount] != null)
                    {
                        decimal amount = 0m;

                        if (record[LRx_FinAnaCiaL.Fields.LRx_TotalAmount] is Money moneyValue)
                        {
                            amount = moneyValue.Value;
                        }
                        else if (record[LRx_FinAnaCiaL.Fields.LRx_TotalAmount] is int intValue)
                        {
                            amount = (decimal)intValue;
                        }

                        // Determine which asset type the record belongs to and update the corresponding total
                        var assetType = (LRx_FinAnaCiaL_LRx_AssetType)((OptionSetValue)record[LRx_FinAnaCiaL.Fields.LRx_AssetType]).Value;

                        switch (assetType)
                        {
                            case LRx_FinAnaCiaL_LRx_AssetType.RealEstate:
                                totalRealEstateAmount += amount;
                                break;
                            case LRx_FinAnaCiaL_LRx_AssetType.Business:
                                totalBusinessAmount += amount;
                                break;
                            case LRx_FinAnaCiaL_LRx_AssetType.IncomeCompensation:
                                totalIncomeCompensationAmount += amount;
                                break;
                            case LRx_FinAnaCiaL_LRx_AssetType.Securities:
                                totalSecuritiesAmount += amount;
                                break;
                            case LRx_FinAnaCiaL_LRx_AssetType.OtherAssets:
                                totalOtherAssetsAmount += amount;
                                break;
                        }
                    }
                }

                var parentOpportunity = new Opportunity
                {
                    Id = financialRecord.LRx_OpportunityToFinancial.Id,

                    // Update the totals for each asset type if applicable
                    LRx_RealestAteTotal = new Money(totalRealEstateAmount),
                    LRx_BusinessesTotal = new Money(totalBusinessAmount),
                    LRx_IncomeCompensationTotal = new Money(totalIncomeCompensationAmount),
                    LRx_SecuritiesTotal = new Money(totalSecuritiesAmount),
                    LRx_OtherAssetsTotal = new Money(totalOtherAssetsAmount)
                };

                // Now you can update the parentOpportunity in your service
                _service.Update(parentOpportunity);
            }
        }

        public void ComputeDonorCommitmentPaid(Guid donorCommitmentId, MsnFp_DonorCommitment donorCommitmentRecord)
        {
            if (donorCommitmentId != Guid.Empty) {
                donorCommitmentRecord = (MsnFp_DonorCommitment)RetrieveRecord(
                    MsnFp_DonorCommitment.EntityLogicalName,
                    donorCommitmentId,
                    MsnFp_DonorCommitment.Fields.LRx_FundingAgreement
                );
            }          

            if (donorCommitmentRecord != null &&
                donorCommitmentRecord.LRx_FundingAgreement != null &&
                donorCommitmentRecord.LRx_FundingAgreement.Id != Guid.Empty) {

                decimal totalAmountPaidRevenue = 0;
                int amountPaidCount = 0;

                totalAmountPaidRevenue = CalculateAmountRevenue(
                    MsnFp_DonorCommitment.EntityLogicalName,
                    MsnFp_DonorCommitment.Fields.LRx_TotalAmountPaid,
                    MsnFp_DonorCommitment.Fields.LRx_FundingAgreement,
                    donorCommitmentRecord.LRx_FundingAgreement.Id,
                    out amountPaidCount
                );
                var parentEvent = new LRx_FundingAgreement
                {
                    Id = donorCommitmentRecord.LRx_FundingAgreement.Id,
                    LRx_DonorCommitmentPaid = new Money(totalAmountPaidRevenue)
                };
                _service.Update(parentEvent);
            }
        }

        public void CheckPledgeMatch(Guid targetId, string targetType)
        { 
            if (targetType.ToLower() == "pledge") {
                
                MsnFp_DonorCommitment donorCommitmentRecord = (MsnFp_DonorCommitment)RetrieveRecord(
                    MsnFp_DonorCommitment.EntityLogicalName,
                    targetId,
                    MsnFp_DonorCommitment.Fields.SiFund_Donor,
                    MsnFp_DonorCommitment.Fields.MsnFp_TotalAmount,
                    MsnFp_DonorCommitment.Fields.LRx_Campaign,
                    MsnFp_DonorCommitment.Fields.MsnFp_BookDate
                );

                if (donorCommitmentRecord != null &&
                    donorCommitmentRecord.SiFund_Donor != null &&
                    donorCommitmentRecord.SiFund_Donor.Id != Guid.Empty)
                {
                    QueryExpression query = new QueryExpression(LRx_PledgeMatch.EntityLogicalName)
                    {
                        ColumnSet = new ColumnSet(LRx_PledgeMatch.Fields.LRx_CustomerToId, LRx_PledgeMatch.Fields.LRx_ApplyToDonationsOrPledges, LRx_PledgeMatch.Fields.LRx_Percentage),
                        Criteria = new FilterExpression
                        {
                            Conditions =
                            {
                                // Filter by related opportunity
                                new ConditionExpression(LRx_PledgeMatch.Fields.LRx_CustomerFromId, ConditionOperator.Equal, donorCommitmentRecord.SiFund_Donor.Id)
                            }
                        }
                    };

                    // Retrieve all financial records for the given opportunity
                    EntityCollection pledgeRecords = _service.RetrieveMultiple(query);
                    if (pledgeRecords.Entities.Count > 0)
                    {
                        
                        foreach (Entity pledgeRecord in pledgeRecords.Entities)
                        {
                            // Check if LRx_CustomerToId and LRx_Percentage are not null
                            if (pledgeRecord.Contains(LRx_PledgeMatch.Fields.LRx_CustomerToId) &&
                                pledgeRecord.Contains(LRx_PledgeMatch.Fields.LRx_Percentage))
                            {
                                int applyToDonationsOrPledgesValue = pledgeRecord.GetAttributeValue<OptionSetValue>(LRx_PledgeMatch.Fields.LRx_ApplyToDonationsOrPledges).Value;
                                string applyToDonationsOrPledgesText = GetOptionSetText(LRx_PledgeMatch.EntityLogicalName, LRx_PledgeMatch.Fields.LRx_ApplyToDonationsOrPledges, applyToDonationsOrPledgesValue);
                                
                                // Check if the text is "Donations"
                                if (!string.Equals(applyToDonationsOrPledgesText.Trim(), "Donations", StringComparison.OrdinalIgnoreCase))
                                {
                                    // Calculate the pledge amount based on percentage
                                    decimal totalAmount = (decimal)donorCommitmentRecord.MsnFp_TotalAmount.Value;
                                    int percentage = (int)pledgeRecord[LRx_PledgeMatch.Fields.LRx_Percentage];
                                    decimal percentageDecimal = (decimal)percentage;
                                    EntityReference customerToId = (EntityReference)pledgeRecord[LRx_PledgeMatch.Fields.LRx_CustomerToId];
                                    decimal computedAmount = (totalAmount * percentageDecimal) / 100;

                                    var newDonorCommitment = new Entity(MsnFp_DonorCommitment.EntityLogicalName)
                                    {
                                        [MsnFp_DonorCommitment.Fields.SiFund_Donor] = new EntityReference(Contact.EntityLogicalName, customerToId.Id),
                                        [MsnFp_DonorCommitment.Fields.MsnFp_TotalAmount] = new Money(computedAmount),
                                        [MsnFp_DonorCommitment.Fields.MsnFp_BookDate] = donorCommitmentRecord.MsnFp_BookDate.Value,
                                        [MsnFp_DonorCommitment.Fields.LRx_Campaign] = new EntityReference(Campaign.EntityLogicalName, donorCommitmentRecord.LRx_Campaign.Id)
                                    };

                                    // Create the donor commitment record in Dynamics 365
                                    var donorCommitmentId = _service.Create(newDonorCommitment);

                                }
                            }
                        }
                    }

                }
            }
            else // for donation
            {
                MsnFp_Transaction transactionRecord = (MsnFp_Transaction)RetrieveRecord(
                    MsnFp_Transaction.EntityLogicalName,
                    targetId,
                    MsnFp_Transaction.Fields.SiFund_Donor,
                    MsnFp_Transaction.Fields.MsnFp_Amount,
                    MsnFp_Transaction.Fields.LRx_Campaign,
                    MsnFp_Transaction.Fields.MsnFp_BookDate
                );
                if (transactionRecord != null &&
                    transactionRecord.SiFund_Donor != null &&
                    transactionRecord.SiFund_Donor.Id != Guid.Empty)
                {
                    QueryExpression query = new QueryExpression(LRx_PledgeMatch.EntityLogicalName)
                    {
                        ColumnSet = new ColumnSet(LRx_PledgeMatch.Fields.LRx_CustomerToId, LRx_PledgeMatch.Fields.LRx_ApplyToDonationsOrPledges, LRx_PledgeMatch.Fields.LRx_Percentage),
                        Criteria = new FilterExpression
                        {
                            Conditions =
                            {
                                // Filter by related opportunity
                                new ConditionExpression(LRx_PledgeMatch.Fields.LRx_CustomerFromId, ConditionOperator.Equal, transactionRecord.SiFund_Donor.Id)
                            }
                        }
                    };

                    // Retrieve all financial records for the given opportunity
                    EntityCollection transactionRecords = _service.RetrieveMultiple(query);
                    if (transactionRecords.Entities.Count > 0)
                    {

                        foreach (Entity tRecord in transactionRecords.Entities)
                        {
                            // Check if LRx_CustomerToId and LRx_Percentage are not null
                            if (tRecord.Contains(LRx_PledgeMatch.Fields.LRx_CustomerToId) &&
                                tRecord.Contains(LRx_PledgeMatch.Fields.LRx_Percentage))
                            {
                                int applyToDonationsOrPledgesValue = tRecord.GetAttributeValue<OptionSetValue>(LRx_PledgeMatch.Fields.LRx_ApplyToDonationsOrPledges).Value;
                                string applyToDonationsOrPledgesText = GetOptionSetText(LRx_PledgeMatch.EntityLogicalName, LRx_PledgeMatch.Fields.LRx_ApplyToDonationsOrPledges, applyToDonationsOrPledgesValue);

                                // Check if the text is "Donations"
                                if (!string.Equals(applyToDonationsOrPledgesText.Trim(), "Pledges", StringComparison.OrdinalIgnoreCase))
                                {
                                    // Calculate the pledge amount based on percentage
                                    decimal totalAmount = (decimal)transactionRecord.MsnFp_Amount.Value;
                                    int percentage = (int)tRecord[LRx_PledgeMatch.Fields.LRx_Percentage];
                                    decimal percentageDecimal = (decimal)percentage;
                                    EntityReference customerToId = (EntityReference)tRecord[LRx_PledgeMatch.Fields.LRx_CustomerToId];
                                    decimal computedAmount = (totalAmount * percentageDecimal) / 100;

                                    var newDonorCommitment = new Entity(MsnFp_DonorCommitment.EntityLogicalName)
                                    {
                                        [MsnFp_DonorCommitment.Fields.SiFund_Donor] = new EntityReference(Contact.EntityLogicalName, customerToId.Id),
                                        [MsnFp_DonorCommitment.Fields.MsnFp_TotalAmount] = new Money(computedAmount),
                                        [MsnFp_DonorCommitment.Fields.MsnFp_BookDate] = transactionRecord.MsnFp_BookDate.Value,
                                        [MsnFp_DonorCommitment.Fields.LRx_Campaign] = new EntityReference(Campaign.EntityLogicalName, transactionRecord.LRx_Campaign.Id)
                                    };

                                    // Create the donor commitment record in Dynamics 365
                                    var donorCommitmentId = _service.Create(newDonorCommitment);

                                }
                            }
                        }
                    }
                }
            }       
        }

        //Handle Delete event for Transactions
        public void YearlyGivingReclaculation(Guid donorId, string transactionAmount)
        {
            //Testing
            var contact = new Contact
            {
                ContactId = donorId,
                LRx_InsTagRam = "Updated from Delete event" + transactionAmount
            };
            
            _service.Update(contact);
        }

        //-- START OF HELPER METHODS
        // Method to perform dynamic roll-up calculation for giving amounts
        public decimal CalculateAmountRevenue(string childEntityLogicalName, string fieldToBeComputed, string parentLookUpName, Guid eventID, out int recordCount)
        {
            // Create the query to retrieve all child records related to the parent eventID
            QueryExpression query = new QueryExpression(childEntityLogicalName)
            {
                ColumnSet = new ColumnSet(fieldToBeComputed),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression(parentLookUpName, ConditionOperator.Equal, eventID)
                    }
                }
            };

            // Execute the query to get related records
            EntityCollection registrationRecords = _service.RetrieveMultiple(query);

            // Get the count of records and store it in the out parameter
            recordCount = registrationRecords.Entities.Count;

            // If no records are returned, simply return 0 for the revenue
            if (registrationRecords.Entities == null || recordCount == 0)
            {
                return 0;
            }

            // Sum up the values using LINQ for cleaner code
            decimal totalAmountRevenue = registrationRecords.Entities
                .Where(record => record.Contains(fieldToBeComputed) && record[fieldToBeComputed] != null)
                .Sum(record =>
                {
                    if (record[fieldToBeComputed] is Money moneyValue)
                    {
                        return moneyValue.Value; // If it's of type Money, return the decimal value.
                    }
                    else if (record[fieldToBeComputed] is int intValue)
                    {
                        return (decimal)intValue; // If it's an int, convert to decimal.
                    }
                    else
                    {
                        return 0m; // If it's neither Money nor int, return 0 or handle as appropriate.
                    }
                });

            return totalAmountRevenue;
        }


        public decimal CalculateGivingRollup(string parentEntityLogicalName, Guid parentId, string childEntityLogicalName, string childToParentLookupField, string rollupField, ColumnSet filterFields,
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

        public void AutoPopulateRefundAmounts(MsnFp_Transaction transaction)
        {
            MsnFp_Transaction transactionrecord = (MsnFp_Transaction)RetrieveRecord(MsnFp_Transaction.EntityLogicalName, transaction.Id, MsnFp_Transaction.Fields.MsnFp_Amount);

            if (transactionrecord.MsnFp_Amount != null)
            {
                var updateTransaction = new MsnFp_Transaction
                {
                    Id = transaction.Id,
                    SiFund_Amount_Receipted = transactionrecord.MsnFp_Amount,
                };

                _service.Update(updateTransaction);
            }
        }
        public string GetOptionSetText(string entityName, string attributeName, int optionSetValue)
        {
            RetrieveAttributeRequest attributeRequest = new RetrieveAttributeRequest
            {
                EntityLogicalName = entityName,
                LogicalName = attributeName,
                RetrieveAsIfPublished = true
            };

            RetrieveAttributeResponse attributeResponse = (RetrieveAttributeResponse)_service.Execute(attributeRequest);
            PicklistAttributeMetadata picklistMetadata = (PicklistAttributeMetadata)attributeResponse.AttributeMetadata;

            foreach (var option in picklistMetadata.OptionSet.Options)
            {
                if (option.Value == optionSetValue)
                {
                    return option.Label.UserLocalizedLabel.Label;
                }
            }

            return null; // Return null if not found
        }

    }
}
