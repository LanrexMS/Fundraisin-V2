// Decompiled with JetBrains decompiler
// Type: FundraisinApp_Integration.Plugins.Service.Fundraising_APIService
// Assembly: FundraisinApp-Integration.Plugin, Version=1.0.0.0, Culture=neutral, PublicKeyToken=be17da884c09fb48
// MVID: A80D178E-91E0-4361-9810-5F6936033CCA
// Assembly location: C:\Users\Nico Benito\Downloads\NicoTestSolution_1_0_0_2\PluginAssemblies\FundraisinApp-IntegrationPlugin-AD40DC3F-2002-4C54-899F-1599020D0AFB\FundraisinApp-IntegrationPlugin.dll

using FundraisinApp_Integration.Plugins.Entities.Integration_Data_Model;
using FundraisinApp_IntegrationPlugin.FundraisinApp_Integration.Plugins.Entities.Integration_Data_Model;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using CsvHelper;
using CsvHelper.Configuration;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;
using System.Activities;
using System.Diagnostics.Eventing.Reader;
using System.IdentityModel.Protocols.WSTrust;

#nullable disable
namespace FundraisinApp_Integration.Plugins.Service
{
    public class Fundraising_APIService
    {
        private readonly IOrganizationService _service;
        private readonly IPluginExecutionContext _context;
        private readonly ITracingService _tracingService;
        private string apiDonationBaseUrl = "https://lanrex.funraisin.com.au/api/donations";
        private string apiTransactionBaseUrl = "https://lanrex.funraisin.com.au/api/transactions";
        private string apiEventBaseUrl = "https://lanrex.funraisin.com.au/api/events";
        private string apiParticipantBaseUrl = "https://lanrex.funraisin.com.au/api/participants";
        private string apiParticipantEventBaseUrl = "https://lanrex.funraisin.com.au/api/participantsevents";
        private string username = "nico.benito@lanrex.com.au";
        private string password = "Lanrex12345!";
        private string apikey = "27f88fda055da35f0cf54d8f168a8753";
        private string dateFrom = "2024-09-26";
        private string dateTo = "2024-09-26";
        private int limit = 1000;

        public Fundraising_APIService(
          IOrganizationService service,
          IPluginExecutionContext context,
          ITracingService tracingService)
        {
            this._service = service;
            this._context = context;
            this._tracingService = tracingService;
        }

        public void GetFundraisinDonationRecords()
        {
            string csvContent = CallFundRaisinAPI((object)this.apiDonationBaseUrl);
            List<DonationModel> donationList = this.ParseDonationCsv(csvContent);
        }

        public List<DonationModel> ParseDonationCsv(string csvContent)
        {
            List<DonationModel> donationCsv = new List<DonationModel>();
            string[] strArray1 = csvContent.Split(new string[2] {
                "\r\n",
                "\n"
              }, StringSplitOptions.RemoveEmptyEntries);

            for (int index1 = 1; index1 < strArray1.Length; ++index1)
            {
                string[] strArray2 = strArray1[index1].Split(',');
                for (int index2 = 0; index2 < strArray2.Length; ++index2)
                    strArray2[index2] = strArray2[index2].Trim('"');
                int result1;
                int result2;
                DonationModel donation = new DonationModel()
                {
                    DFname = strArray2[32],
                    DLname = strArray2[33] + strArray2[34],
                    DEmail = strArray2[36],
                    DPhone = strArray2[64],
                    DPhoneWork = strArray2[66],
                    DPhoneMobile = strArray2[68] + strArray2[67],
                    DAddress2 = strArray2[57],
                    DAddressSuburb = strArray2[58],
                    DAddressPCode = strArray2[59],
                    DAddressState = strArray2[60],
                    DAddressCountry = strArray2[61],
                    MemberId = int.TryParse(strArray2[5], out result1) ? result1 : 0,
                    DStatus = strArray2[105],
                    DonationId = int.TryParse(strArray2[1], out result2) ? result2 : 0,
                    EventId = int.TryParse(strArray2[2], out result1) ? result1 : 0,
                };
                Entity existingContact = this.FindExistingContact(donation.DFname, donation.DLname, donation.DEmail);
                if (existingContact != null)
                    this.UpdateContactRecord(existingContact.Id, donation);
                else
                    this.CreateContactRecord(donation);

                donationCsv.Add(donation);
            }
            return donationCsv;
        }
        private void CreateContactRecord(DonationModel donation)
        {
            Guid contactId = this._service.Create(new Entity("contact")
            {
                ["firstname"] = (object)donation.DFname,
                ["lastname"] = (object)donation.DLname,
                ["emailaddress1"] = (object)donation.DEmail,
                ["telephone1"] = (object)donation.DPhone,
                ["mobilephone"] = (object)donation.DPhoneMobile,
                ["lrx_fundraisinmemberid"] = (object)donation.MemberId
            });
            this.GetFundraisinTransactionRecord(donation, contactId);
        }

        private void UpdateContactRecord(Guid contactId, DonationModel donation)
        {
            this._service.Update(new Entity("contact", contactId)
            {
                ["firstname"] = (object)donation.DFname,
                ["lastname"] = (object)donation.DLname,
                ["emailaddress1"] = (object)donation.DEmail,
                ["telephone1"] = (object)donation.DPhone,
                ["mobilephone"] = (object)donation.DPhoneMobile,
                ["lrx_fundraisinmemberid"] = (object)donation.MemberId
            });
            this.GetFundraisinTransactionRecord(donation, contactId);
        }

        public void GetFundraisinTransactionRecord(DonationModel donation, Guid contactId)
        {
            string requestUri = string.Format("{0}?username={1}&password={2}&apikey={3}&limit={4}&donation_id={5}", (object)this.apiTransactionBaseUrl, (object)this.username, (object)this.password, (object)this.apikey, (object)this.limit, (object)donation.DonationId);
            string csvContent = "";
            // do not reuse api call function here as it has a different parameter
            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
                try
                {
                    HttpResponseMessage result = httpClient.GetAsync(requestUri).Result;
                    if (result.IsSuccessStatusCode)
                    {
                        csvContent = result.Content.ReadAsStringAsync().Result;
                    }
                    else
                        this._tracingService.Trace("API Request failed with status code: " + result.StatusCode.ToString(), Array.Empty<object>());
                }
                catch (HttpRequestException ex)
                {
                    this._tracingService.Trace("API Request exception: " + ex.Message, Array.Empty<object>());
                }
            }
            
            List<TransactionModel> transactionList = this.ParseTransactionCsv(csvContent, contactId);
        }

        public List<TransactionModel> ParseTransactionCsv(string csvContent, Guid contactId)
        {
            List<TransactionModel> source = new List<TransactionModel>();
            string[] strArray1 = csvContent.Split(new string[2] {
                "\r\n",
                "\n"
              }, StringSplitOptions.RemoveEmptyEntries);
            for (int rowIndex = 1; rowIndex < strArray1.Length; ++rowIndex)
            {
                // Split the current row into columns
                string[] columns = strArray1[rowIndex].Split(',');

                // Trim quotes from each column
                for (int colIndex = 0; colIndex < columns.Length; ++colIndex)
                {
                    columns[colIndex] = columns[colIndex].Trim('"');
                }

                // Create and populate the TransactionModel instance
                var transactionModel = new TransactionModel
                {
                    TotalRecords = int.TryParse(columns[0], out int totalRecords) ? totalRecords : 0,
                    TransactionId = int.TryParse(columns[1], out int transactionId) ? transactionId : 0,
                    TransactionType = columns[2],
                    CharityId = int.TryParse(columns[3], out int charityId) ? charityId : 0,
                    TransactionValue = decimal.TryParse(columns[4], out decimal transactionValue) ? transactionValue : 0M,
                    Currency = columns[5],
                    CurrencyRate = decimal.TryParse(columns[6], out decimal currencyRate) ? currencyRate : 0M,
                    TransactionFees = decimal.TryParse(columns[7], out decimal transactionFees) ? transactionFees : 0M,
                    TransactionFeesRate = decimal.TryParse(columns[8], out decimal feesRate) ? feesRate : 0M,
                    TransactionFeesGateway = decimal.TryParse(columns[9], out decimal gatewayFees) ? gatewayFees : 0M,
                    TransactionFeesMandatory = decimal.TryParse(columns[10], out decimal mandatoryFees) ? mandatoryFees : 0M,
                    TransactionTax = decimal.TryParse(columns[11], out decimal transactionTax) ? transactionTax : 0M,
                    IsReconciled = columns[12] == "Y",
                    TransactionNotes = columns[13],
                    PaymentType = columns[14],
                    PaymentReference = columns[15],
                    BalanceTransactionId = columns[16],
                    PayoutId = columns[17],
                    AccountId = columns[18],
                    PoNumber = columns[19],
                    MemberId = int.TryParse(columns[20], out int memberId) ? memberId : 0,
                    HistoryId = int.TryParse(columns[21], out int historyId) ? historyId : 0,
                    DonationId = int.TryParse(columns[22], out int donationId) ? donationId : 0,
                    ScheduleId = int.TryParse(columns[23], out int scheduleId) ? scheduleId : 0,
                    BillingId = int.TryParse(columns[24], out int billingId) ? billingId : 0,
                    PaymentId = int.TryParse(columns[25], out int paymentId) ? paymentId : 0,
                    SaleId = int.TryParse(columns[26], out int saleId) ? saleId : 0,
                    RaffleId = int.TryParse(columns[27], out int raffleId) ? raffleId : 0,
                    EventId = int.TryParse(columns[28], out int eventId) ? eventId : 0,
                    PageId = int.TryParse(columns[29], out int pageId) ? pageId : 0,
                    EventPageId = int.TryParse(columns[30], out int eventPageId) ? eventPageId : 0,
                    RelatedTransactionId = int.TryParse(columns[31], out int relatedTransactionId) ? relatedTransactionId : 0,
                    GlCode1 = columns[32],
                    GlCode2 = columns[33],
                    FbPaymentId = columns[34],
                    CrmTransactionId = columns[35],
                    FunraisinSynced = columns[36] == "Y",
                    GiftaidClaimed = columns[37] == "Y",
                    DateCreated = DateTime.TryParse(columns[38], out DateTime dateCreated) ? dateCreated : DateTime.MinValue
                };

                // Add the model to the collection
                source.Add(transactionModel);
            }

            foreach (TransactionModel transaction in source.OrderBy<TransactionModel, int>((Func<TransactionModel, int>)(t => !(t.TransactionType.ToLower() == "refund") ? 0 : 1)).ThenBy<TransactionModel, DateTime>((Func<TransactionModel, DateTime>)(t => t.DateCreated)).ToList<TransactionModel>())
            {
                if (this.FindExistingTransaction(transaction) != null)
                {
                    this.UpdateTransactionRecord(contactId, transaction);
                }
                else
                {
                    this.CreateTransactionRecord(contactId, transaction);
                }

            }
            return source;
        }

        private Entity FindExistingTransaction(TransactionModel transaction)
        {
            QueryExpression queryExpression = new QueryExpression("msnfp_transaction")
            {
                ColumnSet = new ColumnSet(true)
            };
            queryExpression.Criteria.AddCondition("lrx_fundraisindonationid", (ConditionOperator)0, new object[1] {
                (object) transaction.DonationId
              });
            return ((IEnumerable<Entity>)this._service.RetrieveMultiple((QueryBase)queryExpression).Entities).FirstOrDefault<Entity>();
        }

        private void CreateTransactionRecord(Guid contactId, TransactionModel transaction)
        {
            int num = 856660001; //completed for now but nee
            switch (transaction.TransactionType.ToLower())
            {
                case "donation":
                    num = 856660001;
                    Entity transactionEntity = null;
                    if (transaction.EventId != 0)
                    {
                        Entity existingEvent = FindExistingEventID(transaction.EventId);
                        transactionEntity = new Entity("msnfp_transaction")
                        {
                            ["sifund_donor"] = (object)new EntityReference("contact", contactId),
                            ["msnfp_amount"] = (object)new Money(Math.Abs(transaction.TransactionValue)),
                            ["statuscode"] = (object)num,
                            ["msnfp_bookdate"] = (object)transaction.DateCreated,
                            ["lrx_fundraisindonationid"] = (object)transaction.DonationId,
                            ["lrx_event"] = (object)new EntityReference("lrx_event", existingEvent.Id),
                        };
                    }
                    else
                    {
                        transactionEntity = new Entity("msnfp_transaction")
                        {
                            ["sifund_donor"] = (object)new EntityReference("contact", contactId),
                            ["msnfp_amount"] = (object)new Money(Math.Abs(transaction.TransactionValue)),
                            ["statuscode"] = (object)num,
                            ["msnfp_bookdate"] = (object)transaction.DateCreated,
                            ["lrx_fundraisindonationid"] = (object)transaction.DonationId
                        };
                    }
                    
                    this._service.Create(transactionEntity);
                    break;
                case "refund":
                    num = 856660005;
                    //for discussion
                    break;
            }
        }

        private void UpdateTransactionRecord(Guid contactId, TransactionModel transaction)
        {
            //for discussion
        }

        public void GetFundraisinEventRecords()
        {
            string csvContent = CallFundRaisinAPI((object)this.apiEventBaseUrl);
            //List<EventModel> eventList = this.ParseEventCsvHelper(csvContent);
            var eventList = ParseCsvHelper<EventModel, EventModelMap>(csvContent);
            foreach (var eventRecord in eventList)
            {
                Entity existingEvent = this.FindExistingEvent(eventRecord);
                if (existingEvent != null)
                    this.UpdateEventRecord(existingEvent.Id, eventRecord);
                else
                    this.CreateEventRecord(eventRecord);
            }
        }
        public Entity FindExistingEvent(EventModel eventList)
        {
            QueryExpression queryExpression = new QueryExpression("lrx_event")
            {
                ColumnSet = new ColumnSet(true)
            };
            queryExpression.Criteria.AddCondition("lrx_fundraisineventid", (ConditionOperator)0, new object[1] {
                (object) eventList.EventId
              });
            return ((IEnumerable<Entity>)this._service.RetrieveMultiple((QueryBase)queryExpression).Entities).FirstOrDefault<Entity>();
        }
        public void CreateEventRecord(EventModel EventRecord)
        {
            Guid eventId = this._service.Create(new Entity("lrx_event")
            {
                ["lrx_name"] = (string)EventRecord.EventName,
                ["lrx_campaign"] = (object)new EntityReference("campaign", new Guid("d5bf32ce-d9e1-4a2a-914f-9ded53e1b41a")),
                ["lrx_fundraisineventid"] = (int)EventRecord.EventId
            });
        }
        public void UpdateEventRecord(Guid EventId, EventModel EventRecord)
        {
            this._service.Update(new Entity("lrx_event", EventId)
            {               
                ["lrx_name"] = (string)EventRecord.EventName,
                ["lrx_fundraisineventid"] = (int)EventRecord.EventId
            });
        }

        public void GetFundraisinParticipantRecords()
        {
            string csvContent = CallFundRaisinAPI((object)this.apiParticipantBaseUrl);
            //List<ParticipantModel> participantList = this.ParseParticipantCsvHelper(csvContent);
            var participantList = ParseCsvHelper<ParticipantModel, ParticipantModelMap>(csvContent);
            foreach (var participant in participantList)
            {
                Entity existingMember = FindExistingContactMemberID(int.Parse(participant.MemberId));
                if (existingMember == null)
                {
                    Entity existingContact = FindExistingContact(participant.MFname, participant.MLname, participant.MEmail);
                    if (existingContact == null) {
                        Guid contactId = this._service.Create(new Entity("contact")
                        {
                            ["firstname"] = (object)participant.MFname,
                            ["lastname"] = (object)participant.MLname,
                            ["emailaddress1"] = (object)participant.MEmail,
                            ["telephone1"] = (object)participant.MPhoneHome,
                            ["mobilephone"] = (object)participant.MPhoneMobile,
                            ["address1_line1"] = (object)participant.MAddressStreet,
                            ["address1_city"] = (object)participant.MAddressSuburb,
                            ["address1_postalcode"] = (object)participant.MAddressPCode,
                            ["address1_stateorprovince"] = (object)participant.MAddressState,
                            ["address1_country"] = (object)participant.MAddressCountry,
                            ["lrx_fundraisinmemberid"] = int.Parse(participant.MemberId)
                        });
                    }
                    else
                    {
                        this._service.Update(new Entity("contact", existingContact.Id)
                        {
                            ["telephone1"] = (object)participant.MPhoneHome,
                            ["mobilephone"] = (object)participant.MPhoneMobile,
                            ["address1_line1"] = (object)participant.MAddressStreet,
                            ["address1_city"] = (object)participant.MAddressSuburb,
                            ["address1_postalcode"] = (object)participant.MAddressPCode,
                            ["address1_stateorprovince"] = (object)participant.MAddressState,
                            ["address1_country"] = (object)participant.MAddressCountry,
                            ["lrx_fundraisinmemberid"] = int.Parse(participant.MemberId)
                        });
                    }
                }
                else
                {
                    this._service.Update(new Entity("contact", existingMember.Id)
                    {
                        ["firstname"] = (object)participant.MFname,
                        ["lastname"] = (object)participant.MLname,
                        ["emailaddress1"] = (object)participant.MEmail,
                        ["telephone1"] = (object)participant.MPhoneHome,
                        ["mobilephone"] = (object)participant.MPhoneMobile,
                        ["address1_line1"] = (object)participant.MAddressStreet,
                        ["address1_city"] = (object)participant.MAddressSuburb,
                        ["address1_postalcode"] = (object)participant.MAddressPCode,
                        ["address1_stateorprovince"] = (object)participant.MAddressState,
                        ["address1_country"] = (object)participant.MAddressCountry,
                        ["lrx_fundraisinmemberid"] = int.Parse(participant.MemberId)
                    });
                }
            }
        }

        public void GetRegistrationFromParticipantEventRecord()
        {
            string csvContent = CallFundRaisinAPI((object)this.apiParticipantEventBaseUrl);

            var participantEventList = ParseCsvHelper<ParticipantEventModel, ParticipantEventModelMap>(csvContent);
            foreach (var participantEvent in participantEventList)
            {
                Guid contactID = Guid.Empty;
                Guid eventID = Guid.Empty;
                Entity existingMember = FindExistingContactMemberID(int.Parse(participantEvent.Member_Id));
                if (existingMember == null)
                    contactID = (Guid)existingMember.Id;

                Entity existingEvent = FindExistingEventID(int.Parse(participantEvent.Event_Id));
                if (existingEvent == null)
                    eventID = (Guid)existingEvent.Id;

                if (contactID == Guid.Empty || eventID == Guid.Empty)
                {
                    this._tracingService.Trace("No ID found for record " + participantEvent.Member_Id);
                    continue;
                }
                    
                Entity existingRegistration = FindExistingRegistration(contactID);
                if(existingRegistration == null)
                {
                    Guid registrationID = this._service.Create(new Entity("lrx_registrations")
                    {
                        ["lrx_event"] = (object)new EntityReference("lrx_event", eventID),
                        ["lrx_constituentorganization"] = (object)new EntityReference("contact", contactID)
                    });
                }
                else
                {
                    this._service.Update(new Entity("lrx_registrations", existingRegistration.Id)
                    {
                        ["lrx_event"] = (object)new EntityReference("lrx_event", eventID),
                        ["lrx_constituentorganization"] = (object)new EntityReference("contact", contactID)
                    });
                }
            }
        }

        //reusable functions
        private Entity FindExistingContact(string fName, string lNAme, string emailAdd)
        {
            QueryExpression queryExpression = new QueryExpression("contact")
            {
                ColumnSet = new ColumnSet(true)
            };
            queryExpression.Criteria.AddCondition("firstname", (ConditionOperator)0, new object[1] {
                fName
            });
            queryExpression.Criteria.AddCondition("lastname", (ConditionOperator)0, new object[1] {
                lNAme
            });
            queryExpression.Criteria.AddCondition("emailaddress1", (ConditionOperator)0, new object[1] {
                emailAdd
            });

            return ((IEnumerable<Entity>)this._service.RetrieveMultiple((QueryBase)queryExpression).Entities).FirstOrDefault<Entity>();
        }

        public Entity FindExistingContactMemberID(int MemberId)
        {
            QueryExpression queryExpression = new QueryExpression("contact")
            {
                ColumnSet = new ColumnSet(true)
            };
            queryExpression.Criteria.AddCondition("lrx_fundraisinmemberid", (ConditionOperator)0, new object[1] {
                (object) MemberId
              });
            return ((IEnumerable<Entity>)this._service.RetrieveMultiple((QueryBase)queryExpression).Entities).FirstOrDefault<Entity>();
        }

        public Entity FindExistingEventID(int eventID)
        {
            QueryExpression queryExpression = new QueryExpression("lrx_event")
            {
                ColumnSet = new ColumnSet(true)
            };
            queryExpression.Criteria.AddCondition("lrx_fundraisineventid", (ConditionOperator)0, new object[1] {
                (object) eventID
              });
            return ((IEnumerable<Entity>)this._service.RetrieveMultiple((QueryBase)queryExpression).Entities).FirstOrDefault<Entity>();
        }

        public Entity FindExistingRegistration(Guid contactID)
        {
            QueryExpression queryExpression = new QueryExpression("lrx_registrations")
            {
                ColumnSet = new ColumnSet(true)
            };
            queryExpression.Criteria.AddCondition("lrx_constituentorganization", (ConditionOperator)0, new object[1] {
                (object) contactID
              });
            return ((IEnumerable<Entity>)this._service.RetrieveMultiple((QueryBase)queryExpression).Entities).FirstOrDefault<Entity>();
        }

        public List<TModel> ParseCsvHelper<TModel, TMap>(string csvContent)
        where TMap : ClassMap<TModel>
        {
            var resultList = new List<TModel>();

            using (var reader = new StringReader(csvContent))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true, // Read the header row
                TrimOptions = CsvHelper.Configuration.TrimOptions.Trim, // Remove extra spaces
            }))
            {
                // Register the custom mapping
                csv.Context.RegisterClassMap<TMap>();

                // Read and map the records
                resultList = csv.GetRecords<TModel>().ToList();
            }

            return resultList;
        }

        public string CallFundRaisinAPI(object apiEndpoint)
        {
            string requestUri = string.Format("{0}?username={1}&password={2}&apikey={3}&limit={4}", (object)apiEndpoint, (object)this.username, (object)this.password, (object)this.apikey, (object)this.limit);
            string csvContent = "";
            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
                try
                {
                    HttpResponseMessage result = httpClient.GetAsync(requestUri).Result;
                    if (result.IsSuccessStatusCode)
                    {
                        csvContent = result.Content.ReadAsStringAsync().Result;
                        this._tracingService.Trace("API Success", Array.Empty<object>());
                    }
                    else
                        this._tracingService.Trace("API Request failed with status code: " + result.StatusCode.ToString(), Array.Empty<object>());
                }
                catch (HttpRequestException ex)
                {
                    this._tracingService.Trace("API Request exception: " + ex.Message, Array.Empty<object>());
                }
            }
            return csvContent;
        }
    }
}