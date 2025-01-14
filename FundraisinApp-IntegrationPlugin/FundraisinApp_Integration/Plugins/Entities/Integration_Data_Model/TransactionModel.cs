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
        public string Total_records { get; set; }
        public string Transaction_id { get; set; }
        public string Transaction_type { get; set; }
        public string Charity_id { get; set; }
        public string Transaction_value { get; set; }
        public string Currency { get; set; }
        public string Currency_rate { get; set; }
        public string Transaction_fees { get; set; }
        public string Transaction_fees_rate { get; set; }
        public string Transaction_fees_gateway { get; set; }
        public string Transaction_fees_mandatory { get; set; }
        public string Transaction_tax { get; set; }
        public string Is_reconciled { get; set; }
        public string Transaction_notes { get; set; }
        public string Payment_type { get; set; }
        public string Payment_reference { get; set; }
        public string Balance_transaction_id { get; set; }
        public string Payout_id { get; set; }
        public string Account_id { get; set; }
        public string Po_number { get; set; }
        public string Member_id { get; set; }
        public string History_id { get; set; }
        public string Donation_id { get; set; }
        public string Schedule_id { get; set; }
        public string Billing_id { get; set; }
        public string Payment_id { get; set; }
        public string Sale_id { get; set; }
        public string Raffle_id { get; set; }
        public string Event_id { get; set; }
        public string Page_id { get; set; }
        public string Event_page_id { get; set; }
        public string Related_transaction_id { get; set; }
        public string Gl_code1 { get; set; }
        public string Gl_code2 { get; set; }
        public string Fb_payment_id { get; set; }
        public string Crm_transaction_id { get; set; }
        public string Funraisin_synced { get; set; }
        public string Giftaid_claimed { get; set; }
        public string Date_created { get; set; }
    }

}
