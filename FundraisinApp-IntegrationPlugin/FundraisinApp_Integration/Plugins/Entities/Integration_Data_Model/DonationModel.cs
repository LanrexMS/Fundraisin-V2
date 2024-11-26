// Decompiled with JetBrains decompiler
// Type: FundraisinApp_Integration.Plugins.Entities.Integration_Data_Model.DonationModel
// Assembly: FundraisinApp-Integration.Plugin, Version=1.0.0.0, Culture=neutral, PublicKeyToken=be17da884c09fb48
// MVID: A80D178E-91E0-4361-9810-5F6936033CCA
// Assembly location: C:\Users\Nico Benito\Downloads\NicoTestSolution_1_0_0_2\PluginAssemblies\FundraisinApp-IntegrationPlugin-AD40DC3F-2002-4C54-899F-1599020D0AFB\FundraisinApp-IntegrationPlugin.dll

using System;

#nullable disable
namespace FundraisinApp_Integration.Plugins.Entities.Integration_Data_Model
{
  public class DonationModel
  {
    public int TotalRecords { get; set; }

    public int DonationId { get; set; }

    public int EventId { get; set; }

    public int OrgId { get; set; }

    public int TeamId { get; set; }

    public int MemberId { get; set; }

    public int CauseId { get; set; }

    public string FbUserId { get; set; }

    public string FbUserPic { get; set; }

    public int HistoryId { get; set; }

    public int CharityId { get; set; }

    public int PageId { get; set; }

    public int EventPageId { get; set; }

    public int SmsCodeId { get; set; }

    public int SaleId { get; set; }

    public int SaleItemId { get; set; }

    public int ProductId { get; set; }

    public int RaffleSaleId { get; set; }

    public int MatchedId { get; set; }

    public int RelatedDonationId { get; set; }

    public string DonationHash { get; set; }

    public string DonationType { get; set; }

    public int CampaignId { get; set; }

    public string DonationFrequency { get; set; }

    public string DonationPeriod { get; set; }

    public int NumberDonations { get; set; }

    public string DonationReason { get; set; }

    public string DonationReasonFor { get; set; }

    public int DonationInterval { get; set; }

    public string DisplayOn { get; set; }

    public string DPhoto { get; set; }

    public string DTitle { get; set; }

    public string DFname { get; set; }

    public string DLname { get; set; }

    public string DLnamePrefix { get; set; }

    public string DOrganisation { get; set; }

    public string DEmail { get; set; }

    public bool IsFundraiserEmail { get; set; }

    public string CareOfEmail { get; set; }

    public bool DOptin { get; set; }

    public bool DExternalOptout { get; set; }

    public DateTime DExternalOptoutDate { get; set; }

    public bool DOptinEmail { get; set; }

    public bool DOptinSms { get; set; }

    public bool DOptinPost { get; set; }

    public bool DOptinPhone { get; set; }

    public string DOptinText { get; set; }

    public bool DOptinCharity { get; set; }

    public bool DOptinFees { get; set; }

    public Decimal DOptinFeesRate { get; set; }

    public Decimal DFee { get; set; }

    public string DGender { get; set; }

    public string DLanguage { get; set; }

    public DateTime DDob { get; set; }

    public string DAddressUnit { get; set; }

    public string DAddressNumber { get; set; }

    public string DAddressStreet { get; set; }

    public string DAddress2 { get; set; }

    public string DAddressSuburb { get; set; }

    public string DAddressPCode { get; set; }

    public string DAddressState { get; set; }

    public string DAddressCountry { get; set; }

    public string DAddressDpid { get; set; }

    public string DAddressBarcode { get; set; }

    public string DPhone { get; set; }

    public string DPhoneHome { get; set; }

    public string DPhoneWork { get; set; }

    public string DPhoneMobile { get; set; }

    public string DPhoneMobileSuffix { get; set; }

    public string DComments { get; set; }

    public string DResponse { get; set; }

    public bool DLeaveMessage { get; set; }

    public string DDisplayName { get; set; }

    public string DReceipt { get; set; }

    public bool DAnonymous { get; set; }

    public Decimal DAmount { get; set; }

    public Decimal DAmountLocal { get; set; }

    public string DCurrency { get; set; }

    public Decimal DCurrencyRate { get; set; }

    public Decimal DCurrencyPlatformRate { get; set; }

    public Decimal DAmountFree { get; set; }

    public Decimal DAmountSel { get; set; }

    public string GatewayCustomerRef { get; set; }

    public string GatewayCardRef { get; set; }

    public string PoNumber { get; set; }

    public string TaxRef { get; set; }

    public string PaymentIntentId { get; set; }

    public DateTime PaymentIntentCreated { get; set; }

    public string StripePaymentMethod { get; set; }

    public string CardType { get; set; }

    public string CardCountry { get; set; }

    public string CardBrand { get; set; }

    public string PaymentMethod { get; set; }

    public string CardNumber { get; set; }

    public string CardName { get; set; }

    public string CardExpiry { get; set; }

    public string ChequeName { get; set; }

    public string ChequeNumber { get; set; }

    public string EftReference { get; set; }

    public bool IsEft { get; set; }

    public string BsbNumber { get; set; }

    public string AccountName { get; set; }

    public string AccountNumber { get; set; }

    public DateTime DatePaid { get; set; }

    public DateTime DateBanked { get; set; }

    public string DStatus { get; set; }

    public string DReceiptNum { get; set; }

    public bool DReceiptSent { get; set; }

    public bool IsMobile { get; set; }

    public bool IsDonation { get; set; }

    public bool IsProfileDonation { get; set; }

    public Decimal DRefundAmount { get; set; }

    public Decimal DRefundAmountLocal { get; set; }

    public string DRefundReason { get; set; }

    public DateTime DRefundDate { get; set; }

    public DateTime DReissueDate { get; set; }

    public bool GiftAid { get; set; }

    public bool SentThanks { get; set; }

    public bool InMemory { get; set; }

    public string InMemoryTitle { get; set; }

    public string InMemoryFname { get; set; }

    public string InMemoryLname { get; set; }

    public string InMemoryEmail { get; set; }

    public string InMemoryAddress1 { get; set; }

    public string InMemoryAddress2 { get; set; }

    public string InMemoryCard { get; set; }

    public string InMemoryCardTo { get; set; }

    public string EcardFirstName { get; set; }

    public string EcardLastName { get; set; }

    public string EcardEmail { get; set; }

    public string EcardComments { get; set; }

    public string EcardAddress { get; set; }

    public string KwAddress { get; set; }

    public double DonorLat { get; set; }

    public double DonorLng { get; set; }

    public string DonorIp { get; set; }

    public string DonorUserAgent { get; set; }

    public string UtmCampaign { get; set; }

    public string UtmSource { get; set; }

    public string UtmMedium { get; set; }

    public string UtmContent { get; set; }

    public string UtmTerm { get; set; }

    public string DonationTags { get; set; }

    public string DonorMfaCode { get; set; }

    public string CrmDonorId { get; set; }

    public string FbDonationId { get; set; }

    public bool FunraisinSynced { get; set; }

    public string EcrmCustomerId { get; set; }

    public string ScrmCustomerId { get; set; }

    public DateTime LastLoggedIn { get; set; }

    public DateTime EcrmLastSyncedDate { get; set; }

    public DateTime ScrmLastSyncedDate { get; set; }

    public string Dtd { get; set; }

    public bool DtdProcessed { get; set; }

    public int DtdCompanyId { get; set; }

    public string DtdCompanyName { get; set; }

    public string DtdIdentifier { get; set; }

    public DateTime LastUpdated { get; set; }

    public DateTime DateCreated { get; set; }
  }
}
