using DataverseModel;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FNQHF_Fundraising_Engagement.Service
{
    public class FundraisingEngagementService
    {
        private readonly IOrganizationService _service;
        private readonly IPluginExecutionContext _context;
        private readonly ITracingService _tracingService;

        public FundraisingEngagementService(IOrganizationService service, IPluginExecutionContext context, ITracingService tracingService)
        {
            _service = service;
            _context = context;
            _tracingService = tracingService;
        }

        //-- START OF HELPER METHODS
        // Method to perform dynamic roll-up calculation for giving amounts
        public void CalculateTotalTransaction(MsnFp_Transaction transaction)
        {
            var transactionrecord = (MsnFp_Transaction)RetrieveRecord(
                MsnFp_Transaction.EntityLogicalName,
                transaction.Id,
                MsnFp_Transaction.Fields.MsnFp_CustomerId
            );
         
            // Create the query to retrieve all child records related to the parent eventID
            // Query to retrieve donation transactions for the specific event
            QueryExpression query = new QueryExpression(MsnFp_Transaction.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(MsnFp_Transaction.Fields.MsnFp_Amount),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression(MsnFp_Transaction.Fields.MsnFp_CustomerId, ConditionOperator.Equal, transactionrecord.MsnFp_CustomerId.Id),
                        new ConditionExpression(MsnFp_Transaction.Fields.StatusCode, ConditionOperator.Equal, (int)MsnFp_Transaction_StatusCode.Completed),
                        new ConditionExpression(MsnFp_Transaction.Fields.MsnFp_TypeCode, ConditionOperator.Equal, (int)MsnFp_Transaction_MsnFp_TypeCode.Donation),
                        new ConditionExpression(MsnFp_Transaction.Fields.FNQHF_ImportFilename, ConditionOperator.NotLike, "%RallyUp%")
                    }
                },
                PageInfo = new PagingInfo
                {
                    PageNumber = 1,
                    Count = 5000 // Set page size (1,000 records per page)
                }
            };

            // Variables to track total donation amount and count
            decimal totalTransactionAmount= 0m;
            int donationCount = 0;
            EntityCollection donationRecords;

            do
            {
                // Retrieve a batch of records
                donationRecords = _service.RetrieveMultiple(query);

                // Process each retrieved record in the batch
                foreach (var record in donationRecords.Entities)
                {
                    if (record.Contains(MsnFp_Transaction.Fields.MsnFp_Amount) && record[MsnFp_Transaction.Fields.MsnFp_Amount] != null)
                    {
                        // Check if the amount is of type Money and add it to the total
                        if (record[MsnFp_Transaction.Fields.MsnFp_Amount] is Money moneyValue)
                        {
                            totalTransactionAmount += moneyValue.Value;
                        }
                        else if (record[MsnFp_Transaction.Fields.MsnFp_Amount] is int intValue)
                        {
                            totalTransactionAmount += (decimal)intValue;
                        }
                    }

                    // Increment the donation count
                    donationCount++;
                }

                // Check if there are more records to retrieve
                if (donationRecords.MoreRecords)
                {
                    // Move to the next page
                    query.PageInfo.PageNumber++;
                    query.PageInfo.PagingCookie = donationRecords.PagingCookie;
                }
            } while (donationRecords.MoreRecords);

            if (transactionrecord.MsnFp_CustomerId.LogicalName == Contact.EntityLogicalName) {
                var parentContact = new Contact
                {
                    Id = transactionrecord.MsnFp_CustomerId.Id,
                    FNQHF_TotalTransactionAmount = new Money(totalTransactionAmount)
                };

                // Update the contact record in CRM
                _service.Update(parentContact); // Perform the update to save changes
            }
            else if (transactionrecord.MsnFp_CustomerId.LogicalName == Account.EntityLogicalName) {
                var parentAccount = new Account
                {
                    Id = transactionrecord.MsnFp_CustomerId.Id,
                    FNQHF_TotalTransactionAmount = new Money(totalTransactionAmount)
                };

                // Update the contact record in CRM
                _service.Update(parentAccount); // Perform the update to save changes
            }
        }

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
