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
using System.Diagnostics.Tracing;
using System.Web.Util;
using System.Data.Common;
using System.Threading.Tasks;

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

            string format = "MM-dd-yyyy";
            CultureInfo provider = CultureInfo.InvariantCulture;

            if (DateTime.TryParseExact(jsonInput["dateFrom"]?.ToString(), format, provider, DateTimeStyles.None, out DateTime parsedDateFrom))
            {
                this.dateFrom = parsedDateFrom.ToString("yyyy-MM-dd");
            }

            if (DateTime.TryParseExact(jsonInput["dateTo"]?.ToString(), format, provider, DateTimeStyles.None, out DateTime parsedDateTo))
            {
                this.dateTo = parsedDateTo.ToString("yyyy-MM-dd");
            }

            if (!string.IsNullOrEmpty(baseURL) && baseURL.Length > 4)
            {
                baseURLCustom = baseURL.Substring(0, this.baseURL.Length - 4) + "customcode/";
            }
        }

        public Task GetFundraisinEventRecords()
        {
            List<EventModel> fundraisingEvents = this.ParseCsvHelper<EventModel, EventModelMap>(this.CallFundRaisinAPI((object)(this.baseURL + "events")));

            Guid campaignId = Guid.Empty;
            Entity existingCampaign = this.FindExistingRecord("campaign", new List<ConditionExpression>()
            {
                new ConditionExpression("name", ConditionOperator.Equal, (object)this.campaignName)
            });

            if (existingCampaign != null)
                campaignId = existingCampaign.Id;

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
                        //["lrx_campaign"] = campaignId != Guid.Empty ? (object)new EntityReference("campaign", campaignId) : null,
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
                        //["lrx_campaign"] = campaignId != Guid.Empty ? (object)new EntityReference("campaign", campaignId) : null,
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
                var contactFields = new Dictionary<string, object>
                {
                    ["firstname"] = participant.MFname,
                    ["lastname"] = participant.MLname,
                    ["emailaddress1"] = participant.MEmail,
                    ["telephone1"] = participant.MPhoneHome,
                    ["mobilephone"] = participant.MPhoneMobile,
                    ["address1_line1"] = participant.MAddressStreet,
                    ["address1_city"] = participant.MAddressSuburb,
                    ["address1_postalcode"] = participant.MAddressPCode,
                    ["address1_stateorprovince"] = participant.MAddressState,
                    ["address1_country"] = participant.MAddressCountry,
                    ["lrx_fundraisinmemberid"] = int.TryParse(participant.MemberId, out int memberId) ? memberId : (int?)null
                };

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
                decimal ticketAmount = 0;
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
                ticketAmount = entryFeeRecord;
                string eventTicketName = EventName + " - Entree Fee ";
                var TicketSearchConditions = new List<ConditionExpression>
                {
                    new ConditionExpression("lrx_name", ConditionOperator.Equal, eventTicketName),
                    new ConditionExpression("lrx_event", ConditionOperator.Equal, eventID),
                    new ConditionExpression("lrx_amount", ConditionOperator.Equal, entryFeeRecord)
                };

                Entity existingTicket = FindExistingRecord("lrx_eventticket", TicketSearchConditions);

                if (existingTicket == null)
                {
                    TicketID = this._service.Create(new Entity("lrx_eventticket")
                    {
                        ["lrx_name"] = (object)eventTicketName,
                        ["lrx_quantity"] = 1000,
                        ["lrx_eventticketdescription"] = "Fundraisin ticket for registering in event " + EventName,
                        ["lrx_amount"] = new Money(entryFeeRecord),
                        ["lrx_event"] = (object)new EntityReference("lrx_event", eventID),
                    });
                }
                else
                {
                    TicketID = (Guid)existingTicket.Id;
                }

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
                    }
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
                    ["lrx_priceperregistration"] = new Money(ticketAmount),
                    ["lrx_constituentorganization"] = new EntityReference("contact", contactID),
                    ["lrx_transaction"] = TransactionID != Guid.Empty ? new EntityReference("msnfp_transaction", TransactionID) : null,
                    ["lrx_registeredby"] = paidByMember != Guid.Empty ? new EntityReference("contact", paidByMember) : new EntityReference("contact", contactID),
                    ["lrx_registrationpaidby"] = paidMemberRegistration != Guid.Empty ? new EntityReference("lrx_registrations", paidMemberRegistration) : null,
                    ["lrx_promoid"] = int.TryParse(participantEvent.Promo_Id.ToString(), out int promoId) ? promoId : (int?)null,
                    ["lrx_fundraisinregistrationid"] = int.TryParse(participantEvent.History_Id, out int historyId) ? historyId : (int?)null
                };

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
                },
                new ColumnSet("lrx_transaction"));

                if (existingRegistration != null) {
                    registrationId = existingRegistration.Id;
                    if (existingRegistration.Attributes.TryGetValue("lrx_transaction", out var transactionObj) &&
                    transactionObj is EntityReference transactionRef)
                    {
                        TransactionID = transactionRef.Id;
                        _tracingService.Trace($"Transaction ID: {TransactionID}");
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
                        ["lrx_priceperregistration"] = new Money(0),
                        ["lrx_constituentorganization"] = new EntityReference("contact", guestId),
                        ["lrx_transaction"] = TransactionID != Guid.Empty ? new EntityReference("msnfp_transaction", TransactionID) : null,
                        ["lrx_registeredby"] = contactID != Guid.Empty ? new EntityReference("contact", contactID) : null,
                        ["lrx_registrationpaidby"] = registrationId != Guid.Empty ? new EntityReference("lrx_registrations", registrationId) : null
                    };

                    if (existingTicketRegistration == null)
                    {
                        registrationEntity.Id = this._service.Create(registrationEntity);
                    }
                    else
                    {
                        if (registrationId == existingTicketRegistration.Id)
                        {
                            registrationEntity["lrx_registrationpaidby"] = null;
                        }
                        registrationEntity.Id = existingTicketRegistration.Id;
                        this._service.Update(registrationEntity);
                    }
                }
                else
                {
                    if (ticketId != Guid.Empty)
                    {
                        if (ticketHolder.history_id != ticketHolder.related_history_id)
                        {
                            Entity relatedRegistration = this.FindExistingRecord("lrx_registrations", new List<ConditionExpression>()
                            {
                                new ConditionExpression("lrx_fundraisinregistrationid", ConditionOperator.Equal, (object)ticketHolder.related_history_id)
                            });

                            if (relatedRegistration != null)
                                relatedRegistrationId = relatedRegistration.Id;

                            if (relatedRegistrationId != Guid.Empty)
                            {
                                if (registrationId == relatedRegistrationId) {
                                    relatedRegistrationId = Guid.Empty; //making sure that the payor is not related to her own registration
                                }
                                this._service.Update(new Entity("lrx_registrations", registrationId)
                                {
                                    ["lrx_eventticket"] = ticketId != Guid.Empty ? (object)new EntityReference("lrx_eventticket", ticketId) : (object)null,
                                    ["lrx_registrationpaidby"] = relatedRegistrationId != Guid.Empty ? (object)new EntityReference("lrx_registrations", relatedRegistrationId) : (object)null
                                });
                            }
                        }
                        else
                        {
                            this._service.Update(new Entity("lrx_registrations", registrationId)
                            {
                                ["lrx_eventticket"] = ticketId != Guid.Empty ? (object)new EntityReference("lrx_eventticket", ticketId) : (object)null
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

            this._tracingService.Trace("Product Record Fundraisin API Completed");
            return Task.CompletedTask;
        }

        public Task GetFundRaisinProductOptionsRecord()
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
            var raffleList = this.GetData<RaffleModel, RaffleModelMap>(this.baseURL, "raffles");
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
            var raffleTicketList = this.GetData<RaffleTicketModel, RaffleTicketModelMap>(this.baseURL, "raffletickets");
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
                Entity raffleTicketEntity = new Entity("lrx_raffle")
                {
                    ["lrx_name"] = raffleTicket.option_description,
                    ["lrx_tickets"] = raffleTicket.option_tickets,
                    ["lrx_price"] = decimal.TryParse(raffleTicket.option_price, out decimal price) ? new Money(price) : new Money(0),
                    ["lrx_raffle"] = new EntityReference("lrx_raffle", raffleID),
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

        public Task GetFundRaisinTransactionRecord()
        {
            var TransactionList = this.GetData<TransactionModel, TransactionModelMap>(this.baseURL, "transactions");
            var donationList = this.GetData<DonationModel, DonationModelMap>(this.baseURL, "donations");
            var scheduledDonationList = this.GetData<ScheduleModel, ScheduleModelMap>(this.baseURL, "scheduleddonations");
            var saleItemList = this.GetData<SaleItemModel, SaleItemModelMap>(this.baseURL, "salesitems");
            var productList = this.GetData<ProductModel, ProductModelMap>(this.baseURL, "products");
            var productOptionList = this.GetData<ProductOptionModel, ProductOptionModelMap>(this.baseURL, "productoptions");
            var participantList = this.GetData<ParticipantModel, ParticipantModelMap>(this.baseURL, "participants");
            var eventList = this.GetData<EventModel, EventModelMap>(this.baseURL, "events");

            if (TransactionList != null &&
                donationList != null &&
                scheduledDonationList != null &&
                saleItemList != null &&
                productList != null &&
                productOptionList != null &&
                participantList != null &&
                eventList != null)
            {
                Guid defaultCampaignID = Guid.Empty;
                var CampaignSearchConditions = new List<ConditionExpression>
                {
                    new ConditionExpression("name", ConditionOperator.Equal, (object)this.campaignName)
                };

                Entity existingCampaign = FindExistingRecord("campaign", CampaignSearchConditions);
                if (existingCampaign != null)
                {
                    defaultCampaignID = existingCampaign.Id;
                }

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

                    if (transactions.Event_id.Trim() != "0")
                    {          
                        eventID = CheckAndUpdateEvent(transactions.Event_id.Trim(), eventList, out campaignGuid, out appealGuid, out packageGuid);                         
                    }

                    if (transactions.Transaction_type == "donation")
                    {
                        decimal totalDonation = decimal.Parse(transactions.Transaction_value) - decimal.Parse(transactions.Transaction_fees);
                        if (totalDonation == 0)
                        {
                            continue;
                        }
                        var matchDonationID = donationList.FirstOrDefault(d => d.Donation_id == transactions.Donation_id);

                        if (matchDonationID != null)
                        {
                            contactID = UpsertContact(matchDonationID);

                            string pMethodUniqueName = (object)this.paymentMethod + " - " + contactID.ToString();
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
                                    ["lrx_customer"] = (object)new EntityReference("contact", contactID),
                                    ["msnfp_type"] = new OptionSetValue(100000000)

                                });
                                defaultPaymentMethodId = pmethodId;
                            }

                            var matchScheduleDonationID = scheduledDonationList.FirstOrDefault(sd => sd.donation_id == transactions.Donation_id);
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
                                    //["lrx_campaign"] = defaultCampaignID != Guid.Empty ? new EntityReference("campaign", defaultCampaignID),
                                    ["lrx_paymentmethod"] = defaultPaymentMethodId != Guid.Empty ? new EntityReference("msnfp_paymentmethod", defaultPaymentMethodId) : null,
                                    ["sifund_scheduletypecode"] = new OptionSetValue(844060003),
                                    ["sifund_paymenttypecode"] = new OptionSetValue(existingRecord == null ? 844060008 : 844060002), // Handles different payment type codes
                                    ["msnfp_recurringamount"] = new Money(totalRecurringAmmount),
                                    ["msnfp_frequency"] = new OptionSetValue(frequencyType),
                                    ["msnfp_frequencyinterval"] = int.Parse(matchScheduleDonationID.donation_day),
                                    ["sifund_bookdate"] = DateTime.Parse(matchScheduleDonationID.date_created),
                                    ["msnfp_lastpaymentdate"] = DateTime.Parse(transactions.Date_created),
                                    ["lrx_fundraisinpaymentscheduleid"] = int.Parse(matchScheduleDonationID.ScheduleId)
                                };

                                if (existingRecord == null)
                                {
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
                                string pageMemberId = pageDetailList.FirstOrDefault()?.member_id;
                                string pageFname = pageDetailList.FirstOrDefault()?.m_fname;
                                string pageLname = pageDetailList.FirstOrDefault()?.m_lname;

                                string pageFullName = $"{pageFname} {pageLname}".Trim();
                                string matchFullName = $"{matchDonationID.D_fname} {matchDonationID.D_lname}".Trim();

                                if (!string.Equals(matchFullName, pageFullName, StringComparison.OrdinalIgnoreCase))
                                {
                                    var SolicitorSearchConditions = new List<ConditionExpression>
                                    {
                                        new ConditionExpression("lrx_fundraisinmemberid", ConditionOperator.Equal, int.Parse(pageMemberId)),
                                    };

                                    Entity existingSolicitor = FindExistingRecord("contact", SolicitorSearchConditions);
                                    if (existingSolicitor != null)
                                    {
                                        solicitorID = existingSolicitor.Id;
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
                        } // end of match donation variable

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
                            ["lrx_campaign"] = campaignGuid != Guid.Empty ? (object)new EntityReference("campaign", campaignGuid) : (object)(EntityReference)null,
                            ["sifund_appeal"] = appealGuid != Guid.Empty ? (object)new EntityReference("sifund_appeal", appealGuid) : (object)(EntityReference)null,
                            ["sifund_package"] = packageGuid != Guid.Empty ? (object)new EntityReference("sifund_package", packageGuid) : (object)(EntityReference)null,
                            //["msnfp_transaction_paymentmethodid"] = defaultPaymentMethodId != Guid.Empty ? new EntityReference("msnfp_paymentmethod", defaultPaymentMethodId) : null,
                            ["msnfp_transaction_paymentscheduleid"] = scheduleID != Guid.Empty ? new EntityReference("msnfp_paymentschedule", scheduleID) : null,
                            ["msnfp_amount"] = new Money(decimal.Parse(transactions.Transaction_value) - decimal.Parse(transactions.Transaction_fees)),
                            ["msnfp_bookdate"] = DateTime.Parse(transactions.Date_created),
                            ["sifund_paymenttypecode"] = new OptionSetValue(844060002),
                            ["lrx_donationpaymenttype"] = new OptionSetValue(scheduleID != Guid.Empty ? 856660001 : 856660000),
                            ["statuscode"] = new OptionSetValue(856660001),
                            ["sifund_typecode"] = new OptionSetValue(844060000),
                            ["lrx_fundraisintransactionid"] = int.Parse(transactions.Transaction_id)
                        };

                        if (existingTransaction == null)
                        {
                            this._service.Create(transactionEntity);
                        }
                        else if (this.updateTransaction)
                        {
                            transactionEntity.Id = existingTransaction.Id;
                            this._service.Update(transactionEntity);
                        }
                    } //end of donation transaction type


                    if (transactions.Transaction_type == "registration" || transactions.Transaction_type == "merchandise")
                    {
                        int transactionType = 844060003; //default registration

                        var ContactSearchConditions = new List<ConditionExpression>
                        {
                            new ConditionExpression("lrx_fundraisinmemberid", ConditionOperator.Equal, transactions.Member_id)
                        };

                        Entity existingContact = FindExistingRecord("contact", ContactSearchConditions);

                        if (existingContact != null)
                            contactID = (Guid)existingContact.Id;
                        if (contactID == Guid.Empty)
                        {
                            continue;
                        }
                        else
                        {
                            string pMethodUniqueName = (object)this.paymentMethod + " - " + contactID.ToString();
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
                                    ["lrx_customer"] = (object)new EntityReference("contact", contactID),
                                    ["msnfp_type"] = new OptionSetValue(100000000)

                                });
                                defaultPaymentMethodId = pmethodId;
                            }
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

                        if (existingTransaction == null)
                        {
                            Guid transactionId = this._service.Create(new Entity("msnfp_transaction")
                            {
                                ["sifund_donor"] = new EntityReference("contact", contactID),
                                ["lrx_campaign"] = campaignGuid != Guid.Empty ? (object)new EntityReference("campaign", campaignGuid) : (object)(EntityReference)null,
                                ["sifund_appeal"] = appealGuid != Guid.Empty ? (object)new EntityReference("sifund_appeal", appealGuid) : (object)(EntityReference)null,
                                ["sifund_package"] = packageGuid != Guid.Empty ? (object)new EntityReference("sifund_package", packageGuid) : (object)(EntityReference)null,
                                //["msnfp_transaction_paymentmethodid"] = defaultPaymentMethodId != Guid.Empty ? new EntityReference("msnfp_paymentmethod", defaultPaymentMethodId) : null,
                                ["lrx_event"] = eventID != Guid.Empty ? new EntityReference("lrx_event", eventID) : null,
                                ["lrx_registrations"] = registrationID != Guid.Empty ? new EntityReference("lrx_registrations", registrationID) : null,
                                ["lrx_eventteam"] = teamID != Guid.Empty ? new EntityReference("lrx_eventteam", teamID) : null,
                                ["lrx_promocode"] = promoGuid != Guid.Empty ? new EntityReference("lrx_promocodeanddiscount", promoGuid) : null,
                                ["msnfp_amount"] = new Money(decimal.Parse(transactions.Transaction_value) - decimal.Parse(transactions.Transaction_fees)),
                                ["msnfp_bookdate"] = DateTime.Parse(transactions.Date_created),
                                ["sifund_paymenttypecode"] = new OptionSetValue(844060002),
                                ["statuscode"] = new OptionSetValue(856660001),
                                ["sifund_typecode"] = new OptionSetValue(transactionType),
                                ["lrx_fundraisintransactionid"] = int.Parse(transactions.Transaction_id)
                            });

                            if (registrationID != Guid.Empty)
                            {
                                this._service.Update(new Entity("lrx_registrations", registrationID)
                                {
                                    ["lrx_transaction"] = transactionId != Guid.Empty ? new EntityReference("msnfp_transaction", transactionId) : null
                                });
                            }
                        }
                        else
                        {
                            if (this.updateTransaction)
                            {
                                this._service.Update(new Entity("msnfp_transaction", existingTransaction.Id)
                                {
                                    ["sifund_donor"] = new EntityReference("contact", contactID),
                                    ["lrx_campaign"] = campaignGuid != Guid.Empty ? (object)new EntityReference("campaign", campaignGuid) : (object)(EntityReference)null,
                                    ["sifund_appeal"] = appealGuid != Guid.Empty ? (object)new EntityReference("sifund_appeal", appealGuid) : (object)(EntityReference)null,
                                    ["sifund_package"] = packageGuid != Guid.Empty ? (object)new EntityReference("sifund_package", packageGuid) : (object)(EntityReference)null,
                                    //["msnfp_transaction_paymentmethodid"] = defaultPaymentMethodId != Guid.Empty ? new EntityReference("msnfp_paymentmethod", defaultPaymentMethodId) : null,
                                    ["lrx_event"] = eventID != Guid.Empty ? new EntityReference("lrx_event", eventID) : null,
                                    ["lrx_registrations"] = registrationID != Guid.Empty ? new EntityReference("lrx_registrations", registrationID) : null,
                                    ["lrx_eventteam"] = teamID != Guid.Empty ? new EntityReference("lrx_eventteam", teamID) : null,
                                    ["lrx_promocode"] = promoGuid != Guid.Empty ? new EntityReference("lrx_promocodeanddiscount", promoGuid) : null,
                                    ["msnfp_amount"] = new Money(decimal.Parse(transactions.Transaction_value) - decimal.Parse(transactions.Transaction_fees)),
                                    ["msnfp_bookdate"] = DateTime.Parse(transactions.Date_created),
                                    ["sifund_paymenttypecode"] = new OptionSetValue(844060002),
                                    ["statuscode"] = new OptionSetValue(856660001),
                                    ["sifund_typecode"] = new OptionSetValue(transactionType),
                                    ["lrx_fundraisintransactionid"] = int.Parse(transactions.Transaction_id)
                                });

                                if (registrationID != Guid.Empty)
                                {
                                    this._service.Update(new Entity("lrx_registrations", registrationID)
                                    {
                                        ["lrx_transaction"] = existingTransaction.Id != Guid.Empty ? new EntityReference("msnfp_transaction", existingTransaction.Id) : null
                                    });
                                }
                            }
                        }

                        if (transactions.Sale_id != "0")
                        {
                            var salesItemMatchID = saleItemList.FirstOrDefault(si => si.sale_id == transactions.Sale_id);

                            if (salesItemMatchID != null)
                            {
                                Guid productID = Guid.Empty;
                                Guid productOption = Guid.Empty;
                                string productName = "";
                                string productOptionName = "";

                                var matchingProduct = productList.FirstOrDefault(p => p.product_id == salesItemMatchID.product_id);
                                if (matchingProduct != null)
                                {
                                    productName = matchingProduct.product_name;
                                }

                                var matchingProductOption = productOptionList.FirstOrDefault(p => p.product_id == salesItemMatchID.product_id);
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

                                var eventProductSearchConditions = new List<ConditionExpression>
                            {
                                new ConditionExpression("lrx_fundraisineventproductid", ConditionOperator.Equal, int.Parse(salesItemMatchID.id))
                            };
                                Entity existingEventProduct = FindExistingRecord("lrx_eventproduct", eventProductSearchConditions);

                                if (existingEventProduct == null)
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

                                    Guid eventProductID = _service.Create(eventProduct);

                                    var saleProduct = new Entity("lrx_product")
                                    {
                                        ["lrx_name"] = $"{productName} - {productOptionName}",
                                        ["lrx_constituentorganisation"] = new EntityReference("contact", contactID)
                                    };

                                    // Parse `quantity` safely
                                    int parsedQuantity = 0;
                                    if (int.TryParse(salesItemMatchID.quantity, out parsedQuantity))
                                    {
                                        saleProduct["lrx_quantity"] = parsedQuantity;
                                    }

                                    // Parse `unit_cost` safely
                                    decimal parsedPrice = 0;
                                    if (decimal.TryParse(salesItemMatchID.unit_cost, out parsedPrice))
                                    {
                                        saleProduct["lrx_priceperproduct"] = new Money(parsedPrice);
                                    }

                                    // Add lookup fields only if they have valid GUIDs
                                    if (eventID != Guid.Empty)
                                    {
                                        saleProduct["lrx_event"] = new EntityReference("lrx_event", eventID);
                                    }
                                    if (eventProductID != Guid.Empty)
                                    {
                                        saleProduct["lrx_eventproduct"] = new EntityReference("lrx_eventproduct", eventProductID);
                                    }
                                    if (productOption != Guid.Empty)
                                    {
                                        saleProduct["lrx_productoption"] = new EntityReference("lrx_productoptions", productOption);
                                    }

                                    _service.Create(saleProduct);
                                }
                            }
                        }
                    }//end of registration transaction


                    if (transactions.Transaction_type == "refund")
                    {
                        var originalTransaction = TransactionList
                            .FirstOrDefault(t => t.Donation_id == transactions.Donation_id && t.Transaction_type != "refund");

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
                                if (existingRefund == null)
                                {
                                    Guid RefundRecordId = this._service.Create(new Entity("lrx_refund")
                                    {
                                        ["lrx_customer"] = (object)new EntityReference("contact", donorId),
                                        ["lrx_transaction"] = (object)new EntityReference("msnfp_transaction", existingTransaction.Id),
                                        ["lrx_totalamountpaidrefund"] = new Money(transactionAmount),
                                        ["lrx_amountreceiptablerefund"] = new Money(transactionAmount),
                                        ["lrx_totalamountpaid"] = new Money(transactionAmount),
                                        ["lrx_amountreceiptable"] = new Money(transactionAmount),
                                        ["lrx_refunddate"] = DateTime.Parse(transactions.Date_created),
                                        ["lrx_refundtype"] = new OptionSetValue(844060002),
                                        ["statuscode"] = new OptionSetValue(376750001),
                                        ["lrx_fundraisinrefundid"] = int.Parse(transactions.Transaction_id)
                                    });

                                    this._service.Update(new Entity("msnfp_transaction", existingTransaction.Id)
                                    {
                                        ["statuscode"] = new OptionSetValue(856660005)
                                    });
                                }
                                else
                                {
                                    if (this.updateTransaction)
                                    {
                                        this._service.Update(new Entity("lrx_refund", existingRefund.Id)
                                        {
                                            ["lrx_customer"] = (object)new EntityReference("contact", donorId),
                                            ["lrx_transaction"] = (object)new EntityReference("msnfp_transaction", existingTransaction.Id),
                                            ["lrx_totalamountpaidrefund"] = new Money(transactionAmount),
                                            ["lrx_amountreceiptablerefund"] = new Money(transactionAmount),
                                            ["lrx_totalamountpaid"] = new Money(transactionAmount),
                                            ["lrx_amountreceiptable"] = new Money(transactionAmount),
                                            ["lrx_refunddate"] = DateTime.Parse(transactions.Date_created),
                                            ["lrx_refundtype"] = new OptionSetValue(844060002),
                                            ["statuscode"] = new OptionSetValue(376750001),
                                            ["lrx_fundraisinrefundid"] = int.Parse(transactions.Transaction_id)
                                        });

                                        this._service.Update(new Entity("msnfp_transaction", existingTransaction.Id)
                                        {
                                            ["statuscode"] = new OptionSetValue(856660005)
                                        });
                                    }
                                }
                            }
                        }
                    }
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
            return csvContent;
        }

        public string CallFundRaisinCustomAPI(object apiEndpoint, string historyID)
        {
            string requestUri = string.Format("{0}?apikey={1}&history_id={2}", (object)apiEndpoint, (object)this.apikey, historyID);
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

        public List<T> GetData<T, TMap>(string baseUrl, string endpoint)
        where TMap : ClassMap<T>, new()
        {
            string fullUrl = baseUrl + endpoint;
            string csvContent = CallFundRaisinAPI((object)fullUrl);
            return ParseCsvHelper<T, TMap>(csvContent);
        }

        private Guid UpsertContact(dynamic matchDonationID)
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
            var contactAttributes = new Dictionary<string, object>
            {
                ["firstname"] = matchDonationID.D_fname,
                ["lastname"] = matchDonationID.D_lname,
                ["emailaddress1"] = matchDonationID.D_email,
                ["telephone1"] = matchDonationID.D_phone,
                ["mobilephone"] = matchDonationID.D_phone_mobile,
                ["address1_line1"] = addressStreet,
                ["address1_city"] = matchDonationID.D_address_suburb,
                ["address1_postalcode"] = matchDonationID.D_address_pcode,
                ["address1_stateorprovince"] = matchDonationID.D_address_state,
                ["address1_country"] = matchDonationID.D_address_country,
                ["lrx_fundraisinmemberid"] = int.Parse(matchDonationID.Member_id)
            };

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

        private Guid UpsertContactFromRaffleSales(RaffleSalesModel raffleSales)
        {
            // Define search conditions to find an existing contact
            var contactSearchConditions = new List<ConditionExpression>
            {
                new ConditionExpression("firstname", ConditionOperator.Equal, raffleSales.first_name),
                new ConditionExpression("lastname", ConditionOperator.Equal, raffleSales.last_name),
                new ConditionExpression("emailaddress1", ConditionOperator.Equal, raffleSales.email)
            };

            Entity existingContact = FindExistingRecord("contact", contactSearchConditions);
            var addressStreet = $"{raffleSales.address_number} {raffleSales.address_street}".Trim();

            // Prepare contact attributes
            var contactAttributes = new Dictionary<string, object>
            {
                ["firstname"] = raffleSales.first_name,
                ["lastname"] = raffleSales.last_name,
                ["emailaddress1"] = raffleSales.email,
                ["telephone1"] = raffleSales.phone,
                ["mobilephone"] = raffleSales.mobile,
                ["address1_line1"] = addressStreet,
                ["address1_city"] = raffleSales.address_suburb,
                ["address1_postalcode"] = raffleSales.address_postcode,
                ["address1_stateorprovince"] = raffleSales.address_state,
                ["address1_country"] = raffleSales.address_country,
            };

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

        private Guid CheckAndUpdateEvent(
            string eventId,
            List<EventModel> eventList,
            out Guid campaignId,
            out Guid appealId,
            out Guid packageId)
        {
            campaignId = Guid.Empty;
            appealId = Guid.Empty;
            packageId = Guid.Empty;

            EventModel matchedEvent = eventList.FirstOrDefault(e => e.EventId == eventId);
            if (matchedEvent == null)
                return Guid.Empty;

            Entity existingEvent = FindExistingRecord("lrx_event", new List<ConditionExpression>
            {
                new ConditionExpression("lrx_fundraisineventid", ConditionOperator.Equal, matchedEvent.EventId)
            });

            if (existingEvent != null)
            {
                if (existingEvent.Contains("lrx_campaign") && existingEvent["lrx_campaign"] is EntityReference campaignRef)
                    campaignId = campaignRef.Id;
                else
                    campaignId = Guid.Empty;

                if (existingEvent.Contains("lrx_sifund_appeal") && existingEvent["lrx_sifund_appeal"] is EntityReference appealRef)
                    appealId = appealRef.Id;
                else
                    appealId = Guid.Empty;

                if (existingEvent.Contains("lrx_sifund_package") && existingEvent["lrx_sifund_package"] is EntityReference packageRef)
                    packageId = packageRef.Id;
                else
                    packageId = Guid.Empty;

                return existingEvent.Id;
            }
            else
            {
                return Guid.Empty;
            }
        }

    }
}