using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FundraisinApp_IntegrationPlugin.FundraisinApp_Integration.Plugins.Entities.Integration_Data_Model
{
    public class EventModel
    {
        public int TotalRecords { get; set; }
        public int EventId { get; set; }
        public int CreatedMemberId { get; set; }
        public int DiyCategoryId { get; set; }
        public int CharityId { get; set; }
        public int PageId { get; set; }
        public string EventKey { get; set; }
        public string EventCode { get; set; }
        public bool IsLocked { get; set; }
        public bool IsFeatured { get; set; }
        public bool UseCustomNav { get; set; }
        public bool UseCustomDollarHandles { get; set; }
        public bool HasPage { get; set; }
        public string EventName { get; set; }
        public string EventType { get; set; }
        public string EventCategory { get; set; }
        public bool AllowEntries { get; set; }
        public string EntryType { get; set; }
        public string EntryLimit { get; set; }
        public bool UseEntryCode { get; set; }
        public string EntryCode { get; set; }
        public decimal EventFee { get; set; }
        public string EventFeeDescription { get; set; }
        public decimal EventTicketPrice { get; set; }
        public int EventTickets { get; set; }
        public bool EventTicketsAllowMultiple { get; set; }
        public int EventTicketsMax { get; set; }
        public int EventTicketsMin { get; set; }
        public bool EventAllowTables { get; set; }
        public int EventTables { get; set; }
        public decimal EventTablePrice { get; set; }
        public int EventSeatsPerTable { get; set; }
        public bool EventFundraising { get; set; }
        public string EventDomain { get; set; }
        public string StPrefixDonation { get; set; }
        public string StPrefixRegistration { get; set; }
        public string StPrefixTicket { get; set; }
        public string StPrefixShop { get; set; }
        public bool EventClosed { get; set; }
        public DateTime? EventClosedDate { get; set; }
        public string EventClosedMsg { get; set; }
        public string EventShortDesc { get; set; }
        public string EventAboutInfo { get; set; }
        public string EventBanner { get; set; }
        public string EventMobileBanner { get; set; }
        public string EventImage { get; set; }
        public string EventTarget { get; set; }
        public DateTime EventDate { get; set; }
        public DateTime EventExpiry { get; set; }
        public string EventLocation { get; set; }
        public string EventState { get; set; }
        public string EventCountry { get; set; }
        public double EventLat { get; set; }
        public double EventLng { get; set; }
        public bool ShowAddress { get; set; }
        public bool ShowEmergencyContact { get; set; }
        public bool ShowTerms { get; set; }
        public bool ShowWaiver { get; set; }
        public string EventWaiver { get; set; }
        public bool ShowDob { get; set; }
        public bool ShowGender { get; set; }
        public bool ShowPhone { get; set; }
        public bool ShowDonation { get; set; }
        public bool ShowProgress { get; set; }
        public string EventFundraisingBanner { get; set; }
        public int SortOrder { get; set; }
        public string EventTags { get; set; }
        public string EventStatus { get; set; }
        public Guid CrmEventId { get; set; }
        public DateTime LastUpdated { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
