using CrmEarlyBound;
using DataverseModel;
using Fundraising_Engagement.Plugins.Plugins;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.PluginTelemetry;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IdentityModel.Metadata;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using Fundraising_Engagement.Plugins.Entities.Integration_Data_Model;
using System.Net.Http.Headers;


namespace Fundraising_Engagement.Plugins.Service
{
    public class Fundraising_APIService
    {
        private readonly IOrganizationService _service;
        private readonly IPluginExecutionContext _context;
        private readonly ITracingService _tracingService;

        // API URL and parameters
        string apiDonationBaseUrl = "https://lanrex.funraisin.com.au/api/donations";
        string apiTransactionBaseUrl = "https://lanrex.funraisin.com.au/api/transactions";
        string username = "nico.benito@lanrex.com.au";
        string password = "Lanrex12345!"; // Replace with actual password or use secure method
        string apikey = "46833669ffda16af6598c321ed4b1af1";
        string dateFrom = "2024-09-26";
        string dateTo = "2024-09-26";
        int limit = 1000;

        public Fundraising_APIService(IOrganizationService service, IPluginExecutionContext context, ITracingService tracingService)
        {
            _service = service;
            _context = context;
            _tracingService = tracingService;
        }

        public void GetFundraisinDonationRecords()
        {
            string url = $"{apiDonationBaseUrl}?username={username}&password={password}&apikey={apikey}&limit={limit}";
            _tracingService.Trace("url: " + url);
            // Fetch the CSV data from the API
            string csvData = "";
            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));

                try
                {
                    HttpResponseMessage response = httpClient.GetAsync(url).Result;

                    // Check if the response is successful
                    if (response.IsSuccessStatusCode)
                    {
                        // Read and process the response content
                        csvData = response.Content.ReadAsStringAsync().Result;

                        _tracingService.Trace("API Success");
                    }
                    else
                    {
                        _tracingService.Trace("API Request failed with status code: " + response.StatusCode);
                    }
                }
                catch (HttpRequestException e)
                {
                    _tracingService.Trace("API Request exception: " + e.Message);
                }
            }
            
            List<DonationModel> donationModels = ParseDonationCsv(csvData);

            // Display parsed data
            foreach (var donation in donationModels)
            {
                _tracingService.Trace($"Donor: {donation.DFname} {donation.DLname} ; Phone: {donation.DPhone} ; Email: {donation.DEmail} ; Status: {donation.DStatus}");
            }
        }

        public List<DonationModel> ParseDonationCsv(string csvContent)
        {
            var donations = new List<DonationModel>();
            var lines = csvContent.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            _tracingService.Trace("Lines per csv content: " + lines.Length.ToString());
            // Skip the header line
            for (int i = 1; i < lines.Length; i++)
            {
                var values = lines[i].Split(',');

                for (int j = 0; j < values.Length; j++)
                {
                    values[j] = values[j].Trim('"');
                }

                var donation = new DonationModel
                {
                    DFname = values[32], // Donation.d_fname
                    DLname = values[33] + values[34], // Donation.d_lname_prefix + Donation.d_lname
                    DEmail = values[36], // Donation.d_email
                    DPhone = values[64], // Donation.d_phone
                    DPhoneWork = values[66], // Donation.d_phone_work
                    DPhoneMobile = values[68] + values[67], // Donation.d_phone_mobile_suffix + Donation.d_phone_mobile
                    DAddress2 = values[57], // Donation.d_address_2
                    DAddressSuburb = values[58], // Donation.d_address_suburb
                    DAddressPCode = values[59], // Donation.d_address_pcode
                    DAddressState = values[60], // Donation.d_address_state
                    DAddressCountry = values[61], // Donation.d_address_country
                    MemberId = int.TryParse(values[5], out int memberId) ? memberId : 0, // Donation.member_id
                    DStatus = values[105],
                    DonationId = int.TryParse(values[1], out int donationId) ? donationId : 0
                };

                var existingContact = FindExistingContact(donation);
                if (existingContact != null)
                {
                    // Update existing record
                    UpdateContactRecord(existingContact.Id, donation);
                }
                else
                {
                    // Create new record
                    CreateContactRecord(donation);
                }

                donations.Add(donation);
            }

            return donations;
        }

        private Entity FindExistingContact(DonationModel donation)
        {
            // Construct a query to find existing contacts
            var query = new QueryExpression(Contact.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(true) // Select all fields for the contact
            };

            // Set conditions for first name, last name, and email
            query.Criteria.AddCondition(Contact.Fields.FirstName, ConditionOperator.Equal, donation.DFname);
            query.Criteria.AddCondition(Contact.Fields.LastName, ConditionOperator.Equal, donation.DLname);
            query.Criteria.AddCondition(Contact.Fields.EmailAddress1, ConditionOperator.Equal, donation.DEmail);

            // Execute the query
            var results = _service.RetrieveMultiple(query);
            return results.Entities.FirstOrDefault(); // Return the first matching entity, or null if none found
        }

        // Function to create a new contact record
        private void CreateContactRecord(DonationModel donation)
        {
            var newContact = new Entity("contact")
            {
                ["firstname"] = donation.DFname,
                ["lastname"] = donation.DLname,
                ["emailaddress1"] = donation.DEmail,
                ["telephone1"] = donation.DPhone,
                ["mobilephone"] = donation.DPhoneMobile
                // Map other necessary fields here
            };

            // Create the contact in Dynamics 365
            var contactId = _service.Create(newContact);

            GetFundraisinTransactionRecord(donation, contactId);
        }

        // Function to update an existing contact record
        private void UpdateContactRecord(Guid contactId, DonationModel donation)
        {
            var updatedContact = new Entity("contact", contactId)
            {
                ["firstname"] = donation.DFname,
                ["lastname"] = donation.DLname,
                ["emailaddress1"] = donation.DEmail,
                ["telephone1"] = donation.DPhone,
                ["mobilephone"] = donation.DPhoneMobile
                // Map other necessary fields here
            };

            // Update the contact in Dynamics 365
            _service.Update(updatedContact);

            GetFundraisinTransactionRecord(donation, contactId);
        }

        public void GetFundraisinTransactionRecord(DonationModel donation, Guid contactId)
        {
            string url = $"{apiTransactionBaseUrl}?username={username}&password={password}&apikey={apikey}&limit={limit}&donation_id={donation.DonationId}";
            
            _tracingService.Trace("url: " + url);
            // Fetch the CSV data from the API
            string csvData = "";
            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));

                try
                {
                    HttpResponseMessage response = httpClient.GetAsync(url).Result;

                    // Check if the response is successful
                    if (response.IsSuccessStatusCode)
                    {
                        // Read and process the response content
                        csvData = response.Content.ReadAsStringAsync().Result;

                        _tracingService.Trace("API Success");
                    }
                    else
                    {
                        _tracingService.Trace("API Request failed with status code: " + response.StatusCode);
                    }
                }
                catch (HttpRequestException e)
                {
                    _tracingService.Trace("API Request exception: " + e.Message);
                }
            }

            List<TransactionModel> TransactionModels = ParseTransactionCsv(csvData, contactId);

            // Display parsed data
            foreach (var transaction in TransactionModels)
            {
                _tracingService.Trace($"DonationID: {transaction.DonationId} ; DonationAmount: {transaction.TransactionValue}");
            }
        }

        public List<TransactionModel> ParseTransactionCsv(string csvContent, Guid contactId)
        {
            var transactions = new List<TransactionModel>();
            var lines = csvContent.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            // Skip the header line
            for (int i = 1; i < lines.Length; i++)
            {
                var values = lines[i].Split(',');

                for (int j = 0; j < values.Length; j++)
                {
                    values[j] = values[j].Trim('"');
                }

                var transaction = new TransactionModel
                {
                    TotalRecords = int.TryParse(values[0], out int totalRecords) ? totalRecords : 0, // total_records
                    TransactionId = int.TryParse(values[1], out int transactionId) ? transactionId : 0, // transaction_id
                    TransactionType = values[2], // transaction_type
                    CharityId = int.TryParse(values[3], out int charityId) ? charityId : 0, // charity_id
                    TransactionValue = decimal.TryParse(values[4], out decimal transactionValue) ? transactionValue : 0, // transaction_value
                    Currency = values[5], // currency
                    CurrencyRate = decimal.TryParse(values[6], out decimal currencyRate) ? currencyRate : 0, // currency_rate
                    TransactionFees = decimal.TryParse(values[7], out decimal transactionFees) ? transactionFees : 0, // transaction_fees
                    TransactionFeesRate = decimal.TryParse(values[8], out decimal transactionFeesRate) ? transactionFeesRate : 0, // transaction_fees_rate
                    TransactionFeesGateway = decimal.TryParse(values[9], out decimal transactionFeesGateway) ? transactionFeesGateway : 0, // transaction_fees_gateway
                    TransactionFeesMandatory = decimal.TryParse(values[10], out decimal transactionFeesMandatory) ? transactionFeesMandatory : 0, // transaction_fees_mandatory
                    TransactionTax = decimal.TryParse(values[11], out decimal transactionTax) ? transactionTax : 0, // transaction_tax
                    IsReconciled = values[12] == "Y", // is_reconciled
                    TransactionNotes = values[13], // transaction_notes
                    PaymentType = values[14], // payment_type
                    PaymentReference = values[15], // payment_reference
                    BalanceTransactionId = values[16], // balance_transaction_id
                    PayoutId = values[17], // payout_id
                    AccountId = values[18], // account_id
                    PoNumber = values[19], // po_number
                    MemberId = int.TryParse(values[20], out int memberId) ? memberId : 0, // member_id
                    HistoryId = int.TryParse(values[21], out int historyId) ? historyId : 0, // history_id
                    DonationId = int.TryParse(values[22], out int donationId) ? donationId : 0, // donation_id
                    ScheduleId = int.TryParse(values[23], out int scheduleId) ? scheduleId : 0, // schedule_id
                    BillingId = int.TryParse(values[24], out int billingId) ? billingId : 0, // billing_id
                    PaymentId = int.TryParse(values[25], out int paymentId) ? paymentId : 0, // payment_id
                    SaleId = int.TryParse(values[26], out int saleId) ? saleId : 0, // sale_id
                    RaffleId = int.TryParse(values[27], out int raffleId) ? raffleId : 0, // raffle_id
                    EventId = int.TryParse(values[28], out int eventId) ? eventId : 0, // event_id
                    PageId = int.TryParse(values[29], out int pageId) ? pageId : 0, // page_id
                    EventPageId = int.TryParse(values[30], out int eventPageId) ? eventPageId : 0, // event_page_id
                    RelatedTransactionId = int.TryParse(values[31], out int relatedTransactionId) ? relatedTransactionId : 0, // related_transaction_id
                    GlCode1 = values[32], // gl_code1
                    GlCode2 = values[33], // gl_code2
                    FbPaymentId = values[34], // fb_payment_id
                    CrmTransactionId = values[35], // crm_transaction_id
                    FunraisinSynced = values[36] == "Y", // funraisin_synced
                    GiftaidClaimed = values[37] == "Y", // giftaid_claimed
                    DateCreated = DateTime.TryParse(values[38], out DateTime dateCreated) ? dateCreated : DateTime.MinValue // date_created
                };

                transactions.Add(transaction);
            }

            var sortedTransactions = transactions
                                    .OrderBy(t => t.TransactionType.ToLower() == "refund" ? 1 : 0) // "refund" will get a higher sort value
                                    .ThenBy(t => t.DateCreated) // Optional: sort by date created if needed
                                    .ToList();

            // Process sorted transactions
            foreach (var transaction in sortedTransactions)
            {
                var existingTransaction = FindExistingTransaction(transaction);
                if (existingTransaction != null)
                {
                    // Update existing record to refund
                    if(transaction.TransactionType.ToLower() == "refund")
                    {
                        //refund
                    }
                    else
                    {
                        //normal update of transaction

                    }
                }
                else
                {
                    // Create new record
                    CreateTransactionRecord(contactId, transaction);
                }
            }

            return transactions;
        }

        private Entity FindExistingTransaction(TransactionModel transaction)
        {          
            // Construct a query to find existing contacts
            var query = new QueryExpression(MsnFp_Transaction.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(true) // Select all fields for the contact
            };
            
            query.Criteria.AddCondition(MsnFp_Transaction.Fields.LRx_FundRaisinDonationId, ConditionOperator.Equal, transaction.DonationId);
                         
            // Execute the query
            var results = _service.RetrieveMultiple(query);
            return results.Entities.FirstOrDefault(); // Return the first matching entity, or null if none found
        }

        // Function to create a new contact record
        private void CreateTransactionRecord(Guid contactId, TransactionModel transaction)
        {
            int transactionStatusCode = (int)MsnFp_Transaction_StatusCode.Completed;
            
            switch (transaction.TransactionType.ToLower())
            {
                case "donation":
                    transactionStatusCode = (int)MsnFp_Transaction_StatusCode.Completed;
                    break;
                case "refund":
                    transactionStatusCode = (int)MsnFp_Transaction_StatusCode.Refund;
                    break;
                // Add other cases as needed
                default:
                    // Handle default case if needed
                    break;
            }

            var newTransaction = new Entity(MsnFp_Transaction.EntityLogicalName)
            {
                [MsnFp_Transaction.Fields.SiFund_Donor] = new EntityReference(Contact.EntityLogicalName, contactId),
                [MsnFp_Transaction.Fields.MsnFp_Amount] = new Money(Math.Abs(transaction.TransactionValue)),
                [MsnFp_Transaction.Fields.StatusCode] = transactionStatusCode,
                [MsnFp_Transaction.Fields.MsnFp_BookDate] = transaction.DateCreated,
                [MsnFp_Transaction.Fields.LRx_FundRaisinDonationId] = (int)transaction.DonationId
            };

            _tracingService.Trace($"Donor: {contactId} ; DonationType: {transactionStatusCode}");

            // Create the transaction record in Dynamics 365
            var transactionId = _service.Create(newTransaction);
        }

        // Function to update an existing contact record
        private void UpdateTransactionRecord(Guid contactId, DonationModel donation)
        {
            
        }
    }
}
