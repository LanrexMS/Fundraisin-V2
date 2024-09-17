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

        public void CampaignPerformanceTransaction(MsnFp_Transaction transaction)
        {
            MsnFp_Transaction transactionrecord = (MsnFp_Transaction)RetrieveRecord(
                MsnFp_Transaction.EntityLogicalName, 
                transaction.Id, 
                MsnFp_Transaction.Fields.LRx_Campaign,
                MsnFp_Transaction.Fields.SiFund_Appeal,
                MsnFp_Transaction.Fields.SiFund_Package
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

            // Total Donations for Campaign
            if (transactionrecord.LRx_Campaign != null && transactionrecord.LRx_Campaign.Id != Guid.Empty)
            {
                Guid campaignId = transactionrecord.LRx_Campaign.Id;

                decimal campaignDonationsAmount = CalculateGivingRollup(Campaign.EntityLogicalName, campaignId, MsnFp_Transaction.EntityLogicalName, MsnFp_Transaction.Fields.LRx_Campaign, MsnFp_Transaction.Fields.MsnFp_Amount,
                   filterFields, donationCriteria);

                decimal campaignDonationsCount = CalculateCount(Campaign.EntityLogicalName, campaignId, MsnFp_Transaction.EntityLogicalName, MsnFp_Transaction.Fields.LRx_Campaign,
                   filterFields, donationCriteria);

                var parentCampaign = new Campaign
                {
                    Id = campaignId,
                    LRx_TotalDonations = new Money(campaignDonationsAmount),
                    LRx_DonationCount = (int)campaignDonationsCount
                };

                _service.Update(parentCampaign);

            }

            // Total Donations for Appeal
            if (transactionrecord.SiFund_Appeal != null && transactionrecord.SiFund_Appeal.Id != Guid.Empty)
            {
                Guid appealId = transactionrecord.SiFund_Appeal.Id;

                decimal appealDonationsAmount = CalculateGivingRollup(SiFund_Appeal.EntityLogicalName, appealId, MsnFp_Transaction.EntityLogicalName, MsnFp_Transaction.Fields.SiFund_Appeal, MsnFp_Transaction.Fields.MsnFp_Amount,
                   filterFields, donationCriteria);

                decimal appealDonationsCount = CalculateCount(SiFund_Appeal.EntityLogicalName, appealId, MsnFp_Transaction.EntityLogicalName, MsnFp_Transaction.Fields.SiFund_Appeal,
                   filterFields, donationCriteria);

                var parentAppeal = new SiFund_Appeal
                {
                    Id = appealId,
                    LRx_TotalDonations = new Money(appealDonationsAmount),
                    LRx_DonationCount = (int)appealDonationsCount
                };

                _service.Update(parentAppeal);

            }

            // Total Donations for Package
            if (transactionrecord.SiFund_Package != null && transactionrecord.SiFund_Package.Id != Guid.Empty)
            {
                Guid packageId = transactionrecord.SiFund_Package.Id;

                decimal packageDonationsAmount = CalculateGivingRollup(SiFund_Package.EntityLogicalName, packageId, MsnFp_Transaction.EntityLogicalName, MsnFp_Transaction.Fields.SiFund_Package, MsnFp_Transaction.Fields.MsnFp_Amount,
                   filterFields, donationCriteria);

                decimal packageDonationsCount = CalculateCount(SiFund_Package.EntityLogicalName, packageId, MsnFp_Transaction.EntityLogicalName, MsnFp_Transaction.Fields.SiFund_Package,
                   filterFields, donationCriteria);

                var parentPackage = new SiFund_Package
                {
                    Id = packageId,
                    LRx_TotalDonations = new Money(packageDonationsAmount),
                    LRx_DonationCount = (int)packageDonationsCount
                };

                _service.Update(parentPackage);

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

            ColumnSet filterFields = new ColumnSet(
                   MsnFp_DonorCommitment.Fields.MsnFp_TotalAmount,
                   MsnFp_DonorCommitment.Fields.SiFund_TotalAmount_Balance
             );

            var commitmentCriteria = new Dictionary<string, (ConditionOperator, object)>
            {
                //add filtering criteria for commitment if any   
            };

            // Total Pledges for Campaign
            if (commitmentRecord.LRx_Campaign != null && commitmentRecord.LRx_Campaign.Id != Guid.Empty)
            {
                Guid campaignId = commitmentRecord.LRx_Campaign.Id;

                decimal campaignPledgesAmount = CalculateGivingRollup(Campaign.EntityLogicalName, campaignId, MsnFp_DonorCommitment.EntityLogicalName, MsnFp_DonorCommitment.Fields.LRx_Campaign, MsnFp_DonorCommitment.Fields.MsnFp_TotalAmount,
                   filterFields, commitmentCriteria);

                decimal campiangPledgesCount = CalculateCount(Campaign.EntityLogicalName, campaignId, MsnFp_DonorCommitment.EntityLogicalName, MsnFp_DonorCommitment.Fields.LRx_Campaign,
                   filterFields, commitmentCriteria);

                decimal campaignAmountBalance = CalculateGivingRollup(Campaign.EntityLogicalName, campaignId, MsnFp_DonorCommitment.EntityLogicalName, MsnFp_DonorCommitment.Fields.LRx_Campaign, MsnFp_DonorCommitment.Fields.SiFund_TotalAmount_Balance,
                   filterFields, commitmentCriteria);

                var parentCampaign = new Campaign
                {
                    Id = campaignId,
                    LRx_TotalPledges = new Money(campaignPledgesAmount),
                    LRx_PledgeCount = (int)campiangPledgesCount,
                    LRx_TotalOutstandingPledges = new Money(campaignAmountBalance),
                };
                _service.Update(parentCampaign);
            }

            // Total Pledges for Package
            if (commitmentRecord.SiFund_Package != null && commitmentRecord.SiFund_Package.Id != Guid.Empty)
            {
                Guid packageId = commitmentRecord.SiFund_Package.Id;

                decimal packagePledgesAmount = CalculateGivingRollup(SiFund_Package.EntityLogicalName, packageId, MsnFp_DonorCommitment.EntityLogicalName, MsnFp_DonorCommitment.Fields.SiFund_Package, MsnFp_DonorCommitment.Fields.MsnFp_TotalAmount,
                   filterFields, commitmentCriteria);

                decimal packagePledgesCount = CalculateCount(SiFund_Package.EntityLogicalName, packageId, MsnFp_DonorCommitment.EntityLogicalName, MsnFp_DonorCommitment.Fields.SiFund_Package,
                   filterFields, commitmentCriteria);

                decimal packageAmountBalance = CalculateGivingRollup(SiFund_Package.EntityLogicalName, packageId, MsnFp_DonorCommitment.EntityLogicalName, MsnFp_DonorCommitment.Fields.SiFund_Package, MsnFp_DonorCommitment.Fields.SiFund_TotalAmount_Balance,
                   filterFields, commitmentCriteria);

                var parentPackage = new SiFund_Package
                {
                    Id = packageId,
                    LRx_TotalPledges = new Money(packagePledgesAmount),
                    LRx_PledgeCount = (int)packagePledgesCount,
                    LRx_TotalOutstandingPledges= new Money(packageAmountBalance),

                };

                _service.Update(parentPackage);

            }

            // Total Pledges for Appeal
            if (commitmentRecord.SiFund_Appeal != null && commitmentRecord.SiFund_Appeal.Id != Guid.Empty)
            {
                Guid appealId = commitmentRecord.SiFund_Appeal.Id;

                decimal appealPledgesAmount = CalculateGivingRollup(SiFund_Appeal.EntityLogicalName, appealId, MsnFp_DonorCommitment.EntityLogicalName, MsnFp_DonorCommitment.Fields.SiFund_Appeal, MsnFp_DonorCommitment.Fields.MsnFp_TotalAmount,
                   filterFields, commitmentCriteria);

                decimal appealPledgesCount = CalculateCount(SiFund_Appeal.EntityLogicalName, appealId, MsnFp_DonorCommitment.EntityLogicalName, MsnFp_DonorCommitment.Fields.SiFund_Appeal,
                   filterFields, commitmentCriteria);

                decimal appealAmountBalance = CalculateGivingRollup(SiFund_Appeal.EntityLogicalName, appealId, MsnFp_DonorCommitment.EntityLogicalName, MsnFp_DonorCommitment.Fields.SiFund_Appeal, MsnFp_DonorCommitment.Fields.SiFund_TotalAmount_Balance,
                   filterFields, commitmentCriteria);

                var parentAppeal = new SiFund_Appeal
                {
                    Id = appealId,
                    LRx_TotalPledges = new Money(appealPledgesAmount),
                    LRx_PledgeCount = (int)appealPledgesCount,
                    LRx_TotalOutstandingPledges = new Money(appealAmountBalance),
                };
                _service.Update(parentAppeal);
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
                //add filtering criteria for commitment if any   
            };

            // Total Writeoff for DonorCommitment
            if (writeOffRecord.LRx_MsnFp_DonorCommitment != null && writeOffRecord.LRx_MsnFp_DonorCommitment.Id != Guid.Empty)
            {
                Guid donnorCommitmentId = writeOffRecord.LRx_MsnFp_DonorCommitment.Id;

                decimal writeOffAmount = CalculateGivingRollup(MsnFp_DonorCommitment.EntityLogicalName, donnorCommitmentId, LRx_WriteOff.EntityLogicalName, LRx_WriteOff.Fields.LRx_MsnFp_DonorCommitment, LRx_WriteOff.Fields.LRx_WriteOffAmount,
                   filterFields, writeOffCriteria);

                var parentDonorCommitment = new MsnFp_DonorCommitment
                {
                    Id = donnorCommitmentId,
                    LRx_TotalAmountWRiTenOff = new Money(writeOffAmount),
                  
                };
                _service.Update(parentDonorCommitment);
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
