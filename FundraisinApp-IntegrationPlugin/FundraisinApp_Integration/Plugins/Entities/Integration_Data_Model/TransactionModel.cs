// Decompiled with JetBrains decompiler
// Type: FundraisinApp_Integration.Plugins.Entities.Integration_Data_Model.TransactionModel
// Assembly: FundraisinApp-Integration.Plugin, Version=1.0.0.0, Culture=neutral, PublicKeyToken=be17da884c09fb48
// MVID: A80D178E-91E0-4361-9810-5F6936033CCA
// Assembly location: C:\Users\Nico Benito\Downloads\NicoTestSolution_1_0_0_2\PluginAssemblies\FundraisinApp-IntegrationPlugin-AD40DC3F-2002-4C54-899F-1599020D0AFB\FundraisinApp-IntegrationPlugin.dll

using System;

#nullable disable
namespace FundraisinApp_Integration.Plugins.Entities.Integration_Data_Model
{
  public class TransactionModel
  {
    public int TotalRecords { get; set; }

    public int TransactionId { get; set; }

    public string TransactionType { get; set; }

    public int CharityId { get; set; }

    public Decimal TransactionValue { get; set; }

    public string Currency { get; set; }

    public Decimal CurrencyRate { get; set; }

    public Decimal TransactionFees { get; set; }

    public Decimal TransactionFeesRate { get; set; }

    public Decimal TransactionFeesGateway { get; set; }

    public Decimal TransactionFeesMandatory { get; set; }

    public Decimal TransactionTax { get; set; }

    public bool IsReconciled { get; set; }

    public string TransactionNotes { get; set; }

    public string PaymentType { get; set; }

    public string PaymentReference { get; set; }

    public string BalanceTransactionId { get; set; }

    public string PayoutId { get; set; }

    public string AccountId { get; set; }

    public string PoNumber { get; set; }

    public int MemberId { get; set; }

    public int HistoryId { get; set; }

    public int DonationId { get; set; }

    public int ScheduleId { get; set; }

    public int BillingId { get; set; }

    public int PaymentId { get; set; }

    public int SaleId { get; set; }

    public int RaffleId { get; set; }

    public int EventId { get; set; }

    public int PageId { get; set; }

    public int EventPageId { get; set; }

    public int RelatedTransactionId { get; set; }

    public string GlCode1 { get; set; }

    public string GlCode2 { get; set; }

    public string FbPaymentId { get; set; }

    public string CrmTransactionId { get; set; }

    public bool FunraisinSynced { get; set; }

    public bool GiftaidClaimed { get; set; }

    public DateTime DateCreated { get; set; }
  }
}
