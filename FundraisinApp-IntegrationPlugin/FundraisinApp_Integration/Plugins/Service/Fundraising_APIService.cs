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

#nullable disable
namespace FundraisinApp_Integration.Plugins.Service
{
    public class Fundraising_APIService
    {
        private readonly IOrganizationService _service;
        private readonly IPluginExecutionContext _context;
        private readonly ITracingService _tracingService;
        public string baseURL = "https://lanrex.funraisin.com.au/api/";
        private string apikey = "27f88fda055da35f0cf54d8f168a8753";
        private string campaignName = "";
        private string dateFrom = "";
        private string dateTo = "";

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
        }


        public void GetFundraisinEventRecords()
        {
            string eventURL = baseURL + "events";
            string csvContent = CallFundRaisinAPI((object)eventURL);
            var eventList = ParseCsvHelper<EventModel, EventModelMap>(csvContent);
            foreach (var eventRecord in eventList)
            {
                var EventSearchConditions = new List<ConditionExpression>
                {
                    new ConditionExpression("lrx_fundraisineventid", ConditionOperator.Equal, eventRecord.EventId)
                };
                Entity existingEvent = FindExistingRecord("lrx_event", EventSearchConditions); ;
                if (existingEvent != null)
                {
                    this._service.Update(new Entity("lrx_event", existingEvent.Id)
                    {
                        ["lrx_name"] = (string)eventRecord.EventName,
                        ["lrx_goal"] = new Money(decimal.Parse(eventRecord.EventTarget)),
                        ["lrx_description"] = eventRecord.EventShortDesc,
                        //["lrx_campaign"] = (object)new EntityReference("campaign", new Guid("d5bf32ce-d9e1-4a2a-914f-9ded53e1b41a")),
                        ["lrx_fundraisineventid"] = (int)eventRecord.EventId
                    });
                }
                else {
                    Guid eventId = this._service.Create(new Entity("lrx_event")
                    {
                        ["lrx_name"] = (string)eventRecord.EventName,
                        ["lrx_goal"] = new Money(decimal.Parse(eventRecord.EventTarget)),
                        ["lrx_description"] = eventRecord.EventShortDesc,
                        //["lrx_campaign"] = (object)new EntityReference("campaign", new Guid("d5bf32ce-d9e1-4a2a-914f-9ded53e1b41a")),
                        ["lrx_fundraisineventid"] = (int)eventRecord.EventId
                    });
                }                    
            }
            this._tracingService.Trace("Event Record Fundraisin API Completed");
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

            this._tracingService.Trace("Participant Record Fundraisin API Completed");
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
                string ContactFullName = "";
                string EventName = "";
                var MemberSearchConditions = new List<ConditionExpression>
                {
                    new ConditionExpression("lrx_fundraisinmemberid", ConditionOperator.Equal, participantEvent.Member_Id)
                };

                Entity existingMember = FindExistingRecord("contact", MemberSearchConditions);
                if (existingMember != null) {
                    contactID = (Guid)existingMember.Id;
                    // Retrieve full name if available
                    if (existingMember.Attributes.Contains("fullname"))
                    {
                        ContactFullName = existingMember["fullname"].ToString();
                    }
                }
                    

                var EventSearchConditions = new List<ConditionExpression>
                {
                    new ConditionExpression("lrx_fundraisineventid", ConditionOperator.Equal, participantEvent.Event_Id)
                };

                Entity existingEvent = FindExistingRecord("lrx_event", EventSearchConditions);
                if (existingEvent != null) {
                    eventID = (Guid)existingEvent.Id;
                    // Retrieve event name if available
                    if (existingEvent.Attributes.Contains("lrx_name"))
                    {
                        EventName = existingEvent["lrx_name"].ToString();
                    }
                }                   

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
                string identifierName = ContactFullName + " - " + EventName;
                Entity existingRegistration = FindExistingRecord("lrx_registrations", RegistrationSearchConditions);
                if(existingRegistration == null)
                {
                    Guid registrationID = this._service.Create(new Entity("lrx_registrations")
                    {
                        ["lrx_event"] = (object)new EntityReference("lrx_event", eventID),
                        ["lrx_name"] = (object)identifierName,
                        ["lrx_constituentorganization"] = (object)new EntityReference("contact", contactID)                        
                    });
                }
                else
                {
                    this._service.Update(new Entity("lrx_registrations", existingRegistration.Id)
                    {
                        ["lrx_event"] = (object)new EntityReference("lrx_event", eventID),
                        ["lrx_name"] = (object)identifierName,
                        ["lrx_constituentorganization"] = (object)new EntityReference("contact", contactID)
                    });
                }
            }

            this._tracingService.Trace("Registration Record Fundraisin API Completed");
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
                    this._tracingService.Trace("No event found for ticket record " + tickets.ticket_id);
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

            this._tracingService.Trace("Ticket Record Fundraisin API Completed");
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

            this._tracingService.Trace("Ticket Holder Record Fundraisin API Completed");
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

            this._tracingService.Trace("Product Record Fundraisin API Completed");
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
                    statusCode = 856660001;

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
        }

        public void GetFundRaisinEventTeamsRecord()
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
                    this._tracingService.Trace("No event found for team record " + eventTeams.team_id);
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
                    this._tracingService.Trace("No captain found for team record " + eventTeams.team_id);
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
        }

        public void GetFundRaisinOrganisationRecord()
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
                    this._tracingService.Trace("No primary contact found for organisation record " + organisations.org_id);
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
        }

        public void GetFundRaisinTransactionRecord()
        {
            string url = baseURL + "transactions";
            string csvContent = CallFundRaisinAPI((object)url);

            var TransactionList = ParseCsvHelper<TransactionModel, TransactionModelMap>(csvContent);
            Guid defaultCampaignID = Guid.Empty;
            var CampaignSearchConditions = new List<ConditionExpression>
            {
                new ConditionExpression("name", ConditionOperator.Equal, (object)this.campaignName)
            };

            Entity existingCampaign = FindExistingRecord("campaign", CampaignSearchConditions);
            if (existingCampaign != null) {
                defaultCampaignID = existingCampaign.Id;
            }

            foreach (var transactions in TransactionList)
            {         
                if (transactions.Transaction_type == "donation") {
                    string donationUrl = baseURL + "donations";
                    string csvDonationContent = CallFundRaisinAPI((object)donationUrl); 
                    var donationList = ParseCsvHelper<DonationModel, DonationModelMap>(csvDonationContent);
                    
                    Guid contactID = Guid.Empty;
                    foreach (var donations in donationList) 
                    {
                        if (donations.Donation_id == transactions.Donation_id) {
                            var ContactSearchConditions = new List<ConditionExpression>
                            {
                                new ConditionExpression("firstname", ConditionOperator.Equal, donations.D_fname),
                                new ConditionExpression("lastname", ConditionOperator.Equal, donations.D_lname),
                                new ConditionExpression("emailaddress1", ConditionOperator.Equal, donations.D_email)
                            };

                            Entity existingContact = FindExistingRecord("contact", ContactSearchConditions);

                            //create contact if not existing else update contact
                            if (existingContact == null)
                            {
                                string addressStreet = donations.D_address_number + donations.D_address_street;
                                Guid contactId = this._service.Create(new Entity("contact")
                                {
                                    ["firstname"] = (object)donations.D_fname,
                                    ["lastname"] = (object)donations.D_lname,
                                    ["emailaddress1"] = (object)donations.D_email,
                                    ["telephone1"] = (object)donations.D_phone,
                                    ["mobilephone"] = (object)donations.D_phone_mobile,
                                    ["address1_line1"] = (object)addressStreet,
                                    ["address1_city"] = (object)donations.D_address_suburb,
                                    ["address1_postalcode"] = (object)donations.D_address_pcode,
                                    ["address1_stateorprovince"] = (object)donations.D_address_state,
                                    ["address1_country"] = (object)donations.D_address_country,
                                    ["lrx_fundraisinmemberid"] = int.Parse(donations.Member_id)
                                });
                                contactID = contactId;
                            }
                            else
                            {
                                string addressStreet = donations.D_address_number + donations.D_address_street;
                                this._service.Update(new Entity("contact", existingContact.Id)
                                {
                                    ["firstname"] = (object)donations.D_fname,
                                    ["lastname"] = (object)donations.D_lname,
                                    ["emailaddress1"] = (object)donations.D_email,
                                    ["telephone1"] = (object)donations.D_phone,
                                    ["mobilephone"] = (object)donations.D_phone_mobile,
                                    ["address1_line1"] = (object)addressStreet,
                                    ["address1_city"] = (object)donations.D_address_suburb,
                                    ["address1_postalcode"] = (object)donations.D_address_pcode,
                                    ["address1_stateorprovince"] = (object)donations.D_address_state,
                                    ["address1_country"] = (object)donations.D_address_country,
                                    ["lrx_fundraisinmemberid"] = int.Parse(donations.Member_id)
                                });
                                contactID = existingContact.Id;
                            }
                        }                       
                    }

                    Guid scheduleID = Guid.Empty;
                    if (transactions.Schedule_id != "0") 
                    {
                        string ScheduledonationUrl = baseURL + "scheduleddonations";
                        string csvScheduleDonationContent = CallFundRaisinAPI((object)ScheduledonationUrl);

                        var scheduledDonationList = ParseCsvHelper<ScheduleModel, ScheduleModelMap>(csvScheduleDonationContent);

                        foreach (var scheduleddonations in scheduledDonationList)
                        {
                            if (scheduleddonations.ScheduleId == transactions.Schedule_id)
                            {
                                var ContactSearchConditions = new List<ConditionExpression>
                                {
                                    new ConditionExpression("lrx_fundraisinpaymentscheduleid", ConditionOperator.Equal, scheduleddonations.ScheduleId),
                                };

                                Entity existingRecord = FindExistingRecord("msnfp_paymentschedule", ContactSearchConditions);

                                var frequencyType = 856660003; // default to monthly
                                if (scheduleddonations.donation_frequency == "weekly")
                                    frequencyType = 856660002; //change to weekly
                                if (scheduleddonations.donation_frequency == "yearly")
                                    frequencyType = 856660004; //change to years
                                if (scheduleddonations.donation_frequency == "fortnightly")
                                    frequencyType = 856660005; //change to forthnightly

                                decimal totalRecurringAmmount = decimal.Parse(transactions.Transaction_value) * int.Parse(scheduleddonations.donation_period);

                                //create payment schedule if not existing else update contact
                                if (existingRecord == null)
                                {
                                    Guid paymentScheduleId = this._service.Create(new Entity("msnfp_paymentschedule")
                                    {
                                        ["sifund_donor"] = (object)new EntityReference("contact", contactID),
                                        ["sifund_scheduletypecode"] = new OptionSetValue(844060003),
                                        ["sifund_paymenttypecode"] = new OptionSetValue(844060002),
                                        ["msnfp_recurringamount"] = new Money(totalRecurringAmmount),
                                        ["msnfp_frequency"] = new OptionSetValue(frequencyType),
                                        ["msnfp_frequencyinterval"] = int.Parse(scheduleddonations.donation_day),
                                        ["sifund_bookdate"] = DateTime.Parse(scheduleddonations.date_created),
                                        ["msnfp_lastpaymentdate"] = DateTime.Parse(transactions.Date_created),                                     
                                        ["lrx_fundraisinpaymentscheduleid"] = int.Parse(scheduleddonations.ScheduleId)
                                    });
                                    scheduleID = paymentScheduleId;
                                }
                                else
                                {
                                    this._service.Update(new Entity("msnfp_paymentschedule", existingRecord.Id)
                                    {
                                        ["sifund_donor"] = (object)new EntityReference("contact", contactID),
                                        ["sifund_scheduletypecode"] = new OptionSetValue(844060003),
                                        ["sifund_paymenttypecode"] = new OptionSetValue(844060002),
                                        ["msnfp_recurringamount"] = new Money(totalRecurringAmmount),
                                        ["msnfp_frequency"] = new OptionSetValue(frequencyType),
                                        ["msnfp_frequencyinterval"] = int.Parse(scheduleddonations.donation_day),
                                        ["sifund_bookdate"] = DateTime.Parse(scheduleddonations.date_created),
                                        ["msnfp_lastpaymentdate"] = DateTime.Parse(transactions.Date_created),
                                        ["lrx_fundraisinpaymentscheduleid"] = int.Parse(scheduleddonations.ScheduleId)
                                    });
                                    scheduleID = existingRecord.Id;
                                }
                            }
                        }
                    }
                    

                    Guid eventID = Guid.Empty;
                    if (transactions.Event_id != "0")
                    {
                        eventID = Guid.Empty;
                        var EventSearchConditions = new List<ConditionExpression>
                        {
                            new ConditionExpression("lrx_fundraisineventid", ConditionOperator.Equal, transactions.Event_id)
                        };

                        Entity existingEvent = FindExistingRecord("lrx_event", EventSearchConditions);
                        if (existingEvent != null)
                            eventID = (Guid)existingEvent.Id;

                        if (eventID == Guid.Empty)
                        {
                            this._tracingService.Trace("No event found for transaction record " + transactions.Transaction_id);
                        }
                    }

                    var TransactionSearchConditions = new List<ConditionExpression>
                    {
                        new ConditionExpression("lrx_fundraisintransactionid", ConditionOperator.Equal, transactions.Transaction_id),
                    };

                    Entity existingTransaction = FindExistingRecord("msnfp_transaction", TransactionSearchConditions);
                    if(existingTransaction == null)
                    {
                        Guid transactionId = this._service.Create(new Entity("msnfp_transaction")
                        {
                            ["sifund_donor"] = (object)new EntityReference("contact", contactID),
                            ["lrx_event"] = eventID != Guid.Empty ? (object)new EntityReference("lrx_event", eventID) : null,
                            ["lrx_campaign"] = defaultCampaignID != Guid.Empty ? (object)new EntityReference("campaign", defaultCampaignID) : null,
                            ["msnfp_transaction_paymentscheduleid"] = scheduleID != Guid.Empty ? (object)new EntityReference("msnfp_paymentschedule", scheduleID) : null,
                            ["msnfp_amount"] = new Money(decimal.Parse(transactions.Transaction_value)),
                            ["msnfp_bookdate"] = DateTime.Parse(transactions.Date_created),
                            ["sifund_paymenttypecode"] = new OptionSetValue(844060002),
                            ["statuscode"] = new OptionSetValue(856660001),
                            ["sifund_typecode"] = new OptionSetValue(844060000),
                            ["lrx_fundraisintransactionid"] = int.Parse(transactions.Transaction_id)
                        });
                    }
                } //end of donation transaction type

                if (transactions.Transaction_type == "registration" || transactions.Transaction_type == "merchandise")
                {
                    int transactionType = 844060003; //default registration
                    if (transactions.Transaction_type == "merchandise")
                        transactionType = 844060004;

                    Guid eventID = Guid.Empty;
                    if (transactions.Event_id != "0")
                    {
                        eventID = Guid.Empty;
                        var EventSearchConditions = new List<ConditionExpression>
                        {
                            new ConditionExpression("lrx_fundraisineventid", ConditionOperator.Equal, transactions.Event_id)
                        };

                        Entity existingEvent = FindExistingRecord("lrx_event", EventSearchConditions);
                        if (existingEvent != null)
                            eventID = (Guid)existingEvent.Id;

                        if (eventID == Guid.Empty)
                        {
                            this._tracingService.Trace("No event found for transaction record " + transactions.Transaction_id);
                        }
                    }
                    else
                    {
                        continue;
                    }

                    Guid contactID = Guid.Empty;
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

                    var TransactionSearchConditions = new List<ConditionExpression>
                    {
                        new ConditionExpression("lrx_fundraisintransactionid", ConditionOperator.Equal, transactions.Transaction_id),
                    };

                    Entity existingTransaction = FindExistingRecord("msnfp_transaction", TransactionSearchConditions);

                    if (existingTransaction == null)
                    {
                        Guid transactionId = this._service.Create(new Entity("msnfp_transaction")
                        {
                            ["sifund_donor"] = (object)new EntityReference("contact", contactID),
                            ["lrx_campaign"] = defaultCampaignID != Guid.Empty ? (object)new EntityReference("campaign", defaultCampaignID) : null,
                            ["lrx_event"] = eventID != Guid.Empty ? (object)new EntityReference("lrx_event", eventID) : null,
                            ["msnfp_amount"] = new Money(decimal.Parse(transactions.Transaction_value)),
                            ["msnfp_bookdate"] = DateTime.Parse(transactions.Date_created),
                            ["sifund_paymenttypecode"] = new OptionSetValue(844060002),
                            ["statuscode"] = new OptionSetValue(856660001),
                            ["sifund_typecode"] = new OptionSetValue(transactionType),
                            ["lrx_fundraisintransactionid"] = int.Parse(transactions.Transaction_id)
                        });
                    }
                }
            }

            this._tracingService.Trace("Transaction Record Fundraisin API Completed");
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
    }
}