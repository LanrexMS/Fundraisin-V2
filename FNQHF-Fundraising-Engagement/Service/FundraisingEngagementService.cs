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

        public void CalculateTotalTransactionForAllContacts()
        {
            // Query to retrieve all contacts in batches (you can adjust the page size based on your needs)
            QueryExpression queryContacts = new QueryExpression(Contact.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(Contact.Fields.ContactId),  // Only fetch the ContactId field to minimize data load
                PageInfo = new PagingInfo
                {
                    PageNumber = 1,
                    Count = 1000 // Set the batch size for contacts (1000 records per page)
                }
            };

            EntityCollection contactRecords;
            do
            {
                // Retrieve a batch of contacts
                contactRecords = _service.RetrieveMultiple(queryContacts);

                // Loop through each contact and call CalculateTotalTransaction for each
                foreach (var contact in contactRecords.Entities)
                {
                    // Get the contact ID
                    var contactID = contact.Id;

                    // Call the existing function to calculate total transaction and donation count
                    int donationCount;
                    CalculateTotalTransaction(contactID, out donationCount);

                    // You can log or do additional processing here as needed, e.g., save results back to contact record
                    Console.WriteLine($"Contact ID: {contactID}, Donations: {donationCount}");
                }

                // Check if there are more contacts to retrieve
                if (contactRecords.MoreRecords)
                {
                    queryContacts.PageInfo.PageNumber++;
                    queryContacts.PageInfo.PagingCookie = contactRecords.PagingCookie;
                }

            } while (contactRecords.MoreRecords);
        }

        //-- START OF HELPER METHODS
        // Method to perform dynamic roll-up calculation for giving amounts
        public void CalculateTotalTransaction(Guid contactID, out int donationCount)
        {
            // Create the query to retrieve all child records related to the parent eventID
            // Query to retrieve donation transactions for the specific event
            QueryExpression query = new QueryExpression(MsnFp_Transaction.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(MsnFp_Transaction.Fields.MsnFp_Amount),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression(MsnFp_Transaction.Fields.MsnFp_CustomerId, ConditionOperator.Equal, contactID),
                        new ConditionExpression(MsnFp_Transaction.Fields.StatusCode, ConditionOperator.Equal, (int)MsnFp_Transaction_StatusCode.Completed),
                        new ConditionExpression(MsnFp_Transaction.Fields.MsnFp_TypeCode, ConditionOperator.Equal, (int)MsnFp_Transaction_MsnFp_TypeCode.Donation)
                    }
                },
                PageInfo = new PagingInfo
                {
                    PageNumber = 1,
                    Count = 1000 // Set page size (1,000 records per page)
                }
            };

            // Variables to track total donation amount and count
            decimal totalTransactionAmount= 0m;
            donationCount = 0;
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

            var parentContact = new Contact
            {
                Id = contactID,
                FNQHF_TotalTransactionAmount = new Money(totalTransactionAmount)
            };
        }
    }
}
