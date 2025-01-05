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
using Microsoft.Xrm.Sdk.PluginTelemetry;
using Newtonsoft.Json.Linq;

#nullable disable
namespace FundraisinApp_Integration.Plugins.Service
{
    public class Fundraising_APIService
    {
        private readonly IOrganizationService _service;
        private readonly IPluginExecutionContext _context;
        private readonly ITracingService _tracingService;
        public string baseURL = "https://lanrex.funraisin.com.au/api/";
        private string username = "nico.benito@lanrex.com.au";
        private string password = "Lanrex12345!";
        private string apikey = "27f88fda055da35f0cf54d8f168a8753";
        private string dateFrom = "";
        private string dateTo = "";
        private int limit = 1000;

        public Fundraising_APIService(
          IOrganizationService service,
          IPluginExecutionContext context,
          ITracingService tracingService,
          object JSONinput)
        {
            this._service = service;
            this._context = context;
            this._tracingService = tracingService;

            // Parse the JSON input
            JObject jsonInput = JObject.Parse(JSONinput.ToString());

            // Assign the values to the variables
            this.baseURL = jsonInput["baseURL"]?.ToString();
            this.username = jsonInput["username"]?.ToString();
            this.password = jsonInput["password"]?.ToString();
            this.apikey = jsonInput["apikey"]?.ToString();
            this.dateFrom = jsonInput["dateFrom"]?.ToString();
            this.dateTo = jsonInput["dateTo"]?.ToString();
        }

        public void GetFundraisinDonationRecords()
        {
            string donationURL = baseURL + "donations";
            string csvContent = CallFundRaisinAPI((object)donationURL);
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

                var conditions = new List<ConditionExpression>
                {
                    new ConditionExpression("firstname", ConditionOperator.Equal, donation.DFname),
                    new ConditionExpression("lastname", ConditionOperator.Equal, donation.DLname),
                    new ConditionExpression("emailaddress1", ConditionOperator.Equal, donation.DEmail)
                };

                Entity existingContact = FindExistingRecord("contact", conditions);
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
            string transactionURL = baseURL + "transactions";
            string requestUri = string.Format("{0}?username={1}&password={2}&apikey={3}&limit={4}&donation_id={5}", (object)transactionURL, (object)this.username, (object)this.password, (object)this.apikey, (object)this.limit, (object)donation.DonationId);
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
                        var EventSearchConditions = new List<ConditionExpression>
                        {
                            new ConditionExpression("lrx_fundraisineventid", ConditionOperator.Equal, transaction.EventId)
                        };

                        Entity existingEvent = FindExistingRecord("lrx_event", EventSearchConditions);
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
            string eventURL = baseURL + "events";
            string csvContent = CallFundRaisinAPI((object)eventURL);
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
            string participantURL = baseURL + "participants";
            string csvContent = CallFundRaisinAPI((object)participantURL);
            //List<ParticipantModel> participantList = this.ParseParticipantCsvHelper(csvContent);
            var participantList = ParseCsvHelper<ParticipantModel, ParticipantModelMap>(csvContent);
            foreach (var participant in participantList)
            {
                var MemberSearchConditions = new List<ConditionExpression>
                {
                    new ConditionExpression("lrx_fundraisinmemberid", ConditionOperator.Equal, participant.MemberId)
                };

                Entity existingMember = FindExistingRecord("contact", MemberSearchConditions);
                if (existingMember == null)
                {
                    var ContactSearchConditions = new List<ConditionExpression>
                    {
                        new ConditionExpression("firstname", ConditionOperator.Equal, participant.MFname),
                        new ConditionExpression("lastname", ConditionOperator.Equal, participant.MLname),
                        new ConditionExpression("emailaddress1", ConditionOperator.Equal, participant.MEmail)
                    };

                    Entity existingContact = FindExistingRecord("contact", ContactSearchConditions);
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
            string participantEventURL = baseURL + "participantsevents";
            string csvContent = CallFundRaisinAPI((object)participantEventURL);

            var participantEventList = ParseCsvHelper<ParticipantEventModel, ParticipantEventModelMap>(csvContent);
            foreach (var participantEvent in participantEventList)
            {
                Guid contactID = Guid.Empty;
                Guid eventID = Guid.Empty;
                var MemberSearchConditions = new List<ConditionExpression>
                {
                    new ConditionExpression("lrx_fundraisinmemberid", ConditionOperator.Equal, participantEvent.Member_Id)
                };

                Entity existingMember = FindExistingRecord("contact", MemberSearchConditions);
                if (existingMember != null)
                    contactID = (Guid)existingMember.Id;

                var EventSearchConditions = new List<ConditionExpression>
                {
                    new ConditionExpression("lrx_fundraisineventid", ConditionOperator.Equal, participantEvent.Event_Id)
                };

                Entity existingEvent = FindExistingRecord("lrx_event", EventSearchConditions);
                if (existingEvent != null)
                    eventID = (Guid)existingEvent.Id;

                if (contactID == Guid.Empty || eventID == Guid.Empty)
                {
                    this._tracingService.Trace("No contact or event found for record " + participantEvent.Member_Id);
                    continue;
                }

                var RegistrationSearchConditions = new List<ConditionExpression>
                {
                    new ConditionExpression("lrx_constituentorganization", ConditionOperator.Equal, contactID),
                    new ConditionExpression("lrx_event", ConditionOperator.Equal, eventID)
                };

                Entity existingRegistration = FindExistingRecord("lrx_registrations", RegistrationSearchConditions);
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

        public void GetFundraisinTicketRecords()
        {
            string ticketURL = baseURL + "tickets";
            string csvContent = CallFundRaisinAPI((object)ticketURL);

            var ticketList = ParseCsvHelper<TicketsModel, TicketsModelMap>(csvContent);
            foreach (var tickets in ticketList)
            {
                Guid eventID = Guid.Empty;
                var EventSearchConditions = new List<ConditionExpression>
                {
                    new ConditionExpression("lrx_fundraisineventid", ConditionOperator.Equal, tickets.event_id)
                };

                Entity existingEvent = FindExistingRecord("lrx_event", EventSearchConditions);
                if (existingEvent != null)
                    eventID = (Guid)existingEvent.Id;

                if (eventID == Guid.Empty)
                {
                    this._tracingService.Trace("No event found for record " + tickets.ticket_id);
                    continue;
                }

                var TicketSearchConditions = new List<ConditionExpression>
                {
                    new ConditionExpression("lrx_fundraisinticketid", ConditionOperator.Equal, tickets.ticket_id)
                };

                Entity existingTicket = FindExistingRecord("lrx_eventticket", TicketSearchConditions);
                if (existingTicket == null)
                {
                    Guid TicketID = this._service.Create(new Entity("lrx_eventticket")
                    {
                        ["lrx_name"] = (object)tickets.ticket_name,
                        ["lrx_quantity"] = int.Parse(tickets.num_tickets),
                        ["lrx_eventticketdescription"] = (object)tickets.ticket_description,
                        ["lrx_amount"] = new Money(decimal.Parse(tickets.ticket_price)),
                        ["lrx_event"] = (object)new EntityReference("lrx_event", eventID),
                        ["lrx_fundraisinticketid"] = int.Parse(tickets.ticket_id)
                    });
                }
                else
                {
                    this._service.Update(new Entity("lrx_eventticket", existingTicket.Id)
                    {
                        ["lrx_name"] = (object)tickets.ticket_name,
                        ["lrx_quantity"] = int.Parse(tickets.num_tickets),
                        ["lrx_eventticketdescription"] = (object)tickets.ticket_description,
                        ["lrx_amount"] = new Money(decimal.Parse(tickets.ticket_price)),
                        ["lrx_event"] = (object)new EntityReference("lrx_event", eventID),
                        ["lrx_fundraisinticketid"] = int.Parse(tickets.ticket_id)
                    });
                }
            }
        }

        public void GetFundraisinTicketHolderRecord()
        {
            string ticketHolderURL = baseURL + "ticketholders";
            string csvContent = CallFundRaisinAPI((object)ticketHolderURL);

            var TicketHolderList = ParseCsvHelper<TicketHolderModel, TicketHolderModelMap>(csvContent);
            foreach (var TicketHolders in TicketHolderList)
            {
                Guid GuestcontactId = Guid.Empty;
                var GuestSearchConditions = new List<ConditionExpression>
                {
                    new ConditionExpression("lrx_fundraisinguestid", ConditionOperator.Equal, TicketHolders.guest_id)
                };
                Entity existingGuest = FindExistingRecord("contact", GuestSearchConditions);
                if (existingGuest == null)
                {
                    var ContactSearchConditions = new List<ConditionExpression>
                    {
                        new ConditionExpression("firstname", ConditionOperator.Equal, TicketHolders.g_fname),
                        new ConditionExpression("lastname", ConditionOperator.Equal, TicketHolders.g_lname),
                        new ConditionExpression("emailaddress1", ConditionOperator.Equal, TicketHolders.g_email)
                    };

                    Entity existingContact = FindExistingRecord("contact", ContactSearchConditions);
                    if (existingContact == null)
                    {
                        GuestcontactId = this._service.Create(new Entity("contact")
                        {
                            ["firstname"] = (object)TicketHolders.g_fname,
                            ["lastname"] = (object)TicketHolders.g_lname,
                            ["emailaddress1"] = (object)TicketHolders.g_email,
                            ["telephone1"] = (string)TicketHolders.g_phone_suffix + (string)TicketHolders.g_phone,
                            ["mobilephone"] = (string)TicketHolders.g_phone_suffix + (string)TicketHolders.g_phone,
                            ["address1_line1"] = (string)TicketHolders.g_address_unit + (string)TicketHolders.g_address_street,
                            ["address1_city"] = (object)TicketHolders.g_address_suburb,
                            ["address1_postalcode"] = (object)TicketHolders.g_address_pcode,
                            ["address1_stateorprovince"] = (object)TicketHolders.g_address_state,
                            ["address1_country"] = (object)TicketHolders.g_address_country,
                            ["lrx_fundraisinguestid"] = int.Parse(TicketHolders.guest_id)
                        });
                    }
                    else
                    {
                        GuestcontactId = existingContact.Id;
                        this._service.Update(new Entity("contact", existingContact.Id)
                        {
                            ["telephone1"] = (string)TicketHolders.g_phone_suffix + (string)TicketHolders.g_phone,
                            ["mobilephone"] = (string)TicketHolders.g_phone_suffix + (string)TicketHolders.g_phone,
                            ["address1_line1"] = (string)TicketHolders.g_address_unit + (string)TicketHolders.g_address_street,
                            ["address1_city"] = (object)TicketHolders.g_address_suburb,
                            ["address1_postalcode"] = (object)TicketHolders.g_address_pcode,
                            ["address1_stateorprovince"] = (object)TicketHolders.g_address_state,
                            ["address1_country"] = (object)TicketHolders.g_address_country,
                            ["lrx_fundraisinguestid"] = int.Parse(TicketHolders.guest_id)
                        });
                    }
                }
                else
                {
                    GuestcontactId = existingGuest.Id;
                    this._service.Update(new Entity("contact", existingGuest.Id)
                    {
                        ["firstname"] = (object)TicketHolders.g_fname,
                        ["lastname"] = (object)TicketHolders.g_lname,
                        ["emailaddress1"] = (object)TicketHolders.g_email,
                        ["telephone1"] = (string)TicketHolders.g_phone_suffix + (string)TicketHolders.g_phone,
                        ["mobilephone"] = (string)TicketHolders.g_phone_suffix + (string)TicketHolders.g_phone,
                        ["address1_line1"] = (string)TicketHolders.g_address_unit + (string)TicketHolders.g_address_street,
                        ["address1_city"] = (object)TicketHolders.g_address_suburb,
                        ["address1_postalcode"] = (object)TicketHolders.g_address_pcode,
                        ["address1_stateorprovince"] = (object)TicketHolders.g_address_state,
                        ["address1_country"] = (object)TicketHolders.g_address_country,
                        ["lrx_fundraisinguestid"] = int.Parse(TicketHolders.guest_id)
                    });
                }

                Guid registrationContactID = Guid.Empty;
                Guid eventID = Guid.Empty;
                Guid registrationID = Guid.Empty;
                var MemberSearchConditions = new List<ConditionExpression>
                {
                    new ConditionExpression("lrx_fundraisinmemberid", ConditionOperator.Equal, TicketHolders.member_id)
                };

                Entity existingMember = FindExistingRecord("contact", MemberSearchConditions);
                if (existingMember != null)
                    registrationContactID = (Guid)existingMember.Id;

                var EventSearchConditions = new List<ConditionExpression>
                {
                    new ConditionExpression("lrx_fundraisineventid", ConditionOperator.Equal, TicketHolders.event_id)
                };

                Entity existingEvent = FindExistingRecord("lrx_event", EventSearchConditions);
                if (existingEvent != null)
                    eventID = (Guid)existingEvent.Id;

                var RegistrationSearchConditions = new List<ConditionExpression>
                {
                    new ConditionExpression("lrx_constituentorganization", ConditionOperator.Equal, registrationContactID),
                    new ConditionExpression("lrx_event", ConditionOperator.Equal, eventID)
                };

                Entity existingRegistration = FindExistingRecord("lrx_registrations", RegistrationSearchConditions);
                if (existingRegistration != null)
                    registrationID = (Guid)existingRegistration.Id;

                if (registrationContactID == Guid.Empty || eventID == Guid.Empty || registrationID == Guid.Empty)
                {
                    this._tracingService.Trace("No registration record or event found for record " + TicketHolders.guest_id);
                    continue;
                }

                var TicketHolderSearchConditions = new List<ConditionExpression>
                {
                    new ConditionExpression("lrx_tickerholder", ConditionOperator.Equal, GuestcontactId),
                    new ConditionExpression("lrx_event", ConditionOperator.Equal, eventID),
                    new ConditionExpression("lrx_parentregistration", ConditionOperator.Equal, registrationID),
                };

                Entity existingTicketHolder = FindExistingRecord("lrx_ticketholders", TicketHolderSearchConditions);
                if (existingTicketHolder == null)
                {
                    Guid ticketHolderID = this._service.Create(new Entity("lrx_ticketholders")
                    {
                        ["lrx_event"] = (object)new EntityReference("lrx_event", eventID),
                        ["lrx_tickerholder"] = (object)new EntityReference("contact", GuestcontactId),
                        ["lrx_parentregistration"] = (object)new EntityReference("contact", registrationID)
                    });
                }
                else
                {
                    this._service.Update(new Entity("lrx_ticketholders", existingTicketHolder.Id)
                    {
                        ["lrx_event"] = (object)new EntityReference("lrx_event", eventID),
                        ["lrx_tickerholder"] = (object)new EntityReference("contact", GuestcontactId),
                        ["lrx_parentregistration"] = (object)new EntityReference("contact", registrationID)
                    });
                }
            }
        }

        public void GetFundRaisinProductRecord()
        {
            string url = baseURL + "products";
            string csvContent = CallFundRaisinAPI((object)url);

            var productList = ParseCsvHelper<ProductModel, ProductModelMap>(csvContent);
            foreach (var products in productList)
            {
                var productType = 856660000; //Default to product type
                if (products.product_type == "ecard")
                    productType = 856660001; //change to virtual type if ecard

                var ProductSearchConditions = new List<ConditionExpression>
                {
                    new ConditionExpression("lrx_fundraisinproductid", ConditionOperator.Equal, products.product_id)
                };
                Entity existingProduct = FindExistingRecord("lrx_inventoryproduct", ProductSearchConditions);
                
                if (existingProduct == null)
                {
                    Guid productID = this._service.Create(new Entity("lrx_inventoryproduct")
                    {
                        ["lrx_name"] = (object)products.product_name,
                        ["lrx_producttype"] = new OptionSetValue(productType),
                        ["lrx_productprice"] = (object)new Money(decimal.Parse(products.product_price)),
                        ["lrx_productcost"] = (object)new Money(decimal.Parse(products.product_cost)),
                        ["lrx_maximumbuyqty"] = int.Parse(products.max_buy_limit),
                        ["lrx_minimumbuyqty"] = int.Parse(products.min_buy_limit),
                        ["lrx_stocklevels"] = int.Parse(products.product_stock),
                        ["lrx_crmid"] = (object)products.crm_product_id,
                        ["lrx_description"] = (object)products.product_description,
                        ["lrx_fundraisinproductid"] = int.Parse(products.product_id)
                    });
                }
                else
                {
                    this._service.Update(new Entity("lrx_inventoryproduct", existingProduct.Id)
                    {
                        ["lrx_name"] = (object)products.product_name,
                        ["lrx_producttype"] = new OptionSetValue(productType),
                        ["lrx_productprice"] = (object)new Money(decimal.Parse(products.product_price)),
                        ["lrx_productcost"] = (object)new Money(decimal.Parse(products.product_cost)),
                        ["lrx_maximumbuyqty"] = int.Parse(products.max_buy_limit),
                        ["lrx_minimumbuyqty"] = int.Parse(products.min_buy_limit),
                        ["lrx_stocklevels"] = int.Parse(products.product_stock),
                        ["lrx_crmid"] = (object)products.crm_product_id,
                        ["lrx_description"] = (object)products.product_description,
                        ["lrx_fundraisinproductid"] = int.Parse(products.product_id)
                    });
                }
            }
        }

        public void GetFundRaisinProductOptionsRecord()
        {
            string url = baseURL + "productoptions";
            string csvContent = CallFundRaisinAPI((object)url);

            var productOptionList = ParseCsvHelper<ProductOptionModel, ProductOptionModelMap>(csvContent);
            foreach (var productoptions in productOptionList)
            {
                var productOptionType = 856660002; // default to others
                if (productoptions.option_type == "size")
                    productOptionType = 856660000; //change to size
                if (productoptions.option_type == "colour")
                    productOptionType = 856660001; //change to size

                var statusCode = 1; // default to draft
                if (productoptions.option_status == "1")
                    statusCode = 856660001; //change to size

                var ProductSearchConditions = new List<ConditionExpression>
                {
                    new ConditionExpression("lrx_fundraisinproductid", ConditionOperator.Equal, productoptions.product_id)
                };
                Entity existingProduct = FindExistingRecord("lrx_inventoryproduct", ProductSearchConditions);

                if (existingProduct != null) {
                    Guid productID = (Guid)existingProduct.Id;
                    var ProductOptionSearchConditions = new List<ConditionExpression>
                    {
                        new ConditionExpression("lrx_inventoryproduct", ConditionOperator.Equal, existingProduct.Id),
                        new ConditionExpression("lrx_name", ConditionOperator.Equal, productoptions.option_name)
                    };
                    Entity existingProductOption = FindExistingRecord("lrx_productoptions", ProductOptionSearchConditions);

                    if (existingProductOption == null) {
                        Guid productOptionID = this._service.Create(new Entity("lrx_productoptions")
                        {
                            ["lrx_name"] = (object)productoptions.option_name,
                            ["lrx_optiontype"] = new OptionSetValue(productOptionType),
                            ["statuscode"] = new OptionSetValue(statusCode),
                            ["lrx_stock"] = (object)productoptions.option_stock,
                            ["lrx_inventoryproduct"] = (object)new EntityReference("lrx_inventoryproduct", productID)
                        });
                    }
                    else
                    {
                        this._service.Update(new Entity("lrx_productoptions", existingProductOption.Id)
                        {
                            ["lrx_name"] = (object)productoptions.option_name,
                            ["lrx_optiontype"] = new OptionSetValue(productOptionType),
                            ["statuscode"] = new OptionSetValue(statusCode),
                            ["lrx_stock"] = (object)productoptions.option_stock,
                            ["lrx_inventoryproduct"] = (object)new EntityReference("lrx_inventoryproduct", productID)
                        });
                    }
                }
            }
        }

         //reusable functions
        public Entity FindExistingRecord(string entityName, List<ConditionExpression> conditions, ColumnSet columnSet = null)
        {
            if (string.IsNullOrEmpty(entityName))
                throw new ArgumentException("Entity name cannot be null or empty.", nameof(entityName));

            if (conditions == null || conditions.Count == 0)
                throw new ArgumentException("At least one condition must be provided.", nameof(conditions));

            var queryExpression = new QueryExpression(entityName)
            {
                ColumnSet = columnSet ?? new ColumnSet(true) // Default to retrieve all columns if none are specified
            };

            foreach (var condition in conditions)
            {
                queryExpression.Criteria.AddCondition(condition);
            }

            return this._service.RetrieveMultiple(queryExpression).Entities.FirstOrDefault();
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

            string requestUri = "";
            if (dateFrom != "" && dateTo != "")
            {
                requestUri = string.Format("{0}?username={1}&password={2}&apikey={3}&limit={4}&date_from={5}&date_to={6}", (object)apiEndpoint, (object)this.username, (object)this.password, (object)this.apikey, (object)this.limit, (object)this.dateFrom, (object)this.dateTo);
            }
            else
            {
                requestUri = string.Format("{0}?username={1}&password={2}&apikey={3}&limit={4}", (object)apiEndpoint, (object)this.username, (object)this.password, (object)this.apikey, (object)this.limit);
            }
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