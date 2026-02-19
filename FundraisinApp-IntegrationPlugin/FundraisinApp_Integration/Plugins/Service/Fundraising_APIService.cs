// Decompiled with JetBrains decompiler
// Type: FundraisinApp_Integration.Plugins.Service.Fundraising_APIService
// Assembly: FundraisinApp-Integration.Plugin, Version=1.0.0.0, Culture=neutral, PublicKeyToken=be17da884c09fb48
// MVID: A80D178E-91E0-4361-9810-5F6936033CCA
// Assembly location: C:\Users\Nico Benito\Downloads\NicoTestSolution_1_0_0_2\PluginAssemblies\FundraisinApp-IntegrationPlugin-AD40DC3F-2002-4C54-899F-1599020D0AFB\FundraisinApp-IntegrationPlugin.dll

using CrmEarlyBound;
using CsvHelper;
using CsvHelper.Configuration;
using FundraisinApp_Integration.Plugins.Entities.Integration_Data_Model;
using FundraisinApp_IntegrationPlugin.FundraisinApp_Integration.Plugins.Entities.Integration_Data_Model;
using Microsoft.SqlServer.Server;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.PluginTelemetry;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.Eventing.Reader;
using System.Diagnostics.Tracing;
using System.Globalization;
using System.IdentityModel.Protocols.WSTrust;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Services.Description;
using System.Web.UI;
using System.Web.Util;
using System.Windows;
using System.Workflow.Runtime.Tracking;

#nullable disable
namespace FundraisinApp_Integration.Plugins.Service
{
    public class Fundraising_APIService
    {
        private readonly IOrganizationService _service;
        private readonly IPluginExecutionContext _context;
        private readonly ITracingService _tracingService;
        public string baseURL = "https://lanrex.funraisin.com.au/api/";
        public string baseURLCustom = "https://lanrex.funraisin.com.au/customcode/";
        private string apikey = "27f88fda055da35f0cf54d8f168a8753";
        private string campaignName = "";
        private string paymentMethod = "";
        private string dateFrom = "";
        private string dateTo = "";
        bool updateTransaction = false;

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
            this.apikey = jsonInput["apikey"]?.ToString();
            this.campaignName = jsonInput["defaultCampaignName"]?.ToString();
            this.paymentMethod = jsonInput["defaultPaymentMethodName"]?.ToString();
            this.updateTransaction = bool.Parse(jsonInput["updateTransaction"]?.ToString());

            string format = "MM-dd-yyyy HH:mm:ss";
            CultureInfo provider = CultureInfo.InvariantCulture;

            if (DateTime.TryParseExact(jsonInput["dateFrom"]?.ToString(), format, provider, DateTimeStyles.None, out DateTime parsedDateFrom))
            {
                this.dateFrom = parsedDateFrom.ToString("yyyy-MM-dd HH:mm:ss");
            }

            if (DateTime.TryParseExact(jsonInput["dateTo"]?.ToString(), format, provider, DateTimeStyles.None, out DateTime parsedDateTo))
            {
                this.dateTo = parsedDateTo.ToString("yyyy-MM-dd HH:mm:ss");
            }

            if (!string.IsNullOrEmpty(baseURL) && baseURL.Length > 4)
            {
                baseURLCustom = baseURL.Substring(0, this.baseURL.Length - 4) + "customcode/";
            }
        }

        public Task GetFundraisinEventRecords()
        {
            List<EventModel> fundraisingEvents = this.ParseCsvHelper<EventModel, EventModelMap>(this.CallFundRaisinAPI((object)(this.baseURL + "events")));

            foreach (EventModel eventModel in fundraisingEvents)
            {
                Guid eventRecordId = Guid.Empty;
                Entity existingEventRecord = this.FindExistingRecord("lrx_event", new List<ConditionExpression>()
                {
                    new ConditionExpression("lrx_fundraisineventid", ConditionOperator.Equal, (object)eventModel.EventId)
                });

                if (existingEventRecord != null)
                {
                    Entity updatedEventRecord = new Entity("lrx_event", existingEventRecord.Id)
                    {
                        ["lrx_name"] = (object)eventModel.EventName,
                        ["lrx_goal"] = (object)new Money(Decimal.Parse(eventModel.EventTarget)),
                        ["lrx_description"] = (object)eventModel.EventShortDesc,
                        ["lrx_fundraisineventid"] = int.Parse(eventModel.EventId),
                        ["lrx_location"] = (object)eventModel.EventLocation
                    };

                    DateTime eventStartDate;
                    if (eventModel.EventDate != "0000-00-00" && DateTime.TryParse(eventModel.EventDate, out eventStartDate))
                        updatedEventRecord["lrx_proposedstart"] = (object)eventStartDate;

                    DateTime eventEndDate;
                    if (eventModel.EventClosedDate != "0000-00-00" && DateTime.TryParse(eventModel.EventClosedDate, out eventEndDate))
                        updatedEventRecord["lrx_proposedend"] = (object)eventEndDate;

                    this._service.Update(updatedEventRecord);
                    eventRecordId = existingEventRecord.Id;
                }
                else
                {
                    Entity newEventRecord = new Entity("lrx_event")
                    {
                        ["lrx_name"] = (object)eventModel.EventName,
                        ["lrx_goal"] = (object)new Money(Decimal.Parse(eventModel.EventTarget)),
                        ["lrx_description"] = (object)eventModel.EventShortDesc,
                        ["lrx_fundraisineventid"] = int.Parse(eventModel.EventId),
                        ["lrx_location"] = (object)eventModel.EventLocation
                    };

                    DateTime eventStartDate;
                    if (eventModel.EventDate != "0000-00-00" && DateTime.TryParse(eventModel.EventDate, out eventStartDate))
                        newEventRecord["lrx_proposedstart"] = (object)eventStartDate;

                    DateTime eventEndDate;
                    if (eventModel.EventClosedDate != "0000-00-00" && DateTime.TryParse(eventModel.EventClosedDate, out eventEndDate))
                        newEventRecord["lrx_proposedend"] = (object)eventEndDate;

                    eventRecordId = this._service.Create(newEventRecord);
                }
            }

            this._tracingService.Trace("Fundraising Event Record Sync Completed", Array.Empty<object>());
            return Task.CompletedTask;
        }

        public Task GetAllFundraisinEventRecords()
        {
            List<EventModel> fundraisingEvents = this.ParseCsvHelper<EventModel, EventModelMap>(this.CallFundRaisinAPIAllData((object)(this.baseURL + "events")));

            foreach (EventModel eventModel in fundraisingEvents)
            {
                Guid eventRecordId = Guid.Empty;
                Entity existingEventRecord = this.FindExistingRecord("lrx_event", new List<ConditionExpression>()
                {
                    new ConditionExpression("lrx_fundraisineventid", ConditionOperator.Equal, (object)eventModel.EventId)
                });

                if (existingEventRecord != null)
                {
                    Entity updatedEventRecord = new Entity("lrx_event", existingEventRecord.Id)
                    {
                        ["lrx_name"] = (object)eventModel.EventName,
                        ["lrx_goal"] = (object)new Money(Decimal.Parse(eventModel.EventTarget)),
                        ["lrx_description"] = (object)eventModel.EventShortDesc,
                        ["lrx_fundraisineventid"] = int.Parse(eventModel.EventId),
                        ["lrx_location"] = (object)eventModel.EventLocation
                    };

                    DateTime eventStartDate;
                    if (eventModel.EventDate != "0000-00-00" && DateTime.TryParse(eventModel.EventDate, out eventStartDate))
                        updatedEventRecord["lrx_proposedstart"] = (object)eventStartDate;

                    DateTime eventEndDate;
                    if (eventModel.EventClosedDate != "0000-00-00" && DateTime.TryParse(eventModel.EventClosedDate, out eventEndDate))
                        updatedEventRecord["lrx_proposedend"] = (object)eventEndDate;

                    this._service.Update(updatedEventRecord);
                    eventRecordId = existingEventRecord.Id;
                }
                else
                {
                    Entity newEventRecord = new Entity("lrx_event")
                    {
                        ["lrx_name"] = (object)eventModel.EventName,
                        ["lrx_goal"] = (object)new Money(Decimal.Parse(eventModel.EventTarget)),
                        ["lrx_description"] = (object)eventModel.EventShortDesc,
                        ["lrx_fundraisineventid"] = int.Parse(eventModel.EventId),
                        ["lrx_location"] = (object)eventModel.EventLocation
                    };

                    DateTime eventStartDate;
                    if (eventModel.EventDate != "0000-00-00" && DateTime.TryParse(eventModel.EventDate, out eventStartDate))
                        newEventRecord["lrx_proposedstart"] = (object)eventStartDate;

                    DateTime eventEndDate;
                    if (eventModel.EventClosedDate != "0000-00-00" && DateTime.TryParse(eventModel.EventClosedDate, out eventEndDate))
                        newEventRecord["lrx_proposedend"] = (object)eventEndDate;

                    eventRecordId = this._service.Create(newEventRecord);
                }
            }

            this._tracingService.Trace("Fundraising Event Record Sync Completed", Array.Empty<object>());
            return Task.CompletedTask;
        }

        public Task GetFundraisinParticipantRecords()
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
                var contactFields = new Dictionary<string, object>();

                void AddIfValid(string key, object value)
                {
                    if (value == null)
                        return;

                    if (value is string str && string.IsNullOrWhiteSpace(str))
                        return;

                    contactFields[key] = value;
                }

                // Safely parse MemberId
                int? memberIdValue = int.TryParse(participant.MemberId, out int parsedMemberId)
                    ? parsedMemberId
                    : (int?)null;

                // Add fields
                AddIfValid("firstname", participant.MFname);
                AddIfValid("lastname", participant.MLname);
                AddIfValid("emailaddress1", participant.MEmail);
                AddIfValid("telephone1", participant.MPhoneHome);
                AddIfValid("mobilephone", participant.MPhoneMobile);
                AddIfValid("address1_line1", participant.MAddressStreet);
                AddIfValid("address1_city", participant.MAddressSuburb);
                AddIfValid("address1_postalcode", participant.MAddressPCode);
                AddIfValid("address1_stateorprovince", participant.MAddressState);
                AddIfValid("address1_country", participant.MAddressCountry);
                AddIfValid("lrx_fundraisinmemberid", memberIdValue);


                if (existingMember == null)
                {
                    var searchConditions = new List<ConditionExpression>
                    {
                        new ConditionExpression("firstname", ConditionOperator.Equal, participant.MFname),
                        new ConditionExpression("lastname", ConditionOperator.Equal, participant.MLname),
                        new ConditionExpression("emailaddress1", ConditionOperator.Equal, participant.MEmail)
                    };

                    Entity existingContact = FindExistingRecord("contact", searchConditions);

                    if (existingContact == null)
                    {
                        Entity newContact = new Entity("contact");

                        foreach (var field in contactFields)
                        {
                            newContact[field.Key] = field.Value;
                        }

                        this._service.Create(newContact);
                    }
                    else
                    {
                        contactFields.Remove("firstname"); // Only update contact details, not name/email
                        contactFields.Remove("lastname");
                        contactFields.Remove("emailaddress1");

                        var entityToUpdate = new Entity("contact") { Id = existingContact.Id };
                        foreach (var field in contactFields)
                        {
                            entityToUpdate[field.Key] = field.Value;
                        }

                        this._service.Update(entityToUpdate);
                    }
                }
                else
                {
                    var entityToUpdate = new Entity("contact") { Id = existingMember.Id };
                    foreach (var field in contactFields)
                    {
                        entityToUpdate[field.Key] = field.Value;
                    }

                    this._service.Update(entityToUpdate);
                }
            }

            this._tracingService.Trace("Participant Record Fundraisin API Completed");
            return Task.CompletedTask;
        }

        public Task GetRegistrationFromParticipantEventRecord()
        {
            string participantEventURL = baseURL + "participantsevents";
            string csvContent = CallFundRaisinAPI((object)participantEventURL);

            var participantEventList = ParseCsvHelper<ParticipantEventModel, ParticipantEventModelMap>(csvContent);
            foreach (var participantEvent in participantEventList)
            {
                Guid contactID = Guid.Empty;
                Guid eventID = Guid.Empty;
                Guid TicketID = Guid.Empty;
                Guid paidByMember = Guid.Empty;
                Guid paidMemberRegistration = Guid.Empty;
                Guid TeamId = Guid.Empty;
                Guid TransactionID = Guid.Empty;
                decimal entreeAmount = 0;
                string ContactFullName = "";
                string EventName = "";

                //Get Member / Contact
                var MemberSearchConditions = new List<ConditionExpression>
                {
                    new ConditionExpression("lrx_fundraisinmemberid", ConditionOperator.Equal, participantEvent.Member_Id)
                };

                Entity existingMember = FindExistingRecord("contact", MemberSearchConditions);
                if (existingMember != null)
                {
                    contactID = (Guid)existingMember.Id;
                    // Retrieve full name if available
                    if (existingMember.Attributes.Contains("fullname"))
                    {
                        ContactFullName = existingMember["fullname"].ToString();
                    }
                }

                //Get Event Id
                var EventSearchConditions = new List<ConditionExpression>
                {
                    new ConditionExpression("lrx_fundraisineventid", ConditionOperator.Equal, participantEvent.Event_Id)
                };

                Entity existingEvent = FindExistingRecord("lrx_event", EventSearchConditions);
                if (existingEvent != null)
                {
                    eventID = (Guid)existingEvent.Id;
                    // Retrieve event name if available
                    if (existingEvent.Attributes.Contains("lrx_name"))
                    {
                        EventName = existingEvent["lrx_name"].ToString();
                    }
                }

                if (contactID == Guid.Empty || eventID == Guid.Empty)
                {
                    continue;
                }

                //Get or Create Ticket
                decimal entryFeeRecord = decimal.Parse(participantEvent.Total_Paid_Entry.ToString());
                entreeAmount = entryFeeRecord;

                //Get Member / Contact who paid for registration
                if (participantEvent.Paid_Member_Id.Trim() != "0")
                {
                    var matchMemberID = participantEventList.FirstOrDefault(m => m.Member_Id.Trim() == participantEvent.Paid_Member_Id.Trim());
                    if (matchMemberID != null)
                    {
                        var PaidMemberRegistrationSearchConditions = new List<ConditionExpression>
                        {
                            new ConditionExpression("lrx_fundraisinregistrationid", ConditionOperator.Equal, matchMemberID.History_Id)
                        };
                        Entity existingPaidRegistration = FindExistingRecord("lrx_registrations", PaidMemberRegistrationSearchConditions);

                        if (existingPaidRegistration != null)
                        {
                            paidMemberRegistration = existingPaidRegistration.Id;
                            if (existingPaidRegistration.Contains("lrx_transaction") && existingPaidRegistration["lrx_transaction"] is EntityReference transactionRef)
                            {
                                TransactionID = transactionRef.Id;
                            }
                        }

                        var PaidMemberSearchConditions = new List<ConditionExpression>
                        {
                            new ConditionExpression("lrx_fundraisinmemberid", ConditionOperator.Equal, participantEvent.Paid_Member_Id)
                        };

                        Entity existingPaidMember = FindExistingRecord("contact", PaidMemberSearchConditions);
                        if (existingPaidMember != null) 
                        {
                            paidByMember = existingPaidMember.Id;
                        }
                    }
                }

                var EventTeamSearchConditions = new List<ConditionExpression>
                {
                    new ConditionExpression("lrx_fundraisinteamid", ConditionOperator.Equal, participantEvent.Team_Id)
                };

                Entity existingEventTeam = FindExistingRecord("lrx_eventteam", EventTeamSearchConditions);
                if (existingEventTeam != null)
                { 
                    TeamId = existingEventTeam.Id;
                }

                var RegistrationSearchConditions = new List<ConditionExpression>
                {
                    new ConditionExpression("lrx_fundraisinregistrationid", ConditionOperator.Equal, participantEvent.History_Id)
                };
                string identifierName = ContactFullName + " - " + EventName;
                Entity existingRegistration = FindExistingRecord("lrx_registrations", RegistrationSearchConditions);
                var entity = new Entity("lrx_registrations")
                {
                    ["lrx_event"] = new EntityReference("lrx_event", eventID),
                    ["lrx_name"] = identifierName,
                    ["lrx_eventticket"] = TicketID != Guid.Empty ? new EntityReference("lrx_eventticket", TicketID) : null,
                    ["lrx_priceperregistration"] = new Money(entreeAmount),
                    ["lrx_constituentorganization"] = new EntityReference("contact", contactID),
                    ["lrx_eventteam"] = TeamId != Guid.Empty ? new EntityReference("lrx_eventteam", TeamId) : null,
                    ["lrx_transaction"] = TransactionID != Guid.Empty ? new EntityReference("msnfp_transaction", TransactionID) : null,
                    ["lrx_registeredby"] = paidByMember != Guid.Empty ? new EntityReference("contact", paidByMember) : new EntityReference("contact", contactID),
                    ["lrx_registrationpaidby"] = paidMemberRegistration != Guid.Empty ? new EntityReference("lrx_registrations", paidMemberRegistration) : null,
                    ["lrx_promoid"] = int.TryParse(participantEvent.Promo_Id.ToString(), out int promoId) ? promoId : (int?)null,
                    ["lrx_date"] = DateTime.Parse(participantEvent.Date_Created),
                    ["lrx_fundraisinregistrationid"] = int.TryParse(participantEvent.History_Id, out int historyId) ? historyId : (int?)null
                };

                if (participantEvent.Is_Paid != "Y") 
                {
                    entity["statuscode"] = new OptionSetValue(1);
                }

                if (existingRegistration == null)
                {
                    entity.Id = this._service.Create(entity);
                }
                else
                {
                    entity.Id = existingRegistration.Id;
                    this._service.Update(entity);
                }

            }
            this._tracingService.Trace("Registration Record Fundraisin API Completed");
            return Task.CompletedTask;
        }

        public Task GetFundraisinTicketRecords()
        {
            var ticketList = this.GetData<TicketsModel, TicketsModelMap>(this.baseURL, "tickets");

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
                    continue;
                }

                var TicketSearchConditions = new List<ConditionExpression>
                {
                    new ConditionExpression("lrx_fundraisinticketid", ConditionOperator.Equal, tickets.ticket_id)
                };

                Entity existingTicket = FindExistingRecord("lrx_eventticket", TicketSearchConditions);
                Entity ticketEntity = new Entity("lrx_eventticket")
                {
                    ["lrx_name"] = tickets.ticket_name,
                    ["lrx_quantity"] = int.TryParse(tickets.ticket_limit, out int quantity) ? quantity : 0,
                    ["lrx_eventticketdescription"] = tickets.ticket_description,
                    ["lrx_amount"] = decimal.TryParse(tickets.ticket_price, out decimal price) ? new Money(price) : new Money(0),
                    ["lrx_event"] = new EntityReference("lrx_event", eventID),
                    ["lrx_fundraisinticketid"] = int.TryParse(tickets.ticket_id, out int ticketId) ? ticketId : 0
                };

                if (existingTicket == null)
                {
                    this._service.Create(ticketEntity);
                }
                else
                {
                    ticketEntity.Id = existingTicket.Id;
                    this._service.Update(ticketEntity);
                }

            }

            this._tracingService.Trace("Ticket Record Fundraisin API Completed");
            return Task.CompletedTask;
        }

        public Task GetFundraisinTicketHolderRecord()
        {
            foreach (TicketHolderModel ticketHolder in this.ParseCsvHelper<TicketHolderModel, TicketHolderModelMap>(this.CallFundRaisinAPI((object)(this.baseURL + "ticketholders"))))
            {
                Guid eventId = Guid.Empty;
                Guid registrationId = Guid.Empty;
                Guid relatedRegistrationId = Guid.Empty;
                Guid ticketId = Guid.Empty;
                Guid TransactionID = Guid.Empty;
                Guid contactID = Guid.Empty;
                Guid eventTable = Guid.Empty;

                string EventName = string.Empty;

                var EventSearchConditions = new List<ConditionExpression>
                {
                    new ConditionExpression("lrx_fundraisineventid", ConditionOperator.Equal, ticketHolder.event_id)
                };

                Entity existingEvent = FindExistingRecord("lrx_event", EventSearchConditions);
                if (existingEvent != null)
                {
                    eventId = (Guid)existingEvent.Id;
                    // Retrieve event name if available
                    if (existingEvent.Attributes.Contains("lrx_name"))
                    {
                        EventName = existingEvent["lrx_name"].ToString();
                    }
                }
                
                Entity existingTicket = this.FindExistingRecord("lrx_eventticket", new List<ConditionExpression>()
                {
                    new ConditionExpression("lrx_fundraisinticketid", ConditionOperator.Equal, (object)ticketHolder.ticket_id)
                });

                if (existingTicket != null)
                    ticketId = existingTicket.Id;

                var existingRegistration = FindExistingRecord("lrx_registrations",
                new List<ConditionExpression>
                {
                    new ConditionExpression("lrx_fundraisinregistrationid", ConditionOperator.Equal, ticketHolder.history_id)
                });

                
                if (existingRegistration != null) {
                    registrationId = existingRegistration.Id;
                    if (existingRegistration.Attributes.TryGetValue("lrx_transaction", out var transactionObj) &&
                    transactionObj is EntityReference transactionRef)
                    {
                        TransactionID = transactionRef.Id;
                    }
                }                

                var MemberSearchConditions = new List<ConditionExpression>
                {
                    new ConditionExpression("lrx_fundraisinmemberid", ConditionOperator.Equal, ticketHolder.member_id)
                };

                Entity existingMember = FindExistingRecord("contact", MemberSearchConditions);
                if (existingMember != null)
                {
                    contactID = (Guid)existingMember.Id;
                }

                if (eventId == Guid.Empty || registrationId == Guid.Empty)
                {
                    continue;
                }
                if (ticketHolder.table_id != "0") 
                {
                    eventTable = GetFundraisinTableRecord(ticketHolder.event_id, ticketHolder.table_id, eventId, ticketId);
                }             

                if (ticketHolder.related_member_id == "0" && ticketHolder.related_history_id == "0")
                {
                    string ContactFullName = ticketHolder.g_fname + " " + ticketHolder.g_lname;
                    Entity existingGuest = this.FindExistingRecord("contact", new List<ConditionExpression>()
                    {
                        new ConditionExpression("lrx_fundraisinguestid", ConditionOperator.Equal, (object)ticketHolder.guest_id)
                    });

                    int guestIdValue;
                    Entity guestEntity = new Entity("contact")
                    {
                        ["firstname"] = (object)ticketHolder.g_fname,
                        ["lastname"] = (object)ticketHolder.g_lname,
                        ["emailaddress1"] = (object)ticketHolder.g_email,
                        ["telephone1"] = (object)(ticketHolder.g_phone_suffix + ticketHolder.g_phone),
                        ["mobilephone"] = (object)(ticketHolder.g_phone_suffix + ticketHolder.g_phone),
                        ["address1_line1"] = (object)(ticketHolder.g_address_unit + ticketHolder.g_address_street),
                        ["address1_city"] = (object)ticketHolder.g_address_suburb,
                        ["address1_postalcode"] = (object)ticketHolder.g_address_pcode,
                        ["address1_stateorprovince"] = (object)ticketHolder.g_address_state,
                        ["address1_country"] = (object)ticketHolder.g_address_country,
                        ["lrx_fundraisinguestid"] = (object)(int.TryParse(ticketHolder.guest_id, out guestIdValue) ? guestIdValue : 0)
                    };

                    Guid guestId;
                    if (existingGuest != null)
                    {
                        guestId = existingGuest.Id;
                        guestEntity.Id = existingGuest.Id;
                        this._service.Update(guestEntity);
                    }
                    else
                    {
                        Entity matchingGuest = this.FindExistingRecord("contact", new List<ConditionExpression>()
                        {
                            new ConditionExpression("firstname", ConditionOperator.Equal, (object)ticketHolder.g_fname),
                            new ConditionExpression("lastname", ConditionOperator.Equal, (object)ticketHolder.g_lname),
                            new ConditionExpression("emailaddress1", ConditionOperator.Equal, (object)ticketHolder.g_email)
                        });

                        if (matchingGuest != null)
                        {
                            guestId = matchingGuest.Id;
                            guestEntity.Id = matchingGuest.Id;
                            this._service.Update(guestEntity);
                        }
                        else
                        {
                            guestId = this._service.Create(guestEntity);
                        }
                    }

                    var RegistrationSearchConditions = new List<ConditionExpression>
                    {
                        new ConditionExpression("lrx_constituentorganization", ConditionOperator.Equal, guestId),
                        new ConditionExpression("lrx_event", ConditionOperator.Equal, eventId)
                    };
                    string identifierName = ContactFullName + " - " + EventName;
                    Entity existingTicketRegistration = FindExistingRecord("lrx_registrations", RegistrationSearchConditions);
                    var registrationEntity = new Entity("lrx_registrations")
                    {
                        ["lrx_event"] = new EntityReference("lrx_event", eventId),
                        ["lrx_name"] = identifierName,
                        ["lrx_eventticket"] = ticketId != Guid.Empty ? new EntityReference("lrx_eventticket", ticketId) : null,
                        ["lrx_eventtable"] = eventTable != Guid.Empty ? new EntityReference("lrx_eventtable", eventTable) : null,
                        ["lrx_priceperregistration"] = new Money(0),
                        ["lrx_constituentorganization"] = new EntityReference("contact", guestId),
                        ["lrx_transaction"] = TransactionID != Guid.Empty ? new EntityReference("msnfp_transaction", TransactionID) : null,
                        ["lrx_registeredby"] = contactID != Guid.Empty ? new EntityReference("contact", contactID) : null,
                        ["lrx_date"] = DateTime.Parse(ticketHolder.date_created),
                        ["lrx_registrationpaidby"] = registrationId != Guid.Empty ? new EntityReference("lrx_registrations", registrationId) : null
                    };

                    if (existingTicketRegistration == null)
                    {
                        registrationEntity.Id = this._service.Create(registrationEntity);
                    }
                    else
                    {
                        registrationEntity.Id = existingTicketRegistration.Id;
                        this._service.Update(registrationEntity);
                    }
                }
                else
                {
                    if (ticketId != Guid.Empty)
                    {
                        if (ticketHolder.history_id.Trim() != ticketHolder.related_history_id.Trim())
                        {
                            Entity relatedRegistration = this.FindExistingRecord("lrx_registrations", new List<ConditionExpression>()
                            {
                                new ConditionExpression("lrx_fundraisinregistrationid", ConditionOperator.Equal, (object)ticketHolder.related_history_id)
                            });

                            if (relatedRegistration != null)
                                relatedRegistrationId = relatedRegistration.Id;

                            if (relatedRegistrationId != Guid.Empty)
                            {
                                this._service.Update(new Entity("lrx_registrations", relatedRegistrationId)
                                {
                                    ["lrx_eventtable"] = eventTable != Guid.Empty ? new EntityReference("lrx_eventtable", eventTable) : null,
                                    ["lrx_eventticket"] = ticketId != Guid.Empty ? (object)new EntityReference("lrx_eventticket", ticketId) : (object)null,
                                    ["lrx_registrationpaidby"] = registrationId != Guid.Empty ? (object)new EntityReference("lrx_registrations", registrationId) : (object)null
                                });
                            }
                        }
                        else
                        {
                            this._service.Update(new Entity("lrx_registrations", registrationId)
                            {
                                ["lrx_eventtable"] = eventTable != Guid.Empty ? new EntityReference("lrx_eventtable", eventTable) : null,
                                ["lrx_eventticket"] = ticketId != Guid.Empty ? (object)new EntityReference("lrx_eventticket", ticketId) : (object)null,
                                ["lrx_registrationpaidby"] = (object)null //do not reference self as paid by self
                            });
                        }
                    }
                }
            }

            this._tracingService.Trace("Ticket Holder Record Fundraising API Completed", Array.Empty<object>());
            return Task.CompletedTask;
        }

        public Task GetFundRaisinProductRecord()
        {
            var productList = this.GetData<ProductModel, ProductModelMap>(this.baseURL, "products");
            foreach (var products in productList)
            {
                var productType = products.product_type?.Trim() == "ecard" ? 856660001 : 856660000;

                var productIdTrimmed = products.product_id?.Trim();
                var productSearchConditions = new List<ConditionExpression>
                {
                    new ConditionExpression("lrx_fundraisinproductid", ConditionOperator.Equal, productIdTrimmed)
                };

                Entity existingProduct = FindExistingRecord("lrx_inventoryproduct", productSearchConditions);

                Entity productEntity = new Entity("lrx_inventoryproduct");

                if (existingProduct != null)
                    productEntity.Id = existingProduct.Id;

                productEntity["lrx_name"] = products.product_name?.Trim();
                productEntity["lrx_producttype"] = new OptionSetValue(productType);
                productEntity["lrx_productprice"] = new Money(decimal.Parse(products.product_price.Trim()));
                productEntity["lrx_productcost"] = new Money(decimal.Parse(products.product_cost.Trim()));
                productEntity["lrx_maximumbuyqty"] = int.Parse(products.max_buy_limit.Trim());
                productEntity["lrx_minimumbuyqty"] = int.Parse(products.min_buy_limit.Trim());
                productEntity["lrx_stocklevels"] = int.Parse(products.product_stock.Trim());
                productEntity["lrx_crmid"] = products.crm_product_id?.Trim();
                productEntity["lrx_description"] = products.product_description?.Trim();
                productEntity["lrx_fundraisinproductid"] = int.Parse(productIdTrimmed);

                if (existingProduct == null)
                    this._service.Create(productEntity);
                else
                    this._service.Update(productEntity);
            }

            this._tracingService.Trace("Product Record Fundraisin API Completed");
            return Task.CompletedTask;
        }

        public Task GetFundRaisinProductOptionsRecord()
        {
            var productOptionList = this.GetAllData<ProductOptionModel, ProductOptionModelMap>(this.baseURL, "productoptions");

            foreach (var productoptions in productOptionList)
            {
                var productOptionType = 856660002; // default to others
                if (productoptions.option_type == "size")
                    productOptionType = 856660000; //change to size
                if (productoptions.option_type == "colour")
                    productOptionType = 856660001; //change to size

                var statusCode = 1; // default to draft
                if (productoptions.option_status == "1")
                    statusCode = 856660001;

                var ProductSearchConditions = new List<ConditionExpression>
                {
                    new ConditionExpression("lrx_fundraisinproductid", ConditionOperator.Equal, productoptions.product_id)
                };
                Entity existingProduct = FindExistingRecord("lrx_inventoryproduct", ProductSearchConditions);

                if (existingProduct != null)
                {
                    Guid productID = (Guid)existingProduct.Id;
                    var ProductOptionSearchConditions = new List<ConditionExpression>
                    {
                        new ConditionExpression("lrx_inventoryproduct", ConditionOperator.Equal, existingProduct.Id),
                        new ConditionExpression("lrx_name", ConditionOperator.Equal, productoptions.option_name)
                    };
                    Entity existingProductOption = FindExistingRecord("lrx_productoptions", ProductOptionSearchConditions);

                    if (existingProductOption == null)
                    {
                        Guid productOptionID = this._service.Create(new Entity("lrx_productoptions")
                        {
                            ["lrx_name"] = (object)productoptions.option_name,
                            ["lrx_optiontype"] = new OptionSetValue(productOptionType),
                            ["statuscode"] = new OptionSetValue(statusCode),
                            ["lrx_stock"] = int.Parse(productoptions.option_stock),
                            ["lrx_inventoryproduct"] = (object)new EntityReference("lrx_inventoryproduct", productID),
                            ["lrx_fundraisinoptionid"] = int.Parse(productoptions.option_id)
                        });
                    }
                    else
                    {
                        this._service.Update(new Entity("lrx_productoptions", existingProductOption.Id)
                        {
                            ["lrx_name"] = (object)productoptions.option_name,
                            ["lrx_optiontype"] = new OptionSetValue(productOptionType),
                            ["statuscode"] = new OptionSetValue(statusCode),
                            ["lrx_stock"] = int.Parse(productoptions.option_stock),
                            ["lrx_inventoryproduct"] = (object)new EntityReference("lrx_inventoryproduct", productID),
                            ["lrx_fundraisinoptionid"] = int.Parse(productoptions.option_id)
                        });
                    }
                }
            }

            this._tracingService.Trace("Product Option Record Fundraisin API Completed");
            return Task.CompletedTask;
        }

        public Task GetFundraisinProductSalesItem()
        {
            var saleItemList = this.GetData<SaleItemModel, SaleItemModelMap>(this.baseURL, "salesitems");
            var productList = this.GetAllData<ProductModel, ProductModelMap>(this.baseURL, "products");
            var productOptionList = this.GetAllData<ProductOptionModel, ProductOptionModelMap>(this.baseURL, "productoptions");
            string previousSaleID = string.Empty;
            Guid contactID = Guid.Empty;
            saleItemList = saleItemList.OrderBy(x => x.sale_id).ToList();

            foreach (var saleitem in saleItemList)
            {
                Guid productID = Guid.Empty;
                Guid productOption = Guid.Empty;

                string contactFullName = string.Empty;
                decimal GSTamount = 0;
                string productName = "";
                string productOptionName = "";

                var currentSaleID = saleitem.sale_id.Trim();

                if (!string.Equals(previousSaleID, currentSaleID, StringComparison.Ordinal))
                {
                    contactID = UpsertContactFromSales(currentSaleID, out contactFullName, out GSTamount);
                    previousSaleID = currentSaleID;
                }

                var matchingProduct = productList.FirstOrDefault(p => p.product_id.Trim() == saleitem.product_id.Trim());

                if (matchingProduct != null)
                {
                    productName = matchingProduct.product_name;
                }

                var matchingProductOption = productOptionList.FirstOrDefault(p => p.product_id.Trim() == saleitem.product_id.Trim());
                if (matchingProductOption != null)
                {
                    productOptionName = matchingProductOption.option_name;
                }

                var productSearchConditions = new List<ConditionExpression>
                {
                    new ConditionExpression("lrx_fundraisinproductid", ConditionOperator.Equal, saleitem.product_id)
                };
                Entity existingInventoryProduct = FindExistingRecord("lrx_inventoryproduct", productSearchConditions);

                if (existingInventoryProduct != null)
                {
                    productID = existingInventoryProduct.Id;
                }
                else
                {
                    continue;
                }

                var productOptionSearchConditions = new List<ConditionExpression>
                {
                    new ConditionExpression("lrx_fundraisinoptionid", ConditionOperator.Equal, saleitem.option_id)
                };
                Entity existingProductOption = FindExistingRecord("lrx_productoptions", productOptionSearchConditions);

                if (existingProductOption != null)
                {
                    productOption = existingProductOption.Id;
                }

                var saleProduct = new Entity("lrx_product")
                {
                    ["lrx_name"] = $"{productName} - {productOptionName}",
                    ["lrx_constituentorganisation"] = new EntityReference("contact", contactID),
                    ["lrx_date"] = DateTime.Parse(saleitem.date_created),
                    ["lrx_productmigrationid"] = saleitem.id.Trim(),
                    ["lrx_fundraisinsalesid"] = int.Parse(saleitem.sale_id.Trim())
                };

                // Parse quantity safely
                if (int.TryParse(saleitem.quantity, out int parsedQuantity))
                {
                    saleProduct["lrx_quantity"] = parsedQuantity;
                }

                // Parse unit cost safely
                if (decimal.TryParse(saleitem.unit_cost, out decimal parsedPrice))
                {
                    saleProduct["lrx_priceperproduct"] = new Money(parsedPrice);
                    saleProduct["lrx_productamount"] = new Money(parsedPrice);
                }


                if (productOption != Guid.Empty)
                {
                    saleProduct["lrx_productoption"] = new EntityReference("lrx_productoptions", productOption);
                }

                // 🔍 Check existing product sale
                var productSaleSearchConditions = new List<ConditionExpression>
                {
                    new ConditionExpression(
                        "lrx_productmigrationid",
                        ConditionOperator.Equal,
                        saleitem.id.Trim()
                    )
                };

                Guid ProductSaleGuid = Guid.Empty;

                Entity existingProductSale = FindExistingRecord("lrx_product", productSaleSearchConditions);

                if (existingProductSale == null)
                {
                    ProductSaleGuid = _service.Create(saleProduct);
                }
                else
                {
                    ProductSaleGuid = existingProductSale.Id;
                    var saleProductUpdate = new Entity("lrx_product", existingProductSale.Id);

                    saleProductUpdate.Attributes.AddRange(saleProduct.Attributes);

                    _service.Update(saleProductUpdate);
                }
            }

            return Task.CompletedTask;
        }

        public Task GetFundRaisinEventTeamsRecord()
        {
            string url = baseURL + "teams";
            string csvContent = CallFundRaisinAPI((object)url);

            var EventTeamList = ParseCsvHelper<EventTeamModel, EventTeamModelMap>(csvContent);
            foreach (var eventTeams in EventTeamList)
            {
                Guid eventID = Guid.Empty;
                var EventSearchConditions = new List<ConditionExpression>
                {
                    new ConditionExpression("lrx_fundraisineventid", ConditionOperator.Equal, eventTeams.event_id)
                };

                Entity existingEvent = FindExistingRecord("lrx_event", EventSearchConditions);
                if (existingEvent != null)
                    eventID = (Guid)existingEvent.Id;

                if (eventID == Guid.Empty)
                {
                    continue;
                }

                Guid contactID = Guid.Empty;
                var ContactSearchConditions = new List<ConditionExpression>
                {
                    new ConditionExpression("lrx_fundraisinmemberid", ConditionOperator.Equal, eventTeams.captain_id)
                };

                Entity existingContact = FindExistingRecord("contact", ContactSearchConditions);

                if (existingContact != null)
                    contactID = (Guid)existingContact.Id;

                if (contactID == Guid.Empty)
                {
                    continue;
                }

                var EventTeamSearchConditions = new List<ConditionExpression>
                {
                    new ConditionExpression("lrx_fundraisinteamid", ConditionOperator.Equal, eventTeams.team_id)
                };

                Entity existingEventTeam = FindExistingRecord("lrx_eventteam", EventTeamSearchConditions);
                if (existingEventTeam == null)
                {
                    Guid eventTeamID = this._service.Create(new Entity("lrx_eventteam")
                    {
                        ["lrx_name"] = (object)eventTeams.t_name,
                        ["lrx_registeredby"] = (object)new EntityReference("contact", contactID),
                        ["lrx_dateregistered"] = (object)eventTeams.date_created,
                        ["lrx_fundraisinggoalpledge"] = new Money(decimal.Parse(eventTeams.t_target)),
                        ["lrx_teamdescription"] = (object)eventTeams.t_page_title,
                        ["lrx_event"] = (object)new EntityReference("lrx_event", eventID),
                        ["lrx_fundraisinteamid"] = int.Parse(eventTeams.team_id)
                    });
                }
                else
                {
                    this._service.Update(new Entity("lrx_eventteam", existingEventTeam.Id)
                    {
                        ["lrx_name"] = (object)eventTeams.t_name,
                        ["lrx_registeredby"] = (object)new EntityReference("contact", contactID),
                        ["lrx_dateregistered"] = (object)eventTeams.date_created,
                        ["lrx_fundraisinggoalpledge"] = new Money(decimal.Parse(eventTeams.t_target)),
                        ["lrx_teamdescription"] = (object)eventTeams.t_page_title,
                        ["lrx_event"] = (object)new EntityReference("lrx_event", eventID),
                        ["lrx_fundraisinteamid"] = int.Parse(eventTeams.team_id)
                    });
                }
            }

            this._tracingService.Trace("Event Team Record Fundraisin API Completed");
            return Task.CompletedTask;
        }

        public Task GetFundRaisinOrganisationRecord()
        {
            string url = baseURL + "orgpages";
            string csvContent = CallFundRaisinAPI((object)url);

            var OrganisationList = ParseCsvHelper<OrganisationModel, OrganisationModelMap>(csvContent);
            foreach (var organisations in OrganisationList)
            {
                Guid contactID = Guid.Empty;
                var ContactSearchConditions = new List<ConditionExpression>
                {
                    new ConditionExpression("lrx_fundraisinmemberid", ConditionOperator.Equal, organisations.created_member_id)
                };

                Entity existingContact = FindExistingRecord("contact", ContactSearchConditions);

                if (existingContact != null)
                    contactID = (Guid)existingContact.Id;

                if (contactID == Guid.Empty)
                {
                    continue;
                }

                var OrganisationSearchConditions = new List<ConditionExpression>
                {
                    new ConditionExpression("lrx_fundraisinorgid", ConditionOperator.Equal, organisations.org_id)
                };

                Entity existingOrganisation = FindExistingRecord("account", OrganisationSearchConditions);
                if (existingOrganisation == null)
                {
                    Guid organisationID = this._service.Create(new Entity("account")
                    {
                        ["name"] = (object)organisations.org_name,
                        ["primarycontactid"] = (object)new EntityReference("contact", contactID),
                        ["msnfp_accounttype"] = new OptionSetValue(844060001),
                        ["lrx_fundraisinorgid"] = int.Parse(organisations.org_id)
                    });
                }
                else
                {
                    this._service.Update(new Entity("account", existingOrganisation.Id)
                    {
                        ["name"] = (object)organisations.org_name,
                        ["primarycontactid"] = (object)new EntityReference("contact", contactID),
                        ["msnfp_accounttype"] = new OptionSetValue(844060001),
                        ["lrx_fundraisinorgid"] = int.Parse(organisations.org_id)
                    });
                }
            }

            this._tracingService.Trace("Organisation Record Fundraisin API Completed");
            return Task.CompletedTask;
        }

        public Task GetFundRaisinPromoCodeRecord()
        {
            string url = baseURL + "promocodes";
            string csvContent = CallFundRaisinAPI((object)url);

            var PromoList = ParseCsvHelper<PromoCodeModel, PromoCodeModelMap>(csvContent);
            foreach (var promos in PromoList)
            {
                int promoValueType = 856660000;
                if (promos.promo_type == "percentage")
                {
                    promoValueType = 856660001;
                }
                var PromoSearchConditions = new List<ConditionExpression>
                {
                    new ConditionExpression("lrx_fundraisinpromoid", ConditionOperator.Equal, promos.promo_id)
                };

                Entity existingPromo = FindExistingRecord("lrx_promocodeanddiscount", PromoSearchConditions);
                if (existingPromo == null)
                {
                    Guid promoID = this._service.Create(new Entity("lrx_promocodeanddiscount")
                    {
                        ["lrx_fundraisinpromoid"] = int.Parse(promos.promo_id),
                        ["lrx_promocode"] = (object)promos.promo_code,
                        ["lrx_promovalue"] = decimal.Parse(promos.promo_value),
                        ["lrx_promovaluetype"] = new OptionSetValue(promoValueType)
                    });
                }
                else
                {
                    this._service.Update(new Entity("lrx_promocodeanddiscount", existingPromo.Id)
                    {
                        ["lrx_fundraisinpromoid"] = int.Parse(promos.promo_id),
                        ["lrx_promocode"] = (object)promos.promo_code,
                        ["lrx_promovalue"] = decimal.Parse(promos.promo_value),
                        ["lrx_promovaluetype"] = new OptionSetValue(promoValueType)
                    });
                }
            }

            this._tracingService.Trace("Promocode Record Fundraisin API Completed");
            return Task.CompletedTask;
        }

        public Task GetFundraisinRaffleRecords()
        {
            var raffleList = this.GetAllData<RaffleModel, RaffleModelMap>(this.baseURL, "raffles");
            
            foreach (var raffle in raffleList)
            {
                Guid raffleID = Guid.Empty;
                int entryStatus = 856660000; // default to open entry
                int allowSinglePurchase = 856660000; // default to open entry

                if (raffle.entries_closed != "N")
                {
                    entryStatus = 856660001;
                }

                if (raffle.allow_single_tickets != "Y")
                {
                    allowSinglePurchase = 856660001;
                }
                var RaffleSearchConditions = new List<ConditionExpression>
                {
                    new ConditionExpression("lrx_platformid", ConditionOperator.Equal, raffle.raffle_id)
                };

                Entity existingRaffle = FindExistingRecord("lrx_raffle", RaffleSearchConditions);
              
                Entity raffleEntity = new Entity("lrx_raffle")
                {
                    ["lrx_name"] = raffle.raffle_name,
                    ["lrx_rafflecode"] = raffle.raffle_code,
                    ["lrx_entrystatus"] = new OptionSetValue(entryStatus),
                    ["lrx_closedmessage"] = raffle.raffle_closed_message,
                    ["lrx_numberofticketsavailable"] = int.Parse(raffle.number_tickets),
                    ["lrx_startingnumber"] = int.Parse(raffle.ticket_start),
                    ["lrx_singleticketsallowsingleticketpurchases"] = new OptionSetValue(allowSinglePurchase),
                    ["lrx_maximumpurchasable"] = int.Parse(raffle.max_tickets),
                    ["lrx_singleticketprice"] = decimal.TryParse(raffle.ticket_price, out decimal price) ? new Money(price) : new Money(0),
                    ["lrx_raffleshortdescription"] = raffle.raffle_short_desc,
                    ["lrx_platformid"] = raffle.raffle_id,
                    ["lrx_fundraisinraffleid"] = int.Parse(raffle.raffle_id)
                };

                if (!string.IsNullOrWhiteSpace(raffle.raffle_end_date) && raffle.raffle_end_date != "0000-00-00" &&
                DateTime.TryParse(raffle.raffle_end_date, out DateTime raffleEndDate))
                {
                    raffleEntity["lrx_raffleexpiry"] = raffleEndDate;
                }

                if (existingRaffle == null)
                {
                    this._service.Create(raffleEntity);
                }
                else
                {
                    raffleEntity.Id = existingRaffle.Id;
                    this._service.Update(raffleEntity);
                }

            }

            this._tracingService.Trace("Raffle Record Fundraisin API Completed");
            return Task.CompletedTask;
        }

        public Task GetFundraisinRaffleTicketOptionRecords()
        {
            var raffleTicketList = this.GetAllData<RaffleTicketModel, RaffleTicketModelMap>(this.baseURL, "raffletickets");
            foreach (var raffleTicket in raffleTicketList)
            {
                Guid raffleTicketID = Guid.Empty;
                Guid raffleID = Guid.Empty;

                var RaffleSearchConditions = new List<ConditionExpression>
                {
                    new ConditionExpression("lrx_platformid", ConditionOperator.Equal, raffleTicket.raffle_id)
                };

                Entity existingRaffle = FindExistingRecord("lrx_raffle", RaffleSearchConditions);

                if(existingRaffle == null)
                {
                    continue;
                }
                else
                {
                    raffleID = existingRaffle.Id;
                }

                var RaffleTicketSearchConditions = new List<ConditionExpression>
                {
                    new ConditionExpression("lrx_fundraisinraffleoptionid", ConditionOperator.Equal, raffleTicket.option_id)
                };

                Entity existingRaffleTicket = FindExistingRecord("lrx_raffleticketoption", RaffleTicketSearchConditions);
                Entity raffleTicketEntity = new Entity("lrx_raffleticketoption")
                {
                    ["lrx_name"] = raffleTicket.option_description,
                    ["lrx_tickets"] = int.Parse(raffleTicket.option_tickets),
                    ["lrx_price"] = decimal.TryParse(raffleTicket.option_price, out decimal price) ? new Money(price) : new Money(0),
                    ["lrx_raffle"] = raffleID != Guid.Empty ? new EntityReference("lrx_raffle", raffleID) : null,
                    ["lrx_fundraisinraffleoptionid"] = int.Parse(raffleTicket.option_id)
                };

                if (existingRaffleTicket == null)
                {
                    this._service.Create(raffleTicketEntity);
                }
                else
                {
                    raffleTicketEntity.Id = existingRaffleTicket.Id;
                    this._service.Update(raffleTicketEntity);
                }
            }

            this._tracingService.Trace("Raffle Ticket Option Record Fundraisin API Completed");
            return Task.CompletedTask;
        }

        public Task GetFundraisinRaffleSalesRecords()
        {
            var raffleSalesList = this.GetData<RaffleSalesModel, RaffleSalesModelMap>(this.baseURL, "rafflesales");
            var raffleList = this.GetAllData<RaffleModel, RaffleModelMap>(this.baseURL, "raffles");
            foreach (var raffleSales in raffleSalesList)
            {
                Guid raffleSalesID = Guid.Empty;
                Guid raffleOptionD = Guid.Empty;
                Guid raffleID = Guid.Empty;
                Guid contactID = Guid.Empty;

                var RaffleSearchConditions = new List<ConditionExpression>
                {
                    new ConditionExpression("lrx_platformid", ConditionOperator.Equal, raffleSales.raffle_id)
                };

                Entity existingRaffle = FindExistingRecord("lrx_raffle", RaffleSearchConditions);

                if (existingRaffle == null)
                {
                    continue;
                }
                else
                {
                    raffleID = existingRaffle.Id;
                }

                var RaffleTicketSearchConditions = new List<ConditionExpression>
                {
                    new ConditionExpression("lrx_fundraisinraffleoptionid", ConditionOperator.Equal, raffleSales.option_id)
                };                

                Entity existingRaffleTicket = FindExistingRecord("lrx_raffleticketoption", RaffleTicketSearchConditions);
                if (existingRaffleTicket != null)
                {
                    raffleOptionD = existingRaffleTicket.Id;
                }

                contactID = UpsertContactFromRaffleSales(raffleSales);

                var RaffleSalesSearchConditions = new List<ConditionExpression>
                {
                    new ConditionExpression("lrx_fundraisinrafflesalesid", ConditionOperator.Equal, raffleSales.sale_id)
                };

                var raffleRecord = raffleList.FirstOrDefault(r => r.raffle_id.Trim() == raffleSales.raffle_id.Trim());
                string identifierName = $"{raffleSales.first_name} {raffleSales.last_name} - {raffleRecord.raffle_name}";
                Entity existingRaffleSales = FindExistingRecord("lrx_rafflesales", RaffleSalesSearchConditions);
                Entity raffleSalesEntity = new Entity("lrx_rafflesales")
                {
                    ["lrx_name"] = identifierName,
                    ["lrx_customer"] = contactID != Guid.Empty ? new EntityReference("contact", contactID) : null,                 
                    ["lrx_raffle"] = raffleID != Guid.Empty ? new EntityReference("lrx_raffle", raffleID) : null,
                    ["lrx_raffleoption"] = raffleOptionD != Guid.Empty ? new EntityReference("lrx_raffleticketoption", raffleOptionD) : null,
                    ["lrx_amountpaid"] = decimal.TryParse(raffleSales.sub_total, out decimal price) ? new Money(price) : new Money(0),
                    ["lrx_ponumber"] = raffleSales.po_number,
                    ["lrx_tickets"] = raffleSales.number_tickets,
                    ["lrx_startingnumber"] = raffleSales.ticket_start,
                    ["lrx_endingnumber"] = raffleSales.ticket_end,
                    ["lrx_fundraisinrafflesalesid"] = int.Parse(raffleSales.sale_id)
                };

                if (!string.IsNullOrWhiteSpace(raffleSales.date_paid) && raffleSales.date_paid != "0000-00-00" &&
                DateTime.TryParse(raffleSales.date_paid, out DateTime raffleSalesDate))
                {
                    raffleSalesEntity["lrx_datetimeofsale"] = raffleSalesDate;
                }

                if (existingRaffleSales == null)
                {
                    this._service.Create(raffleSalesEntity);
                }
                else
                {
                    raffleSalesEntity.Id = existingRaffleSales.Id;
                    this._service.Update(raffleSalesEntity);
                }

            }

            this._tracingService.Trace("Raffle Sales Record Fundraisin API Completed");
            return Task.CompletedTask;
        }

        public Task GetFundRaisinTransactionRecord()
        {
            var TransactionList = this.GetData<TransactionModel, TransactionModelMap>(this.baseURL, "transactions");
            var donationList = this.GetData<DonationModel, DonationModelMap>(this.baseURL, "donations");
            var scheduledDonationList = this.GetAllData<ScheduleModel, ScheduleModelMap>(this.baseURL, "scheduleddonations");
            var saleItemList = this.GetData<SaleItemModel, SaleItemModelMap>(this.baseURL, "salesitems");
            var productList = this.GetAllData<ProductModel, ProductModelMap>(this.baseURL, "products");
            var productOptionList = this.GetAllData<ProductOptionModel, ProductOptionModelMap>(this.baseURL, "productoptions");
            var participantList = this.GetData<ParticipantModel, ParticipantModelMap>(this.baseURL, "participants");
            var eventList = this.GetAllData<EventModel, EventModelMap>(this.baseURL, "events");
            var raffleSalesList = this.GetData<RaffleSalesModel, RaffleSalesModelMap>(this.baseURL, "rafflesales");

            if (TransactionList != null)
            {
                foreach (var transactions in TransactionList)
                {
                    Guid defaultPaymentMethodId = Guid.Empty;
                    Guid registrationID = Guid.Empty;
                    Guid teamID = Guid.Empty;
                    Guid contactID = Guid.Empty;
                    Guid eventID = Guid.Empty;
                    Guid scheduleID = Guid.Empty;
                    Guid solicitorID = Guid.Empty;
                    Guid promoGuid = Guid.Empty;
                    Guid campaignGuid = Guid.Empty;
                    Guid appealGuid = Guid.Empty;
                    Guid packageGuid = Guid.Empty;
                    Guid raffleSaleGuid = Guid.Empty;
                    Guid raffleGuid = Guid.Empty;
                    Guid transactionId = Guid.Empty;
                    Guid membershipTypeId = Guid.Empty;
                    Guid designationGUID = Guid.Empty;
                    string CustomDonationDate = "";

                    if (transactions.Event_id.Trim() != "0")
                    {          
                        eventID = CheckAndUpdateEvent(transactions.Event_id.Trim(), eventList, out campaignGuid, out appealGuid, out packageGuid, out designationGUID);                         
                    }

                    if (this.campaignName != string.Empty && campaignGuid == Guid.Empty)
                    {
                        var CampaignSearchConditions = new List<ConditionExpression>
                        {
                            new ConditionExpression("name", ConditionOperator.Equal, (object)this.campaignName)
                        };

                        Entity existingCampaign = FindExistingRecord("campaign", CampaignSearchConditions);
                        if (existingCampaign != null)
                        {
                            campaignGuid = existingCampaign.Id;
                        }
                    }

                    Entity existingDesignation = null;

                    if (!string.IsNullOrWhiteSpace(transactions.Gl_code1))
                    {
                        var condition1 = new List<ConditionExpression>
                        {
                            new ConditionExpression("msnfp_designationcode", ConditionOperator.Equal, transactions.Gl_code1.Trim())
                        };

                        existingDesignation = FindExistingRecord("msnfp_designation", condition1);
                    }

                    if (existingDesignation == null && !string.IsNullOrWhiteSpace(transactions.Gl_code2))
                    {
                        var condition2 = new List<ConditionExpression>
                        {
                            new ConditionExpression("msnfp_designationcode", ConditionOperator.Equal, transactions.Gl_code2.Trim())
                        };

                        existingDesignation = FindExistingRecord("msnfp_designation", condition2);
                    }

                    if (existingDesignation != null)
                    {
                        designationGUID = existingDesignation.Id;
                    }

                    if (transactions.Transaction_type == "donation")
                    {
                        decimal totalDonation = decimal.Parse(transactions.Transaction_value) - decimal.Parse(transactions.Transaction_fees);
                        if (totalDonation == 0)
                        {
                            continue;
                        }
                        var matchDonationID = donationList.FirstOrDefault(d => d.Donation_id.Trim() == transactions.Donation_id.Trim());
                        if (matchDonationID == null) //Check from previous transaction if donation id already made and get date
                        {
                            var PreviousTransactionSearchConditions = new List<ConditionExpression>
                            {
                                new ConditionExpression("lrx_fundraisindonationid", ConditionOperator.Equal, transactions.Donation_id),
                            };

                            Entity previousTransaction = FindExistingRecord("msnfp_transaction", PreviousTransactionSearchConditions);
                            if (previousTransaction != null && previousTransaction.Contains("lrx_fundraisindonationdate"))
                            {
                                CustomDonationDate = previousTransaction.GetAttributeValue<string>("lrx_fundraisindonationdate");

                                var customDonationList = this.GetData<DonationModel, DonationModelMap>(this.baseURL, "donations", CustomDonationDate);
                                if (customDonationList != null)
                                    matchDonationID = customDonationList.FirstOrDefault(d => d.Donation_id.Trim() == transactions.Donation_id.Trim());
                            }
                        }

                        if (matchDonationID != null)
                        {
                            contactID = UpsertContact(matchDonationID, transactions.Member_id);
                            string pMethodUniqueName = (object)this.paymentMethod + " - Default";
                            var PMethodSearchConditions = new List<ConditionExpression>
                            {
                                new ConditionExpression("msnfp_name", ConditionOperator.Equal, pMethodUniqueName)
                            };
                            Entity existingPMethod = FindExistingRecord("msnfp_paymentmethod", PMethodSearchConditions);
                            if (existingPMethod != null)
                            {
                                defaultPaymentMethodId = existingPMethod.Id;
                            }
                            else
                            {
                                Guid pmethodId = this._service.Create(new Entity("msnfp_paymentmethod")
                                {
                                    ["msnfp_name"] = pMethodUniqueName,
                                    ["msnfp_type"] = new OptionSetValue(100000000)

                                });
                                defaultPaymentMethodId = pmethodId;
                            }

                            var matchScheduleDonationID = scheduledDonationList.FirstOrDefault(sd => sd.donation_id.Trim() == transactions.Donation_id.Trim());
                            if (matchScheduleDonationID != null)
                            {
                                var PScheduleSearchConditions = new List<ConditionExpression>
                                {
                                    new ConditionExpression("lrx_fundraisinpaymentscheduleid", ConditionOperator.Equal, matchScheduleDonationID.ScheduleId),
                                };

                                Entity existingRecord = FindExistingRecord("msnfp_paymentschedule", PScheduleSearchConditions);

                                var frequencyType = 856660003; // default to monthly
                                if (matchScheduleDonationID.donation_frequency == "weekly")
                                    frequencyType = 856660002; //change to weekly
                                if (matchScheduleDonationID.donation_frequency == "yearly")
                                    frequencyType = 856660004; //change to years
                                if (matchScheduleDonationID.donation_frequency == "fortnightly")
                                    frequencyType = 856660005; //change to forthnightly

                                decimal totalRecurringAmmount = decimal.Parse(matchScheduleDonationID.d_amount) - decimal.Parse(transactions.Transaction_fees);

                                Entity paymentSchedule = new Entity("msnfp_paymentschedule")
                                {
                                    ["sifund_donor"] = new EntityReference("contact", contactID),
                                    ["lrx_paymentmethod"] = defaultPaymentMethodId != Guid.Empty ? new EntityReference("msnfp_paymentmethod", defaultPaymentMethodId) : null,
                                    ["sifund_scheduletypecode"] = new OptionSetValue(844060003),
                                    ["sifund_paymenttypecode"] = new OptionSetValue(existingRecord == null ? 844060008 : 844060002), // Handles different payment type codes
                                    ["msnfp_recurringamount"] = new Money(totalRecurringAmmount),
                                    ["msnfp_frequency"] = new OptionSetValue(frequencyType),
                                    ["msnfp_frequencyinterval"] = 1,
                                    ["sifund_bookdate"] = DateTime.Parse(matchScheduleDonationID.date_created),
                                    ["msnfp_lastpaymentdate"] = DateTime.Parse(transactions.Date_created),
                                    ["lrx_fundraisinpaymentscheduleid"] = int.Parse(matchScheduleDonationID.ScheduleId)
                                };

                                if (existingRecord == null)
                                {
                                    paymentSchedule["lrx_billingstartdate"] = DateTime.Parse(matchScheduleDonationID.date_created);
                                    scheduleID = this._service.Create(paymentSchedule);
                                }
                                else
                                {
                                    paymentSchedule.Id = existingRecord.Id;
                                    this._service.Update(paymentSchedule);
                                    scheduleID = existingRecord.Id;
                                }
                            }

                            if (matchDonationID.History_id.Trim() != "0")
                            {
                                string customPageDetailURL = baseURLCustom + "getFundraiserPageDetails";
                                string csvCustomPageDetailContent = CallFundRaisinCustomAPI((object)customPageDetailURL, matchDonationID.History_id);

                                var pageDetailList = ParseCsvHelper<CustomPageDetailsModel, CustomPageDetailsModelMap>(csvCustomPageDetailContent);
                                string pageMemberId = pageDetailList.FirstOrDefault()?.member_id.Trim();

                                if (pageMemberId.Trim() != "" && pageMemberId.Trim() != string.Empty) 
                                {
                                    var SolicitorContactSearchConditions = new List<ConditionExpression>
                                    {
                                        new ConditionExpression("lrx_fundraisinmemberid", ConditionOperator.Equal, pageMemberId)
                                    };

                                    Entity existingSolicitorRecord = FindExistingRecord("contact", SolicitorContactSearchConditions);
                                    if (existingSolicitorRecord != null)
                                    {
                                        if (existingSolicitorRecord.Id != contactID)
                                            solicitorID = existingSolicitorRecord.Id;
                                    }
                                }                                   
                            }

                            if (matchDonationID.Team_id.Trim() != "0")
                            {
                                var EventTeamSearchConditions = new List<ConditionExpression>
                                {
                                    new ConditionExpression("lrx_fundraisinteamid", ConditionOperator.Equal, matchDonationID.Team_id.Trim())
                                };

                                Entity existingEventTeam = FindExistingRecord("lrx_eventteam", EventTeamSearchConditions);
                                if (existingEventTeam != null)
                                {
                                    teamID = existingEventTeam.Id;
                                }
                            }

                            var TransactionSearchConditions = new List<ConditionExpression>
                            {
                                new ConditionExpression("lrx_fundraisintransactionid", ConditionOperator.Equal, transactions.Transaction_id),
                            };

                            Entity existingTransaction = FindExistingRecord("msnfp_transaction", TransactionSearchConditions);
                            Entity transactionEntity = new Entity("msnfp_transaction")
                            {
                                ["sifund_donor"] = new EntityReference("contact", contactID),
                                ["lrx_solicitor"] = solicitorID != Guid.Empty ? new EntityReference("contact", solicitorID) : null,
                                ["lrx_event"] = eventID != Guid.Empty ? new EntityReference("lrx_event", eventID) : null,
                                ["lrx_registrations"] = registrationID != Guid.Empty ? new EntityReference("lrx_registrations", registrationID) : null,
                                ["lrx_eventteam"] = teamID != Guid.Empty ? new EntityReference("lrx_eventteam", teamID) : null,
                                ["lrx_campaign"] = campaignGuid != Guid.Empty ? (object)new EntityReference("campaign", campaignGuid) : null,
                                ["sifund_primarydesignation"] = designationGUID != Guid.Empty ? (object)new EntityReference("msnfp_designation", designationGUID) : null,
                                ["sifund_appeal"] = appealGuid != Guid.Empty ? (object)new EntityReference("sifund_appeal", appealGuid) : null,
                                ["sifund_package"] = packageGuid != Guid.Empty ? (object)new EntityReference("sifund_package", packageGuid) : null,
                                ["msnfp_transaction_paymentmethodid"] = defaultPaymentMethodId != Guid.Empty ? new EntityReference("msnfp_paymentmethod", defaultPaymentMethodId) : null,
                                ["msnfp_transaction_paymentscheduleid"] = scheduleID != Guid.Empty ? new EntityReference("msnfp_paymentschedule", scheduleID) : null,
                                ["msnfp_amount"] = new Money(decimal.Parse(transactions.Transaction_value) - decimal.Parse(transactions.Transaction_fees)),
                                ["msnfp_bookdate"] = DateTime.Parse(transactions.Date_created),
                                ["sifund_paymenttypecode"] = new OptionSetValue(844060002),
                                ["lrx_donationpaymenttype"] = new OptionSetValue(scheduleID != Guid.Empty ? 856660001 : 856660000),
                                ["statuscode"] = new OptionSetValue(856660001),
                                ["sifund_typecode"] = new OptionSetValue(844060000),
                                ["lrx_fundraisintransactionid"] = int.Parse(transactions.Transaction_id),
                                ["lrx_fundraisindonationid"] = int.Parse(matchDonationID.Donation_id),
                                ["lrx_fundraisindonationdate"] = matchDonationID.Date_created
                            };

                            if (existingTransaction == null)
                            {
                                // Create new transaction
                                transactionId = this._service.Create(transactionEntity);
                            }
                            else
                            {
                                // Update only if flag is enabled
                                if (this.updateTransaction)
                                {
                                    transactionEntity.Id = existingTransaction.Id;
                                    this._service.Update(transactionEntity);
                                }

                                transactionId = existingTransaction.Id; // ✅ Always assign the ID
                            }
                        }
                    } //end of donation transaction type


                    if (transactions.Transaction_type == "registration" || transactions.Transaction_type == "merchandise")
                    {
                        int transactionType = 844060003; //default registration
                        string contactFullName = string.Empty;
                        int includeGST = 0;
                        decimal GSTamount = 0;

                        var ContactSearchConditions = new List<ConditionExpression>
                        {
                            new ConditionExpression("lrx_fundraisinmemberid", ConditionOperator.Equal, transactions.Member_id)
                        };

                        Entity existingContact = FindExistingRecord("contact", ContactSearchConditions);

                        if (existingContact != null)
                            contactID = (Guid)existingContact.Id;
                        if (contactID == Guid.Empty)
                        {
                            if (transactions.Sale_id.Trim() != "0")
                            {
                                contactFullName = string.Empty;
                                contactID = UpsertContactFromSales(transactions.Sale_id, out contactFullName, out GSTamount);
                            }

                            if (contactID == Guid.Empty)
                                continue;
                        }

                        string pMethodUniqueName = (object)this.paymentMethod + " - Default";
                        var PMethodSearchConditions = new List<ConditionExpression>
                        {
                            new ConditionExpression("msnfp_name", ConditionOperator.Equal, pMethodUniqueName)
                        };
                        Entity existingPMethod = FindExistingRecord("msnfp_paymentmethod", PMethodSearchConditions);
                        if (existingPMethod != null)
                        {
                            defaultPaymentMethodId = existingPMethod.Id;
                        }
                        else
                        {
                            Guid pmethodId = this._service.Create(new Entity("msnfp_paymentmethod")
                            {
                                ["msnfp_name"] = pMethodUniqueName,
                                ["msnfp_type"] = new OptionSetValue(100000000)

                            });
                            defaultPaymentMethodId = pmethodId;
                        }
                        
                        if (transactions.Transaction_type == "merchandise")
                        {
                            transactionType = 844060004;
                        }
                        else
                        {
                            var RegistrationSearchConditions = new List<ConditionExpression>
                            {
                                new ConditionExpression("lrx_fundraisinregistrationid", ConditionOperator.Equal, transactions.History_id)
                            };
                            Entity existingRegistration = FindExistingRecord("lrx_registrations", RegistrationSearchConditions);
                            if (existingRegistration != null)
                            {
                                registrationID = existingRegistration.Id;

                                if (existingRegistration.Attributes.Contains("lrx_promoid"))
                                {
                                    var PromoSearchConditions = new List<ConditionExpression>
                                    {
                                        new ConditionExpression("lrx_fundraisinpromoid", ConditionOperator.Equal, existingRegistration["lrx_promoid"].ToString())
                                    };
                                    Entity existingPromo = FindExistingRecord("lrx_promocodeanddiscount", PromoSearchConditions);
                                    if (existingPromo != null)
                                    {
                                        promoGuid = existingPromo.Id;
                                    }
                                }
                            }
                        }
                        var TransactionSearchConditions = new List<ConditionExpression>
                        {
                            new ConditionExpression("lrx_fundraisintransactionid", ConditionOperator.Equal, transactions.Transaction_id),
                        };

                        Entity existingTransaction = FindExistingRecord("msnfp_transaction", TransactionSearchConditions);
                        var transaction = existingTransaction == null
                            ? new Entity("msnfp_transaction")
                            : new Entity("msnfp_transaction", existingTransaction.Id);

                        // ✅ Common fields
                        transaction["sifund_donor"] = new EntityReference("contact", contactID);
                        transaction["lrx_campaign"] = campaignGuid != Guid.Empty ? new EntityReference("campaign", campaignGuid) : null;
                        transaction["sifund_primarydesignation"] = designationGUID != Guid.Empty ? new EntityReference("msnfp_designation", designationGUID) : null;
                        transaction["sifund_appeal"] = appealGuid != Guid.Empty ? new EntityReference("sifund_appeal", appealGuid) : null;
                        transaction["sifund_package"] = packageGuid != Guid.Empty ? new EntityReference("sifund_package", packageGuid) : null;
                        transaction["msnfp_transaction_paymentmethodid"] = defaultPaymentMethodId != Guid.Empty
                                ? new EntityReference("msnfp_paymentmethod", defaultPaymentMethodId)
                                : null;
                        transaction["lrx_event"] = eventID != Guid.Empty ? new EntityReference("lrx_event", eventID) : null;
                        transaction["lrx_registrations"] = registrationID != Guid.Empty ? new EntityReference("lrx_registrations", registrationID) : null;
                        transaction["lrx_eventteam"] = teamID != Guid.Empty ? new EntityReference("lrx_eventteam", teamID) : null;
                        transaction["lrx_promocode"] = promoGuid != Guid.Empty ? new EntityReference("lrx_promocodeanddiscount", promoGuid) : null;

                        transaction["msnfp_amount"] = new Money(decimal.Parse(transactions.Transaction_value) - decimal.Parse(transactions.Transaction_fees));

                        transaction["msnfp_bookdate"] = DateTime.Parse(transactions.Date_created);
                        transaction["sifund_paymenttypecode"] = new OptionSetValue(844060002);
                        transaction["statuscode"] = new OptionSetValue(856660001);
                        transaction["sifund_typecode"] = new OptionSetValue(transactionType);
                        transaction["lrx_fundraisintransactionid"] = int.Parse(transactions.Transaction_id);
                        transaction["lrx_fundraisindonationid"] = int.Parse(transactions.Donation_id);
                        transaction["lrx_fundraisindonationdate"] = transactions.Date_created;

                        if (GSTamount > 0)
                        {
                            //transaction["sifund_amount_tax"] = new Money(GSTamount); //for confirmation as the business rule hinders the change of GST
                        }

                        if (existingTransaction == null)
                        {
                            transactionId = this._service.Create(transaction);
                        }
                        else
                        {
                            transactionId = existingTransaction.Id;

                            if (this.updateTransaction)
                            {
                                this._service.Update(transaction);
                            }
                        }

                        if (registrationID != Guid.Empty)
                        {
                            this._service.Update(new Entity("lrx_registrations", registrationID)
                            {
                                ["lrx_transaction"] = existingTransaction != null ? new EntityReference("msnfp_transaction", existingTransaction.Id) : new EntityReference("msnfp_transaction", transactionId),
                                ["statuscode"] = new OptionSetValue(1)
                            });
                        }
                        
                        if (transactions.Sale_id != "0")
                        {
                            //var salesItemMatchID = saleItemList.FirstOrDefault(si => si.sale_id.Trim() == transactions.Sale_id.Trim());
                            
                            decimal netTransactionAmount = decimal.Parse(transactions.Transaction_value, CultureInfo.InvariantCulture) - decimal.Parse(transactions.Transaction_fees, CultureInfo.InvariantCulture);

                            var salesItemMatchID = saleItemList.FirstOrDefault(si =>
                                string.Equals(si.sale_id?.Trim(), transactions.Sale_id?.Trim(), StringComparison.Ordinal)
                                && decimal.TryParse(si.unit_cost, NumberStyles.Any, CultureInfo.InvariantCulture, out var unitCost)
                                && unitCost == netTransactionAmount
                            );
                            
                            if (salesItemMatchID != null)
                            {
                                Guid productID = Guid.Empty;
                                Guid productOption = Guid.Empty;
                                string productName = "";
                                string productOptionName = "";

                                var matchingProduct = productList.FirstOrDefault(p => p.product_id.Trim() == salesItemMatchID.product_id.Trim());
                                if (matchingProduct != null)
                                {
                                    productName = matchingProduct.product_name;
                                }

                                var matchingProductOption = productOptionList.FirstOrDefault(p => p.product_id.Trim() == salesItemMatchID.product_id.Trim());
                                if (matchingProductOption != null)
                                {
                                    productOptionName = matchingProductOption.option_name;
                                }

                                var productSearchConditions = new List<ConditionExpression>
                                {
                                    new ConditionExpression("lrx_fundraisinproductid", ConditionOperator.Equal, salesItemMatchID.product_id)
                                };
                                Entity existingInventoryProduct = FindExistingRecord("lrx_inventoryproduct", productSearchConditions);

                                if (existingInventoryProduct != null)
                                {
                                    productID = existingInventoryProduct.Id;
                                }
                                else
                                {
                                    continue;
                                }

                                var productOptionSearchConditions = new List<ConditionExpression>
                                {
                                    new ConditionExpression("lrx_fundraisinoptionid", ConditionOperator.Equal, salesItemMatchID.option_id)
                                };
                                Entity existingProductOption = FindExistingRecord("lrx_productoptions", productOptionSearchConditions);

                                if (existingProductOption != null)
                                {
                                    productOption = existingProductOption.Id;
                                }

                                if (!string.IsNullOrEmpty(productName))
                                {
                                    
                                    var eventProductSearchConditions = new List<ConditionExpression>
                                    {
                                        new ConditionExpression("lrx_fundraisineventproductid", ConditionOperator.Equal, int.Parse(salesItemMatchID.id))
                                    };
                                    Entity existingEventProduct = FindExistingRecord("lrx_eventproduct", eventProductSearchConditions);
                                    Guid eventProductID = Guid.Empty;
                                    if (transactions.Event_id.Trim() != "0") {
                                        if (existingEventProduct == null && transactions.Event_id != "")
                                        {
                                            
                                            var eventProduct = new Entity("lrx_eventproduct")
                                            {
                                                ["lrx_name"] = $"{productName} - {productOptionName}",
                                                ["lrx_priceperproduct"] = new Money(decimal.TryParse(salesItemMatchID.unit_cost, out var price) ? price : 0),
                                                ["lrx_quantity"] = int.TryParse(salesItemMatchID.quantity, out var quantity) ? quantity : 0,
                                                ["lrx_fundraisineventproductid"] = int.TryParse(salesItemMatchID.id, out var eventProductId) ? eventProductId : 0
                                            };

                                            // Add lookup fields only if they have valid GUIDs
                                            if (eventID != Guid.Empty)
                                            {
                                                eventProduct["lrx_event"] = new EntityReference("lrx_event", eventID);
                                            }
                                            if (productID != Guid.Empty)
                                            {                                               
                                                eventProduct["lrx_product"] = new EntityReference("lrx_inventoryproduct", productID);
                                            }
                                            
                                            eventProductID = _service.Create(eventProduct);
                                        }
                                        else
                                        {                                          
                                            eventProductID = existingEventProduct.Id;
                                        }                                      
                                    }
                                    
                                    var saleProduct = new Entity("lrx_product")
                                    {
                                        ["lrx_name"] = $"{productName} - {productOptionName}",
                                        ["lrx_constituentorganisation"] = new EntityReference("contact", contactID),
                                        ["lrx_date"] = DateTime.Parse(transactions.Date_created),
                                        ["lrx_productmigrationid"] = salesItemMatchID.id.Trim(),
                                        ["lrx_fundraisinsalesid"] = int.Parse(salesItemMatchID.sale_id.Trim())
                                    };

                                    // Parse quantity safely
                                    if (int.TryParse(salesItemMatchID.quantity, out int parsedQuantity))
                                    {
                                        saleProduct["lrx_quantity"] = parsedQuantity;
                                    }

                                    // Parse unit cost safely
                                    if (decimal.TryParse(salesItemMatchID.unit_cost, out decimal parsedPrice))
                                    {
                                        saleProduct["lrx_priceperproduct"] = new Money(parsedPrice);
                                    }

                                    Entity updateTransaction = new Entity("msnfp_transaction")
                                    {
                                        Id = transactionId
                                    };

                                    if (eventID != Guid.Empty)
                                    {
                                        saleProduct["lrx_event"] = new EntityReference("lrx_event", eventID);
                                    }

                                    if (eventProductID != Guid.Empty)
                                    {
                                        saleProduct["lrx_eventproduct"] = new EntityReference("lrx_eventproduct", eventProductID);
                                        updateTransaction["lrx_eventproduct"] = new EntityReference("lrx_eventproduct", eventProductID);
                                    }

                                    if (productOption != Guid.Empty)
                                    {
                                        saleProduct["lrx_productoption"] =
                                            new EntityReference("lrx_productoptions", productOption);
                                    }

                                    if (transactionId != Guid.Empty)
                                    {
                                        saleProduct["lrx_transaction"] =
                                            new EntityReference("msnfp_transaction", transactionId);
                                    }
                                    
                                    // 🔍 Check existing product sale
                                    var productSaleSearchConditions = new List<ConditionExpression>
                                    {
                                        new ConditionExpression(
                                            "lrx_productmigrationid",
                                            ConditionOperator.Equal,
                                            salesItemMatchID.id.Trim()
                                        )
                                    };

                                    Guid ProductSaleGuid = Guid.Empty;

                                    Entity existingProductSale =
                                        FindExistingRecord("lrx_product", productSaleSearchConditions);
                                    
                                    if (existingProductSale == null)
                                    {
                                        ProductSaleGuid = _service.Create(saleProduct);
                                    }
                                    else
                                    {
                                        ProductSaleGuid = existingProductSale.Id;
                                        var saleProductUpdate =
                                            new Entity("lrx_product", existingProductSale.Id);

                                        saleProductUpdate.Attributes.AddRange(saleProduct.Attributes);

                                        _service.Update(saleProductUpdate);
                                    }
                                    
                                    updateTransaction["lrx_product"] = ProductSaleGuid != Guid.Empty ? new EntityReference("lrx_product", ProductSaleGuid) : null;
                                    this._service.Update(updateTransaction);

                                    var productSaleEventSearchConditions = new List<ConditionExpression>
                                    {
                                        new ConditionExpression(
                                            "lrx_fundraisinsalesid",
                                            ConditionOperator.Equal,
                                            salesItemMatchID.sale_id.Trim()
                                        )
                                    };

                                    var matchingProducts = FindAllRecords("lrx_product", productSaleEventSearchConditions);

                                    if (matchingProducts.Count > 1)
                                    {
                                        foreach (var product in matchingProducts)
                                        {
                                            // Update the entity as needed
                                            if (eventID != Guid.Empty)
                                            {
                                                product["lrx_event"] = new EntityReference("lrx_event", eventID);
                                            }

                                            // Persist update
                                            _service.Update(product);
                                        }
                                    }
                                }
                            }
                        }
                    }//end of registration or merchandise transaction

                    if (transactions.Transaction_type == "raffle")
                    {
                        int transactionType = 844060005; //default raffle

                        var raffleSalesRecord = raffleSalesList?.FirstOrDefault(rs => rs.sale_id.Trim() == transactions.Sale_id.Trim());

                        if (raffleSalesRecord != null)
                        {
                            contactID = UpsertContactFromRaffleSales(raffleSalesRecord);
                        }

                        if (contactID == Guid.Empty)
                        {
                            continue;
                        }
                        else
                        {
                            string pMethodUniqueName = (object)this.paymentMethod + " - Default";
                            var PMethodSearchConditions = new List<ConditionExpression>
                            {
                                new ConditionExpression("msnfp_name", ConditionOperator.Equal, pMethodUniqueName)
                            };
                            Entity existingPMethod = FindExistingRecord("msnfp_paymentmethod", PMethodSearchConditions);
                            if (existingPMethod != null)
                            {
                                defaultPaymentMethodId = existingPMethod.Id;
                            }
                            else
                            {
                                Guid pmethodId = this._service.Create(new Entity("msnfp_paymentmethod")
                                {
                                    ["msnfp_name"] = pMethodUniqueName,
                                    ["msnfp_type"] = new OptionSetValue(100000000)
                                });
                                defaultPaymentMethodId = pmethodId;
                            }
                        }

                        var RaffleSalesSearchConditions = new List<ConditionExpression>
                        {
                            new ConditionExpression("lrx_fundraisinrafflesalesid", ConditionOperator.Equal, raffleSalesRecord.sale_id)
                        };

                        Entity existingRaffleSales = FindExistingRecord("lrx_rafflesales", RaffleSalesSearchConditions);

                        if (existingRaffleSales != null) {
                            raffleSaleGuid = existingRaffleSales.Id;

                            Guid raffleID = existingRaffleSales.Contains("lrx_raffle")
                                            ? ((EntityReference)existingRaffleSales["lrx_raffle"]).Id
                                            : Guid.Empty;

                            var RaffleSearchConditions = new List<ConditionExpression>
                            {
                                new ConditionExpression("lrx_raffleid", ConditionOperator.Equal, raffleID)
                            };

                            Entity existingRaffle = FindExistingRecord(
                                                        "lrx_raffle",
                                                        RaffleSearchConditions,
                                                        new ColumnSet("lrx_campaign", "lrx_event")
                                                    );

                            if (existingRaffle != null) {
                                raffleGuid = existingRaffle.Id;
                                campaignGuid = existingRaffle.Contains("lrx_campaign")
                                                ? ((EntityReference)existingRaffle["lrx_campaign"]).Id
                                                : Guid.Empty;

                                eventID = existingRaffle.Contains("lrx_event")
                                            ? ((EntityReference)existingRaffle["lrx_event"]).Id
                                            : Guid.Empty;

                                if (eventID != Guid.Empty)
                                {
                                    var raffleSalesUpdate = new Entity("lrx_rafflesales", raffleSaleGuid);
                                    raffleSalesUpdate["lrx_event"] =
                                        new EntityReference("lrx_event", eventID);

                                    _service.Update(raffleSalesUpdate);
                                }
                            }
                        }
                        else
                        {
                            continue;
                        }

                        var TransactionSearchConditions = new List<ConditionExpression>
                        {
                            new ConditionExpression("lrx_fundraisintransactionid", ConditionOperator.Equal, transactions.Transaction_id),
                        };

                        Entity existingTransaction = FindExistingRecord("msnfp_transaction", TransactionSearchConditions);

                        Entity transactionEntity = new Entity("msnfp_transaction")
                        {
                            ["sifund_donor"] = new EntityReference("contact", contactID),
                            ["lrx_campaign"] = campaignGuid != Guid.Empty ? new EntityReference("campaign", campaignGuid) : null,
                            ["sifund_primarydesignation"] = designationGUID != Guid.Empty ? (object)new EntityReference("msnfp_designation", designationGUID) : null,
                            ["sifund_appeal"] = appealGuid != Guid.Empty ? new EntityReference("sifund_appeal", appealGuid) : null,
                            ["sifund_package"] = packageGuid != Guid.Empty ? new EntityReference("sifund_package", packageGuid) : null,
                            ["msnfp_transaction_paymentmethodid"] = defaultPaymentMethodId != Guid.Empty ? new EntityReference("msnfp_paymentmethod", defaultPaymentMethodId) : null,
                            ["lrx_event"] = eventID != Guid.Empty ? new EntityReference("lrx_event", eventID) : null,
                            ["lrx_registrations"] = registrationID != Guid.Empty ? new EntityReference("lrx_registrations", registrationID) : null,
                            ["lrx_eventteam"] = teamID != Guid.Empty ? new EntityReference("lrx_eventteam", teamID) : null,
                            ["lrx_promocode"] = promoGuid != Guid.Empty ? new EntityReference("lrx_promocodeanddiscount", promoGuid) : null,
                            ["msnfp_amount"] = new Money(decimal.Parse(transactions.Transaction_value) - decimal.Parse(transactions.Transaction_fees)),
                            ["msnfp_bookdate"] = DateTime.Parse(transactions.Date_created),
                            ["sifund_paymenttypecode"] = new OptionSetValue(844060002),
                            ["lrx_raffle"] = raffleGuid != Guid.Empty ? new EntityReference("lrx_raffle", raffleGuid) : null,
                            ["lrx_rafflesales"] = raffleSaleGuid != Guid.Empty ? new EntityReference("lrx_rafflesales", raffleSaleGuid) : null,
                            ["statuscode"] = new OptionSetValue(856660001),
                            ["sifund_typecode"] = new OptionSetValue(transactionType),
                            ["lrx_fundraisintransactionid"] = int.Parse(transactions.Transaction_id),
                            ["lrx_fundraisindonationid"] = int.Parse(transactions.Donation_id),
                            ["lrx_fundraisindonationdate"] = transactions.Date_created
                        };

                        if (existingTransaction == null)
                        {
                            transactionId = this._service.Create(transactionEntity);
                        }
                        else if (this.updateTransaction)
                        {
                            transactionEntity.Id = existingTransaction.Id;
                            this._service.Update(transactionEntity);
                        }

                        if (raffleSaleGuid != Guid.Empty)
                        {
                            this._service.Update(new Entity("lrx_rafflesales", raffleSaleGuid)
                            {
                                ["lrx_transaction"] = existingTransaction != null ? new EntityReference("msnfp_transaction", existingTransaction.Id) : new EntityReference("msnfp_transaction", transactionId)
                            });
                        }
                    }//end of Raffle transaction

                    if (transactions.Transaction_type == "refund")
                    {
                        var originalTransaction = TransactionList
                            .FirstOrDefault(t => t.Donation_id.Trim() == transactions.Donation_id.Trim() && t.Transaction_type.Trim() != "refund");

                        if (originalTransaction != null)
                        {
                            var originalTransactionId = originalTransaction.Transaction_id;
                            decimal transactionAmount = decimal.Parse(originalTransaction.Transaction_value) - decimal.Parse(originalTransaction.Transaction_fees);

                            var TransactionSearchConditions = new List<ConditionExpression>
                            {
                                new ConditionExpression("lrx_fundraisintransactionid", ConditionOperator.Equal, originalTransaction.Transaction_id),
                            };

                            Entity existingTransaction = FindExistingRecord("msnfp_transaction", TransactionSearchConditions);
                            if (existingTransaction != null && existingTransaction.Attributes.Contains("sifund_donor"))
                            {
                                Guid donorId = Guid.Empty; // Get the GUID of the donor
                                var donor = existingTransaction.GetAttributeValue<EntityReference>("sifund_donor");
                                if (donor != null)
                                {
                                    donorId = donor.Id; // Get the GUID of the donor
                                }
                                var RefundSearchConditions = new List<ConditionExpression>
                            {
                                new ConditionExpression("lrx_fundraisinrefundid", ConditionOperator.Equal, transactions.Transaction_id),
                            };

                                Entity existingRefund = FindExistingRecord("lrx_refund", RefundSearchConditions);

                                var refundEntity = new Entity("lrx_refund")
                                {
                                    ["lrx_customer"] = new EntityReference("contact", donorId),
                                    ["lrx_transaction"] = new EntityReference("msnfp_transaction", existingTransaction.Id),
                                    ["lrx_totalamountpaidrefund"] = new Money(transactionAmount),
                                    ["lrx_amountreceiptablerefund"] = new Money(transactionAmount),
                                    ["lrx_totalamountpaid"] = new Money(transactionAmount),
                                    ["lrx_amountreceiptable"] = new Money(transactionAmount),
                                    ["lrx_refunddate"] = DateTime.Parse(transactions.Date_created),
                                    ["lrx_refundtype"] = new OptionSetValue(844060002),
                                    ["statuscode"] = new OptionSetValue(376750001),
                                    ["lrx_fundraisinrefundid"] = int.Parse(transactions.Transaction_id)
                                };

                                if (existingRefund == null)
                                {
                                    refundEntity.Id = this._service.Create(refundEntity);
                                }
                                else if (this.updateTransaction)
                                {
                                    refundEntity.Id = existingRefund.Id;
                                    this._service.Update(refundEntity);
                                }
                            
                                this._service.Update(new Entity("msnfp_transaction", existingTransaction.Id)
                                {
                                    ["statuscode"] = new OptionSetValue(856660005)
                                });      
                            }
                        }
                    }//End of Refund
                }
            }

            this._tracingService.Trace("Transaction Record Fundraisin API Completed");
            return Task.CompletedTask;
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

        public List<Entity> FindAllRecords(string entityName, List<ConditionExpression> conditions, ColumnSet columnSet = null)
        {
            if (string.IsNullOrEmpty(entityName))
                throw new ArgumentException("Entity name cannot be null or empty.", nameof(entityName));

            if (conditions == null || conditions.Count == 0)
                throw new ArgumentException("At least one condition must be provided.", nameof(conditions));

            var queryExpression = new QueryExpression(entityName)
            {
                ColumnSet = columnSet ?? new ColumnSet(true)
            };

            foreach (var condition in conditions)
            {
                queryExpression.Criteria.AddCondition(condition);
            }

            return _service.RetrieveMultiple(queryExpression).Entities.ToList();
        }

        public List<TModel> ParseCsvHelper<TModel, TMap>(string csvContent)
        where TMap : ClassMap<TModel>
        {
            using (var reader = new StringReader(csvContent))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true, // Read the header row
                TrimOptions = CsvHelper.Configuration.TrimOptions.Trim, // Remove extra spaces
                MissingFieldFound = null, // Ignore missing fields
                HeaderValidated = null,   // Ignore header mismatches
                BadDataFound = null       // Ignore bad data like trailing commas
            }))
            {
                // Register the custom mapping
                csv.Context.RegisterClassMap<TMap>();

                // Read and map the records
                return csv.GetRecords<TModel>().ToList();
            }
        }

        public string CallFundRaisinAPI(object apiEndpoint, string customDate = "")
        {

            string requestUri = "";
            if (customDate != "")
            {
                string convertedDate = "";
                string[] formats = {
                    "dd/MM/yyyy hh:mm:ss tt",
                    "dd/MM/yyyy HH:mm:ss",
                    "d/M/yyyy hh:mm:ss tt",
                };

                if (DateTime.TryParseExact(customDate, formats, CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out DateTime parsedDate))
                {
                    convertedDate = parsedDate.ToString("yyyy-MM-dd");
                }

                requestUri = string.Format("{0}?apikey={1}&date_from={2}&date_to={3}", (object)apiEndpoint, (object)this.apikey, (object)convertedDate, (object)convertedDate);
            }
            else
            if (dateFrom != "" && dateTo != "")
            {
                requestUri = string.Format("{0}?apikey={1}&date_from={2}&date_to={3}", (object)apiEndpoint, (object)this.apikey, (object)this.dateFrom, (object)this.dateTo);
            }
            else
            {
                requestUri = string.Format("{0}?apikey={1}", (object)apiEndpoint, (object)this.apikey);
            }

            string csvContent = "";
            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
                HttpResponseMessage result = httpClient.GetAsync(requestUri).Result;
                if (result.IsSuccessStatusCode)
                {
                    csvContent = result.Content.ReadAsStringAsync().Result;
                }
                else
                    this._tracingService.Trace("API Request failed with status code: " + result.StatusCode.ToString(), Array.Empty<object>());               
            }
            return csvContent;
        }

        public string CallFundRaisinCustomAPI(object apiEndpoint, string historyID)
        {
            string requestUri = string.Format("{0}?apikey={1}&history_id={2}", (object)apiEndpoint, (object)this.apikey, historyID);
            string csvContent = "";
            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
                HttpResponseMessage result = httpClient.GetAsync(requestUri).Result;
                if (result.IsSuccessStatusCode)
                {
                    csvContent = result.Content.ReadAsStringAsync().Result;
                }
                else
                    this._tracingService.Trace("API Request failed with status code: " + result.StatusCode.ToString(), Array.Empty<object>());
            }
            return csvContent;
        }

        public string CallFundRaisinAPIAllData(object apiEndpoint, string customDate = "")
        {

            string requestUri = "";
            
            requestUri = string.Format("{0}?apikey={1}", (object)apiEndpoint, (object)this.apikey);

            string csvContent = "";
            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
                HttpResponseMessage result = httpClient.GetAsync(requestUri).Result;
                if (result.IsSuccessStatusCode)
                {
                    csvContent = result.Content.ReadAsStringAsync().Result;
                }
                else
                    this._tracingService.Trace("API Request failed with status code: " + result.StatusCode.ToString(), Array.Empty<object>());
            }
            return csvContent;
        }

        public List<T> GetData<T, TMap>(string baseUrl, string endpoint, string customDate = "")
        where TMap : ClassMap<T>, new()
        {
            string fullUrl = baseUrl + endpoint;
            string csvContent = CallFundRaisinAPI((object)fullUrl, customDate);
            return ParseCsvHelper<T, TMap>(csvContent);
        }

        public List<T> GetAllData<T, TMap>(string baseUrl, string endpoint, string customDate = "")
        where TMap : ClassMap<T>, new()
        {
            string fullUrl = baseUrl + endpoint;
            string csvContent = CallFundRaisinAPIAllData((object)fullUrl, customDate);
            return ParseCsvHelper<T, TMap>(csvContent);
        }

        private Guid UpsertContact(dynamic matchDonationID, string TransMemberID)
        {
            // Define search conditions to find an existing contact
            var contactSearchConditions = new List<ConditionExpression>
            {
                new ConditionExpression("firstname", ConditionOperator.Equal, matchDonationID.D_fname),
                new ConditionExpression("lastname", ConditionOperator.Equal, matchDonationID.D_lname),
                new ConditionExpression("emailaddress1", ConditionOperator.Equal, matchDonationID.D_email)
            };

            Entity existingContact = FindExistingRecord("contact", contactSearchConditions);
            var addressStreet = matchDonationID.D_address_number + matchDonationID.D_address_street;

            // Prepare contact attributes
            var contactAttributes = new Dictionary<string, object>();
            void AddIfValid(string key, object value)
            {
                // Ignore null
                if (value == null)
                    return;

                // If value is string, ignore empty/whitespace
                if (value is string strValue && string.IsNullOrWhiteSpace(strValue))
                    return;

                contactAttributes[key] = value;
            }            
            AddIfValid("firstname", matchDonationID.D_fname);
            AddIfValid("lastname", matchDonationID.D_lname);
            AddIfValid("emailaddress1", matchDonationID.D_email);
            AddIfValid("telephone1", matchDonationID.D_phone);
            AddIfValid("mobilephone", matchDonationID.D_phone_mobile);
            AddIfValid("address1_line1", addressStreet);
            AddIfValid("address1_city", matchDonationID.D_address_suburb);
            AddIfValid("address1_postalcode", matchDonationID.D_address_pcode);
            AddIfValid("address1_stateorprovince", matchDonationID.D_address_state);
            AddIfValid("address1_country", matchDonationID.D_address_country);

            // Conditional integer field
            int? memberIdValue = TransMemberID != "0"
                ? int.Parse(matchDonationID.Member_id)
                : (int?)null;

            AddIfValid("lrx_fundraisinmemberid", memberIdValue);

            Guid contactID;

            if (existingContact == null)
            {
                var contactEntity = new Entity("contact");
                foreach (var kvp in contactAttributes)
                {
                    contactEntity[kvp.Key] = kvp.Value;
                }

                contactID = this._service.Create(contactEntity);
            }
            else
            {
                var contactEntity = new Entity("contact", existingContact.Id);
                foreach (var kvp in contactAttributes)
                {
                    contactEntity[kvp.Key] = kvp.Value;
                }

                this._service.Update(contactEntity);
                contactID = existingContact.Id;
            }

            return contactID;
        }

        private Guid UpsertContactFromSales(string SalesID, out string fullName, out decimal GSTamount)
        {
            fullName = string.Empty;
            GSTamount = 0;

            var ProductSale = this.GetData<ProductSales, ProductSalesModelMap>(this.baseURL, "sales");
            var matchSalesID = ProductSale.FirstOrDefault(p => p.sale_id.Trim() == SalesID.Trim());
            
            if (matchSalesID == null)
                return Guid.Empty;

            // Build full name
            fullName = $"{matchSalesID.first_name} {matchSalesID.last_name}".Trim();
            decimal.TryParse(
                        matchSalesID.gst,
                        NumberStyles.Any,
                        CultureInfo.InvariantCulture,
                        out GSTamount
                    );

            // Define search conditions to find an existing contact
            var contactSearchConditions = new List<ConditionExpression>
            {
                new ConditionExpression("firstname", ConditionOperator.Equal, matchSalesID.first_name?.Trim()),
                new ConditionExpression("lastname", ConditionOperator.Equal, matchSalesID.last_name?.Trim())
            };

            if (!string.IsNullOrWhiteSpace(matchSalesID.email))
            {
                contactSearchConditions.Add(new ConditionExpression("emailaddress1", ConditionOperator.Equal, matchSalesID.email.Trim()));
            }

            Entity existingContact = FindExistingRecord("contact", contactSearchConditions);

            var addressStreet = matchSalesID.number + " " + matchSalesID.street;

            // Prepare contact attributes
            var contactAttributes = new Dictionary<string, object>();

            void AddIfValid(string key, object value)
            {
                if (value == null)
                    return;

                if (value is string str && string.IsNullOrWhiteSpace(str))
                    return;

                contactAttributes[key] = value;
            }

            // Build mobile number safely
            string mobileNumber = (matchSalesID.mobile_suffix ?? "") + (matchSalesID.mobile ?? "");

            // Add fields with validation
            AddIfValid("firstname", matchSalesID.first_name);
            AddIfValid("lastname", matchSalesID.last_name);
            AddIfValid("emailaddress1", matchSalesID.email);
            AddIfValid("mobilephone", mobileNumber);
            AddIfValid("address1_line1", addressStreet);
            AddIfValid("address1_city", matchSalesID.suburb);
            AddIfValid("address1_postalcode", matchSalesID.postcode);
            AddIfValid("address1_stateorprovince", matchSalesID.state);
            AddIfValid("address1_country", matchSalesID.country);

            Guid contactID;

            if (existingContact == null)
            {
                var contactEntity = new Entity("contact");
                foreach (var kvp in contactAttributes)
                {
                    contactEntity[kvp.Key] = kvp.Value;
                }

                contactID = this._service.Create(contactEntity);
            }
            else
            {
                var contactEntity = new Entity("contact", existingContact.Id);
                foreach (var kvp in contactAttributes)
                {
                    contactEntity[kvp.Key] = kvp.Value;
                }

                this._service.Update(contactEntity);
                contactID = existingContact.Id;
            }

            return contactID;
        }


        private Guid UpsertContactFromRaffleSales(dynamic raffleSales)
        {
            // Define search conditions to find an existing contact
            var contactSearchConditions = new List<ConditionExpression>
            {
                new ConditionExpression("firstname", ConditionOperator.Equal, raffleSales.first_name?.Trim()),
                new ConditionExpression("lastname", ConditionOperator.Equal, raffleSales.last_name?.Trim())
            };

            if (!string.IsNullOrWhiteSpace(raffleSales.email))
            {
                contactSearchConditions.Add(
                    new ConditionExpression("emailaddress1", ConditionOperator.Equal, raffleSales.email.Trim())
                );
            }

            Entity existingContact = FindExistingRecord("contact", contactSearchConditions);
            var addressStreet = $"{raffleSales.address_number} {raffleSales.address_street}".Trim();

            // Prepare contact attributes
            var contactAttributes = new Dictionary<string, object>();

            void AddIfNotEmpty(string key, string value)
            {
                if (!string.IsNullOrWhiteSpace(value))
                    contactAttributes[key] = value;
            }

            // Build attributes safely
            AddIfNotEmpty("firstname", raffleSales.first_name);
            AddIfNotEmpty("lastname", raffleSales.last_name);
            AddIfNotEmpty("emailaddress1", raffleSales.email);
            AddIfNotEmpty("telephone1", raffleSales.phone);
            AddIfNotEmpty("mobilephone", raffleSales.mobile);
            AddIfNotEmpty("address1_line1", addressStreet);
            AddIfNotEmpty("address1_city", raffleSales.address_suburb);
            AddIfNotEmpty("address1_postalcode", raffleSales.address_postcode);
            AddIfNotEmpty("address1_stateorprovince", raffleSales.address_state);
            AddIfNotEmpty("address1_country", raffleSales.address_country);

            Guid contactID;

            if (existingContact == null)
            {
                var contactEntity = new Entity("contact");
                foreach (var kvp in contactAttributes)
                {
                    contactEntity[kvp.Key] = kvp.Value;
                }

                contactID = this._service.Create(contactEntity);
            }
            else
            {
                var contactEntity = new Entity("contact", existingContact.Id);
                foreach (var kvp in contactAttributes)
                {
                    contactEntity[kvp.Key] = kvp.Value;
                }

                this._service.Update(contactEntity);
                contactID = existingContact.Id;
            }

            return contactID;
        }

        private Guid GetFundraisinTableRecord(string eventId, string tableID, Guid eventCRM, Guid ticketCRM)
        {
            Guid eventTableGuid = Guid.Empty;
            string customPageDetailURL = baseURLCustom + "getEventsTables";
            string requestUri = string.Format("{0}?apikey={1}&event_ids={2}", (object)customPageDetailURL, (object)this.apikey, eventId);
            string csvContent = "";
            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
                HttpResponseMessage result = httpClient.GetAsync(requestUri).Result;
                if (result.IsSuccessStatusCode)
                {
                    csvContent = result.Content.ReadAsStringAsync().Result;
                }
                else
                    this._tracingService.Trace("API Request failed with status code: " + result.StatusCode.ToString(), Array.Empty<object>());
            }

            var eventTableList = ParseCsvHelper<EventTableModel, EventTableModelMap>(csvContent);

            var eventTableRecord = eventTableList.FirstOrDefault(et => et.table_id.Trim() == tableID.Trim());
            if (eventTableRecord != null) 
            {
                var eventTableSearchConditions = new List<ConditionExpression>
                {
                    new ConditionExpression("lrx_fundraisintableid", ConditionOperator.Equal, eventTableRecord.table_id)
                };

                Entity existingEventTable = FindExistingRecord("lrx_eventtable", eventTableSearchConditions);
                Entity eventTableEntity = new Entity("lrx_eventtable")
                {
                    ["lrx_name"] = eventTableRecord.table_name,
                    ["lrx_tablecapacity"] = int.Parse(eventTableRecord.number_seats),
                    ["lrx_tablenumber"] = int.Parse(eventTableRecord.table_number),
                    ["lrx_event"] = eventCRM != Guid.Empty ? new EntityReference("lrx_event", eventCRM) : null,
                    ["lrx_eventticket"] = ticketCRM != Guid.Empty ? new EntityReference("lrx_eventticket", ticketCRM) : null,
                    ["lrx_pricepertable"] = new Money(decimal.Parse(eventTableRecord.table_price)),
                    ["lrx_fundraisintableid"] = eventTableRecord.table_id
                };

                if (existingEventTable == null)
                {
                    eventTableGuid = this._service.Create(eventTableEntity);
                }
                else if (this.updateTransaction)
                {
                    eventTableEntity.Id = existingEventTable.Id;
                    eventTableGuid = existingEventTable.Id;
                    this._service.Update(eventTableEntity);
                }
            }

            return eventTableGuid;
        }

        private Guid CheckAndUpdateEvent(
            string eventId,
            List<EventModel> eventList,
            out Guid campaignId,
            out Guid appealId,
            out Guid packageId,
            out Guid designationId)
        {
            campaignId = Guid.Empty;
            appealId = Guid.Empty;
            packageId = Guid.Empty;
            designationId = Guid.Empty;

            EventModel matchedEvent = eventList.FirstOrDefault(e => e.EventId.Trim() == eventId.Trim());
            if (matchedEvent == null)
                return Guid.Empty;

            // 1️⃣ Try Fundraisin Event ID first
            Entity existingEvent = FindExistingRecord(
                "lrx_event",
                new List<ConditionExpression>
                {
                    new ConditionExpression(
                        "lrx_fundraisineventid",
                        ConditionOperator.Equal,
                        matchedEvent.EventId
                    )
                }
            );

            // 2️⃣ Fallback to Platform Event ID only if not found
            if (existingEvent == null)
            {
                existingEvent = FindExistingRecord(
                    "lrx_event",
                    new List<ConditionExpression>
                    {
                        new ConditionExpression(
                            "lrx_platformeventid",
                            ConditionOperator.Equal,
                            matchedEvent.EventId
                        )
                    }
                );
            }

            if (existingEvent != null)
            {
                if (existingEvent.Contains("lrx_campaign") && existingEvent["lrx_campaign"] is EntityReference campaignRef)
                    campaignId = campaignRef.Id;

                if (existingEvent.Contains("lrx_sifund_appeal") && existingEvent["lrx_sifund_appeal"] is EntityReference appealRef)
                    appealId = appealRef.Id;

                if (existingEvent.Contains("lrx_sifund_package") && existingEvent["lrx_sifund_package"] is EntityReference packageRef)
                    packageId = packageRef.Id;

                if (existingEvent.Contains("lrx_designation") && existingEvent["lrx_designation"] is EntityReference designationRef)
                    designationId = designationRef.Id;

                return existingEvent.Id;
            }
            else
            {
                return Guid.Empty;
            }
        }

    }
}