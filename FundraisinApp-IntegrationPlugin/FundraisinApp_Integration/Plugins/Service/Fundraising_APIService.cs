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
using Newtonsoft.Json;
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
using System.Runtime.Remoting.Services;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Services.Description;
using System.Web.UI;
using System.Web.Util;
using System.Windows;
using System.Workflow.Runtime.Tracking;
using static CrmEarlyBound.SiFund_Package;

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
        private Guid defaultPaymentMethodId = Guid.Empty;
        private Guid defaultPrimaryDesignationId = Guid.Empty;
        private Guid defaultCampaignId = Guid.Empty;
        //private string dateFrom = "";
        //private string dateTo = "";
        bool updateTransaction = false;
        //private TransactionModel _transaction;
        private readonly string _jsonInput;
        private readonly string _entityName;
        private readonly bool _useFirstNameLastNameEmail;
        private readonly bool _useFirstNameLastNameMobile;
        private readonly bool _useFirstNameLastNameDob;

        public Fundraising_APIService(
        IOrganizationService service,
        IPluginExecutionContext context,
        ITracingService tracingService,
        lrx_Configuration configuration,
        object JSONinput,
        string entityName)
        {
            //    this._service = service;
            //    this._context = context;
            //    this._tracingService = tracingService;

            //    // Parse the JSON input
            //    JObject jsonInput = JObject.Parse(JSONinput.ToString());
            //    _transaction = JsonConvert.DeserializeObject<TransactionModel>(JSONinput.ToString());

            //    // Assign the values to the variables
            //    this.baseURL = jsonInput["baseURL"]?.ToString();
            //    this.apikey = jsonInput["apikey"]?.ToString();
            //    this.campaignName = jsonInput["defaultCampaignName"]?.ToString();
            //    this.paymentMethod = jsonInput["defaultPaymentMethodName"]?.ToString();
            //    this.updateTransaction = bool.Parse(jsonInput["updateTransaction"]?.ToString());

            //    string format = "MM-dd-yyyy HH:mm:ss";
            //    CultureInfo provider = CultureInfo.InvariantCulture;

            //    if (DateTime.TryParseExact(jsonInput["dateFrom"]?.ToString(), format, provider, DateTimeStyles.None, out DateTime parsedDateFrom))
            //    {
            //        this.dateFrom = parsedDateFrom.ToString("yyyy-MM-dd HH:mm:ss");
            //    }

            //    if (DateTime.TryParseExact(jsonInput["dateTo"]?.ToString(), format, provider, DateTimeStyles.None, out DateTime parsedDateTo))
            //    {
            //        this.dateTo = parsedDateTo.ToString("yyyy-MM-dd HH:mm:ss");
            //    }

            //    if (!string.IsNullOrEmpty(baseURL) && baseURL.Length > 4)
            //    {
            //        baseURLCustom = baseURL.Substring(0, this.baseURL.Length - 4) + "customcode/";
            //    }
            //}
            this._service = service;
            this._context = context;
            this._tracingService = tracingService;
            _jsonInput = JSONinput.ToString();
            // Deserialize the transaction received from Power Automate
            //_transaction = JsonConvert.DeserializeObject<TransactionModel>(JSONinput.ToString());

            // Read configuration from Dataverse
            this.baseURL = configuration.lrx_FundraisinAPIURL;
            this.apikey = configuration.lrx_FundraisinAPIKey;
            this._entityName = entityName;

            // Lookup fields
            this.campaignName = configuration.lrx_DefaultCampaign?.Name ?? string.Empty;
            this.paymentMethod = configuration.lrx_DefaultPaymentMethod?.Name ?? string.Empty;
            this.defaultPrimaryDesignationId = configuration.lrx_DefaultPrimaryDesignation?.Id ?? Guid.Empty;
            this.defaultPaymentMethodId = configuration.lrx_DefaultPaymentMethod?.Id ?? Guid.Empty;
            this.defaultCampaignId = configuration.lrx_DefaultCampaign?.Id ?? Guid.Empty;

            //duplicate detection fields for participants
            this._useFirstNameLastNameEmail = configuration.lrx_FirstNameLastNameEmail ?? false;
            this._useFirstNameLastNameMobile = configuration.lrx_FirstNameLastNameMobile ?? false;
            this._useFirstNameLastNameDob = configuration.lrx_FirstNameLastNameDob ?? false;
            // If you still want this hardcoded for now
            this.updateTransaction = true;

            // Build Custom Code URL
            if (!string.IsNullOrWhiteSpace(baseURL) && baseURL.EndsWith("/api/"))
            {
                baseURLCustom = baseURL.Replace("/api/", "/customcode/");
            }

            _tracingService.Trace("===== Configuration Loaded =====");
            _tracingService.Trace($"Base URL : {baseURL}");
            _tracingService.Trace($"API Key : {apikey}");
            _tracingService.Trace($"Campaign : {campaignName}");
            _tracingService.Trace($"Payment Method : {paymentMethod}");
            _tracingService.Trace($"Primary Designation Id : {defaultPrimaryDesignationId}");
            _tracingService.Trace("===== Matching Configuration =====");
            _tracingService.Trace("FirstNameLastNameEmail : {0}", _useFirstNameLastNameEmail);
            _tracingService.Trace("FirstNameLastNameMobile : {0}", _useFirstNameLastNameMobile);
            _tracingService.Trace("FirstNameLastNameDob : {0}", _useFirstNameLastNameDob);

        }

        public Task GetFundraisinEventRecords()
        {
            //List<EventModel> fundraisingEvents = this.ParseCsvHelper<EventModel, EventModelMap>(this.CallFundRaisinAPI((object)(this.baseURL + "events")));

            //foreach (EventModel eventModel in fundraisingEvents)
            //{
            EventModel eventModel = GetInputRecord<EventModel>();
            _tracingService.Trace("Raw JSON:");
            _tracingService.Trace(_jsonInput);

            if (eventModel == null)
            {
                _tracingService.Trace("Event JSON is null.");
                LogSkippedRecord(
                        nameof(GetFundraisinEventRecords),
                        "Fundraisin Event JSON",
                        "Skipped: input event payload is null, so no Fundraisin event could be identified.",
                        "Unknown Event",
                        null);
                return Task.CompletedTask;
            }

            _tracingService.Trace($"EventId = {eventModel.EventId}");
            if (!int.TryParse(eventModel.EventId, out int eventId))
            {
                _tracingService.Trace("Invalid EventId.");
                LogSkippedRecord(
                    nameof(GetFundraisinEventRecords),
                    eventModel?.EventId,
                    $"Skipped: invalid Fundraisin EventId '{eventModel?.EventId}'. EventName='{eventModel?.EventName}', EventDate='{eventModel?.EventDate}', Location='{eventModel?.EventLocation}'.",
                    eventModel?.EventName,
                    ParseDate(eventModel?.EventDate));
                return Task.CompletedTask;
            }
            //Compare with crm event id with codename in campaign
            Guid campaignId = ResolveEventCampaign(eventModel.CrmEventId); 
            Guid appealId;
            Guid packageId;

            ResolveEventLookups(
                eventModel.EventId,
                null,
                out _,
                out appealId,
                out packageId);
            void AddIfValid(Entity entity, string key, string value)
            {
                var cleaned = NullIfMissing(value);
                if (cleaned != null)
                    entity[key] = cleaned;
            }

            Guid eventRecordId = Guid.Empty;
            Entity existingEventRecord = this.FindExistingRecord("lrx_event", new List<ConditionExpression>()
                {
                    new ConditionExpression("lrx_fundraisineventid", ConditionOperator.Equal, eventId)
                });
            decimal goal = 0;
            decimal.TryParse(eventModel.EventTarget, NumberStyles.Any, CultureInfo.InvariantCulture, out goal);
            string street1 = string.Join(" ", new[]
            {
                NullIfMissing(eventModel.EventUnit),
                NullIfMissing(eventModel.EventNumber),
                NullIfMissing(eventModel.EventStreet)
             }.Where(x => !string.IsNullOrWhiteSpace(x)));
            OptionSetValue eventCategory = null;
            var eventType = NullIfMissing(eventModel.EventType);
            if (!string.IsNullOrWhiteSpace(eventType))
            {
                switch (eventType.Trim().ToLowerInvariant())
                {
                    case "online":
                        eventCategory = new OptionSetValue(856660000);
                        break;
                    case "offline":
                        eventCategory = new OptionSetValue(856660001);
                        break;
                    case "diy":
                        eventCategory = new OptionSetValue(856660002);
                        break;
                }
            }
            if (existingEventRecord != null)
            {
                Entity updatedEventRecord = new Entity("lrx_event", existingEventRecord.Id)
                {

                    ["lrx_goal"] = new Money(goal),
                    ["lrx_fundraisineventid"] = eventId,
                };

                AddIfValid(updatedEventRecord, "lrx_name", eventModel.EventName);
                AddIfValid(updatedEventRecord, "lrx_description", eventModel.EventShortDesc);
                AddIfValid(updatedEventRecord, "lrx_location", eventModel.EventLocation);
                AddIfValid(updatedEventRecord, "lrx_donationprefix", eventModel.StPrefixDonation);
                AddIfValid(updatedEventRecord, "lrx_registrationprefix", eventModel.StPrefixRegistration);
                AddIfValid(updatedEventRecord, "lrx_shopprefix", eventModel.StPrefixShop);
                AddIfValid(updatedEventRecord, "lrx_ticketprefix", eventModel.StPrefixTicket);
                AddIfValid(updatedEventRecord, "lrx_street1", street1);
                AddIfValid(updatedEventRecord, "lrx_suburb", eventModel.EventSuburb);
                AddIfValid(updatedEventRecord, "lrx_city", eventModel.EventCity);
                AddIfValid(updatedEventRecord, "lrx_postcode", eventModel.EventPostcode);
                AddIfValid(updatedEventRecord, "lrx_state", eventModel.EventState);
                AddIfValid(updatedEventRecord, "lrx_country", eventModel.EventCountry);
                var eventTablesValue = NullIfMissing(eventModel.EventTables);
                if (eventTablesValue != null && int.TryParse(eventTablesValue, out int eventTables))
                    updatedEventRecord["lrx_eventtable"] = eventTables;
                AddIfValid(updatedEventRecord, "lrx_eventcode", eventModel.EventCode);
                AddIfValid(updatedEventRecord, "lrx_website", eventModel.EventWebsite);
                if (eventCategory != null)
                    updatedEventRecord["lrx_eventcategory"] = eventCategory;

                DateTime eventStartDate;
                if (NullIfMissing(eventModel.EventDate) != null && DateTime.TryParse(eventModel.EventDate, out eventStartDate))
                    updatedEventRecord["lrx_proposedstart"] = (object)eventStartDate;

                DateTime eventEndDate;
                if (NullIfMissing(eventModel.EventClosedDate) != null && DateTime.TryParse(eventModel.EventClosedDate, out eventEndDate))
                    updatedEventRecord["lrx_proposedend"] = (object)eventEndDate;
                if (campaignId != Guid.Empty &&
    existingEventRecord.GetAttributeValue<EntityReference>("lrx_campaign") == null)
                {
                    updatedEventRecord["lrx_campaign"] =
                        new EntityReference("campaign", campaignId);
                }

                if (appealId != Guid.Empty &&
                    existingEventRecord.GetAttributeValue<EntityReference>("lrx_sifund_appeal") == null)
                {
                    updatedEventRecord["lrx_sifund_appeal"] =
                        new EntityReference("sifund_appeal", appealId);
                }

                if (packageId != Guid.Empty &&
                    existingEventRecord.GetAttributeValue<EntityReference>("lrx_sifund_package") == null)
                {
                    updatedEventRecord["lrx_sifund_package"] =
                        new EntityReference("sifund_package", packageId);
                }

                this._service.Update(updatedEventRecord);
                eventRecordId = existingEventRecord.Id;
            }
            else
            {
                Entity newEventRecord = new Entity("lrx_event")
                {

                    ["lrx_goal"] = new Money(goal),
                    ["lrx_fundraisineventid"] = eventId
                };
                AddIfValid(newEventRecord, "lrx_name", eventModel.EventName);
                AddIfValid(newEventRecord, "lrx_description", eventModel.EventShortDesc);
                AddIfValid(newEventRecord, "lrx_location", eventModel.EventLocation);
                AddIfValid(newEventRecord, "lrx_donationprefix", eventModel.StPrefixDonation);
                AddIfValid(newEventRecord, "lrx_registrationprefix", eventModel.StPrefixRegistration);
                AddIfValid(newEventRecord, "lrx_shopprefix", eventModel.StPrefixShop);
                AddIfValid(newEventRecord, "lrx_ticketprefix", eventModel.StPrefixTicket);
                AddIfValid(newEventRecord, "lrx_street1", street1);
                AddIfValid(newEventRecord, "lrx_suburb", eventModel.EventSuburb);
                AddIfValid(newEventRecord, "lrx_city", eventModel.EventCity);
                AddIfValid(newEventRecord, "lrx_postcode", eventModel.EventPostcode);
                AddIfValid(newEventRecord, "lrx_state", eventModel.EventState);
                AddIfValid(newEventRecord, "lrx_country", eventModel.EventCountry);
                var eventTablesValue = NullIfMissing(eventModel.EventTables);
                if (eventTablesValue != null && int.TryParse(eventTablesValue, out int eventTables))
                    newEventRecord["lrx_eventtable"] = eventTables;
                AddIfValid(newEventRecord, "lrx_eventcode", eventModel.EventCode);
                AddIfValid(newEventRecord, "lrx_website", eventModel.EventWebsite);
                if (eventCategory != null)
                    newEventRecord["lrx_eventcategory"] = eventCategory;

                DateTime eventStartDate;
                if (NullIfMissing(eventModel.EventDate) != null && DateTime.TryParse(eventModel.EventDate, out eventStartDate))
                    newEventRecord["lrx_proposedstart"] = (object)eventStartDate;

                DateTime eventEndDate;
                if (NullIfMissing(eventModel.EventClosedDate) != null && DateTime.TryParse(eventModel.EventClosedDate, out eventEndDate))
                    newEventRecord["lrx_proposedend"] = (object)eventEndDate;
                if (campaignId != Guid.Empty)
                    newEventRecord["lrx_campaign"] =
                        new EntityReference("campaign", campaignId);

                if (appealId != Guid.Empty)
                    newEventRecord["lrx_sifund_appeal"] =
                        new EntityReference("sifund_appeal", appealId);

                if (packageId != Guid.Empty)
                    newEventRecord["lrx_sifund_package"] =
                        new EntityReference("sifund_package", packageId);

                eventRecordId = this._service.Create(newEventRecord);
            }
            //}

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
            //string participantURL = baseURL + "participants";
            //string csvContent = CallFundRaisinAPI((object)participantURL);
            //List<ParticipantModel> participantList = this.ParseParticipantCsvHelper(csvContent);
            //var participantList = ParseCsvHelper<ParticipantModel, ParticipantModelMap>(csvContent);

            ParticipantModel participant = GetInputRecord<ParticipantModel>();

            _tracingService.Trace("Raw JSON:");
            _tracingService.Trace(_jsonInput);

            if (participant == null)
            {
                _tracingService.Trace("Participant input is null.");
                LogSkippedRecord("GetFundraisinParticipantRecords", "Not Found", "Participant input is null.", "Participant Null", null);
                return Task.CompletedTask;
            }

            _tracingService.Trace($"Participant MemberId = {participant.MemberId}");
            //foreach (var participant in participantList)
            //{
            if (!int.TryParse(participant.MemberId, out int memberId))
            {
                _tracingService.Trace("Invalid Participant MemberId.");
                LogSkippedRecord("GetFundraisinParticipantRecords", participant?.MemberId, "Invalid Participant MemberId.", "Invalid Participant", null);
                return Task.CompletedTask;
            }

            var MemberSearchConditions = new List<ConditionExpression>
                {
                    new ConditionExpression("lrx_fundraisinmemberid", ConditionOperator.Equal, memberId),
                    new ConditionExpression("statecode", ConditionOperator.Equal, 0)
                };

            Entity existingMember = FindExistingRecord("contact", MemberSearchConditions);
            var contactFields = new Dictionary<string, object>();

            void AddIfValid(string key, object value)
            {
                if (value is string str)
                {
                    str = NullIfMissing(str);

                    if (string.IsNullOrWhiteSpace(str))
                        return;

                    contactFields[key] = str;
                    return;
                }

                if (value != null)
                {
                    contactFields[key] = value;
                }
            }

            // Safely parse MemberId
            //int? memberIdValue = memberId;

            // Add fields
            AddIfValid("firstname", participant.MFname);
            AddIfValid("middlename", participant.MMiddle);
            AddIfValid("lastname", participant.MLname);
            switch (NullIfMissing(participant.MTitle))
            {
                case "Mr.":
                    AddIfValid("lrx_title", new OptionSetValue(844060000));
                    break;

                case "Ms.":
                    AddIfValid("lrx_title", new OptionSetValue(844060001));
                    break;

                case "Mrs.":
                    AddIfValid("lrx_title", new OptionSetValue(844060002));
                    break;

                case "Dr.":
                    AddIfValid("lrx_title", new OptionSetValue(844060003));
                    break;

                case "Prof.":
                    AddIfValid("lrx_title", new OptionSetValue(844060004));
                    break;

                case "Rev.":
                    AddIfValid("lrx_title", new OptionSetValue(844060005));
                    break;

                case "Master":
                    AddIfValid("lrx_title", new OptionSetValue(856660001));
                    break;

                case "Miss":
                    AddIfValid("lrx_title", new OptionSetValue(856660002));
                    break;
            }
            switch (NullIfMissing(participant.MGender))
            {
                case "Male":
                    AddIfValid("gendercode", new OptionSetValue(1));
                    break;

                case "Female":
                    AddIfValid("gendercode", new OptionSetValue(2));
                    break;

                case "Non-Binary":
                    AddIfValid("gendercode", new OptionSetValue(856660001));
                    break;

                case "Prefer Not To Say":
                    AddIfValid("gendercode", new OptionSetValue(856660002));
                    break;
            }
            AddIfValid("emailaddress1", participant.MEmail);
            AddIfValid("telephone1", participant.MPhoneHome);
            string mobilePhone = participant.MPhoneMobile;
            AddIfValid("mobilephone", mobilePhone);
            AddIfValid("address1_line1",
                 string.Join(" ",
                 new[]
                 {
                    NullIfMissing(participant.MAddressUnit),
                    NullIfMissing(participant.MAddressNumber),
                    NullIfMissing(participant.MAddressStreet)
                 }.Where(x => !string.IsNullOrWhiteSpace(x))));
            AddIfValid("address1_line2", participant.MAddress2);
            AddIfValid("address1_city", participant.MAddressSuburb);
            AddIfValid("address1_postalcode", participant.MAddressPCode);
            AddIfValid("address1_stateorprovince", participant.MAddressState);
            AddIfValid("address1_country", participant.MAddressCountry);
            AddIfValid("address2_line1",
            string.Join(" ",
                    new[]
                    {
                        NullIfMissing(participant.MPostalUnit),
                        NullIfMissing(participant.MPostalNumber),
                        NullIfMissing(participant.MPostalStreet)
                    }.Where(x => !string.IsNullOrWhiteSpace(x))));

            AddIfValid("address2_line2", participant.MPostalAddress2);
            AddIfValid("address2_city", participant.MPostalSuburb);
            AddIfValid("address2_postalcode", participant.MPostalPCode);
            AddIfValid("address2_stateorprovince", participant.MPostalState);
            AddIfValid("address2_country", participant.MPostalCountry);
            var dob = NullIfMissing(participant.MDob);
            DateTime? participantDob = null;
            if (DateTime.TryParse(dob, out DateTime birthDate))
            {
                participantDob = birthDate.Date;
                AddIfValid("birthdate", birthDate.Date);
            }

            AddIfValid("lrx_fundraisinmemberid", memberId);


            if (existingMember == null)
            {


                Entity existingContact = FindContactByDuplicateRules(
                                            participant.MFname,
                                            participant.MLname,
                                            participant.MEmail,
                                            participant.MPhoneMobile,
                                            participantDob);

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
                    contactFields.Remove("middlename");
                    contactFields.Remove("lastname");
                    contactFields.Remove("emailaddress1");
                    int? existingFundraisinMemberId = null;
                    if (existingContact.Contains("lrx_fundraisinmemberid"))
                    {
                        existingFundraisinMemberId = existingContact.GetAttributeValue<int?>("lrx_fundraisinmemberid");
                    }

                    if (existingFundraisinMemberId.HasValue)
                    {
                        contactFields.Remove("lrx_fundraisinmemberid");
                    }

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


            _tracingService.Trace($"Participant '{participant.MemberId}' processed successfully.");
            this._tracingService.Trace("Participant Record Fundraisin API Completed");
            return Task.CompletedTask;
        }

        
        public Task GetRegistrationFromParticipantEventRecord()
        {
            //string participantEventURL = baseURL + "participantsevents";
            //string csvContent = CallFundRaisinAPI((object)participantEventURL);

            //var participantEventList = ParseCsvHelper<ParticipantEventModel, ParticipantEventModelMap>(csvContent);
            //foreach (var participantEvent in participantEventList)
            //{
            ParticipantEventModel participantEvent = GetInputRecord<ParticipantEventModel>();

            if (participantEvent == null)
            {
                _tracingService.Trace("No Registration record received.");
                LogSkippedRecord("GetRegistrationFromParticipantEventRecord", "Not Found", "Registration input is null.", "Not record", null);
                return Task.CompletedTask;
            }
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
            Guid WaveID = Guid.Empty;
            Guid PromoCodeId = Guid.Empty;

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
                //continue;
                _tracingService.Trace("Contact or Event not found.");
                LogSkippedRecord(
       "GetRegistrationFromParticipantEventRecord",
       participantEvent.History_Id,
       $"Registration skipped. Contact Found: {contactID != Guid.Empty}, Event Found: {eventID != Guid.Empty}, Member ID: {participantEvent.Member_Id}, Event ID: {participantEvent.Event_Id}",
       participantEvent.Member_Id,
       ParseDate(participantEvent.Date_Created)
   );
                return Task.CompletedTask;
            }
            //Adding Ticket Record Logic
            _tracingService.Trace("Looking up participant ticket option. HistoryId={0}, MemberId={1}",
    participantEvent.History_Id, participantEvent.Member_Id);

            var ticketOption = GetParticipantTicketOption(participantEvent.History_Id, participantEvent.Member_Id);

            if (ticketOption == null)
            {
                _tracingService.Trace("No participantoptions ticket record found.");
            }
            else
            {
                _tracingService.Trace("Participant ticket option found. External TicketId={0}, OptionType={1}",
                    ticketOption.ticket_id, ticketOption.option_type);

                if (int.TryParse(ticketOption.ticket_id, out int externalTicketId))
                {
                    _tracingService.Trace("External TicketId parsed successfully: {0}", externalTicketId);

                    var ticketSearchConditions = new List<ConditionExpression>
        {
            new ConditionExpression("lrx_fundraisinticketid", ConditionOperator.Equal, externalTicketId)
        };

                    Entity existingTicket = FindExistingRecord("lrx_eventticket", ticketSearchConditions);

                    if (existingTicket != null)
                    {
                        TicketID = existingTicket.Id;
                        _tracingService.Trace("Dataverse ticket found. Ticket GUID={0}", TicketID);
                    }
                    else
                    {
                        _tracingService.Trace("No Dataverse ticket found for External TicketId={0}", externalTicketId);
                    }
                }
                else
                {
                    _tracingService.Trace("Could not parse External TicketId: {0}", ticketOption.ticket_id);
                }
            }
            //End Ticket record logic

            //Added Wave Lookup logic
            if (!string.IsNullOrWhiteSpace(ticketOption?.wave_id))
            {
                var waveSearchConditions = new List<ConditionExpression>
                {
                    new ConditionExpression("lrx_waveid", ConditionOperator.Equal, ticketOption.wave_id.Trim())
                };

                Entity existingWave = FindExistingRecord("lrx_waves", waveSearchConditions);

                if (existingWave != null)
                {
                    WaveID = existingWave.Id;
                    _tracingService.Trace("Wave found. Wave GUID={0}", WaveID);
                }
                else
                {
                    _tracingService.Trace("No Dataverse Wave found for WaveId={0}", ticketOption.wave_id);
                }
            }
            else
            {
                _tracingService.Trace("Wave Id is empty.");
            }
            //Wave Lookup logic ended
            //Get or Create Ticket
            decimal entryFeeRecord = decimal.Parse(participantEvent.Total_Paid_Entry.ToString());
            entreeAmount = entryFeeRecord;

            //Get Member / Contact who paid for registration
            if (NullIfMissing(participantEvent.Paid_Member_Id.Trim()) != null)
            {
                //var matchMemberID = participantEventList.FirstOrDefault(m => m.Member_Id.Trim() == participantEvent.Paid_Member_Id.Trim());
                //if (matchMemberID != null)
                //{
                /*TODO:
                    Verify the "Paid Member Registration" lookup logic for single - record processing.
                    Previously, this logic searched the entire participantEventList to find the

                    registration(History_Id) belonging to the member specified in Paid_Member_Id.


                    Since Power Automate now sends only one registration record at a time,
                    participantEventList is no longer available.Review this section to determine

                    the correct way to retrieve the paid member's registration (for example,

                    from the incoming JSON payload or by performing an additional lookup / API call),
                     and update the lrx_registrationpaidby and lrx_transaction mappings accordingly.*/
                var PaidMemberRegistrationSearchConditions = new List<ConditionExpression>
                        {
                            new ConditionExpression("lrx_fundraisinregistrationid", ConditionOperator.Equal, participantEvent.History_Id)
                        };
                Entity existingPaidRegistration = FindExistingRecord("lrx_registrations", PaidMemberRegistrationSearchConditions);

                if (existingPaidRegistration != null)
                {
                    paidMemberRegistration = existingPaidRegistration.Id;

                    if (existingPaidRegistration.Contains("lrx_transaction")
                        && existingPaidRegistration["lrx_transaction"] is EntityReference transactionRef)
                    {
                        TransactionID = transactionRef.Id;
                    }
                }

                var PaidMemberSearchConditions =
                    new List<ConditionExpression>
                {
                        new ConditionExpression("lrx_fundraisinmemberid",ConditionOperator.Equal,participantEvent.Paid_Member_Id)
                };

                Entity existingPaidMember =
                    FindExistingRecord("contact", PaidMemberSearchConditions);

                if (existingPaidMember != null)
                {
                    paidByMember = existingPaidMember.Id;
                }
            }
            //}

            var EventTeamSearchConditions = new List<ConditionExpression>
                {
                    new ConditionExpression("lrx_fundraisinteamid", ConditionOperator.Equal, participantEvent.Team_Id)
                };

            Entity existingEventTeam = FindExistingRecord("lrx_eventteam", EventTeamSearchConditions);
            if (existingEventTeam != null)
            {
                TeamId = existingEventTeam.Id;
            }

            //Added Promo Code Record Search
            int promoId = 0;
            if (int.TryParse(NullIfMissing(participantEvent.Promo_Id), out promoId) && promoId != 0)
            {
                var promoSearchConditions = new List<ConditionExpression>
            {
                new ConditionExpression("lrx_fundraisinpromoid", ConditionOperator.Equal, promoId)
            };

                Entity existingPromo = FindExistingRecord("lrx_promocodeanddiscount", promoSearchConditions);

                if (existingPromo != null)
                {
                    PromoCodeId = existingPromo.Id;
                    _tracingService.Trace("Promo Code found. Promo GUID={0}", PromoCodeId);
                }
                else
                {
                    _tracingService.Trace("No Promo Code found for Promo Id={0}", promoId);
                }
            }
            else
            {
                _tracingService.Trace("Promo Id is empty or 0.");
            }
            //Ended Promo Code Record Search

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
                ["lrx_wave"] = WaveID != Guid.Empty ? new EntityReference("lrx_waves", WaveID) : null,
                ["lrx_priceperregistration"] = new Money(entreeAmount),
                ["lrx_constituentorganization"] = new EntityReference("contact", contactID),
                ["lrx_eventteam"] = TeamId != Guid.Empty ? new EntityReference("lrx_eventteam", TeamId) : null,
                ["lrx_transaction"] = TransactionID != Guid.Empty ? new EntityReference("msnfp_transaction", TransactionID) : null,
                ["lrx_registeredby"] = paidByMember != Guid.Empty ? new EntityReference("contact", paidByMember) : new EntityReference("contact", contactID),
                ["lrx_registrationpaidby"] = paidMemberRegistration != Guid.Empty ? new EntityReference("lrx_registrations", paidMemberRegistration) : null,
                ["lrx_promoid"] = promoId != 0 ? promoId : (int?)null,
                ["lrx_promocode"] = PromoCodeId != Guid.Empty? new EntityReference("lrx_promocodeanddiscount", PromoCodeId): null,
                ["lrx_date"] = DateTime.Parse(participantEvent.Date_Created),
                ["lrx_fundraisinregistrationid"] = int.TryParse(participantEvent.History_Id, out int historyId) ? historyId : (int?)null,
                ["lrx_fundraisinggoalpledge"] = new Money(ParseDecimal(NullIfMissing(participantEvent.M_Target)))
            };

            if (participantEvent.Is_Paid != "Y")
            {
                entity["statuscode"] = new OptionSetValue(856660002);
            }
            else
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


            this._tracingService.Trace("Registration Record Fundraisin API Completed");
            return Task.CompletedTask;
        }

        public Task GetFundraisinTicketRecords()
        {
            //var ticketList = this.GetData<TicketsModel, TicketsModelMap>(this.baseURL, "tickets");

            //foreach (var tickets in ticketList)
            //{

            TicketsModel tickets = GetInputRecord<TicketsModel>();

            if (tickets == null)
            {
                _tracingService.Trace("No Ticket record received.");
                LogSkippedRecord("GetFundraisinTicketRecords", "Not Found", "Ticket input is null.", "Not Found Ticket", null);
                return Task.CompletedTask;
            }

            Guid eventID = Guid.Empty;
            Guid eventTicketID = Guid.Empty;
            var EventSearchConditions = new List<ConditionExpression>
                {
                    new ConditionExpression("lrx_fundraisineventid", ConditionOperator.Equal, tickets.event_id)
                };

            Entity existingEvent = FindExistingRecord("lrx_event", EventSearchConditions);
            if (existingEvent != null)
                eventID = (Guid)existingEvent.Id;

            if (eventID == Guid.Empty)
            {
                //continue;
                _tracingService.Trace($"Event not found : {tickets.event_id}");
                LogSkippedRecord("GetFundraisinTicketRecords", tickets.ticket_id, $"Related Event not found. Event ID: {tickets.event_id}", tickets.event_id, ParseDate(tickets.date_created));
                return Task.CompletedTask;
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
                eventTicketID = this._service.Create(ticketEntity);

                _tracingService.Trace(
                    "Event Ticket created successfully. GUID={0}",
                    eventTicketID);
            }
            else
            {
                ticketEntity.Id = existingTicket.Id;

                this._service.Update(ticketEntity);

                eventTicketID = existingTicket.Id;

                _tracingService.Trace(
                    "Event Ticket updated successfully. GUID={0}",
                    eventTicketID);
            }

            if (string.Equals(NullIfMissing(tickets.is_table), "Y", StringComparison.OrdinalIgnoreCase))
            {
                _tracingService.Trace("Ticket is a Table ticket. Processing Event Table.");

                Entity existingEventTable = FindExistingRecord(
                    "lrx_eventtable",
                    new List<ConditionExpression>
                    {
            new ConditionExpression(
                "lrx_eventid",
                ConditionOperator.Equal,
                NullIfMissing(tickets.event_id)),

            new ConditionExpression(
                "lrx_eventticketid",
                ConditionOperator.Equal,
                NullIfMissing(tickets.ticket_id))
                    });

                Entity eventTableEntity = new Entity("lrx_eventtable")
                {
                    ["lrx_name"] = NullIfMissing(tickets.ticket_name),

                    ["lrx_tablecapacity"] =
                        int.TryParse(NullIfMissing(tickets.num_tickets), out int tableCapacity)
                            ? tableCapacity
                            : 0,

                    ["lrx_pricepertable"] =
                        decimal.TryParse(NullIfMissing(tickets.ticket_price), out decimal tablePrice)
                            ? new Money(tablePrice)
                            : new Money(0),

                    ["lrx_date"] =
                        ParseDate(tickets.date_created) ?? DateTime.Now,

                    // Lookup fields
                    ["lrx_event"] =
                        new EntityReference("lrx_event", eventID),

                    ["lrx_eventticket"] =
                        new EntityReference("lrx_eventticket", eventTicketID),

                    // Text fields (Fundraisin IDs)
                    ["lrx_eventid"] =
                        NullIfMissing(tickets.event_id),

                    ["lrx_eventticketid"] =
                        NullIfMissing(tickets.ticket_id)
                };

                if (int.TryParse(NullIfMissing(tickets.ticket_code), out int tableNumber))
                {
                    eventTableEntity["lrx_tablenumber"] = tableNumber;
                }

                if (existingEventTable == null)
                {
                    Guid eventTableId = _service.Create(eventTableEntity);

                    _tracingService.Trace(
                        "Event Table created successfully. GUID={0}",
                        eventTableId);
                }
                else
                {
                    eventTableEntity.Id = existingEventTable.Id;

                    _service.Update(eventTableEntity);

                    _tracingService.Trace(
                        "Event Table updated successfully. GUID={0}",
                        existingEventTable.Id);
                }
            }
            else
            {
                _tracingService.Trace("Ticket is not a Table ticket. Event Table creation skipped.");
            }


            this._tracingService.Trace("Ticket Record Fundraisin API Completed");
            return Task.CompletedTask;
        }

        public Task GetFundraisinTicketHolderRecord()
        {
            //foreach (TicketHolderModel ticketHolder in this.ParseCsvHelper<TicketHolderModel, TicketHolderModelMap>(this.CallFundRaisinAPI((object)(this.baseURL + "ticketholders"))))
            //{
            TicketHolderModel ticketHolder = GetInputRecord<TicketHolderModel>();

            if (ticketHolder == null)
            {
                _tracingService.Trace("No Ticket Holder record received.");
                LogSkippedRecord("GetFundraisinTicketHolderRecord", "Not Found", "Ticket Holder input is null.", "Not Found", null);
                return Task.CompletedTask;
            }
            Guid eventId = Guid.Empty;
            Guid registrationId = Guid.Empty;
            Guid relatedRegistrationId = Guid.Empty;
            Guid ticketId = Guid.Empty;
            Guid TransactionID = Guid.Empty;
            Guid contactID = Guid.Empty;
            Guid eventTable = Guid.Empty;
            Guid waveId = Guid.Empty;

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

            //Added logic to check Wave
            if (!string.IsNullOrWhiteSpace(NullIfMissing(ticketHolder.wave_id)))
            {
                Entity existingWave = FindExistingRecord("lrx_waves",
                    new List<ConditionExpression>
                    {
            new ConditionExpression("lrx_waveid", ConditionOperator.Equal, ticketHolder.wave_id.Trim())
                    });

                if (existingWave != null)
                {
                    waveId = existingWave.Id;
                }
            }
            //Ended Wave

            Entity existingRegistration = null;
            if (int.TryParse(NullIfMissing(ticketHolder.history_id), out int historyIdValue))
            {
                existingRegistration = FindExistingRecord("lrx_registrations",
                    new List<ConditionExpression>
                    {
            new ConditionExpression("lrx_fundraisinregistrationid", ConditionOperator.Equal, historyIdValue)
                    });
            }

            if (existingRegistration != null)
            {
                registrationId = existingRegistration.Id;
                if (existingRegistration.Attributes.TryGetValue("lrx_transaction", out var transactionObj) &&
                transactionObj is EntityReference transactionRef)
                {
                    TransactionID = transactionRef.Id;
                }
            }

            //var MemberSearchConditions = new List<ConditionExpression>
            //    {
            //        new ConditionExpression("lrx_fundraisinmemberid", ConditionOperator.Equal, ticketHolder.member_id)
            //    };

            //Entity existingMember = FindExistingRecord("contact", MemberSearchConditions);
            //if (existingMember != null)
            //{
            //    contactID = (Guid)existingMember.Id;
            //}
            Entity existingMember = null;

            if (HasValue(ticketHolder.member_id))
            {
                existingMember = FindExistingRecord(
                    "contact",
                    new List<ConditionExpression>
                    {
            new ConditionExpression(
                "lrx_fundraisinmemberid",
                ConditionOperator.Equal,
                ticketHolder.member_id.Trim())
                    });
            }

            if (existingMember == null)
            {
                existingMember = FindContactByDuplicateRules(
                    ticketHolder.g_fname,
                    ticketHolder.g_lname,
                    ticketHolder.g_email,                   
                    NullIfMissing(ticketHolder.g_phone),
                    ParseDate(ticketHolder.g_dob));

                if (existingMember != null)
                {
                    Entity fullContact = _service.Retrieve(
                        "contact",
                        existingMember.Id,
                        new ColumnSet(
                            "firstname",
                            "lastname",
                            "emailaddress1",
                            "telephone1",
                            "mobilephone",
                            "address1_line1",
                            "address1_city",
                            "address1_postalcode",
                            "address1_stateorprovince",
                            "address1_country",
                             "lrx_fundraisinmemberid"));

                    Entity updateContact = new Entity("contact", existingMember.Id);
                    if (!fullContact.Contains("lrx_fundraisinmemberid") &&
                    int.TryParse(NullIfMissing(ticketHolder.member_id), out int memberIdValue))
                    {
                        updateContact["lrx_fundraisinmemberid"] = memberIdValue;
                    }

                    if (!fullContact.Contains("firstname") && HasValue(ticketHolder.g_fname))
                        updateContact["firstname"] = ticketHolder.g_fname.Trim();

                    if (!fullContact.Contains("lastname") && HasValue(ticketHolder.g_lname))
                        updateContact["lastname"] = ticketHolder.g_lname.Trim();

                    if (!fullContact.Contains("emailaddress1") && HasValue(ticketHolder.g_email))
                        updateContact["emailaddress1"] = ticketHolder.g_email.Trim();

                    string phone = NullIfMissing(ticketHolder.g_phone);

                    if (!fullContact.Contains("telephone1") && HasValue(phone))
                        updateContact["telephone1"] = phone;

                    if (!fullContact.Contains("mobilephone") && HasValue(phone))
                        updateContact["mobilephone"] = phone;
                    string addressLine =
    (NullIfMissing(ticketHolder.g_address_unit) ?? "") +
    (NullIfMissing(ticketHolder.g_address_street) ?? "");

                    if (!fullContact.Contains("address1_line1") &&
                        HasValue(addressLine))
                    {
                        updateContact["address1_line1"] = addressLine;
                    }

                    if (!fullContact.Contains("address1_city") &&
                        HasValue(ticketHolder.g_address_suburb))
                    {
                        updateContact["address1_city"] =
                            ticketHolder.g_address_suburb.Trim();
                    }

                    if (!fullContact.Contains("address1_postalcode") &&
                        HasValue(ticketHolder.g_address_pcode))
                    {
                        updateContact["address1_postalcode"] =
                            ticketHolder.g_address_pcode.Trim();
                    }

                    if (!fullContact.Contains("address1_stateorprovince") &&
                        HasValue(ticketHolder.g_address_state))
                    {
                        updateContact["address1_stateorprovince"] =
                            ticketHolder.g_address_state.Trim();
                    }

                    if (!fullContact.Contains("address1_country") &&
                        HasValue(ticketHolder.g_address_country))
                    {
                        updateContact["address1_country"] =
                            ticketHolder.g_address_country.Trim();
                    }

                    if (updateContact.Attributes.Count > 0)
                        _service.Update(updateContact);

                    contactID = existingMember.Id;
                }
                else
                {
                    Entity newContact = new Entity("contact")
                    {
                        ["firstname"] = NullIfMissing(ticketHolder.g_fname),
                        ["lastname"] = NullIfMissing(ticketHolder.g_lname),
                        ["emailaddress1"] = NullIfMissing(ticketHolder.g_email),
                        ["telephone1"] = NullIfMissing(ticketHolder.g_phone),
                        ["mobilephone"] = NullIfMissing(ticketHolder.g_phone)
                    };
                    if (int.TryParse(NullIfMissing(ticketHolder.member_id), out int memberIdValue))
                    {
                        newContact["lrx_fundraisinmemberid"] = memberIdValue;
                    }

                    contactID = _service.Create(newContact);
                }
            }
            else
            {
                contactID = existingMember.Id;
            }
            //Added by samir on 20 july 2026
            string relatedMemberId = NullIfMissing(ticketHolder.related_member_id);
            string relatedHistoryId = NullIfMissing(ticketHolder.related_history_id);
            string memberId = NullIfMissing(ticketHolder.member_id);
            string historyId = NullIfMissing(ticketHolder.history_id);

            bool isPrimaryAttendee =
     relatedMemberId != null &&
     relatedHistoryId != null &&
     memberId != null &&
     historyId != null &&
     string.Equals(relatedMemberId, memberId, StringComparison.Ordinal) &&
     string.Equals(relatedHistoryId, historyId, StringComparison.Ordinal);

            bool isChildAttendee = !string.IsNullOrWhiteSpace(NullIfMissing(ticketHolder.guest_id)) && !isPrimaryAttendee;
            if (eventId == Guid.Empty)
            {
                _tracingService.Trace("Event not found.");
                LogSkippedRecord(
                    "GetFundraisinTicketHolderRecord",
                    ticketHolder.history_id,
                    $"Event Found: {eventId != Guid.Empty}, Registration Found: {registrationId != Guid.Empty}, Event ID: {ticketHolder.event_id}, Registration ID: {ticketHolder.history_id}",
                    ticketHolder.ticket_id,
                    ParseDate(ticketHolder.date_created)
                );
                return Task.CompletedTask;
            }

            // TODO:
            // Review this helper for optimization.
            // It currently retrieves all tables for the event and then filters by table_id.
            // If Fundraisin provides an endpoint to retrieve a single table,
            // update this method to avoid downloading the full event table list.
            if (NullIfMissing(ticketHolder.table_id) != null)
            {
                eventTable = GetFundraisinTableRecord(ticketHolder.event_id, ticketHolder.table_id, eventId, ticketId);
            }

            // Create parent registration when missing and current row is the primary attendee
            if (registrationId == Guid.Empty && !isChildAttendee)
            {
                string contactName =
                string.Join(" ",
                 new[]
                {
                     NullIfMissing(ticketHolder.g_fname),
                    NullIfMissing(ticketHolder.g_lname)
                }.Where(x => !string.IsNullOrWhiteSpace(x)));

                string primaryRegistrationName =
                    !string.IsNullOrWhiteSpace(contactName) &&
                    !string.IsNullOrWhiteSpace(EventName)
                        ? contactName + " - " + EventName
                        : "Fundraisin Registration - " + ticketHolder.history_id;

                Entity parentRegistrationEntity = new Entity("lrx_registrations")
                {
                    //["lrx_fundraisinregistrationid"] = ticketHolder.history_id,
                    ["lrx_event"] = new EntityReference("lrx_event", eventId),
                    ["lrx_name"] = primaryRegistrationName,
                    ["lrx_eventticket"] = ticketId != Guid.Empty ? new EntityReference("lrx_eventticket", ticketId) : null,
                    ["lrx_eventtable"] = eventTable != Guid.Empty ? new EntityReference("lrx_eventtable", eventTable) : null,
                    ["lrx_transaction"] = TransactionID != Guid.Empty ? new EntityReference("msnfp_transaction", TransactionID) : null,
                    ["lrx_registeredby"] = contactID != Guid.Empty ? new EntityReference("contact", contactID) : null,
                    ["lrx_constituentorganization"] = contactID != Guid.Empty ? new EntityReference("contact", contactID) : null,
                    ["lrx_date"] = ParseDate(ticketHolder.date_created) ?? DateTime.Now,
                    ["lrx_registrationpaidby"] = null,

                    ["lrx_emergencycontact"] = NullIfMissing(ticketHolder.g_emergency_contact),

                    ["lrx_emergencycontactnumber"] = NullIfMissing(ticketHolder.g_emergency_phone),

                    ["lrx_emergencycontacttype"] = NullIfMissing(ticketHolder.g_emergency_contact_type),

                    ["lrx_guardianname"] = string.Join(" ",
                    new[]
                    {
                        NullIfMissing(ticketHolder.g_guardian_fname),
                        NullIfMissing(ticketHolder.g_guardian_lname)
                    }.Where(x => !string.IsNullOrWhiteSpace(x))),

                    ["lrx_guardianphone"] = NullIfMissing(ticketHolder.g_guardian_phone),

                    ["lrx_guardianemail"] = NullIfMissing(ticketHolder.g_guardian_email),

                    ["lrx_guardianrelationship"] = NullIfMissing(ticketHolder.g_guardian_relationship)


                };


                if (int.TryParse(NullIfMissing(ticketHolder.history_id), out int fundraisinRegistrationId))
                {
                    parentRegistrationEntity["lrx_fundraisinregistrationid"] = fundraisinRegistrationId;
                }
                registrationId = this._service.Create(parentRegistrationEntity);
            }

            // Child row cannot proceed until parent registration exists
            if (registrationId == Guid.Empty && isChildAttendee)
            {
                string parentHistoryId = NullIfMissing(ticketHolder.related_history_id);

                if (string.IsNullOrWhiteSpace(parentHistoryId) || parentHistoryId == "0")
                {
                    parentHistoryId = NullIfMissing(ticketHolder.history_id);
                }

                if (int.TryParse(parentHistoryId, out int parentHistoryIdValue))
                {
                    Entity parentRegistration = FindExistingRecord("lrx_registrations",
                        new List<ConditionExpression>
                        {
                new ConditionExpression("lrx_fundraisinregistrationid", ConditionOperator.Equal, parentHistoryIdValue)
                        });

                    if (parentRegistration != null)
                    {
                        registrationId = parentRegistration.Id;

                        if (parentRegistration.Attributes.TryGetValue("lrx_transaction", out var parentTransactionObj) &&
                            parentTransactionObj is EntityReference parentTransactionRef)
                        {
                            TransactionID = parentTransactionRef.Id;
                        }
                    }
                }

                if (registrationId == Guid.Empty)
                {
                    _tracingService.Trace("Parent registration not found for child ticket holder.");
                    LogSkippedRecord(
                        "GetFundraisinTicketHolderRecord",
                        ticketHolder.history_id,
                        $"Child ticket holder skipped because parent registration is missing. Event ID: {ticketHolder.event_id}, Parent Registration ID: {parentHistoryId}",
                        ticketHolder.ticket_id,
                        ParseDate(ticketHolder.date_created)
                    );
                    return Task.CompletedTask;
                }
            }

            if (isChildAttendee)
            {
                // string ContactFullName = ticketHolder.g_fname + " " + ticketHolder.g_lname;
                //Entity existingGuest = this.FindExistingRecord("contact", new List<ConditionExpression>()
                //    {
                //        new ConditionExpression("lrx_fundraisinguestid", ConditionOperator.Equal, (object)ticketHolder.guest_id)
                //    });
                Entity existingGuest = null;
                int memberIdValue;
                if (int.TryParse(NullIfMissing(ticketHolder.member_id), out memberIdValue))
                {
                    existingGuest = FindExistingRecord(
                        "contact",
                        new List<ConditionExpression>
                        {
            new ConditionExpression(
                "lrx_fundraisinmemberid",
                ConditionOperator.Equal,
                memberIdValue)
                        });
                }

                if (existingGuest == null)
                {
                    existingGuest = FindContactByDuplicateRules(
                        NullIfMissing(ticketHolder.g_fname),
                        NullIfMissing(ticketHolder.g_lname),
                        NullIfMissing(ticketHolder.g_email),
                       NullIfMissing(ticketHolder.g_phone),
                        ParseDate(ticketHolder.g_dob));
                }

                //int guestIdValue;
                Entity guestEntity = new Entity("contact")
                {
                    ["firstname"] = (object)NullIfMissing(ticketHolder.g_fname),
                    ["lastname"] = (object)NullIfMissing(ticketHolder.g_lname),
                    ["emailaddress1"] = (object)NullIfMissing(ticketHolder.g_email),
                    ["telephone1"] = (object)(NullIfMissing(ticketHolder.g_phone)),
                    ["mobilephone"] = (object)(NullIfMissing(ticketHolder.g_phone)),
                    ["address1_line1"] = (object)(NullIfMissing(ticketHolder.g_address_unit) + NullIfMissing(ticketHolder.g_address_street)),
                    ["address1_city"] = (object)NullIfMissing(ticketHolder.g_address_suburb),
                    ["address1_postalcode"] = (object)NullIfMissing(ticketHolder.g_address_pcode),
                    ["address1_stateorprovince"] = (object)NullIfMissing(ticketHolder.g_address_state),
                    ["address1_country"] = (object)NullIfMissing(ticketHolder.g_address_country),

                    // ["lrx_fundraisinguestid"] = (object)(int.TryParse(ticketHolder.guest_id, out guestIdValue) ? guestIdValue : 0)
                };
                if (memberIdValue > 0)
                {
                    guestEntity["lrx_fundraisinmemberid"] = memberIdValue;
                }
                //var cleanedGuestId = NullIfMissing(ticketHolder.guest_id);
                //if (cleanedGuestId != null && int.TryParse(cleanedGuestId, out int guestIdValue))
                //{
                //    guestEntity["lrx_fundraisinguestid"] = guestIdValue;
                //}

                Guid guestId;
                if (existingGuest != null)
                {
                    guestId = existingGuest.Id;

                    Entity fullContact = _service.Retrieve(
                        "contact",
                        guestId,
                        new ColumnSet(
                            "firstname",
                            "lastname",
                            "emailaddress1",
                            "telephone1",
                            "mobilephone",
                            "address1_line1",
                            "address1_city",
                            "address1_postalcode",
                            "address1_stateorprovince",
                            "address1_country",
                            "lrx_fundraisinmemberid"));

                    Entity updateGuest = new Entity("contact", guestId);
                    if (!fullContact.Contains("lrx_fundraisinmemberid") &&
                    int.TryParse(NullIfMissing(ticketHolder.member_id), out  int guestmemberIdValue))
                    {
                        updateGuest["lrx_fundraisinmemberid"] = guestmemberIdValue;
                    }
                    if (!fullContact.Contains("firstname") && HasValue(ticketHolder.g_fname))
                        updateGuest["firstname"] = ticketHolder.g_fname.Trim();

                    if (!fullContact.Contains("lastname") && HasValue(ticketHolder.g_lname))
                        updateGuest["lastname"] = ticketHolder.g_lname.Trim();

                    if (!fullContact.Contains("emailaddress1") && HasValue(ticketHolder.g_email))
                        updateGuest["emailaddress1"] = ticketHolder.g_email.Trim();

                    string phone = NullIfMissing(ticketHolder.g_phone);

                    if (!fullContact.Contains("telephone1") && HasValue(phone))
                        updateGuest["telephone1"] = phone;

                    if (!fullContact.Contains("mobilephone") && HasValue(phone))
                        updateGuest["mobilephone"] = phone;
                    string addressLine =
    (NullIfMissing(ticketHolder.g_address_unit) ?? "") +
    (NullIfMissing(ticketHolder.g_address_street) ?? "");

                    if (!fullContact.Contains("address1_line1") &&
                        HasValue(addressLine))
                    {
                        updateGuest["address1_line1"] = addressLine;
                    }

                    if (!fullContact.Contains("address1_city") &&
                        HasValue(ticketHolder.g_address_suburb))
                    {
                        updateGuest["address1_city"] =
                            ticketHolder.g_address_suburb.Trim();
                    }

                    if (!fullContact.Contains("address1_postalcode") &&
                        HasValue(ticketHolder.g_address_pcode))
                    {
                        updateGuest["address1_postalcode"] =
                            ticketHolder.g_address_pcode.Trim();
                    }

                    if (!fullContact.Contains("address1_stateorprovince") &&
                        HasValue(ticketHolder.g_address_state))
                    {
                        updateGuest["address1_stateorprovince"] =
                            ticketHolder.g_address_state.Trim();
                    }

                    if (!fullContact.Contains("address1_country") &&
                        HasValue(ticketHolder.g_address_country))
                    {
                        updateGuest["address1_country"] =
                            ticketHolder.g_address_country.Trim();
                    }

                    if (updateGuest.Attributes.Count > 0)
                        _service.Update(updateGuest);
                }
                else
                {
                    //Entity matchingGuest = this.FindExistingRecord("contact", new List<ConditionExpression>()
                    //    {
                    //        new ConditionExpression("firstname", ConditionOperator.Equal, (object)ticketHolder.g_fname),
                    //        new ConditionExpression("lastname", ConditionOperator.Equal, (object)ticketHolder.g_lname),
                    //        new ConditionExpression("emailaddress1", ConditionOperator.Equal, (object)ticketHolder.g_email)
                    //    });

                    //if (matchingGuest != null)
                    //{
                    //    guestId = matchingGuest.Id;
                    //    guestEntity.Id = matchingGuest.Id;
                    //    this._service.Update(guestEntity);
                    //}
                    //else
                    //{
                    guestId = this._service.Create(guestEntity);
                    //}
                }
                UpsertTicketHolderRecord(guestId, eventId, registrationId, waveId, ticketHolder);
                //var RegistrationSearchConditions = new List<ConditionExpression>
                //    {
                //        new ConditionExpression("lrx_constituentorganization", ConditionOperator.Equal, guestId),
                //        new ConditionExpression("lrx_event", ConditionOperator.Equal, eventId)
                //    };
                //string identifierName = ContactFullName + " - " + EventName;
                //Entity existingTicketRegistration = FindExistingRecord("lrx_registrations", RegistrationSearchConditions);
                //var registrationEntity = new Entity("lrx_registrations")
                //{
                //    ["lrx_event"] = new EntityReference("lrx_event", eventId),
                //    ["lrx_name"] = identifierName,
                //    ["lrx_eventticket"] = ticketId != Guid.Empty ? new EntityReference("lrx_eventticket", ticketId) : null,
                //    ["lrx_eventtable"] = eventTable != Guid.Empty ? new EntityReference("lrx_eventtable", eventTable) : null,
                //    ["lrx_priceperregistration"] = new Money(0),
                //    ["lrx_constituentorganization"] = new EntityReference("contact", guestId),
                //    ["lrx_transaction"] = TransactionID != Guid.Empty ? new EntityReference("msnfp_transaction", TransactionID) : null,
                //    ["lrx_registeredby"] = contactID != Guid.Empty ? new EntityReference("contact", contactID) : null,
                //    ["lrx_date"] = DateTime.Parse(ticketHolder.date_created),
                //    ["lrx_registrationpaidby"] = registrationId != Guid.Empty ? new EntityReference("lrx_registrations", registrationId) : null
                //};

                //if (existingTicketRegistration == null)
                //{
                //    registrationEntity.Id = this._service.Create(registrationEntity);
                //}
                //else
                //{
                //    registrationEntity.Id = existingTicketRegistration.Id;
                //    this._service.Update(registrationEntity);
                //}
            }
            else
            {
                if (ticketId != Guid.Empty)
                {
                    //if (ticketHolder.history_id.Trim() != ticketHolder.related_history_id.Trim())
                    if (!string.Equals(
        NullIfMissing(ticketHolder.history_id),
        NullIfMissing(ticketHolder.related_history_id),
        StringComparison.Ordinal))
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
                                ["lrx_wave"] = waveId != Guid.Empty ? (object)new EntityReference("lrx_waves", waveId) : null,
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
                            ["lrx_wave"] = waveId != Guid.Empty ? (object)new EntityReference("lrx_waves", waveId) : (object)null,
                            ["lrx_registrationpaidby"] = (object)null //do not reference self as paid by self
                        });
                    }
                }
            }


            this._tracingService.Trace("Ticket Holder Record Fundraising API Completed", Array.Empty<object>());
            return Task.CompletedTask;
        }

        public Task GetFundRaisinProductRecord()
        {
            //var productList = this.GetData<ProductModel, ProductModelMap>(this.baseURL, "products");
            //foreach (var products in productList)
            //{
            ProductModel products = GetInputRecord<ProductModel>();

            if (products == null)
            {
                _tracingService.Trace("No Product record received.");
                LogSkippedRecord("GetFundRaisinProductRecord", "Unknown", "Product input is null.", "not found", null);
                return Task.CompletedTask;
            }
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


            this._tracingService.Trace("Product Record Fundraisin API Completed");
            return Task.CompletedTask;
        }

        public Task GetFundRaisinProductOptionsRecord()
        {
            ProductOptionModel productoptions = GetInputRecord<ProductOptionModel>();

            if (productoptions == null)
            {
                _tracingService.Trace("No Product Option record received.");
                LogSkippedRecord("GetFundRaisinProductOptionsRecord", "Not Found", "Product Option input is null.", "not found", null);
                return Task.CompletedTask;
            }
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
            if (existingProduct == null)
            {
                _tracingService.Trace("Related Product not found.");
                LogSkippedRecord(
        "GetFundRaisinProductOptionsRecord",
        productoptions.option_id,
        $"Related Product not found. ProductId: {productoptions.product_id}",
        productoptions.option_name,
        ParseDate(productoptions.date_created)
    );
                return Task.CompletedTask;
            }

            //Guid productID = (Guid)existingProduct.Id;
            Guid productID = existingProduct.Id;
            var ProductOptionSearchConditions = new List<ConditionExpression>
            {
                    new ConditionExpression("lrx_inventoryproduct", ConditionOperator.Equal, existingProduct.Id),
                    new ConditionExpression("lrx_fundraisinoptionid",ConditionOperator.Equal,int.Parse(productoptions.option_id))
            };
            Entity existingProductOption = FindExistingRecord("lrx_productoptions", ProductOptionSearchConditions);

            if (existingProductOption == null)
            {
                this._service.Create(new Entity("lrx_productoptions")
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


            this._tracingService.Trace("Product Option Record Fundraisin API Completed");
            return Task.CompletedTask;
        }

        public Task GetFundraisinProductSalesItem()
        {
            //var saleItemList = this.GetData<SaleItemModel, SaleItemModelMap>(this.baseURL, "salesitems");
            //var productList = this.GetAllData<ProductModel, ProductModelMap>(this.baseURL, "products");
            //var productOptionList = this.GetAllData<ProductOptionModel, ProductOptionModelMap>(this.baseURL, "productoptions");
            //string previousSaleID = string.Empty;
            //Guid contactID = Guid.Empty;
            //saleItemList = saleItemList.OrderBy(x => x.sale_id).ToList();

            //foreach (var saleitem in saleItemList)
            //{
            SaleItemModel saleitem = GetInputRecord<SaleItemModel>();

            if (saleitem == null)
            {
                _tracingService.Trace("No Product Sales record received.");
                LogSkippedRecord("GetFundraisinProductSalesItem", "Unknown", "Product Sales input is null.", "Not Found", null);
                return Task.CompletedTask;
            }

            Guid contactID = Guid.Empty;

            Guid productID = Guid.Empty;
            Guid productOption = Guid.Empty;

            string contactFullName = string.Empty;
            decimal GSTamount = 0;
            string productName = "";
            string productOptionName = "";

            //var currentSaleID = saleitem.sale_id.Trim();

            //if (!string.Equals(previousSaleID, currentSaleID, StringComparison.Ordinal))
            //{
            //    contactID = UpsertContactFromSales(currentSaleID, out contactFullName, out GSTamount);
            //    previousSaleID = currentSaleID;
            //}
            // TODO:
            // Remove the API call once the Sales record is available from Power Automate.
            // Instead of downloading all Sales and searching by Sale ID,
            // retrieve the matching Sales record directly from the input payload.
            contactID = UpsertContactFromSales(saleitem.sale_id.Trim(), out contactFullName, out GSTamount);
            if (contactID == Guid.Empty)
            {
                _tracingService.Trace("Unable to create/find Contact for Sale ID: " + saleitem.sale_id);
                LogSkippedRecord("GetFundraisinProductSalesItem", saleitem.sale_id, "Contact not found for Sale ID.", saleitem.recipient_email, ParseDate(saleitem.date_created));
                return Task.CompletedTask;
            }

            //var matchingProduct = productList.FirstOrDefault(p => p.product_id.Trim() == saleitem.product_id.Trim());

            //    if (matchingProduct != null)
            //    {
            //        productName = matchingProduct.product_name;
            //    }
            var productSearchConditions = new List<ConditionExpression>
            {
                    new ConditionExpression("lrx_fundraisinproductid", ConditionOperator.Equal, saleitem.product_id)
            };

            Entity existingInventoryProduct =
                FindExistingRecord("lrx_inventoryproduct", productSearchConditions);

            if (existingInventoryProduct == null)
            {
                _tracingService.Trace("Related Product not found.");
                LogSkippedRecord("GetFundraisinProductSalesItem", saleitem.id, "Related Product not found.", saleitem.product_id, ParseDate(saleitem.date_created));
                return Task.CompletedTask;
            }

            productID = existingInventoryProduct.Id;

            if (existingInventoryProduct.Contains("lrx_name"))
            {
                productName = existingInventoryProduct["lrx_name"].ToString();
            }

            //var matchingProductOption = productOptionList.FirstOrDefault(p => p.product_id.Trim() == saleitem.product_id.Trim());
            //    if (matchingProductOption != null)
            //    {
            //        productOptionName = matchingProductOption.option_name;
            //    }
            var productOptionSearchConditions = new List<ConditionExpression>
            {
                    new ConditionExpression("lrx_fundraisinoptionid", ConditionOperator.Equal, saleitem.option_id)
            };

            Entity existingProductOption =
                FindExistingRecord("lrx_productoptions", productOptionSearchConditions);

            if (existingProductOption != null)
            {
                productOption = existingProductOption.Id;

                if (existingProductOption.Contains("lrx_name"))
                {
                    productOptionName = existingProductOption["lrx_name"].ToString();
                }
            }

            //var productSearchConditions = new List<ConditionExpression>
            //    {
            //        new ConditionExpression("lrx_fundraisinproductid", ConditionOperator.Equal, saleitem.product_id)
            //    };
            //    Entity existingInventoryProduct = FindExistingRecord("lrx_inventoryproduct", productSearchConditions);

            //    if (existingInventoryProduct != null)
            //    {
            //        productID = existingInventoryProduct.Id;
            //    }
            //    else
            //    {
            //        continue;
            //    }

            //var productOptionSearchConditions = new List<ConditionExpression>
            //{
            //    new ConditionExpression("lrx_fundraisinoptionid", ConditionOperator.Equal, saleitem.option_id)
            //};
            //Entity existingProductOption = FindExistingRecord("lrx_productoptions", productOptionSearchConditions);

            //if (existingProductOption != null)
            //{
            //    productOption = existingProductOption.Id;
            //}

            var saleProduct = new Entity("lrx_product")
            {
                ["lrx_name"] = $"{productName} - {productOptionName}",
                ["lrx_productname"] = $"{productName} - {productOptionName}",
                ["lrx_constituentorganisation"] = new EntityReference("contact", contactID),
                ["lrx_date"] = DateTime.Parse(saleitem.date_created),
                //["lrx_productmigrationid"] = saleitem.id.Trim(),
                //["lrx_fundraisinsalesid"] = int.Parse(saleitem.sale_id.Trim())
                ["lrx_fundraisinsalesid"] = int.Parse(saleitem.id.Trim())
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
                        "lrx_fundraisinsalesid",
                        ConditionOperator.Equal,
                        int.Parse(saleitem.id.Trim())
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


            _tracingService.Trace("Product Sales Record Fundraisin API Completed");
            return Task.CompletedTask;
        }

        public Task GetFundRaisinEventTeamsRecord()
        {
            //string url = baseURL + "teams";
            //string csvContent = CallFundRaisinAPI((object)url);

            //var EventTeamList = ParseCsvHelper<EventTeamModel, EventTeamModelMap>(csvContent);
            //foreach (var eventTeams in EventTeamList)
            //{
            EventTeamModel eventTeams = GetInputRecord<EventTeamModel>();

            if (eventTeams == null)
            {
                _tracingService.Trace("No Event Team record received.");
                LogSkippedRecord("GetFundRaisinEventTeamsRecord", "", "Event Team input is null.", "not found", null);
                return Task.CompletedTask;
            }
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
                //continue;
                _tracingService.Trace($"Event not found : {eventTeams.event_id}");
                LogSkippedRecord("GetFundRaisinEventTeamsRecord", eventTeams.team_id, $"Related Event not found. Event ID: {eventTeams.event_id}", eventTeams.t_name, ParseDate(eventTeams.date_created));
                return Task.CompletedTask;
            }

            Guid contactID = Guid.Empty;
            var ContactSearchConditions = new List<ConditionExpression>
                {
                    new ConditionExpression("lrx_fundraisinmemberid", ConditionOperator.Equal, eventTeams.captain_id)
                };

            Entity existingContact = FindExistingRecord("contact", ContactSearchConditions);

            if (existingContact != null)
                contactID = (Guid)existingContact.Id;

            var EventTeamSearchConditions = new List<ConditionExpression>
                {
                    new ConditionExpression("lrx_fundraisinteamid", ConditionOperator.Equal, eventTeams.team_id)
                };

            Entity existingEventTeam = FindExistingRecord("lrx_eventteam", EventTeamSearchConditions);
            if (existingEventTeam == null)
            {
                Entity eventTeam = new Entity("lrx_eventteam")
                {
                    ["lrx_name"] = eventTeams.t_name,
                    ["lrx_dateregistered"] = DateTime.Parse(eventTeams.date_created).ToString("dd/MM/yyyy"),
                    ["lrx_fundraisinggoalpledge"] = new Money(decimal.Parse(eventTeams.t_target)),
                    ["lrx_teamdescription"] = eventTeams.t_page_title,
                    ["lrx_event"] = new EntityReference("lrx_event", eventID),
                    ["lrx_fundraisinteamid"] = int.Parse(eventTeams.team_id)
                };

                if (contactID != Guid.Empty)
                {
                    eventTeam["lrx_registeredby"] = new EntityReference("contact", contactID);
                }

                Guid eventTeamID = this._service.Create(eventTeam);
            }
            else
            {
                Entity eventTeam = new Entity("lrx_eventteam", existingEventTeam.Id)
                {
                    ["lrx_name"] = eventTeams.t_name,
                    ["lrx_dateregistered"] = DateTime.Parse(eventTeams.date_created).ToString("dd/MM/yyyy"),
                    ["lrx_fundraisinggoalpledge"] = new Money(decimal.Parse(eventTeams.t_target)),
                    ["lrx_teamdescription"] = eventTeams.t_page_title,
                    ["lrx_event"] = new EntityReference("lrx_event", eventID),
                    ["lrx_fundraisinteamid"] = int.Parse(eventTeams.team_id)
                };

                if (contactID != Guid.Empty)
                {
                    eventTeam["lrx_registeredby"] = new EntityReference("contact", contactID);
                }

                this._service.Update(eventTeam);
            }


            this._tracingService.Trace("Event Team Record Fundraisin API Completed");
            return Task.CompletedTask;
        }

        public Task GetFundRaisinOrganisationRecord()
        {
            //string url = baseURL + "orgpages";
            //string csvContent = CallFundRaisinAPI((object)url);

            //var OrganisationList = ParseCsvHelper<OrganisationModel, OrganisationModelMap>(csvContent);
            //foreach (var organisations in OrganisationList)
            //{
            OrganisationModel organisations = GetInputRecord<OrganisationModel>();

            if (organisations == null)
            {
                _tracingService.Trace("Organisation record is null.");
                LogSkippedRecord("GetFundRaisinOrganisationRecord", "Not Found", "Organisation input is null.", "not found", null);
                return Task.CompletedTask;
            }
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
                //continue;
                _tracingService.Trace(
        $"Skipping Organisation. Contact not found. " +
        $"Organisation ID: {organisations.org_id}, " +
        $"Organisation Name: {organisations.org_name}, " +
        $"Created Member ID: {organisations.created_member_id}");
                LogSkippedRecord("GetFundRaisinOrganisationRecord", organisations.org_id, $"Contact not found. Created Member ID: {organisations.created_member_id}", organisations.org_name, ParseDate(organisations.date_created));
                return Task.CompletedTask;
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


            this._tracingService.Trace("Organisation Record Fundraisin API Completed");
            return Task.CompletedTask;
        }

        public Task GetFundRaisinPromoCodeRecord()
        {
            //string url = baseURL + "promocodes";
            //string csvContent = CallFundRaisinAPI((object)url);

            //var PromoList = ParseCsvHelper<PromoCodeModel, PromoCodeModelMap>(csvContent);
            //foreach (var promos in PromoList)
            //{
            PromoCodeModel promos = GetInputRecord<PromoCodeModel>();

            if (promos == null)
            {
                _tracingService.Trace("No Promo Code record received.");
                LogSkippedRecord("GetFundRaisinPromoCodeRecord", "Not Found", "Promo Code input is null.", "not found", null);
                return Task.CompletedTask;
            }
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


            this._tracingService.Trace("Promocode Record Fundraisin API Completed");
            return Task.CompletedTask;
        }

        public Task GetFundraisinRaffleRecords()
        {
            //var raffleList = this.GetAllData<RaffleModel, RaffleModelMap>(this.baseURL, "raffles");

            //foreach (var raffle in raffleList)
            //{

            RaffleModel raffle = GetInputRecord<RaffleModel>();

            if (raffle == null)
            {
                _tracingService.Trace("No Raffle record received.");
                LogSkippedRecord("GetFundraisinRaffleRecords", "Unknown", "Raffle input is null.", "Not Found", null);
                return Task.CompletedTask;
            }
            //Guid raffleID = Guid.Empty;
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

            if (!string.IsNullOrWhiteSpace(raffle.raffle_end_date) && NullIfMissing(raffle.raffle_end_date) != null &&
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



            this._tracingService.Trace("Raffle Record Fundraisin API Completed");
            return Task.CompletedTask;
        }

        public Task GetFundraisinRaffleTicketOptionRecords()
        {
            //var raffleTicketList = this.GetAllData<RaffleTicketModel, RaffleTicketModelMap>(this.baseURL, "raffletickets");
            //foreach (var raffleTicket in raffleTicketList)
            //{
            RaffleTicketModel raffleTicket = GetInputRecord<RaffleTicketModel>();

            if (raffleTicket == null)
            {
                _tracingService.Trace("No Raffle Ticket Option record received.");
                LogSkippedRecord("GetFundraisinRaffleTicketOptionRecords", "Not Found", "Raffle Ticket Option input is null.", "not found", null);
                return Task.CompletedTask;
            }
            Guid raffleTicketID = Guid.Empty;
            Guid raffleID = Guid.Empty;

            var RaffleSearchConditions = new List<ConditionExpression>
                {
                    new ConditionExpression("lrx_platformid", ConditionOperator.Equal, raffleTicket.raffle_id)
                };

            Entity existingRaffle = FindExistingRecord("lrx_raffle", RaffleSearchConditions);

            //if(existingRaffle == null)
            //{
            //    continue;
            //}
            //else
            //{
            //    raffleID = existingRaffle.Id;
            //}
            if (existingRaffle == null)
            {
                _tracingService.Trace("Related Raffle not found.");
                LogSkippedRecord(
    "GetFundraisinRaffleTicketOptionRecords",
    "Not Found",
    "Related Raffle not found.",
    raffleTicket.raffle_id,
    ParseDate(raffleTicket.date_created));
                return Task.CompletedTask;
            }

            raffleID = existingRaffle.Id;

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
            //}

            this._tracingService.Trace("Raffle Ticket Option Record Fundraisin API Completed");
            return Task.CompletedTask;
        }

        public Task GetFundraisinRaffleSalesRecords()
        {
            //var raffleSalesList = this.GetData<RaffleSalesModel, RaffleSalesModelMap>(this.baseURL, "rafflesales");
            //var raffleList = this.GetAllData<RaffleModel, RaffleModelMap>(this.baseURL, "raffles");
            //foreach (var raffleSales in raffleSalesList)
            //{
            RaffleSalesModel raffleSales = GetInputRecord<RaffleSalesModel>();

            if (raffleSales == null)
            {
                _tracingService.Trace("No Raffle Sales record received.");
                LogSkippedRecord("GetFundraisinRaffleSalesRecords", "Unknown", "Raffle Sales input is null.", "not found", null);
                return Task.CompletedTask;
            }
            Guid raffleSalesID = Guid.Empty;
            Guid raffleOptionD = Guid.Empty;
            Guid raffleID = Guid.Empty;
            Guid contactID = Guid.Empty;

            var RaffleSearchConditions = new List<ConditionExpression>
                {
                    new ConditionExpression("lrx_platformid", ConditionOperator.Equal, raffleSales.raffle_id)
                };

            Entity existingRaffle = FindExistingRecord("lrx_raffle", RaffleSearchConditions);

            //if (existingRaffle == null)
            //{
            //    continue;
            //}
            //else
            //{
            //    raffleID = existingRaffle.Id;
            //}
            if (existingRaffle == null)
            {
                _tracingService.Trace("Related Raffle not found.");
                LogSkippedRecord("GetFundraisinRaffleSalesRecords", raffleSales.sale_id, $"Related Raffle not found. Raffle ID: {raffleSales.raffle_id}", raffleSales.sale_id, ParseDate(raffleSales.date_created));
                return Task.CompletedTask;
            }

            raffleID = existingRaffle.Id;

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

            if (contactID == Guid.Empty)
            {
                _tracingService.Trace("Unable to create/find Contact for Raffle Sale ID: " + raffleSales.sale_id);
                LogSkippedRecord("GetFundraisinRaffleSalesRecords", raffleSales.sale_id, "Unable to create/find Contact.", raffleSales.raffle_id, ParseDate(raffleSales.date_created));
                return Task.CompletedTask;
            }

            var RaffleSalesSearchConditions = new List<ConditionExpression>
                {
                    new ConditionExpression("lrx_fundraisinrafflesalesid", ConditionOperator.Equal, raffleSales.sale_id)
                };

            //var raffleRecord = raffleList.FirstOrDefault(r => r.raffle_id.Trim() == raffleSales.raffle_id.Trim());
            // TODO:
            // Remove this once the Raffle record is available from the Power Automate payload.
            // For now, retrieve the Raffle name from Dataverse instead of downloading all Raffles.
            string raffleName = string.Empty;

            if (existingRaffle.Contains("lrx_name"))
            {
                raffleName = existingRaffle["lrx_name"].ToString();
            }

            string identifierName = $"{raffleSales.first_name} {raffleSales.last_name} - {raffleName}";
            //string identifierName = $"{raffleSales.first_name} {raffleSales.last_name} - {raffleRecord.raffle_name}";
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

            if (!string.IsNullOrWhiteSpace(raffleSales.date_paid) && NullIfMissing(raffleSales.date_paid) != null &&
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



            this._tracingService.Trace("Raffle Sales Record Fundraisin API Completed");
            return Task.CompletedTask;
        }

        #region Updated by samir on 15 july 2026
        public Task GetFundRaisinTransactionRecord()
        {
            var transaction = GetInputRecord<TransactionModel>();

            if (transaction == null)
            {
                _tracingService.Trace("No Transaction record received.");
                LogSkippedRecord(nameof(GetFundRaisinTransactionRecord), "Not Found", "Transaction input is null.");
                return Task.CompletedTask;
            }

            switch (transaction.Transaction_type?.Trim().ToLowerInvariant())
            {
                case "donation":
                    ProcessDonation(transaction);
                    break;
                case "registration":
                case "merchandise":
                    ProcessRegistrationOrMerchandise(transaction);
                    break;
                case "raffle":
                    ProcessRaffle(transaction);
                    break;
                case "refund":
                    ProcessRefund(transaction);
                    break;
                default:
                    LogSkippedRecord(nameof(GetFundRaisinTransactionRecord), transaction.Transaction_id, $"Unsupported type {transaction.Transaction_type}", transaction.Transaction_id, ParseDate(transaction.Date_created));
                    break;
            }

            return Task.CompletedTask;
        }
        private DonationModel GetDonation(string donationId, string customDate = null)
        {
            if (!HasValue(donationId))
                return null;
            string donationEndpoint = "donations/" + donationId;
            var donationList = GetData<DonationModel, DonationModelMap>(this.baseURL, donationEndpoint, customDate);

            var matchDonation = donationList?.FirstOrDefault();

            if (matchDonation != null)
                return matchDonation;

            //We are filtering by donationId 
            //var previousTransactionSearchConditions = new List<ConditionExpression>
            // {
            //     new ConditionExpression("lrx_fundraisindonationid", ConditionOperator.Equal, donationId)
            // };

            //var previousTransaction = FindExistingRecord("msnfp_transaction", previousTransactionSearchConditions, new ColumnSet("lrx_fundraisindonationdate"));

            //if (previousTransaction != null && previousTransaction.Contains("lrx_fundraisindonationdate"))
            //{
            //    var fallbackDate = previousTransaction.GetAttributeValue<string>("lrx_fundraisindonationdate");

            //    if (!string.IsNullOrWhiteSpace(fallbackDate))
            //    {
            //        var customDonationList = GetData<DonationModel, DonationModelMap>(this.baseURL, "donations", fallbackDate);

            //        return customDonationList?.FirstOrDefault(d =>
            //            string.Equals(d.Donation_id?.Trim(), donationId.Trim(), StringComparison.OrdinalIgnoreCase));
            //    }
            //}

            return null;
        }

        private ScheduleModel GetSchedule(string donationId)
        {
            if (!HasValue(donationId))
                return null;

            string scheduledFilter = "donation_id=" + donationId;

            var scheduleList = GetAllData<ScheduleModel, ScheduleModelMap>(this.baseURL, "scheduleddonations","", scheduledFilter);

            return scheduleList?.FirstOrDefault();
        }

        private ParticipantModel GetParticipant(string memberId)
        {
            if (!HasValue(memberId))
                return null;
            string participantsEndpoint = "participants/" + memberId;
            var participantList = GetData<ParticipantModel, ParticipantModelMap>(this.baseURL, participantsEndpoint);

            return participantList?.FirstOrDefault();
        }

        private EventModel GetEvent(string eventId)
        {
            if (!HasValue(eventId))
                return null;
            string eventEndpoint = "events/" + eventId;
            var eventList = GetData<EventModel, EventModelMap>(this.baseURL, eventEndpoint);

            return eventList?.FirstOrDefault();
        }

        private List<SaleItemModel> GetSales(string saleId)
        {
            if (!HasValue(saleId))
                return new List<SaleItemModel>();

            string SalesItemFilter = "sale_id=" + saleId;

            return GetAllData<SaleItemModel, SaleItemModelMap>(
                this.baseURL,
                "salesitems",
                "",
                SalesItemFilter);
        }
        private ProductModel GetProduct(string productId)
        {
            if (!HasValue(productId))
                return null;
            string ProductEndpoint = "products/" + productId;
            var productList = GetData<ProductModel, ProductModelMap>(this.baseURL, ProductEndpoint);

            return productList?.FirstOrDefault();
        }

        private ProductOptionModel GetProductOption(string optionId)
        {
            if (!HasValue(optionId))
                return null;
            string productOptionEndpoint = "productoptions/" + optionId;
            var optionList = GetData<ProductOptionModel, ProductOptionModelMap>(this.baseURL, productOptionEndpoint);

            return optionList?.FirstOrDefault();
        }

        private RaffleSalesModel GetRaffleSale(string saleId)
        {
            if (!HasValue(saleId))
                return null;
            string raffleSalesEndpoint = "rafflesales/" + saleId;
            var raffleSalesList = GetData<RaffleSalesModel, RaffleSalesModelMap>(this.baseURL, raffleSalesEndpoint);

            return raffleSalesList?.FirstOrDefault();
        }

        private TransactionModel GetOriginalTransaction(string donationId, string currentTransactionId)
        {
            if (!HasValue(donationId))
                return null;

            string transactionFilter = "donation_id=" + donationId;
            var transactionList = GetAllData<TransactionModel, TransactionModelMap>(this.baseURL, "transactions","", transactionFilter);

            return transactionList?.FirstOrDefault(t =>
                !string.Equals(t.Transaction_type?.Trim(), "refund", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(t.Transaction_id?.Trim(), currentTransactionId?.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        // business methods
        private void ProcessDonation(TransactionModel transaction)
        {
            var totalDonation = ParseDecimal(transaction.Transaction_value) - ParseDecimal(transaction.Transaction_fees);
            if (totalDonation <= 0)
            {
                LogSkippedRecord(
                    nameof(ProcessDonation),
                    transaction.Transaction_id,
                    $"Skipped donation because net amount is <= 0. Value={transaction.Transaction_value}, Fees={transaction.Transaction_fees}",
                    transaction.Donation_id,
                    ParseDate(transaction.Date_created));
                return;
            }

            var donation = GetDonation(transaction.Donation_id);
            if (donation == null)
            {
                LogSkippedRecord(nameof(ProcessDonation), transaction.Transaction_id, $"Donation not found. DonationId {transaction.Donation_id}", transaction.Donation_id, ParseDate(transaction.Date_created));
                return;
            }

            Guid eventId = Guid.Empty;
            Guid campaignId = Guid.Empty;
            Guid appealId = Guid.Empty;
            Guid packageId = Guid.Empty;
            Guid designationIdFromEvent = Guid.Empty;

            if (HasValue(transaction.Event_id))
            {

                var eventList = GetEvent(transaction.Event_id);

                eventId = CheckAndUpdateEvent(
                transaction.Event_id.Trim(),
                eventList,
                out _);
               
            }
            //ResolveEventLookups(
            //   NullIfMissing(transaction.Event_id),
            //   NullIfMissing(transaction.Page_id),
            //   out campaignId,
            //   out appealId,
            //   out packageId);
            ResolveTransactionLookups(
                    NullIfMissing(transaction.Event_id),
                    NullIfMissing(transaction.Page_id),
                    NullIfMissing(transaction.Gl_code1),
                    NullIfMissing(transaction.Gl_code2),
                    out campaignId,
                    out appealId,
                    out packageId,
                    out designationIdFromEvent);

            //if (!string.IsNullOrWhiteSpace(this.campaignName))
            //{
            //    var configuredCampaignId = ResolveCampaign(this.campaignName);
            //    if (configuredCampaignId != Guid.Empty)
            //        campaignId = configuredCampaignId;
            //}
            //if (campaignId == Guid.Empty && !string.IsNullOrWhiteSpace(this.campaignName))
            //{
            //    var configuredCampaignId = ResolveCampaign(this.campaignName);
            //    if (configuredCampaignId != Guid.Empty)
            //        campaignId = configuredCampaignId;
            //    _tracingService.Trace("Campaign used: configuration | TransactionId={0} | CampaignName={1}", transaction.Transaction_id, this.campaignName);
            //}
            //else if (campaignId != Guid.Empty)
            //{
            //    _tracingService.Trace("Campaign used: incoming/event | TransactionId={0} | EventId={1}", transaction.Transaction_id, transaction.Event_id);
            //}
            Guid designationId = designationIdFromEvent;
            //var glDesignationId = ResolveDesignation(transaction);
            //if (glDesignationId != Guid.Empty)
            //    designationId = glDesignationId;

            string contactFullName;
            decimal gstAmount;
            var contactId = ResolveContact(transaction, out contactFullName, out gstAmount, donation: donation);

            if (contactId == Guid.Empty)
            {
                LogSkippedRecord(nameof(ProcessDonation), transaction.Transaction_id, $"Unable to resolve donor contact. DonationId {transaction.Donation_id}, MemberId {transaction.Member_id}", transaction.Donation_id, ParseDate(transaction.Date_created));
                return;
            }

            var paymentMethodId = GetOrCreatePaymentMethod(transaction);
            var schedule = GetSchedule(transaction.Donation_id);

            Guid scheduleId = Guid.Empty;
            if (schedule != null)
            {
                var scheduleConditions = new List<ConditionExpression>
        {
            new ConditionExpression("lrx_fundraisinpaymentscheduleid", ConditionOperator.Equal, schedule.ScheduleId)
        };

                var existingSchedule = FindExistingRecord("msnfp_paymentschedule", scheduleConditions, new ColumnSet(false));

                var frequencyType = 856660003;
                if (string.Equals(schedule.donation_frequency, "weekly", StringComparison.OrdinalIgnoreCase)) frequencyType = 856660002;
                if (string.Equals(schedule.donation_frequency, "yearly", StringComparison.OrdinalIgnoreCase)) frequencyType = 856660004;
                if (string.Equals(schedule.donation_frequency, "fortnightly", StringComparison.OrdinalIgnoreCase)) frequencyType = 856660005;

                var paymentSchedule = existingSchedule == null
                    ? new Entity("msnfp_paymentschedule")
                    : new Entity("msnfp_paymentschedule", existingSchedule.Id);

                if (contactId != Guid.Empty)
                    paymentSchedule["sifund_donor"] = new EntityReference("contact", contactId);

                if (paymentMethodId != Guid.Empty)
                    paymentSchedule["lrx_paymentmethod"] = new EntityReference("msnfp_paymentmethod", paymentMethodId);

                paymentSchedule["sifund_scheduletypecode"] = new OptionSetValue(844060003);
                paymentSchedule["sifund_paymenttypecode"] = new OptionSetValue(existingSchedule == null ? 844060008 : 844060002);
                paymentSchedule["msnfp_recurringamount"] = new Money(ParseDecimal(schedule.d_amount) - ParseDecimal(transaction.Transaction_fees));
                paymentSchedule["msnfp_frequency"] = new OptionSetValue(frequencyType);
                paymentSchedule["msnfp_frequencyinterval"] = 1;
                paymentSchedule["sifund_bookdate"] = ParseDate(schedule.date_created) ?? DateTime.Now;
                paymentSchedule["msnfp_lastpaymentdate"] = ParseDate(transaction.Date_created) ?? DateTime.Now;
                paymentSchedule["lrx_fundraisinpaymentscheduleid"] = ParseInt(schedule.ScheduleId);
                paymentSchedule["lrx_billingstartdate"] = ParseDate(schedule.date_created) ?? DateTime.Now;

                scheduleId = existingSchedule == null
                    ? _service.Create(paymentSchedule)
                    : existingSchedule.Id;

                if (existingSchedule != null)
                    _service.Update(paymentSchedule);
            }

            //Guid solicitorId = Guid.Empty;
            //if (HasValue(donation.History_id))
            //{
            //    string customPageDetailURL = baseURL + "Custom/getFundraiserPageDetails";
            //    string csvCustomPageDetailContent = CallFundRaisinCustomAPI(customPageDetailURL, donation.History_id);

            //    if (!string.IsNullOrWhiteSpace(csvCustomPageDetailContent))
            //    {
            //        var pageDetailList = ParseCsvHelper<CustomPageDetailsModel, CustomPageDetailsModelMap>(csvCustomPageDetailContent);
            //        string pageMemberId = pageDetailList.FirstOrDefault()?.member_id?.Trim();

            //        if (HasValue(pageMemberId))
            //        {
            //            var solicitorConditions = new List<ConditionExpression>
            //{
            //    new ConditionExpression("lrx_fundraisinmemberid", ConditionOperator.Equal, pageMemberId)
            //};

            //            var existingSolicitor = FindExistingRecord("contact", solicitorConditions, new ColumnSet(false));
            //            if (existingSolicitor != null && existingSolicitor.Id != contactId)
            //                solicitorId = existingSolicitor.Id;
            //        }
            //    }
            //    else
            //    {
            //        _tracingService.Trace(
            //            "Skipping fundraiser page details parse | TransactionId={0} | HistoryId={1}",
            //            transaction.Transaction_id,
            //            donation.History_id);
            //    }
            //}

            //    Guid teamId = Guid.Empty;
            //    if (HasValue(donation.Team_id))
            //    {
            //        var teamConditions = new List<ConditionExpression>
            //{
            //    new ConditionExpression("lrx_fundraisinteamid", ConditionOperator.Equal, donation.Team_id.Trim())
            //};

            //        var existingTeam = FindExistingRecord("lrx_eventteam", teamConditions, new ColumnSet(false));
            //        if (existingTeam != null)
            //            teamId = existingTeam.Id;
            //    }
            Guid solicitorId = Guid.Empty;
            Guid registrationId = Guid.Empty;
            Guid teamId = Guid.Empty;
            Guid eventTicketId = Guid.Empty;

            if (HasValue(donation.Member_id))
            {
                var solicitorConditions = new List<ConditionExpression>
                {
                    new ConditionExpression(
                        "lrx_fundraisinmemberid",
                         ConditionOperator.Equal,
                        donation.Member_id.Trim())
                };

                Entity existingSolicitor =
                    FindExistingRecord("contact", solicitorConditions, new ColumnSet(false));

                if (existingSolicitor != null)
                {
                    solicitorId = existingSolicitor.Id;
                }
            }


            if (HasValue(donation.History_id))
            {
                var registrationConditions = new List<ConditionExpression>
                {
                        new ConditionExpression(
                        "lrx_fundraisinregistrationid",
                        ConditionOperator.Equal,
                        donation.History_id.Trim())
                };

                Entity existingRegistration =
                    FindExistingRecord("lrx_registrations", registrationConditions, new ColumnSet("lrx_eventticket"));

                if (existingRegistration != null)
                {
                    registrationId = existingRegistration.Id;

                    var eventTicketRef =
                        existingRegistration.GetAttributeValue<EntityReference>("lrx_eventticket");

                    if (eventTicketRef != null)
                    {
                        eventTicketId = eventTicketRef.Id;
                    }
                }
            }


            if (HasValue(donation.Team_id))
            {
                var teamConditions = new List<ConditionExpression>
                    {
                         new ConditionExpression(
                            "lrx_fundraisinteamid",
                            ConditionOperator.Equal,
                            donation.Team_id.Trim())
                     };

                Entity existingTeam =
                    FindExistingRecord("lrx_eventteam", teamConditions, new ColumnSet(false));

                if (existingTeam != null)
                {
                    teamId = existingTeam.Id;
                }
            }


            //Added to map the In_Memory fields
            OptionSetValue tributeOrInMemoryOfType = null;
            string tributeOrInMemoryOfName = null;
            string tributeOrInMemoryOfEmail = null;

            if (string.Equals(NullIfMissing(donation.In_memory), "Y", StringComparison.OrdinalIgnoreCase))
            {
                tributeOrInMemoryOfType = new OptionSetValue(856660002); // In Memory

                tributeOrInMemoryOfName = string.Join(
                    " ",
                    new[]
                    {
            NullIfMissing(donation.In_memory_fname),
            NullIfMissing(donation.In_memory_lname)
                    }
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                );

                tributeOrInMemoryOfEmail = NullIfMissing(donation.In_memory_email);
            }


            var transactionId = CreateOrUpdateTransaction(
                transaction,
                contactId,
                paymentMethodId,
                844060000,
                campaignId: campaignId,
                designationId: designationId,
                appealId: appealId,
                packageId: packageId,
                eventId: eventId,
                scheduleId: scheduleId,
                solicitorId: solicitorId,
                teamId: teamId,
                registrationId: registrationId,
                donationId: ParseInt(donation.Donation_id),
                donationDate: donation.Date_created,
                eventTicketId: eventTicketId,
                tributeOrInMemoryOfType: tributeOrInMemoryOfType,
    tributeOrInMemoryOfName: tributeOrInMemoryOfName,
    tributeOrInMemoryOfEmail: tributeOrInMemoryOfEmail);

            if (transactionId != Guid.Empty)
            {
                var donationTypeUpdate = new Entity("msnfp_transaction", transactionId);
                donationTypeUpdate["lrx_donationpaymenttype"] = new OptionSetValue(scheduleId != Guid.Empty ? 856660001 : 856660000);
                _service.Update(donationTypeUpdate);
            }
        }
        private void ProcessRegistrationOrMerchandise(TransactionModel transaction)
        {
            int transactionType = string.Equals(transaction.Transaction_type, "merchandise", StringComparison.OrdinalIgnoreCase)
        ? 844060004
        : 844060003;

            Guid eventId = Guid.Empty;
            Guid campaignId = Guid.Empty;
            Guid appealId = Guid.Empty;
            Guid packageId = Guid.Empty;
            Guid designationIdFromEvent = Guid.Empty;
            Guid registrationId = Guid.Empty;
            Guid promoId = Guid.Empty;
            Guid eventTicketId = Guid.Empty;

            if (HasValue(transaction.Event_id))
            {
                var eventList = GetEvent(transaction.Event_id);
                eventId = CheckAndUpdateEvent(
                transaction.Event_id.Trim(),
                eventList,             
                out _);

               
            }
            //ResolveEventLookups(
            //       NullIfMissing(transaction.Event_id),
            //       NullIfMissing(transaction.Page_id),
            //       out campaignId,
            //       out appealId,
            //       out packageId);
            //if (campaignId == Guid.Empty && !string.IsNullOrWhiteSpace(this.campaignName))
            //{
            //    campaignId = ResolveCampaign(this.campaignName);
            //}
            ResolveTransactionLookups(
                        transaction.Event_id,
                        transaction.Page_id,
                        transaction.Gl_code1,
                        transaction.Gl_code2,
                        out campaignId,
                        out appealId,
                        out packageId,
                        out designationIdFromEvent);
            Guid designationId = designationIdFromEvent;
            //var glDesignationId = ResolveDesignation(transaction);
            //if (glDesignationId != Guid.Empty)
            //    designationId = glDesignationId;

            string contactFullName;
            decimal gstAmount;
            var contactId = ResolveContact(transaction, out contactFullName, out gstAmount);

            if (contactId == Guid.Empty)
            {
                LogSkippedRecord(nameof(ProcessRegistrationOrMerchandise), transaction.Transaction_id, $"Unable to resolve contact. SaleId={transaction.Sale_id}", transaction.Sale_id, ParseDate(transaction.Date_created));
                return;
            }

            var paymentMethodId = GetOrCreatePaymentMethod(transaction);

            if (!string.Equals(transaction.Transaction_type, "merchandise", StringComparison.OrdinalIgnoreCase) && HasValue(transaction.History_id))
            {
                var registrationConditions = new List<ConditionExpression>();

                if (int.TryParse(transaction.History_id?.Trim(), out int historyIdValue))
                {
                    registrationConditions.Add(
                        new ConditionExpression("lrx_fundraisinregistrationid", ConditionOperator.Equal, historyIdValue)
                    );
                }

                var existingRegistration = FindExistingRecord("lrx_registrations", registrationConditions, new ColumnSet("lrx_promoid", "lrx_eventticket"));
                if (existingRegistration != null)
                {
                    registrationId = existingRegistration.Id;

                    var eventTicketRef =
                    existingRegistration.GetAttributeValue<EntityReference>("lrx_eventticket");

                    if (eventTicketRef != null)
                    {
                        eventTicketId = eventTicketRef.Id;
                    }

                    if (existingRegistration.Attributes.Contains("lrx_promoid") && existingRegistration["lrx_promoid"] != null)
                    {
                        var promoConditions = new List<ConditionExpression>
                {
                    new ConditionExpression("lrx_fundraisinpromoid", ConditionOperator.Equal, existingRegistration["lrx_promoid"].ToString())
                };

                        var existingPromo = FindExistingRecord("lrx_promocodeanddiscount", promoConditions, new ColumnSet(false));
                        if (existingPromo != null)
                            promoId = existingPromo.Id;
                    }
                }
            }

            var transactionId = CreateOrUpdateTransaction(
                transaction,
                contactId,
                paymentMethodId,
                transactionType,
                campaignId: campaignId,
                designationId: designationId,
                appealId: appealId,
                packageId: packageId,
                eventId: eventId,
                registrationId: registrationId,
                promoId: promoId,
                gstAmount: gstAmount,
                donationId: ParseInt(transaction.Donation_id),
                donationDate: transaction.Date_created,
                eventTicketId: eventTicketId);

            if (transactionId == Guid.Empty)
                return;
            var transactionRecord = _service.Retrieve(
                            "msnfp_transaction",
                            transactionId,
                            new ColumnSet(
                                    "sifund_billing_line1",
                                    "sifund_billing_line2",
                                    "sifund_billing_city",
                                    "sifund_billing_stateorprovince",
                                    "sifund_billing_postalcode",
                                    "sifund_billing_country"));

            _tracingService.Trace("Line1: " + transactionRecord.GetAttributeValue<string>("sifund_billing_line1"));
            _tracingService.Trace("City: " + transactionRecord.GetAttributeValue<string>("sifund_billing_city"));
            _tracingService.Trace("State: " + transactionRecord.GetAttributeValue<string>("sifund_billing_stateorprovince"));
            _tracingService.Trace("Postcode: " + transactionRecord.GetAttributeValue<string>("sifund_billing_postalcode"));
            _tracingService.Trace("Country: " + transactionRecord.GetAttributeValue<string>("sifund_billing_country"));

            if (registrationId != Guid.Empty)
            {
                var registrationUpdate = new Entity("lrx_registrations", registrationId);
                registrationUpdate["lrx_transaction"] = new EntityReference("msnfp_transaction", transactionId);
                registrationUpdate["statuscode"] = new OptionSetValue(1);
                _service.Update(registrationUpdate);
            }

            if (!HasValue(transaction.Sale_id))
            {
                //LogSkippedRecord(
                //    nameof(ProcessRegistrationOrMerchandise),
                //    transaction.Transaction_id,
                //    "Transaction created, but sales/product processing skipped because SaleId is missing or 0.",
                //    transaction.Transaction_id,
                //    ParseDate(transaction.Date_created));
                return;
            }

            var saleItems = GetSales(transaction.Sale_id);

            if (saleItems == null || !saleItems.Any())
                return;
            foreach (var salesItemMatch in saleItems)
            {
                var product = GetProduct(salesItemMatch.product_id);
                var productOption = GetProductOption(salesItemMatch.option_id);

                string productName = product?.product_name?.Trim() ?? string.Empty;
                string productOptionName = productOption?.option_name?.Trim() ?? string.Empty;

                Guid inventoryProductId = Guid.Empty;
                _tracingService.Trace("=== INVENTORY PRODUCT CHECK ===");
                _tracingService.Trace($"Transaction ID: {transaction.Transaction_id}");
                _tracingService.Trace($"Sale Item Product ID: '{salesItemMatch.product_id}'");
                if (HasValue(salesItemMatch.product_id))
                {
                    int fundraisinProductId = ParseInt(salesItemMatch.product_id);

                    _tracingService.Trace(
                        $"Searching Inventory Product: lrx_fundraisinproductid = {fundraisinProductId}");

                    var inventoryProductConditions = new List<ConditionExpression>
                    {
                        new ConditionExpression(
                            "lrx_fundraisinproductid",
                            ConditionOperator.Equal,
                            fundraisinProductId)
                    };

                    var existingInventoryProduct = FindExistingRecord("lrx_inventoryproduct", inventoryProductConditions, new ColumnSet(false));
                    if (existingInventoryProduct != null)
                    {
                        inventoryProductId = existingInventoryProduct.Id;
                        _tracingService.Trace(
           $"Inventory Product FOUND: {inventoryProductId}");
                    }
                    else
                    {
                        LogSkippedRecord(nameof(ProcessRegistrationOrMerchandise), transaction.Transaction_id, $"Inventory product not found. ProductId={salesItemMatch.product_id}", salesItemMatch.product_id, ParseDate(transaction.Date_created));
                        continue;
                    }
                }

                Guid productOptionId = Guid.Empty;

                if (HasValue(salesItemMatch.option_id))
                {
                    int fundraisinOptionId = ParseInt(salesItemMatch.option_id);

                    var productOptionConditions = new List<ConditionExpression>
                    {
                        new ConditionExpression(
                            "lrx_fundraisinoptionid",
                            ConditionOperator.Equal,
                            fundraisinOptionId)
                    };

                    var existingProductOption = FindExistingRecord(
                        "lrx_productoptions",
                        productOptionConditions,
                        new ColumnSet(false));

                    if (existingProductOption != null)
                    {
                        productOptionId = existingProductOption.Id;
                    }
                }

                Guid eventProductId = Guid.Empty;
                bool isInventoryProduct = false;

                if (!string.IsNullOrWhiteSpace(productName) && eventId != Guid.Empty)
                {
                    var eventProductSearchConditions = new List<ConditionExpression>
                    {
                        new ConditionExpression(
                        "lrx_fundraisineventid",
                            ConditionOperator.Equal,
                            int.Parse(transaction.Event_id)),

                        new ConditionExpression(
                    "lrx_fundraisininvenoryproductid",
                        ConditionOperator.Equal,
                    int.Parse(salesItemMatch.product_id))
            };

                    Entity existingEventProduct = FindExistingRecord(
                        "lrx_eventproduct",
                        eventProductSearchConditions);

                    if (existingEventProduct != null)
                    {
                        // Event Product exists
                        eventProductId = existingEventProduct.Id;
                        isInventoryProduct = false;

                        _tracingService.Trace(
                            $"Event Product FOUND: {eventProductId}");
                    }
                    else
                    {
                        // Event Product does not exist
                        if (inventoryProductId != Guid.Empty)
                        {
                            isInventoryProduct = true;

                           
                        }
                    }
                }
                else
                {
                    // No Event ID, so there cannot be an Event Product.
                    // Use the Inventory Product already found above.
                    if (inventoryProductId != Guid.Empty)
                    {
                        isInventoryProduct = true;

                        
                    }
                }

                var saleProduct = new Entity("lrx_product");
                saleProduct["lrx_name"] = string.IsNullOrWhiteSpace(productOptionName)
                    ? productName
                    : $"{productName} - {productOptionName}";
                saleProduct["lrx_constituentorganisation"] = new EntityReference("contact", contactId);
                saleProduct["lrx_date"] = ParseDate(transaction.Date_created) ?? DateTime.Now;
                //saleProduct["lrx_productmigrationid"] = salesItemMatch.id?.Trim();
                //saleProduct["lrx_fundraisinsalesid"] = ParseInt(salesItemMatch.sale_id);
                saleProduct["lrx_fundraisinsalesid"] = int.Parse(salesItemMatch.id?.Trim());
                saleProduct["lrx_quantity"] = ParseInt(salesItemMatch.quantity);
                saleProduct["lrx_priceperproduct"] = new Money(ParseDecimal(salesItemMatch.unit_cost));

                if (eventId != Guid.Empty)
                    saleProduct["lrx_event"] = new EntityReference("lrx_event", eventId);
             

                if (eventProductId != Guid.Empty)
                {
                    // Event Product exists
                    saleProduct["lrx_eventproduct"] =
                        new EntityReference("lrx_eventproduct", eventProductId);

                    // Event Product = No
                    saleProduct["lrx_type"] = false;
                }
                else if (isInventoryProduct && inventoryProductId != Guid.Empty)
                {
                    // Event Product does not exist.
                    // Link the already found Inventory Product.
                    saleProduct["lrx_inventoryproduct"] =
                        new EntityReference("lrx_inventoryproduct", inventoryProductId);

                    // Inventory Product = Yes
                    saleProduct["lrx_type"] = true;
                }

                if (productOptionId != Guid.Empty)
                    saleProduct["lrx_productoption"] = new EntityReference("lrx_productoptions", productOptionId);

                if (transactionId != Guid.Empty)
                    saleProduct["lrx_transaction"] = new EntityReference("msnfp_transaction", transactionId);

                if (transactionRecord.Contains("sifund_billing_line1"))
                    saleProduct["lrx_addressline1"] = transactionRecord["sifund_billing_line1"];

                if (transactionRecord.Contains("sifund_billing_line2"))
                    saleProduct["lrx_addressline2"] = transactionRecord["sifund_billing_line2"];

                if (transactionRecord.Contains("sifund_billing_city"))
                    saleProduct["lrx_townsuburb"] = transactionRecord["sifund_billing_city"];

                if (transactionRecord.Contains("sifund_billing_stateorprovince"))
                    saleProduct["lrx_state"] = transactionRecord["sifund_billing_stateorprovince"];

                if (transactionRecord.Contains("sifund_billing_postalcode"))
                    saleProduct["lrx_postcode"] = transactionRecord["sifund_billing_postalcode"];

                if (transactionRecord.Contains("sifund_billing_country"))
                    saleProduct["lrx_country"] = transactionRecord["sifund_billing_country"];

                var productSaleConditions = new List<ConditionExpression>
    {
        new ConditionExpression("lrx_fundraisinsalesid", ConditionOperator.Equal, int.Parse(salesItemMatch.id?.Trim()))
    };

                Guid productSaleId;
                var existingProductSale = FindExistingRecord("lrx_product", productSaleConditions, new ColumnSet(false));

                if (existingProductSale == null)
                {
                    productSaleId = _service.Create(saleProduct);
                }
                else
                {
                    productSaleId = existingProductSale.Id;
                    var saleProductUpdate = new Entity("lrx_product", existingProductSale.Id);
                    foreach (var kvp in saleProduct.Attributes)
                        saleProductUpdate[kvp.Key] = kvp.Value;

                    _service.Update(saleProductUpdate);
                }



                if (productSaleId != Guid.Empty)
                {
                    var productUpdate = new Entity("lrx_product", productSaleId);

                    if (eventId != Guid.Empty)
                        productUpdate["lrx_event"] = new EntityReference("lrx_event", eventId);

                    if (eventProductId != Guid.Empty)
                    {
                        // Event Product exists
                        productUpdate["lrx_eventproduct"] =
                            new EntityReference("lrx_eventproduct", eventProductId);

                        // Event Product = No
                        productUpdate["lrx_type"] = false;
                    }
                    else if (isInventoryProduct && inventoryProductId != Guid.Empty)
                    {
                        // Event Product does not exist.
                        // Use Inventory Product.
                        productUpdate["lrx_inventoryproduct"] =
                            new EntityReference("lrx_inventoryproduct", inventoryProductId);

                        // Inventory Product = Yes
                        productUpdate["lrx_type"] = true;
                    }

                    if (transactionId != Guid.Empty)
                        productUpdate["lrx_transaction"] = new EntityReference("msnfp_transaction", transactionId);
                    

                    _service.Update(productUpdate);
                }

                // Keep transaction lookup pointing to one product (same behaviour as old code)
                //var transactionUpdate = new Entity("msnfp_transaction", transactionId);

                //    if (eventProductId != Guid.Empty)
                //        transactionUpdate["lrx_eventproduct"] = new EntityReference("lrx_eventproduct", eventProductId);

                //    transactionUpdate["lrx_product"] = new EntityReference("lrx_product", matchingProducts.First().Id);

                //    _service.Update(transactionUpdate);
                
            }
        }

        private void ProcessRaffle(TransactionModel transaction)
        {
            var raffleSale = GetRaffleSale(transaction.Sale_id);
            if (raffleSale == null)
            {
                LogSkippedRecord(nameof(ProcessRaffle), transaction.Transaction_id, $"Raffle Sale not found. SaleId={transaction.Sale_id}", transaction.Sale_id, ParseDate(transaction.Date_created));
                return;
            }

            string contactFullName;
            decimal gstAmount;
            var contactId = ResolveContact(transaction, out contactFullName, out gstAmount, raffleSale: raffleSale);

            if (contactId == Guid.Empty)
            {
                LogSkippedRecord(nameof(ProcessRaffle), transaction.Transaction_id, "Unable to resolve raffle contact.", raffleSale?.sale_id, ParseDate(transaction.Date_created));
                return;
            }

            var paymentMethodId = GetOrCreatePaymentMethod(transaction);

            Guid raffleSaleId = Guid.Empty;
            Guid raffleId = Guid.Empty;
            Guid eventId = Guid.Empty;
            Guid campaignId = Guid.Empty;
            Guid appealId = Guid.Empty;
            Guid packageId = Guid.Empty;
            Guid designationId = Guid.Empty;

            var raffleSalesConditions = new List<ConditionExpression>
    {
        new ConditionExpression("lrx_fundraisinrafflesalesid", ConditionOperator.Equal, raffleSale.sale_id)
    };

            var existingRaffleSales = FindExistingRecord(
    "lrx_rafflesales",
    raffleSalesConditions,
    new ColumnSet("lrx_raffle"));
            if (existingRaffleSales == null)
            {
                LogSkippedRecord(nameof(ProcessRaffle), transaction.Transaction_id, $"Dataverse raffle sale not found. SaleId={raffleSale.sale_id}", raffleSale.sale_id, ParseDate(transaction.Date_created));
                return;
            }

            raffleSaleId = existingRaffleSales.Id;

            var raffleRef = existingRaffleSales.GetAttributeValue<EntityReference>("lrx_raffle");
            if (raffleRef != null)
            {
                raffleId = raffleRef.Id;

                var existingRaffle = _service.Retrieve(
                        "lrx_raffle",
                        raffleId,
                        new ColumnSet("lrx_event", "lrx_campaign"));
                eventId = existingRaffle.GetAttributeValue<EntityReference>("lrx_event")?.Id ?? Guid.Empty;


                //just added this untill we not change logic for appeal and package
                ResolveRaffleAppealPackageDesignation(
    NullIfMissing(transaction.Event_id),
    NullIfMissing(transaction.Page_id),
    NullIfMissing(transaction.Gl_code1),
    NullIfMissing(transaction.Gl_code2),
    out appealId,
    out packageId,
    out designationId);

                var raffleCampaignRef = existingRaffle.GetAttributeValue<EntityReference>("lrx_campaign")?.Id ?? Guid.Empty;
                if (raffleCampaignRef != Guid.Empty)
                {
                    campaignId = raffleCampaignRef;
                }
                else
                {
                    campaignId = defaultCampaignId;
                }
            }
            


            var transactionId = CreateOrUpdateTransaction(
                transaction,
                contactId,
                paymentMethodId,
                844060005,
                campaignId: campaignId,
                designationId: designationId,
                appealId: appealId,
                packageId: packageId,
                eventId: eventId,
                raffleId: raffleId,
                raffleSaleId: raffleSaleId,
                donationId: ParseInt(transaction.Donation_id),
                donationDate: transaction.Date_created);

            if (transactionId == Guid.Empty)
                return;

            var raffleSalesUpdate = new Entity("lrx_rafflesales", raffleSaleId);
            raffleSalesUpdate["lrx_transaction"] = new EntityReference("msnfp_transaction", transactionId);

            if (eventId != Guid.Empty)
                raffleSalesUpdate["lrx_event"] = new EntityReference("lrx_event", eventId);

            _service.Update(raffleSalesUpdate);
        }

        private void ProcessRefund(TransactionModel transaction)
        {
            var originalTransaction = GetOriginalTransaction(transaction.Donation_id, transaction.Transaction_id);
            if (originalTransaction == null)
            {
                LogSkippedRecord(nameof(ProcessRefund), transaction.Transaction_id, $"Original transaction not found. DonationId={transaction.Donation_id}", transaction.Donation_id, ParseDate(transaction.Date_created));
                return;
            }

            var originalTransactionConditions = new List<ConditionExpression>
    {
        new ConditionExpression("lrx_fundraisintransactionid", ConditionOperator.Equal, originalTransaction.Transaction_id)
    };

            var existingTransaction = FindExistingRecord(
     "msnfp_transaction",
     originalTransactionConditions,
     new ColumnSet("sifund_donor"));
            if (existingTransaction == null)
            {
                LogSkippedRecord(nameof(ProcessRefund), transaction.Transaction_id, $"Original Dataverse transaction not found. TransactionId={originalTransaction.Transaction_id}", originalTransaction.Transaction_id, ParseDate(transaction.Date_created));
                return;
            }

            var donorId = existingTransaction.GetAttributeValue<EntityReference>("sifund_donor")?.Id ?? Guid.Empty;

            decimal originalAmount =
                ParseDecimal(originalTransaction.Transaction_value) -
                ParseDecimal(originalTransaction.Transaction_fees);

            var refundConditions = new List<ConditionExpression>
    {
        new ConditionExpression("lrx_fundraisinrefundid", ConditionOperator.Equal, transaction.Transaction_id)
    };

            var existingRefund = FindExistingRecord("lrx_refund", refundConditions, new ColumnSet(false));

            var refundEntity = existingRefund == null
                ? new Entity("lrx_refund")
                : new Entity("lrx_refund", existingRefund.Id);

            if (donorId != Guid.Empty)
                refundEntity["lrx_customer"] = new EntityReference("contact", donorId);

            refundEntity["lrx_transaction"] = new EntityReference("msnfp_transaction", existingTransaction.Id);
            refundEntity["lrx_totalamountpaidrefund"] = new Money(originalAmount);
            refundEntity["lrx_amountreceiptablerefund"] = new Money(originalAmount);
            refundEntity["lrx_totalamountpaid"] = new Money(originalAmount);
            refundEntity["lrx_amountreceiptable"] = new Money(originalAmount);
            refundEntity["lrx_refunddate"] = ParseDate(transaction.Date_created) ?? DateTime.Now;
            refundEntity["lrx_refundtype"] = new OptionSetValue(844060002);
            refundEntity["statuscode"] = new OptionSetValue(376750001);
            refundEntity["lrx_fundraisinrefundid"] = ParseInt(transaction.Transaction_id);

            if (existingRefund == null)
                _service.Create(refundEntity);
            else if (this.updateTransaction)
                _service.Update(refundEntity);

            var originalTransactionUpdate = new Entity("msnfp_transaction", existingTransaction.Id);
            originalTransactionUpdate["statuscode"] = new OptionSetValue(856660005);
            _service.Update(originalTransactionUpdate);
        }

        // shared dataverse helpers
        private Guid GetOrCreatePaymentMethod(TransactionModel transaction)
        {
            string paymentMethodSourceName = null;

            if (transaction != null && HasValue(transaction.Payment_type))
            {
                paymentMethodSourceName = transaction.Payment_type.Trim();
            }
            else if (!string.IsNullOrWhiteSpace(this.paymentMethod))
            {
                paymentMethodSourceName = this.paymentMethod.Trim();
            }

            if (string.IsNullOrWhiteSpace(paymentMethodSourceName))
                return Guid.Empty;

            string pMethodUniqueName = $"{paymentMethodSourceName} - Default";
            string paymentMethodSource = transaction != null && HasValue(transaction.Payment_type)
    ? "incoming"
    : "configuration";

            _tracingService.Trace(
   "Payment method used: {0} | TransactionId={1} | Name={2}",
   paymentMethodSource,
   transaction?.Transaction_id,
   pMethodUniqueName);

            var conditions = new List<ConditionExpression>
    {
        new ConditionExpression("msnfp_name", ConditionOperator.Equal, pMethodUniqueName)
    };

            var existingPMethod = FindExistingRecord("msnfp_paymentmethod", conditions, new ColumnSet(false));

            if (existingPMethod != null)
                return existingPMethod.Id;

            var paymentMethod = new Entity("msnfp_paymentmethod");
            paymentMethod["msnfp_name"] = pMethodUniqueName;
            paymentMethod["msnfp_type"] = new OptionSetValue(100000000);

            return _service.Create(paymentMethod);
        }
    //    private Guid ResolveCampaign(string configuredCampaignName)
    //    {
    //        if (string.IsNullOrWhiteSpace(configuredCampaignName))
    //            return Guid.Empty;

    //        var conditions = new List<ConditionExpression>
    //{
    //    new ConditionExpression("name", ConditionOperator.Equal, configuredCampaignName.Trim())
    //};

    //        var existingCampaign = FindExistingRecord("campaign", conditions, new ColumnSet(false));
    //        return existingCampaign?.Id ?? Guid.Empty;
    //    }

        private Guid ResolveDesignation(TransactionModel transaction)
        {
            Entity existingDesignation = null;

            if (HasValue(transaction.Gl_code1))
            {
                var condition1 = new List<ConditionExpression>
        {
            new ConditionExpression("msnfp_designationcode", ConditionOperator.Equal, transaction.Gl_code1.Trim())
        };

                existingDesignation = FindExistingRecord("msnfp_designation", condition1, new ColumnSet(false));

                if (existingDesignation != null)
                {
                    _tracingService.Trace(
                        "Designation used from GL Code 1 | TransactionId={0} | GLCode1={1}",
                        transaction.Transaction_id,
                        transaction.Gl_code1);
                }
            }

            if (existingDesignation == null && HasValue(transaction.Gl_code2))
            {
                var condition2 = new List<ConditionExpression>
        {
            new ConditionExpression("msnfp_designationcode", ConditionOperator.Equal, transaction.Gl_code2.Trim())
        };

                existingDesignation = FindExistingRecord("msnfp_designation", condition2, new ColumnSet(false));

                if (existingDesignation != null)
                {
                    _tracingService.Trace(
                        "Designation used from GL Code 2 | TransactionId={0} | GLCode2={1}",
                        transaction.Transaction_id,
                        transaction.Gl_code2);
                }
            }

            if (existingDesignation != null)
                return existingDesignation.Id;

            if (this.defaultPrimaryDesignationId != Guid.Empty)
            {
                _tracingService.Trace(
                    "Designation used from configuration default primary designation | TransactionId={0} | DesignationId={1}",
                    transaction.Transaction_id,
                    this.defaultPrimaryDesignationId);

                return this.defaultPrimaryDesignationId;
            }

            _tracingService.Trace(
                "No designation found from GL Code 1, GL Code 2, or configuration | TransactionId={0}",
                transaction.Transaction_id);

            return Guid.Empty;
        }
        private Guid GetExistingContactByMemberId(string memberId, out string fullName)
        {
            fullName = string.Empty;

            if (!int.TryParse(NullIfMissing(memberId), out int memberIdValue))
                return Guid.Empty;

            var contactSearchConditions = new List<ConditionExpression>
            {
                    new ConditionExpression(
                    "lrx_fundraisinmemberid",
                    ConditionOperator.Equal,
                    memberIdValue)
            };
            Entity existingContact = FindExistingRecord("contact", contactSearchConditions);

            if (existingContact == null)
                return Guid.Empty;

            if (existingContact.Contains("fullname"))
                fullName = existingContact.GetAttributeValue<string>("fullname") ?? string.Empty;

            return existingContact.Id;
        }
        private Guid ResolveContact(TransactionModel transaction,
    out string contactFullName,
    out decimal gstAmount,
    DonationModel donation = null,
    RaffleSalesModel raffleSale = null)
        {
            contactFullName = string.Empty;
            gstAmount = 0m;

            var type = transaction.Transaction_type?.Trim().ToLowerInvariant();

            switch (type)
            {
                case "donation":
                    return donation != null ? UpsertContact(donation, transaction.Member_id) : Guid.Empty;

                case "registration":
                case "merchandise":
                    {
                        Guid contactId = Guid.Empty;

                        if (HasValue(transaction.Member_id))
                        {
                            contactId = GetExistingContactByMemberId(transaction.Member_id, out contactFullName);

                            _tracingService.Trace(
                                "Registration/Merchandise contact lookup by MemberId. TransactionId={0}, MemberId={1}, ContactId={2}",
                                transaction.Transaction_id,
                                transaction.Member_id,
                                contactId);
                        }

                        if (contactId == Guid.Empty && HasValue(transaction.Sale_id))
                        {
                            contactId = UpsertContactFromSales(transaction.Sale_id, out contactFullName, out gstAmount);

                            _tracingService.Trace(
                                "Registration/Merchandise fallback contact lookup by SaleId. TransactionId={0}, SaleId={1}, ContactId={2}",
                                transaction.Transaction_id,
                                transaction.Sale_id,
                                contactId);
                        }

                        return contactId;
                    }

                case "raffle":
                    return raffleSale != null ? UpsertContactFromRaffleSales(raffleSale) : Guid.Empty;

                default:
                    return Guid.Empty;
            }
        }

        private Guid CreateOrUpdateTransaction(TransactionModel transaction,
    Guid contactId,
    Guid paymentMethodId,
    int transactionType,
    Guid campaignId = default,
    Guid designationId = default,
    Guid appealId = default,
    Guid packageId = default,
    Guid eventId = default,
    Guid registrationId = default,
    Guid teamId = default,
    Guid promoId = default,
    Guid scheduleId = default,
    Guid raffleId = default,
    Guid raffleSaleId = default,
    Guid solicitorId = default,
    decimal gstAmount = 0m,
    string donationDate = null,
    int? donationId = null,
    Guid eventTicketId = default,
    OptionSetValue tributeOrInMemoryOfType = null,
string tributeOrInMemoryOfName = null,
string tributeOrInMemoryOfEmail = null)
        {

            Guid fundraisinPageRecordId = Guid.Empty;

            var pageId = NullIfMissing(transaction.Page_id);

            if (pageId != null && int.TryParse(pageId, out int fundraisinPageNumber))
            {
                var pageConditions = new List<ConditionExpression>
                {
                        new ConditionExpression(
                        "lrx_fundraisinpagesid",
                        ConditionOperator.Equal,
                        fundraisinPageNumber)
                };

                var existingPage = FindExistingRecord(
                    "lrx_fundraisinpage",
                    pageConditions,
                    new ColumnSet(false));

                if (existingPage != null)
                {
                    fundraisinPageRecordId = existingPage.Id;
                }
            }


            var conditions = new List<ConditionExpression>
            {
                    new ConditionExpression("lrx_fundraisintransactionid", ConditionOperator.Equal, transaction.Transaction_id)
            };

            var existingTransaction = FindExistingRecord("msnfp_transaction", conditions, new ColumnSet(false));

            var transactionEntity = existingTransaction == null
                ? new Entity("msnfp_transaction")
                : new Entity("msnfp_transaction", existingTransaction.Id);

            if (contactId != Guid.Empty)
                transactionEntity["sifund_donor"] = new EntityReference("contact", contactId);

            if (solicitorId != Guid.Empty)
                transactionEntity["lrx_solicitor"] = new EntityReference("contact", solicitorId);

            if (campaignId != Guid.Empty)
                transactionEntity["lrx_campaign"] = new EntityReference("campaign", campaignId);

            if (designationId != Guid.Empty)
                transactionEntity["sifund_primarydesignation"] = new EntityReference("msnfp_designation", designationId);

            if (appealId != Guid.Empty)
                transactionEntity["sifund_appeal"] = new EntityReference("sifund_appeal", appealId);

            if (packageId != Guid.Empty)
                transactionEntity["sifund_package"] = new EntityReference("sifund_package", packageId);

            if (paymentMethodId != Guid.Empty)
                transactionEntity["msnfp_transaction_paymentmethodid"] = new EntityReference("msnfp_paymentmethod", paymentMethodId);

            if (scheduleId != Guid.Empty)
                transactionEntity["msnfp_transaction_paymentscheduleid"] = new EntityReference("msnfp_paymentschedule", scheduleId);

            if (eventId != Guid.Empty)
                transactionEntity["lrx_event"] = new EntityReference("lrx_event", eventId);

            if (registrationId != Guid.Empty)
                transactionEntity["lrx_registrations"] = new EntityReference("lrx_registrations", registrationId);

            if (teamId != Guid.Empty)
                transactionEntity["lrx_eventteam"] = new EntityReference("lrx_eventteam", teamId);

            if (promoId != Guid.Empty)
                transactionEntity["lrx_promocode"] = new EntityReference("lrx_promocodeanddiscount", promoId);

            if (fundraisinPageRecordId != Guid.Empty)
            {
                transactionEntity["lrx_fundraisinpage"] =
                    new EntityReference(
                        "lrx_fundraisinpage",
                        fundraisinPageRecordId);
            }

            if (eventTicketId != Guid.Empty)
            {
                transactionEntity["lrx_eventticket"] =
                    new EntityReference("lrx_eventticket", eventTicketId);
            }

            if (tributeOrInMemoryOfType != null)
            {
                transactionEntity["lrx_tributeorinmemoryoftype"] = tributeOrInMemoryOfType;
                transactionEntity["lrx_tributeorinmemoryofname"] = tributeOrInMemoryOfName;
                transactionEntity["lrx_tributeorinmemoryofemail"] = tributeOrInMemoryOfEmail;
            }
            else
            {
                transactionEntity["lrx_tributeorinmemoryoftype"] = null;
                transactionEntity["lrx_tributeorinmemoryofname"] = null;
                transactionEntity["lrx_tributeorinmemoryofemail"] = null;
            }

            //if (raffleId != Guid.Empty)
            //    transactionEntity["lrx_raffle"] = new EntityReference("lrx_raffle", raffleId);

            //if (raffleSaleId != Guid.Empty)
            //    transactionEntity["lrx_rafflesales"] = new EntityReference("lrx_rafflesales", raffleSaleId);

            transactionEntity["msnfp_amount"] = new Money(ParseDecimal(transaction.Transaction_value) - ParseDecimal(transaction.Transaction_fees));
            transactionEntity["msnfp_bookdate"] = ParseDate(transaction.Date_created) ?? DateTime.Now;
            transactionEntity["sifund_paymenttypecode"] = new OptionSetValue(844060002);
            transactionEntity["statuscode"] = new OptionSetValue(856660001);
            transactionEntity["sifund_typecode"] = new OptionSetValue(transactionType);
            transactionEntity["lrx_fundraisintransactionid"] = ParseInt(transaction.Transaction_id);

            if (donationId.HasValue)
                transactionEntity["lrx_fundraisindonationid"] = donationId.Value;

            if (!string.IsNullOrWhiteSpace(donationDate))
                transactionEntity["lrx_fundraisindonationdate"] = donationDate;

            transactionEntity["sifund_thirdpartyreceipt"] = NullIfMissing(transaction.Po_number);

            if (gstAmount > 0)
                transactionEntity["sifund_amount_tax"] = new Money(gstAmount);

            if (existingTransaction == null)
                return _service.Create(transactionEntity);

            if (this.updateTransaction)
                _service.Update(transactionEntity);

            return existingTransaction.Id;
        }
        private bool HasValue(string value)
        {
            return !string.IsNullOrWhiteSpace(value) && value.Trim() != "0";
        }

        private decimal ParseDecimal(string value)
        {
            return decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result)
                ? result
                : 0m;
        }

        private int ParseInt(string value)
        {
            return int.TryParse(value, out var result) ? result : 0;
        }

        private DateTime? ParseDate(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var result))
                return result;

            if (DateTime.TryParse(value, out result))
                return result;

            return null;
        }

        #endregion
        #region Added by samir on 20 july 2026
        public Task GetFundRaisinOflineDonation()
        {
            try
            {
                if (this._jsonInput == null)
                {
                    _tracingService.Trace("No offline donation record received.");
                    LogSkippedRecord(nameof(GetFundRaisinOflineDonation), "Not Found", "Offline donation input is null.");
                    return Task.CompletedTask;
                }

                string donationJson = this._jsonInput.ToString();
                if (string.IsNullOrWhiteSpace(donationJson))
                {
                    _tracingService.Trace("Offline donation JSON is empty.");
                    LogSkippedRecord(nameof(GetFundRaisinOflineDonation), "Not Found", "Offline donation input JSON is empty.");
                    return Task.CompletedTask;
                }

                _tracingService.Trace("Offline donation raw JSON: {0}", donationJson);

                DonationModel donation = JsonConvert.DeserializeObject<DonationModel>(donationJson);
                if (donation == null)
                {
                    LogSkippedRecord(nameof(GetFundRaisinOflineDonation), "Not Found", "Offline donation deserialization returned null.");
                    return Task.CompletedTask;
                }

                string donationId = NullIfMissing(donation.Donation_id) ?? string.Empty;
                string donationType = NullIfMissing(donation.Donation_type);
                string donationStatus = NullIfMissing(donation.D_status);
                string isDonation = NullIfMissing(donation.Is_donation);
                string memberId = NullIfMissing(donation.Member_id) ?? string.Empty;
                string eventIdText = NullIfMissing(donation.Event_id) ?? string.Empty;
                string paymentMethod = NullIfMissing(donation.Payment_method) ?? string.Empty;
                string amountText = NullIfMissing(donation.D_amount);
                string feeText = NullIfMissing(donation.D_fee);
                string datePaid = NullIfMissing(donation.Date_paid) ?? string.Empty;
                string dateCreated = NullIfMissing(donation.Date_created) ?? string.Empty;

                DateTime? donationDate = ParseDate(datePaid) ?? ParseDate(dateCreated);

                if (!HasValue(donationId))
                {
                    LogSkippedRecord(nameof(GetFundRaisinOflineDonation), "Not Found", "Donation_id is missing.", donationId, donationDate);
                    return Task.CompletedTask;
                }

                if (!string.Equals(donationType, "offline", StringComparison.OrdinalIgnoreCase))
                {
                    LogSkippedRecord(
                        nameof(GetFundRaisinOflineDonation),
                        donationId,
                        $"Skipped because Donation_type is '{donationType ?? "null"}' instead of 'offline'.",
                        donationId,
                        donationDate);
                    return Task.CompletedTask;
                }

                if (!string.Equals(isDonation, "Y", StringComparison.OrdinalIgnoreCase))
                {
                    LogSkippedRecord(
                        nameof(GetFundRaisinOflineDonation),
                        donationId,
                        $"Skipped because Is_donation is '{isDonation ?? "null"}' instead of 'Y'.",
                        donationId,
                        donationDate);
                    return Task.CompletedTask;
                }

                if (!string.Equals(donationStatus, "paid", StringComparison.OrdinalIgnoreCase))
                {
                    LogSkippedRecord(
                        nameof(GetFundRaisinOflineDonation),
                        donationId,
                        $"Skipped because D_status is '{donationStatus ?? "null"}' instead of 'paid'.",
                        donationId,
                        donationDate);
                    return Task.CompletedTask;
                }

                decimal amount = ParseDecimal(amountText);
                decimal fee = ParseDecimal(feeText);
                decimal netAmount = amount - fee;

                if (netAmount <= 0)
                {
                    LogSkippedRecord(
                        nameof(GetFundRaisinOflineDonation),
                        donationId,
                        $"Skipped because net amount is 0 or less. Amount={amountText}, Fee={feeText}.",
                        donationId,
                        donationDate);
                    return Task.CompletedTask;
                }

                //Guid contactId = Guid.Empty;
                //if (HasValue(memberId))
                //{
                //    contactId = UpsertContact(donation, memberId);
                //}
                Guid contactId = ResolveOrCreateOfflineDonationDonor(donation);

                if (contactId == Guid.Empty)
                {
                    LogSkippedRecord(
                        nameof(GetFundRaisinOflineDonation),
                        donationId,
                        $"Skipped because donor is required and could not be resolved. MemberId={memberId}, Email={NullIfMissing(donation.D_email) ?? string.Empty}.",
                        donationId,
                        donationDate);

                    return Task.CompletedTask;
                }

                Guid eventId = Guid.Empty;
                Guid campaignId = Guid.Empty;
                Guid appealId = Guid.Empty;
                Guid packageId = Guid.Empty;
                Guid designationId = Guid.Empty;

                if (HasValue(eventIdText))
                {
                    var eventList = GetEvent(eventIdText);
                    eventId = CheckAndUpdateEvent(
                    eventIdText.Trim(),
                    eventList,
                    out designationId);

                }

                // New lookup logic for Campaign / Appeal / Package
                ResolveEventLookups(
                    eventIdText,
                    NullIfMissing(donation.Page_id),
                    out campaignId,
                    out appealId,
                    out packageId);


                if (designationId == Guid.Empty && this.defaultPrimaryDesignationId != Guid.Empty)
                {
                    designationId = this.defaultPrimaryDesignationId;
                    _tracingService.Trace("Designation used from configuration default primary designation. DonationId={0}, DesignationId={1}", donationId, designationId);
                }

                //Guid paymentMethodId = ResolvePaymentMethodForOfflineDonation(donation);

                //if (paymentMethodId == Guid.Empty)
                //{
                //    LogSkippedRecord(
                //        nameof(GetFundRaisinOflineDonation),
                //        donationId,
                //        $"Skipped because payment method could not be resolved or created. PaymentMethod={paymentMethod}.",
                //        donationId,
                //        donationDate);
                //    return Task.CompletedTask;
                //}


                var transactionConditions = new List<ConditionExpression>
        {
            new ConditionExpression("lrx_fundraisindonationid", ConditionOperator.Equal, ParseInt(donationId))
        };

                Entity existingTransaction = FindExistingRecord("msnfp_transaction", transactionConditions, new ColumnSet(false));

                Entity transactionEntity = existingTransaction == null
                    ? new Entity("msnfp_transaction")
                    : new Entity("msnfp_transaction", existingTransaction.Id);


                if (contactId != Guid.Empty)
                    transactionEntity["sifund_donor"] = new EntityReference("contact", contactId);

                if (campaignId != Guid.Empty)
                    transactionEntity["lrx_campaign"] = new EntityReference("campaign", campaignId);

                if (designationId != Guid.Empty)
                    transactionEntity["sifund_primarydesignation"] = new EntityReference("msnfp_designation", designationId);

                if (appealId != Guid.Empty)
                    transactionEntity["sifund_appeal"] = new EntityReference("sifund_appeal", appealId);

                if (packageId != Guid.Empty)
                    transactionEntity["sifund_package"] = new EntityReference("sifund_package", packageId);

                //if (paymentMethodId != Guid.Empty)
                //    transactionEntity["msnfp_transaction_paymentmethodid"] = new EntityReference("msnfp_paymentmethod", paymentMethodId);

                if (eventId != Guid.Empty)
                    transactionEntity["lrx_event"] = new EntityReference("lrx_event", eventId);

                transactionEntity["msnfp_amount"] = new Money(netAmount);

                transactionEntity["msnfp_bookdate"] = donationDate ?? DateTime.Now;

                //transactionEntity["sifund_paymenttypecode"] = new OptionSetValue(844060002);
                transactionEntity["statuscode"] = new OptionSetValue(856660001);
                transactionEntity["sifund_typecode"] = new OptionSetValue(844060000);
                transactionEntity["lrx_fundraisindonationid"] = ParseInt(donationId);


                if (!string.IsNullOrWhiteSpace(datePaid))
                    transactionEntity["lrx_fundraisindonationdate"] = datePaid;
                else if (!string.IsNullOrWhiteSpace(dateCreated))
                    transactionEntity["lrx_fundraisindonationdate"] = dateCreated;
                if (!transactionEntity.Contains("sifund_donor"))
                    throw new InvalidPluginExecutionException("Required field sifund_donor is missing.");

                if (!transactionEntity.Contains("msnfp_amount"))
                    throw new InvalidPluginExecutionException("Required field msnfp_amount is missing.");

                if (!transactionEntity.Contains("msnfp_bookdate"))
                    throw new InvalidPluginExecutionException("Required field msnfp_bookdate is missing.");
                _tracingService.Trace(
    "Tx fields | Donor={0} | Amount={1} | BookDate={2} | Campaign={3} | Designation={4} | Appeal={5} | Package={6} | PaymentMethod={7}",
    transactionEntity.Contains("sifund_donor"),
    transactionEntity.Contains("msnfp_amount"),
    transactionEntity.Contains("msnfp_bookdate"),
    transactionEntity.Contains("lrx_campaign"),
    transactionEntity.Contains("sifund_primarydesignation"),
    transactionEntity.Contains("sifund_appeal"),
    transactionEntity.Contains("sifund_package"),
    transactionEntity.Contains("msnfp_transaction_paymentmethodid"));
                if (existingTransaction == null)
                {
                    _tracingService.Trace(
       "Transaction required field check | DonorSet={0} | AmountSet={1} | BookDateSet={2} | ContactId={3} | NetAmount={4} | DonationDate={5}",
       transactionEntity.Contains("sifund_donor"),
       transactionEntity.Contains("msnfp_amount"),
       transactionEntity.Contains("msnfp_bookdate"),
       contactId,
       netAmount,
       donationDate.HasValue ? donationDate.Value.ToString("o") : "null");
                    Guid createdId = _service.Create(transactionEntity);




                }
                else if (this.updateTransaction)
                {
                    _tracingService.Trace(
    "Transaction required field check | DonorSet={0} | AmountSet={1} | BookDateSet={2} | ContactId={3} | NetAmount={4} | DonationDate={5}",
    transactionEntity.Contains("sifund_donor"),
    transactionEntity.Contains("msnfp_amount"),
    transactionEntity.Contains("msnfp_bookdate"),
    contactId,
    netAmount,
    donationDate.HasValue ? donationDate.Value.ToString("o") : "null");
                    _service.Update(transactionEntity);


                }
                else
                {
                    LogSkippedRecord(
                        nameof(GetFundRaisinOflineDonation),
                        donationId,
                        "Offline donation already exists and updateTransaction is false.",
                        donationId,
                        donationDate);
                }
            }
            catch (Exception ex)
            {
                _tracingService.Trace("Error in GetFundRaisinOflineDonation: {0}", ex.ToString());
                throw;
            }

            return Task.CompletedTask;
        }
        #endregion
        #region Offline Donation Donor Resolution Added on 22 July 2026
        //private Guid ResolvePaymentMethodForOfflineDonation(DonationModel donation)
        //{
        //    string incomingPaymentMethod = NullIfMissing(donation.Payment_method);

        //    if (!string.IsNullOrWhiteSpace(incomingPaymentMethod))
        //    {
        //        string paymentMethodName = $"{incomingPaymentMethod.Trim()} - Default";

        //        var conditions = new List<ConditionExpression>
        //{
        //    new ConditionExpression("msnfp_name", ConditionOperator.Equal, paymentMethodName)
        //};

        //        var existingPaymentMethod = FindExistingRecord("msnfp_paymentmethod", conditions, new ColumnSet(false));
        //        if (existingPaymentMethod != null)
        //        {
        //            _tracingService.Trace("Payment method resolved from incoming data. Name={0}, Id={1}", paymentMethodName, existingPaymentMethod.Id);
        //            return existingPaymentMethod.Id;
        //        }

        //        _tracingService.Trace("Incoming payment method not found. Name={0}", paymentMethodName);
        //    }

        //    if (this.defaultPaymentMethodId != Guid.Empty)
        //    {
        //        _tracingService.Trace("Payment method resolved from configuration default. Id={0}, Name={1}", this.defaultPaymentMethodId, this.paymentMethod);
        //        return this.defaultPaymentMethodId;
        //    }

        //    _tracingService.Trace("No payment method could be resolved from incoming data or configuration.");
        //    return Guid.Empty;
        //}
        private Guid ResolveOrCreateOfflineDonationDonor(DonationModel donation)
        {
            Entity existingContact = null;

            string memberIdText = NullIfMissing(donation.Member_id);

            // Step 1 - Check Member ID
            if (int.TryParse(memberIdText, out int memberId))
            {
                existingContact = FindExistingRecord(
                    "contact",
                    new List<ConditionExpression>
                    {
            new ConditionExpression("lrx_fundraisinmemberid", ConditionOperator.Equal, memberId),
            new ConditionExpression("statecode", ConditionOperator.Equal, 0)
                    });

                if (existingContact != null)
                {
                    _tracingService.Trace("Offline donor matched by Member ID. ContactId={0}", existingContact.Id);
                    return existingContact.Id;
                }
            }

            // Step 2 - Apply duplicate detection rules
            existingContact = FindContactByDuplicateRules(
                donation.D_fname,
                donation.D_lname,
                donation.D_email,
                NullIfMissing(donation.D_phone_mobile),
                ParseDate(donation.D_dob));

            if (existingContact != null)
            {
                _tracingService.Trace("Offline donor matched by duplicate detection rules. ContactId={0}", existingContact.Id);
                return existingContact.Id;
            }

            var contactFields = new Dictionary<string, object>();

            void AddIfValid(string key, object value)
            {
                if (value is string str)
                {
                    str = NullIfMissing(str);

                    if (string.IsNullOrWhiteSpace(str))
                        return;

                    contactFields[key] = str;
                    return;
                }

                if (value != null)
                {
                    contactFields[key] = value;
                }
            }

           
            int? memberIdValue = null;
            if (!string.IsNullOrWhiteSpace(memberIdText) &&
                memberIdText != "0" &&
                int.TryParse(memberIdText, out int parsedMemberId))
            {
                memberIdValue = parsedMemberId;
            }

            AddIfValid("firstname", donation.D_fname);
            //AddIfValid("middlename", donation.D_middle);
            AddIfValid("lastname", donation.D_lname);
            AddIfValid("emailaddress1", donation.D_email);
            AddIfValid("mobilephone", donation.D_phone);
            AddIfValid("telephone1", donation.D_phone_home);

            AddIfValid("address1_line1",
                string.Join(" ",
                    new[]
                    {
                NullIfMissing(donation.D_address_unit),
                NullIfMissing(donation.D_address_number),
                NullIfMissing(donation.D_address_street)
                    }.Where(x => !string.IsNullOrWhiteSpace(x))));

            AddIfValid("address1_line2", donation.D_address_2);
            AddIfValid("address1_city", donation.D_address_suburb);
            AddIfValid("address1_postalcode", donation.D_address_pcode);
            AddIfValid("address1_stateorprovince", donation.D_address_state);
            AddIfValid("address1_country", donation.D_address_country);

            var dobText = NullIfMissing(donation.D_dob);
            if (!string.IsNullOrWhiteSpace(dobText) &&
                !string.Equals(dobText, "0000-00-00", StringComparison.OrdinalIgnoreCase) &&
                DateTime.TryParse(dobText, out DateTime birthDate))
            {
                AddIfValid("birthdate", birthDate.Date);
            }

            if (memberIdValue.HasValue)
            {
                AddIfValid("lrx_fundraisinmemberid", memberIdValue.Value);
            }

            Entity newContact = new Entity("contact");
            foreach (var field in contactFields)
            {
                newContact[field.Key] = field.Value;
            }

            Guid createdContactId = _service.Create(newContact);
            _tracingService.Trace("Offline donor new contact created. ContactId={0}", createdContactId);
            return createdContactId;
        }
        #endregion
        #region Added by Samir on 24 july 2026
        public Task GetFundraisinWaveRecords()
        {
            WaveModel wave = GetInputRecord<WaveModel>();

            _tracingService.Trace("Wave Sync Started");
            _tracingService.Trace("Raw JSON:");
            _tracingService.Trace(_jsonInput);

            if (wave == null)
            {
                _tracingService.Trace("Wave JSON is null.");

                LogSkippedRecord(
                    nameof(GetFundraisinWaveRecords),
                    "Fundraisin Wave JSON",
                    "Skipped: input wave payload is null.",
                    "Unknown Wave",
                    null);

                return Task.CompletedTask;
            }

            if (string.IsNullOrWhiteSpace(wave.wave_id))
            {
                LogSkippedRecord(
                    nameof(GetFundraisinWaveRecords),
                    wave.wave_id,
                    "Skipped: Wave Id is empty.",
                    wave.wave_name,
                    null);

                return Task.CompletedTask;
            }

            if (!int.TryParse(NullIfMissing(wave.event_id), out int eventId))
            {
                _tracingService.Trace("Invalid Event Id.");

                LogSkippedRecord(
                    nameof(GetFundraisinWaveRecords),
                    wave.wave_id,
                    $"Skipped: Invalid Event Id '{wave.event_id}'.",
                    wave.wave_name,
                    null);

                return Task.CompletedTask;
            }



            Entity existingEvent = FindExistingRecord(
                "lrx_event",
                new List<ConditionExpression>()
                {
                new ConditionExpression(
                "lrx_fundraisineventid",
                ConditionOperator.Equal,
                eventId)
                });

            if (existingEvent == null)
            {
                _tracingService.Trace($"Event not found : {eventId}");

                LogSkippedRecord(
                    nameof(GetFundraisinWaveRecords),
                    wave.wave_id,
                    $"Skipped: Event not found for Fundraisin Event Id '{eventId}'.",
                    wave.wave_name,
                    null);

                return Task.CompletedTask;
            }


            EntityReference parentWaveReference = null;


            if (NullIfMissing(wave.parent_id) != null)
            {
                Entity parentWave = FindExistingRecord(
                    "lrx_waves",
                    new List<ConditionExpression>()
                    {
            new ConditionExpression(
                "lrx_waveid",
                ConditionOperator.Equal,
                NullIfMissing(wave.parent_id))
                    });

                if (parentWave != null)
                {
                    parentWaveReference = new EntityReference("lrx_waves", parentWave.Id);
                }
                else
                {
                    _tracingService.Trace($"Parent Wave '{wave.parent_id}' not found. Continuing without parent.");

                    LogSkippedRecord(
                        nameof(GetFundraisinWaveRecords),
                        wave.wave_id,
                        $"Parent Wave '{wave.parent_id}' not found. Wave created without parent.",
                        wave.wave_name,
                        null);
                }
            }

            Entity existingWave = FindExistingRecord(
    "lrx_waves",
    new List<ConditionExpression>()
    {
        new ConditionExpression(
            "lrx_waveid",
            ConditionOperator.Equal,
            wave.wave_id)
    });

            decimal price = 0;
            decimal.TryParse(
                NullIfMissing(wave.wave_price),
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out price);

            int limit = 0;
            int.TryParse(NullIfMissing(wave.wave_limit), out limit);

            DateTime waveDate;
            bool validWaveDate =
                DateTime.TryParse(NullIfMissing(wave.wave_date), out waveDate);


            if (existingWave != null)
            {
                Entity updateWave = new Entity("lrx_waves", existingWave.Id);

                updateWave["lrx_waveid"] = NullIfMissing(wave.wave_id);
                updateWave["lrx_event"] =
                    new EntityReference("lrx_event", existingEvent.Id);

                if (NullIfMissing(wave.wave_name) != null)
                    updateWave["lrx_wavename"] = NullIfMissing(wave.wave_name);

                if (NullIfMissing(wave.wave_code) != null)
                    updateWave["lrx_wavecode"] = NullIfMissing(wave.wave_code);

                if (NullIfMissing(wave.wave_description) != null)
                    updateWave["lrx_wavedescription"] = NullIfMissing(wave.wave_description);

                if (NullIfMissing(wave.wave_tag) != null)
                    updateWave["lrx_wavetag"] = NullIfMissing(wave.wave_tag);

                if (NullIfMissing(wave.wave_time) != null)
                    updateWave["lrx_wavetime"] = NullIfMissing(wave.wave_time);

                updateWave["lrx_wavelimit"] = limit;
                updateWave["lrx_waveprice"] = new Money(price);

                if (validWaveDate)
                    updateWave["lrx_wavedate"] = waveDate;

                if (parentWaveReference != null)
                    updateWave["lrx_parentwave"] = parentWaveReference;

                _service.Update(updateWave);

                _tracingService.Trace($"Wave Updated Successfully : {wave.wave_name}");
            }
            else
            {
                Entity newWave = new Entity("lrx_waves");

                newWave["lrx_waveid"] = NullIfMissing(wave.wave_id);
                newWave["lrx_event"] =
                    new EntityReference("lrx_event", existingEvent.Id);

                if (NullIfMissing(wave.wave_name) != null)
                    newWave["lrx_wavename"] = NullIfMissing(wave.wave_name);

                if (NullIfMissing(wave.wave_code) != null)
                    newWave["lrx_wavecode"] = NullIfMissing(wave.wave_code);

                if (NullIfMissing(wave.wave_description) != null)
                    newWave["lrx_wavedescription"] = NullIfMissing(wave.wave_description);

                if (NullIfMissing(wave.wave_tag) != null)
                    newWave["lrx_wavetag"] = NullIfMissing(wave.wave_tag);

                if (NullIfMissing(wave.wave_time) != null)
                    newWave["lrx_wavetime"] = NullIfMissing(wave.wave_time);

                newWave["lrx_wavelimit"] = limit;
                newWave["lrx_waveprice"] = new Money(price);

                if (validWaveDate)
                    newWave["lrx_wavedate"] = waveDate;

                if (parentWaveReference != null)
                    newWave["lrx_parentwave"] = parentWaveReference;

                Guid waveRecordId = _service.Create(newWave);

                _tracingService.Trace($"Wave Created Successfully : {waveRecordId}");
            }

            _tracingService.Trace("Wave Completed");

            return Task.CompletedTask;
        }
        #endregion
        #region Sync Event Products added on 29 july 2026
        
        public Task GetFundraisinEventProducts()
        {
            _tracingService.Trace("Event Products Sync Started");

            EventProductModel eventProduct = GetInputRecord<EventProductModel>();

            if (eventProduct == null)
            {
                _tracingService.Trace("No Event Product record received.");

                LogSkippedRecord(
                    "GetFundraisinEventProducts",
                    "Not Found",
                    "Event Product input is null.",
                    "Not Found",
                    null);

                return Task.CompletedTask;
            }

            Guid eventId = Guid.Empty;
            Guid productId = Guid.Empty;
            ProductModel product = null;

            // ---------------- Event Lookup ----------------

            _tracingService.Trace("Looking up Event. Fundraisin Event Id={0}", eventProduct.Event_Id);

            Entity existingEvent = FindExistingRecord(
                "lrx_event",
                new List<ConditionExpression>
                {
            new ConditionExpression(
                "lrx_fundraisineventid",
                ConditionOperator.Equal,
                NullIfMissing(eventProduct.Event_Id))
                });

            if (existingEvent == null)
            {
                _tracingService.Trace("Event not found.");

                LogSkippedRecord(
                    "GetFundraisinEventProducts",
                    eventProduct.Event_Id,
                    $"Related Event not found. Event Id : {eventProduct.Event_Id}",
                    eventProduct.Product_Id,
                    ParseDate(eventProduct.Date_Created));

                return Task.CompletedTask;
            }

            eventId = existingEvent.Id;

            _tracingService.Trace("Event found. Event GUID={0}", eventId);

            // ---------------- Inventory Product Lookup ----------------

            int shopProductNumber;

            if (!int.TryParse(NullIfMissing(eventProduct.Shop_Product_Id), out shopProductNumber))
            {
                _tracingService.Trace("Invalid Shop Product Id : {0}", eventProduct.Shop_Product_Id);

                LogSkippedRecord(
                    "GetFundraisinEventProducts",
                    eventProduct.Shop_Product_Id,
                    "Invalid Shop Product Id.",
                    eventProduct.Shop_Product_Id,
                    ParseDate(eventProduct.Date_Created));

                return Task.CompletedTask;
            }

            _tracingService.Trace("Looking up Inventory Product. Fundraisin Product Id={0}", shopProductNumber);

            Entity existingProduct = FindExistingRecord(
                "lrx_inventoryproduct",
                new List<ConditionExpression>
                {
           new ConditionExpression(
    "lrx_fundraisinproductid",
    ConditionOperator.Equal,
    shopProductNumber)
                });

            if (existingProduct != null)
            {
                productId = existingProduct.Id;
                _tracingService.Trace("Inventory Product found. Product GUID={0}", productId);
            }
            else
            {
                _tracingService.Trace("Inventory Product not found. Retrieving Product from Fundraisin.");

                product = GetProduct(eventProduct.Shop_Product_Id);

                if (product == null)
                {
                    _tracingService.Trace("Unable to retrieve Product from Fundraisin.");

                    LogSkippedRecord(
                        "GetFundraisinEventProducts",
                        eventProduct.Product_Id,
                        "Unable to retrieve Product from Fundraisin.",
                        eventProduct.Product_Id,
                        ParseDate(eventProduct.Date_Created));

                    return Task.CompletedTask;
                }

                _tracingService.Trace(
                    "Product retrieved successfully. Product Name={0}, Product Id={1}",
                    product.product_name,
                    product.product_id);

                Entity inventoryProduct = new Entity("lrx_inventoryproduct")
                {
                    ["lrx_name"] = NullIfMissing(product.product_name),
                    ["lrx_producttype"] = new OptionSetValue(
                        product.product_type?.Trim() == "ecard"
                            ? 856660001
                            : 856660000),

                    ["lrx_productprice"] = new Money(ParseDecimal(NullIfMissing(product.product_price))),
                    ["lrx_productcost"] = new Money(ParseDecimal(NullIfMissing(product.product_cost))),
                    ["lrx_stocklevels"] = int.TryParse(NullIfMissing(product.product_stock), out int stock) ? stock : 0,
                    ["lrx_minimumbuyqty"] = int.TryParse(NullIfMissing(product.min_buy_limit), out int minQty) ? minQty : 0,
                    ["lrx_maximumbuyqty"] = int.TryParse(NullIfMissing(product.max_buy_limit), out int maxQty) ? maxQty : 0,
                    ["lrx_crmid"] = NullIfMissing(product.crm_product_id),
                    ["lrx_description"] = NullIfMissing(product.product_description),
                  
                    ["lrx_fundraisinproductid"] = shopProductNumber
                };

                productId = _service.Create(inventoryProduct);

                _tracingService.Trace("Inventory Product created successfully. Product GUID={0}", productId);
            }

            // ---------------- Event Product Lookup ----------------

            _tracingService.Trace("Looking up Event Product.");

            Entity existingEventProduct = FindExistingRecord(
                    "lrx_eventproduct",
                    new List<ConditionExpression>
                     {
                            new ConditionExpression(
                            "lrx_fundraisineventproductid",
                            ConditionOperator.Equal,
                            int.Parse(eventProduct.Product_Id))
                    });

            Entity eventProductEntity = new Entity("lrx_eventproduct")
            {
                ["lrx_event"] = new EntityReference("lrx_event", eventId),
                ["lrx_product"] = new EntityReference("lrx_inventoryproduct", productId),
                ["lrx_priceperproduct"] = new Money(ParseDecimal(NullIfMissing(eventProduct.Product_Price))),
                ["lrx_fundraisineventproductid"] = int.Parse(eventProduct.Product_Id),
                ["lrx_fundraisineventid"] = int.Parse(NullIfMissing(eventProduct.Event_Id)),
                ["lrx_fundraisininvenoryproductid"] = int.Parse(NullIfMissing(eventProduct.Shop_Product_Id))

            };


            if (existingProduct != null)
            {
                if (existingProduct.Contains("lrx_name"))
                {
                    eventProductEntity["lrx_name"] = existingProduct["lrx_name"].ToString();
                }
            }
            else if (product != null)
            {
                eventProductEntity["lrx_name"] = NullIfMissing(product.product_name);
            }

            if (existingEventProduct == null)
            {
                Guid eventProductId = _service.Create(eventProductEntity);

                _tracingService.Trace("Event Product created successfully. GUID={0}", eventProductId);
            }
            else
            {
                eventProductEntity.Id = existingEventProduct.Id;

                _service.Update(eventProductEntity);

                _tracingService.Trace("Event Product updated successfully. GUID={0}", existingEventProduct.Id);
            }

            _tracingService.Trace("Event Products Sync Completed");

            return Task.CompletedTask;
        }

        #endregion Completed event products

        #region Sync Pages

        public Task GetFundraisinPages()
        {
            _tracingService.Trace("Pages Sync Started");

            PageModel page = GetInputRecord<PageModel>();

            if (page == null)
            {
                _tracingService.Trace("No Page record received.");

                LogSkippedRecord(
                    "GetFundraisinPages",
                    "Not Found",
                    "Page input is null.",
                    "Not Found",
                    null);

                return Task.CompletedTask;
            }

            Entity existingPage = FindExistingRecord(
                "lrx_fundraisinpage",
                new List<ConditionExpression>
                {
            new ConditionExpression(
                "lrx_fundraisinpagesid",
                ConditionOperator.Equal,
                int.Parse(page.Page_Id))
                });

            Entity pageEntity = new Entity("lrx_fundraisinpage")
            {
                ["lrx_pagename"] = NullIfMissing(page.Page_Name),
                ["lrx_fundraisinpagesid"] = int.Parse(page.Page_Id)
            };

            if (this.defaultCampaignId != Guid.Empty)
            {
                pageEntity["lrx_campaign"] = new EntityReference("campaign", this.defaultCampaignId);
            }

            if (this.defaultPrimaryDesignationId != Guid.Empty)
            {
                pageEntity["lrx_designation"] = new EntityReference("msnfp_designation", this.defaultPrimaryDesignationId);
            }

            if (existingPage == null)
            {
                Guid pageId = _service.Create(pageEntity);

                _tracingService.Trace("Page created successfully. GUID={0}", pageId);
            }
            else
            {
                pageEntity.Id = existingPage.Id;

                _service.Update(pageEntity);

                _tracingService.Trace("Page updated successfully. GUID={0}", existingPage.Id);
            }

            _tracingService.Trace("Pages Sync Completed");

            return Task.CompletedTask;
        }

        #endregion
        //reusable functions
        #region 23 july 2026
        private Entity FindContactByDuplicateRules(string firstName, string lastName, string email, string mobile, DateTime? dob)
        {
            firstName = NullIfMissing(firstName);
            lastName = NullIfMissing(lastName);
            email = NullIfMissing(email);
            mobile =  NullIfMissing(mobile);

            bool useEmail = _useFirstNameLastNameEmail;
            bool useMobile = _useFirstNameLastNameMobile;
            bool useDob = _useFirstNameLastNameDob;

            // Default behaviour if no toggle is enabled
            if (!useEmail && !useMobile && !useDob)
            {
                useEmail = true;
            }

            var rootFilter = new FilterExpression(LogicalOperator.Or);

            // Rule 1 : First Name + Last Name + Email
            if (useEmail &&
                !string.IsNullOrWhiteSpace(firstName) &&
                !string.IsNullOrWhiteSpace(lastName) &&
                !string.IsNullOrWhiteSpace(email))
            {
                var emailFilter = new FilterExpression(LogicalOperator.And);
                emailFilter.AddCondition("firstname", ConditionOperator.Equal, firstName);
                emailFilter.AddCondition("lastname", ConditionOperator.Equal, lastName);
                emailFilter.AddCondition("emailaddress1", ConditionOperator.Equal, email);

                rootFilter.AddFilter(emailFilter);

                _tracingService.Trace("Duplicate rule added: FirstName + LastName + Email");
            }

            // Rule 2 : First Name + Last Name + Mobile
            if (useMobile &&
                !string.IsNullOrWhiteSpace(firstName) &&
                !string.IsNullOrWhiteSpace(lastName) &&
                !string.IsNullOrWhiteSpace(mobile))
            {
                var mobileFilter = new FilterExpression(LogicalOperator.And);
                mobileFilter.AddCondition("firstname", ConditionOperator.Equal, firstName);
                mobileFilter.AddCondition("lastname", ConditionOperator.Equal, lastName);
                mobileFilter.AddCondition("mobilephone", ConditionOperator.Equal, mobile);

                rootFilter.AddFilter(mobileFilter);

                _tracingService.Trace("Duplicate rule added: FirstName + LastName + Mobile");
            }

            // Rule 3 : First Name + Last Name + DOB
            if (useDob &&
                !string.IsNullOrWhiteSpace(firstName) &&
                !string.IsNullOrWhiteSpace(lastName) &&
                dob.HasValue)
            {
                var dobFilter = new FilterExpression(LogicalOperator.And);
                dobFilter.AddCondition("firstname", ConditionOperator.Equal, firstName);
                dobFilter.AddCondition("lastname", ConditionOperator.Equal, lastName);
                dobFilter.AddCondition("birthdate", ConditionOperator.Equal, dob.Value.Date);

                rootFilter.AddFilter(dobFilter);

                _tracingService.Trace("Duplicate rule added: FirstName + LastName + DOB");
            }

            // Nothing to search with
            if (rootFilter.Filters.Count == 0)
            {
                _tracingService.Trace("Duplicate detection skipped because required values are missing.");
                return null;
            }

            var query = new QueryExpression("contact")
            {
                TopCount = 1,
                ColumnSet = new ColumnSet(
                    "contactid",
                    "firstname",
                    "lastname",
                    "emailaddress1",
                    "mobilephone",
                    "birthdate",
                    "fullname",
                    "lrx_fundraisinmemberid")
            };

            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0);
            query.Criteria.AddFilter(rootFilter);

            var result = _service.RetrieveMultiple(query);

            return result.Entities.FirstOrDefault();
        }
        #endregion
        #region Updated by Samir on 13/7/2026
        private void LogSkippedRecord(string methodName, string recordId, string reason, string recordName = null,
    DateTime? recordCreated = null)
        {
            try
            {
                Entity log = new Entity("lrx_fundraisinlogs");

                log["lrx_name"] = $"{methodName} - {recordId}";
                log["lrx_methodname"] = methodName;
                log["lrx_recordid"] = recordId;
                log["lrx_skipreason"] = reason;
                if (!string.IsNullOrWhiteSpace(recordName))
                    log["lrx_recordname"] = recordName;

                if (recordCreated.HasValue)
                    log["lrx_recordcreated"] = recordCreated.Value;
                _service.Create(log);
            }
            catch (Exception ex)
            {
                _tracingService.Trace("Failed to create skip log.");
                _tracingService.Trace(ex.ToString());
            }
        }
        private T GetInputRecord<T>()
        {
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                }
            };

            return JsonConvert.DeserializeObject<T>(_jsonInput, settings);
        }

        private string NullIfMissing(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            var cleaned = value.Trim();

            if (cleaned == "0" || cleaned == "0000-00-00 00:00:00" || cleaned == "0000-00-00")
                return null;

            return cleaned;
        }
        private string BuildPhone(string suffix, string phone)
        {
            string cleanedSuffix = NullIfMissing(suffix);
            string cleanedPhone = NullIfMissing(phone);

            if (string.IsNullOrWhiteSpace(cleanedSuffix) && string.IsNullOrWhiteSpace(cleanedPhone))
                return null;

            string result = $"{cleanedSuffix ?? string.Empty}{cleanedPhone ?? string.Empty}".Trim();

            return string.IsNullOrWhiteSpace(result) ? null : result;
        }
        #endregion
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

        #region Added 24 july 2026 getting participants option record to get ticket id for registrations

        public ParticipantOptionModel GetParticipantTicketOption(string historyId, string memberId)
        {
            string endpoint = baseURL + "participantsoptions";
            string requestUri = $"{endpoint}?apikey={apikey}&history_id={historyId}&member_id={memberId}&option_type=tickets";

            string csvContent = "";
            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.Timeout = TimeSpan.FromSeconds(15);
                httpClient.DefaultRequestHeaders.ConnectionClose = true;
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));

                HttpResponseMessage result = httpClient.GetAsync(requestUri).Result;
                if (!result.IsSuccessStatusCode)
                {
                    _tracingService.Trace("participantoptions API failed with status code: " + result.StatusCode);
                    return null;
                }

                csvContent = result.Content.ReadAsStringAsync().Result;
            }

            var records = ParseCsvHelper<ParticipantOptionModel, ParticipantOptionModelMap>(csvContent);
            return records.FirstOrDefault();
        }
        #endregion
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
            //else
            //if (dateFrom != "" && dateTo != "")
            //{
            //    requestUri = string.Format("{0}?apikey={1}&date_from={2}&date_to={3}", (object)apiEndpoint, (object)this.apikey, (object)this.dateFrom, (object)this.dateTo);
            //}
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
            string responseContent = "";

            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.Timeout = TimeSpan.FromSeconds(30);
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));

                HttpResponseMessage result = httpClient.GetAsync(requestUri).Result;
                responseContent = result.Content.ReadAsStringAsync().Result;

                if (!result.IsSuccessStatusCode)
                {
                    this._tracingService.Trace(
                        "Fundraisin API failed | RequestUri={0} | HistoryId={1} | StatusCode={2}",
                        requestUri,
                        historyID,
                        result.StatusCode.ToString());
                    return string.Empty;
                }

                if (!string.IsNullOrWhiteSpace(responseContent) &&
                    responseContent.TrimStart().StartsWith("<"))
                {
                    this._tracingService.Trace(
                        "Fundraisin API returned HTML instead of data | RequestUri={0} | HistoryId={1}",
                        requestUri,
                        historyID);
                    return string.Empty;
                }
            }

            return responseContent;
        }

        public string CallFundRaisinAPIAllData(object apiEndpoint, string customDate = "", string queryParam="")
        {

            string requestUri = "";

            requestUri = string.Format("{0}?apikey={1}&{2}", (object)apiEndpoint, (object)this.apikey, queryParam);
            this._tracingService.Trace("Request URL" + requestUri);

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

        public List<T> GetData<T, TMap>(string baseUrl, string endpoint, string customDate = "", string query = null)
        where TMap : ClassMap<T>, new()
        {
            string fullUrl = baseUrl + endpoint;
            string csvContent = CallFundRaisinAPI((object)fullUrl, customDate);
            return ParseCsvHelper<T, TMap>(csvContent);
        }

        public List<T> GetAllData<T, TMap>(string baseUrl, string endpoint, string customDate = "", string queryParams="")
        where TMap : ClassMap<T>, new()
        {
            string fullUrl = baseUrl + endpoint;
            string csvContent = CallFundRaisinAPIAllData((object)fullUrl, customDate, queryParams);
            return ParseCsvHelper<T, TMap>(csvContent);
        }

        private Guid UpsertContact(dynamic matchDonationID, string TransMemberID)
        {


            // Define search conditions to find an existing contact
            string firstName = NullIfMissing(matchDonationID.D_fname) ?? string.Empty;
            string lastName = NullIfMissing(matchDonationID.D_lname) ?? string.Empty;
            string email = NullIfMissing(matchDonationID.D_email) ?? string.Empty;
            int memberIdValue = 0;


            Entity existingContact = null;
            if (HasValue(TransMemberID) &&
     int.TryParse(TransMemberID, out memberIdValue) &&
     memberIdValue != 0)
            {
                existingContact = FindExistingRecord(
                    "contact",
                    new List<ConditionExpression>
                    {
            new ConditionExpression(
                "lrx_fundraisinmemberid",
                ConditionOperator.Equal,
                memberIdValue)
                    });
            }
            if (existingContact == null)
            {
                existingContact = FindContactByDuplicateRules(
                    firstName,
                    lastName,
                    email,                 
                    NullIfMissing(matchDonationID.D_phone_mobile),
                    null);
            }


            //Entity existingContact = FindExistingRecord("contact", contactSearchConditions);
            // var addressStreet = NullIfMissing(matchDonationID.D_address_number) + NullIfMissing(matchDonationID.D_address_street);
            var addressStreet = string.Join(" ", new[]
                {
                        NullIfMissing(matchDonationID.D_address_number),
                        NullIfMissing(matchDonationID.D_address_street)
                }.Where(x => !string.IsNullOrWhiteSpace(x)));


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
            AddIfValid("firstname", NullIfMissing(matchDonationID.D_fname));
            AddIfValid("lastname", NullIfMissing(matchDonationID.D_lname));
            AddIfValid("emailaddress1", NullIfMissing(matchDonationID.D_email));
            AddIfValid("telephone1", NullIfMissing(matchDonationID.D_phone));
            string mobilePhone = NullIfMissing(matchDonationID.D_phone_mobile);

            AddIfValid("mobilephone", mobilePhone);
            AddIfValid("address1_line1", addressStreet);
            AddIfValid("address1_city", NullIfMissing(matchDonationID.D_address_suburb));
            AddIfValid("address1_postalcode", NullIfMissing(matchDonationID.D_address_pcode));
            AddIfValid("address1_stateorprovince", matchDonationID.D_address_state);
            AddIfValid("address1_country", NullIfMissing(matchDonationID.D_address_country));

            // Conditional integer field
            if (memberIdValue != 0)
            {
                AddIfValid("lrx_fundraisinmemberid", memberIdValue);
            }

            //AddIfValid("lrx_fundraisinmemberid", memberIdValue);

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
            Entity existingContact = null;

            if (int.TryParse(NullIfMissing(matchSalesID.member_id), out int contactMemberId))
            {
                existingContact = FindExistingRecord(
                    "contact",
                    new List<ConditionExpression>
                    {
            new ConditionExpression(
                "lrx_fundraisinmemberid",
                ConditionOperator.Equal,
                contactMemberId)
                    });
            }

            if (existingContact == null)
            {
                existingContact = FindContactByDuplicateRules(
                    matchSalesID.first_name,
                    matchSalesID.last_name,
                    matchSalesID.email,               
                    NullIfMissing(matchSalesID.mobile),
                    null // Sales payload has no DOB
                );
            }

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
            var memberId = NullIfMissing(matchSalesID.member_id);

            if (int.TryParse(memberId, out int memberIdValue))
            {
                AddIfValid("lrx_fundraisinmemberid", memberIdValue);
            }
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
            string mobile = NullIfMissing(raffleSales.mobile);
            DateTime? dob = ParseDate(raffleSales.dob);

            // Define search conditions to find an existing contact
            Entity existingContact = FindContactByDuplicateRules(
                                    raffleSales.first_name,
                                    raffleSales.last_name,
                                    raffleSales.email,
                                    mobile,
                                    dob);




            var addressStreet = string.Join(" ",
                                new[]
                                    {
                                        NullIfMissing(raffleSales.address_unit),
                                        NullIfMissing(raffleSales.address_number),
                                        NullIfMissing(raffleSales.address_street)
                                    }.Where(x => !string.IsNullOrWhiteSpace(x)));

            // Prepare contact attributes
            var contactAttributes = new Dictionary<string, object>();

            void AddIfNotEmpty(string key, string value)
            {
                value = NullIfMissing(value);

                if (!string.IsNullOrWhiteSpace(value))
                    contactAttributes[key] = value;
            }

            // Build attributes safely
            AddIfNotEmpty("firstname", raffleSales.first_name);
            AddIfNotEmpty("lastname", raffleSales.last_name);
            AddIfNotEmpty("emailaddress1", raffleSales.email);
            AddIfNotEmpty("telephone1", raffleSales.phone);
            AddIfNotEmpty("mobilephone", mobile);
            AddIfNotEmpty("address1_line1", addressStreet);
            AddIfNotEmpty("address1_city", raffleSales.address_suburb);
            AddIfNotEmpty("address1_postalcode", raffleSales.address_postcode);
            AddIfNotEmpty("address1_stateorprovince", raffleSales.address_state);
            AddIfNotEmpty("address1_country", raffleSales.address_country);
            if (dob.HasValue)
            {
                contactAttributes["birthdate"] = dob.Value.Date;
            }

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

        #region Updated by samir on 20 july 2026
        private void UpsertTicketHolderRecord(Guid ticketHolderContactId, Guid eventId, Guid parentRegistrationId, Guid waveId, TicketHolderModel ticketHolder)
        {
            if (ticketHolderContactId == Guid.Empty || eventId == Guid.Empty || parentRegistrationId == Guid.Empty)
                return;

            Entity existingTicketHolder = FindExistingRecord("lrx_ticketholders", new List<ConditionExpression>
    {
        new ConditionExpression("lrx_tickerholder", ConditionOperator.Equal, ticketHolderContactId),
        new ConditionExpression("lrx_event", ConditionOperator.Equal, eventId),
        new ConditionExpression("lrx_parentregistration", ConditionOperator.Equal, parentRegistrationId)
    });

            Entity ticketHolderEntity = new Entity("lrx_ticketholders")
            {
                ["lrx_tickerholder"] = new EntityReference("contact", ticketHolderContactId),
                ["lrx_event"] = new EntityReference("lrx_event", eventId),
                ["lrx_parentregistration"] = new EntityReference("lrx_registrations", parentRegistrationId),
                ["lrx_wave"] = waveId != Guid.Empty ? new EntityReference("lrx_waves", waveId) : null,
                ["lrx_emergencycontact"] = string.Join(" ",
                new[]
                {
                    NullIfMissing(ticketHolder.g_emergency_contact),
                    NullIfMissing(ticketHolder.g_emergency_contact_alt)
                }.Where(x => !string.IsNullOrWhiteSpace(x))),

                ["lrx_emergencycontactnumber"] = NullIfMissing(ticketHolder.g_emergency_phone),

                ["lrx_emergencycontacttype"] = NullIfMissing(ticketHolder.g_emergency_contact_type),

                ["lrx_guardianname"] = string.Join(" ",
                new[]
                {
                       NullIfMissing(ticketHolder.g_guardian_fname),
                       NullIfMissing(ticketHolder.g_guardian_lname)
                }.Where(x => !string.IsNullOrWhiteSpace(x))),

                ["lrx_guardianphone"] = NullIfMissing(ticketHolder.g_guardian_phone),

                ["lrx_guardianemail"] = NullIfMissing(ticketHolder.g_guardian_email),

                ["lrx_guardianrelationship"] = NullIfMissing(ticketHolder.g_guardian_relationship)
            };

            var cleanedGuestId = NullIfMissing(ticketHolder.guest_id);
            if (!string.IsNullOrWhiteSpace(cleanedGuestId) && int.TryParse(cleanedGuestId, out int guestIdValue))
            {
                ticketHolderEntity["lrx_fundraisinguestid"] = guestIdValue;
            }

            if (existingTicketHolder != null)
            {
                ticketHolderEntity.Id = existingTicketHolder.Id;
                _service.Update(ticketHolderEntity);
            }
            else
            {
                _service.Create(ticketHolderEntity);
            }
        }
        #endregion

        private Guid CheckAndUpdateEvent(
            string eventId,
            EventModel eventRecord,
            out Guid designationId)
        {
            //campaignId = Guid.Empty;
            //appealId = Guid.Empty;
            //packageId = Guid.Empty;
            designationId = Guid.Empty;

            EventModel matchedEvent = eventRecord;
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
               

                if (existingEvent.Contains("lrx_designation") && existingEvent["lrx_designation"] is EntityReference designationRef)
                    designationId = designationRef.Id;

                return existingEvent.Id;
            }
            else
            {
                return Guid.Empty;
            }
        }

        #region Added this method to set campaign, appeal and package on event 28 july 2026
        private void ResolveEventLookups(
    string eventId,
    string pageId,
    out Guid campaignId,
    out Guid appealId,
    out Guid packageId)
        {
            campaignId = Guid.Empty;
            appealId = Guid.Empty;
            packageId = Guid.Empty;

            // ---------------- Campaign ----------------

            Entity campaign = null;

            if (HasValue(eventId))
            {
                campaign = FindExistingRecord(
                    "campaign",
                    new List<ConditionExpression>
                    {
                new ConditionExpression(
                    "lrx_fundraisincampaignid",
                    ConditionOperator.Equal,
                    eventId)
                    });
            }

            if (campaign == null && HasValue(pageId))
            {
                campaign = FindExistingRecord(
                    "campaign",
                    new List<ConditionExpression>
                    {
                new ConditionExpression(
                    "lrx_fundraisincampaignid",
                    ConditionOperator.Equal,
                    pageId)
                    });
            }

            campaignId = campaign?.Id ?? defaultCampaignId;

            // ---------------- Appeal ----------------

            Entity appeal = null;

            if (HasValue(eventId))
            {
                appeal = FindExistingRecord(
                    "sifund_appeal",
                    new List<ConditionExpression>
                    {
                new ConditionExpression(
                    "lrx_fundraisinappealid",
                    ConditionOperator.Equal,
                    eventId)
                    });
            }

            if (appeal == null && HasValue(pageId))
            {
                appeal = FindExistingRecord(
                    "sifund_appeal",
                    new List<ConditionExpression>
                    {
                new ConditionExpression(
                    "lrx_fundraisinappealid",
                    ConditionOperator.Equal,
                    pageId)
                    });
            }

            if (appeal != null)
                appealId = appeal.Id;

            // ---------------- Package ----------------

            Entity package = null;

            if (HasValue(eventId))
            {
                package = FindExistingRecord(
                    "sifund_package",
                    new List<ConditionExpression>
                    {
                new ConditionExpression(
                    "lrx_fundraisinpackageid",
                    ConditionOperator.Equal,
                    eventId)
                    });
            }

            if (package == null && HasValue(pageId))
            {
                package = FindExistingRecord(
                    "sifund_package",
                    new List<ConditionExpression>
                    {
                new ConditionExpression(
                    "lrx_fundraisinpackageid",
                    ConditionOperator.Equal,
                    pageId)
                    });
            }

            if (package != null)
                packageId = package.Id;
        }
        #endregion

        #region Added by samir on 7 august 2026 to resolve Event Campaign

        private Guid ResolveEventCampaign(string crmEventId)
        {
            Entity campaign = null;

            if (HasValue(crmEventId))
            {
                campaign = FindExistingRecord(
                    "campaign",
                    new List<ConditionExpression>
                    {
                new ConditionExpression(
                    "codename",
                    ConditionOperator.Equal,
                    crmEventId)
                    });
            }

            return campaign?.Id ?? defaultCampaignId;
        }

        private void ResolveTransactionLookups(
    string eventId,
    string pageId,
    string glCode1,
    string glCode2,
    out Guid campaignId,
    out Guid appealId,
    out Guid packageId,
    out Guid designationId)
        {
            campaignId = defaultCampaignId;
            appealId = Guid.Empty;
            packageId = Guid.Empty;
            designationId = defaultPrimaryDesignationId;

            // --------------------------------------------------
            // Resolve Designation
            // --------------------------------------------------

            Entity designation = null;

            if (HasValue(glCode1))
            {
                designation = FindExistingRecord(
                    "msnfp_designation",
                    new List<ConditionExpression>
                    {
                new ConditionExpression(
                    "msnfp_designationcode",
                    ConditionOperator.Equal,
                    glCode1)
                    });
            }

            if (designation == null && HasValue(glCode2))
            {
                designation = FindExistingRecord(
                    "msnfp_designation",
                    new List<ConditionExpression>
                    {
                new ConditionExpression(
                    "msnfp_designationcode",
                    ConditionOperator.Equal,
                    glCode2)
                    });
            }

            if (designation != null)
            {
                designationId = designation.Id;
            }

            // --------------------------------------------------
            // Resolve from Event
            // --------------------------------------------------

            if (int.TryParse(eventId, out int eventNumber))
            {
                Entity eventRecord = FindExistingRecord(
                    "lrx_event",
                    new List<ConditionExpression>
                    {
                new ConditionExpression(
                    "lrx_fundraisineventid",
                    ConditionOperator.Equal,
                    eventNumber)
                    });

                if (eventRecord != null)
                {
                    if (eventRecord.GetAttributeValue<EntityReference>("lrx_campaign") != null)
                        campaignId = eventRecord.GetAttributeValue<EntityReference>("lrx_campaign").Id;

                    if (eventRecord.GetAttributeValue<EntityReference>("lrx_sifund_appeal") != null)
                        appealId = eventRecord.GetAttributeValue<EntityReference>("lrx_sifund_appeal").Id;

                    if (eventRecord.GetAttributeValue<EntityReference>("lrx_sifund_package") != null)
                        packageId = eventRecord.GetAttributeValue<EntityReference>("lrx_sifund_package").Id;

                    if (designation == null)
                    {
                        EntityReference eventDesignation =
                            eventRecord.GetAttributeValue<EntityReference>("lrx_designation");

                        if (eventDesignation != null)
                            designationId = eventDesignation.Id;
                    }
                    return;
                }
            }

            // --------------------------------------------------
            // Resolve from Page
            // --------------------------------------------------

            if (int.TryParse(pageId, out int pageNumber))
            {
                Entity pageRecord = FindExistingRecord(
                    "lrx_fundraisinpage",
                    new List<ConditionExpression>
                    {
                new ConditionExpression(
                    "lrx_fundraisinpagesid",
                    ConditionOperator.Equal,
                    pageNumber)
                    });

                if (pageRecord != null)
                {
                    if (pageRecord.GetAttributeValue<EntityReference>("lrx_campaign") != null)
                        campaignId = pageRecord.GetAttributeValue<EntityReference>("lrx_campaign").Id;

                    if (pageRecord.GetAttributeValue<EntityReference>("lrx_appeal") != null)
                        appealId = pageRecord.GetAttributeValue<EntityReference>("lrx_appeal").Id;

                    if (pageRecord.GetAttributeValue<EntityReference>("lrx_package") != null)
                        packageId = pageRecord.GetAttributeValue<EntityReference>("lrx_package").Id;

                    if (designation == null)
                    {
                        EntityReference pageDesignation =
                            pageRecord.GetAttributeValue<EntityReference>("lrx_designation");

                        if (pageDesignation != null)
                            designationId = pageDesignation.Id;
                    }

                    return;
                }
            }

            // Default:
            // campaignId = defaultCampaignId (already set)
            // appealId = Guid.Empty
            // packageId = Guid.Empty
        }

        private void ResolveRaffleAppealPackageDesignation(
    string eventId,
    string pageId,
    string glCode1,
    string glCode2,
    out Guid appealId,
    out Guid packageId,
    out Guid designationId)
        {
            appealId = Guid.Empty;
            packageId = Guid.Empty;
            designationId = defaultPrimaryDesignationId;

            // --------------------------------------------------
            // 1. Resolve Designation from GL Code 1
            // --------------------------------------------------

            Entity designation = null;

            if (HasValue(glCode1))
            {
                designation = FindExistingRecord(
                    "msnfp_designation",
                    new List<ConditionExpression>
                    {
                new ConditionExpression(
                    "msnfp_designationcode",
                    ConditionOperator.Equal,
                    glCode1)
                    });
            }

            // --------------------------------------------------
            // 2. If GL Code 1 not found, check GL Code 2
            // --------------------------------------------------

            if (designation == null && HasValue(glCode2))
            {
                designation = FindExistingRecord(
                    "msnfp_designation",
                    new List<ConditionExpression>
                    {
                new ConditionExpression(
                    "msnfp_designationcode",
                    ConditionOperator.Equal,
                    glCode2)
                    });
            }

            if (designation != null)
            {
                designationId = designation.Id;
            }

            // --------------------------------------------------
            // 3. Resolve Appeal, Package and Designation from Event
            // --------------------------------------------------

            if (int.TryParse(eventId, out int eventNumber))
            {
                Entity eventRecord = FindExistingRecord(
                    "lrx_event",
                    new List<ConditionExpression>
                    {
                new ConditionExpression(
                    "lrx_fundraisineventid",
                    ConditionOperator.Equal,
                    eventNumber)
                    });

                if (eventRecord != null)
                {
                    if (eventRecord.GetAttributeValue<EntityReference>("lrx_sifund_appeal") != null)
                        appealId = eventRecord.GetAttributeValue<EntityReference>("lrx_sifund_appeal").Id;

                    if (eventRecord.GetAttributeValue<EntityReference>("lrx_sifund_package") != null)
                        packageId = eventRecord.GetAttributeValue<EntityReference>("lrx_sifund_package").Id;

                    // Only use Event designation if GL1/GL2 didn't resolve one
                    if (designation == null)
                    {
                        var eventDesignation =
                            eventRecord.GetAttributeValue<EntityReference>("lrx_designation");

                        if (eventDesignation != null)
                            designationId = eventDesignation.Id;
                    }

                    return;
                }
            }

            // --------------------------------------------------
            // 4. Resolve Appeal, Package and Designation from Page
            // --------------------------------------------------

            if (int.TryParse(pageId, out int pageNumber))
            {
                Entity pageRecord = FindExistingRecord(
                    "lrx_fundraisinpage",
                    new List<ConditionExpression>
                    {
                new ConditionExpression(
                    "lrx_fundraisinpagesid",
                    ConditionOperator.Equal,
                    pageNumber)
                    });

                if (pageRecord != null)
                {
                    if (pageRecord.GetAttributeValue<EntityReference>("lrx_appeal") != null)
                        appealId = pageRecord.GetAttributeValue<EntityReference>("lrx_appeal").Id;

                    if (pageRecord.GetAttributeValue<EntityReference>("lrx_package") != null)
                        packageId = pageRecord.GetAttributeValue<EntityReference>("lrx_package").Id;

                    // Only use Page designation if GL1/GL2 didn't resolve one
                    if (designation == null)
                    {
                        var pageDesignation =
                            pageRecord.GetAttributeValue<EntityReference>("lrx_designation");

                        if (pageDesignation != null)
                            designationId = pageDesignation.Id;
                    }

                    return;
                }
            }

            // If nothing is found:
            // designationId = defaultPrimaryDesignationId
            // appealId = Guid.Empty
            // packageId = Guid.Empty
        }

        #endregion
    }
}