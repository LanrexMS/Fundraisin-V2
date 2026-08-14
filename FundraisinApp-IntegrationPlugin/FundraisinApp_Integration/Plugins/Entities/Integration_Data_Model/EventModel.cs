using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FundraisinApp_IntegrationPlugin.FundraisinApp_Integration.Plugins.Entities.Integration_Data_Model
{
    public class EventModel
    {
        public string TotalRecords { get; set; }
        public string EventId { get; set; }
        public string CreatedMemberId { get; set; }
        public string DiyCategoryId { get; set; }
        public string CharityId { get; set; }
        public string PageId { get; set; }
        public string EventKey { get; set; }
        public string EventCode { get; set; }

        public string IsLocked { get; set; }
        public string IsFeatured { get; set; }
        public string UseCustomNav { get; set; }
        public string UseCustomNavTheme { get; set; }
        public string NavThemeId { get; set; }
        public string CharityEmailAccess { get; set; }
        public string UseCustomDollarHandles { get; set; }
        public string HasPage { get; set; }

        public string EventName { get; set; }
        public string EventType { get; set; }
        public string EventCategory { get; set; }

        public string AllowEntries { get; set; }
        public string EntryType { get; set; }
        public string EntryLimit { get; set; }
        public string UseEntryCode { get; set; }
        public string EntryCode { get; set; }

        public string EventFee { get; set; }
        public string EventFeeDescription { get; set; }

        public string EventTicketPrice { get; set; }
        public string EventTickets { get; set; }
        public string EventTicketsAllowMultiple { get; set; }
        public string EventTicketsMax { get; set; }
        public string EventTicketsMin { get; set; }

        public string EventAllowTables { get; set; }
        public string EventTables { get; set; }
        public string EventTablePrice { get; set; }
        public string EventSeatsPerTable { get; set; }

        public string EventFundraising { get; set; }
        public string EventDomain { get; set; }

        public string StPrefixDonation { get; set; }
        public string StPrefixRegistration { get; set; }
        public string StPrefixTicket { get; set; }
        public string StPrefixShop { get; set; }

        public string EventClosed { get; set; }
        public string EventClosedDate { get; set; }
        public string EventClosedTime { get; set; }
        public string EventClosedMsg { get; set; }

        public string EventShortDesc { get; set; }
        public string EventAboutInfo { get; set; }
        public string EventAboutImage { get; set; }
        public string EventImportantInfo { get; set; }
        public string EventImportantImage { get; set; }

        public string EventBanner { get; set; }
        public string EventMobileBanner { get; set; }
        public string EventImage { get; set; }

        public string EventHeaderHeight { get; set; }
        public string EventHeaderBgcolor { get; set; }
        public string EventHeaderHtml { get; set; }
        public string EventHeaderMask { get; set; }
        public string EventHeaderMaskColor { get; set; }
        public string EventHeaderMaskOpacity { get; set; }

        public string EventThumb { get; set; }

        public string EventTarget { get; set; }
        public string EventOffline { get; set; }

        public string EventDate { get; set; }
        public string EventExpiry { get; set; }
        public string EventTime { get; set; }
        public string EventEndTime { get; set; }

        public string EventLocation { get; set; }
        public string EventUnit { get; set; }
        public string EventNumber { get; set; }
        public string EventStreet { get; set; }
        public string EventSuburb { get; set; }
        public string EventCity { get; set; }
        public string EventPostcode { get; set; }
        public string EventState { get; set; }
        public string EventCountry { get; set; }

        public string EventLat { get; set; }
        public string EventLng { get; set; }

        public string EventMap { get; set; }
        public string EventWebsite { get; set; }

        public string ShowAddress { get; set; }
        public string ShowEmergencyContact { get; set; }
        public string ShowTerms { get; set; }
        public string ShowWaiver { get; set; }

        public string EventWaiver { get; set; }

        public string ShowDob { get; set; }
        public string ShowGender { get; set; }
        public string ShowPhone { get; set; }
        public string ShowMobile { get; set; }

        public string ShowDonation { get; set; }
        public string ShowProgress { get; set; }

        public string EventFundraisingBanner { get; set; }
        public string EventFundraisingBannerMobile { get; set; }

        public string StTargetTeam { get; set; }
        public string StTargetTeamMin { get; set; }
        public string StTargetMember { get; set; }
        public string StTargetMemberMin { get; set; }
        public string StTargetDistanceMember { get; set; }
        public string StTargetDistanceMemberMin { get; set; }
        public string StTargetDistanceMemberMax { get; set; }

        public string StTitleMember { get; set; }

        public string EventFundraisingMessage { get; set; }
        public string EventBlogMessage { get; set; }

        public string StTitleTeam { get; set; }

        public string EventTeamfundraisingMessage { get; set; }

        public string EventFacebookLikeMember { get; set; }
        public string EventFacebookLikeTeam { get; set; }

        public string EventTwitterLikeMember { get; set; }
        public string EventTwitterLikeTeam { get; set; }

        public string EventEmailLikeMember { get; set; }
        public string EventEmailLikeTeam { get; set; }

        public string EventEmailLikeSubjectMember { get; set; }
        public string EventEmailLikeSubjectTeam { get; set; }

        public string EventEmailJoinTeam { get; set; }
        public string EventEmailJoinSubjectTeam { get; set; }

        public string EventFacebookShareMember { get; set; }
        public string EventFacebookShareTitleMember { get; set; }
        public string EventFacebookShareTeam { get; set; }

        public string EventTwitterShareMember { get; set; }
        public string EventTwitterShareTeam { get; set; }

        public string EventSocialSharing { get; set; }
        public string EventSocialSharingPublic { get; set; }

        public string EventEmailShareSubjectMember { get; set; }
        public string EventEmailShareMember { get; set; }
        public string EventEmailShareSubjectTeam { get; set; }
        public string EventEmailShareTeam { get; set; }

        public string EventSmsLikeMember { get; set; }
        public string EventSmsLikeTeam { get; set; }

        public string EventAlerts { get; set; }
        public string EventAlertIds { get; set; }
        public string EventCampaignFrom { get; set; }

        public string StTeams { get; set; }
        public string StFundraising { get; set; }
        public string StFitness { get; set; }
        public string StChallengeMode { get; set; }
        public string StDonations { get; set; }
        public string StDonationsPersonal { get; set; }
        public string StFacebook { get; set; }
        public string StFacebookFundraiser { get; set; }

        public string EventOnHeader { get; set; }
        public string EventOnHome { get; set; }
        public string EventOnLanding { get; set; }

        public string EventDefaultImage { get; set; }
        public string EventDefaultTeamImage { get; set; }

        public string ConfirmationPageId { get; set; }
        public string ConfirmationUrl { get; set; }
        public string EventConfirmationHtml { get; set; }

        public string SendConfirmationEmail { get; set; }
        public string ConfirmationEmailTemplateId { get; set; }
        public string ConfirmationEmailSubject { get; set; }
        public string ConfirmationEmailBody { get; set; }

        public string EventPageFacts { get; set; }
        public string EventPageDate { get; set; }
        public string EventPageLocation { get; set; }
        public string EventPageFees { get; set; }
        public string EventPageAbout { get; set; }
        public string EventPageInfo { get; set; }

        public string InSearch { get; set; }
        public string SortOrder { get; set; }

        public string EventTags { get; set; }
        public string EventStatus { get; set; }

        public string CrmEventId { get; set; }
        public string LastUpdated { get; set; }
        public string DateCreated { get; set; }
    }
}
