using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fundraising_Engagement.Plugins.Entities.Integration_Data_Model
{
    public class TransactionModel
    {
        public int TotalRecords { get; set; } // total_records
        public int TransactionId { get; set; } // transaction_id
        public string TransactionType { get; set; } // transaction_type
        public int CharityId { get; set; } // charity_id
        public decimal TransactionValue { get; set; } // transaction_value
        public string Currency { get; set; } // currency
        public decimal CurrencyRate { get; set; } // currency_rate
        public decimal TransactionFees { get; set; } // transaction_fees
        public decimal TransactionFeesRate { get; set; } // transaction_fees_rate
        public decimal TransactionFeesGateway { get; set; } // transaction_fees_gateway
        public decimal TransactionFeesMandatory { get; set; } // transaction_fees_mandatory
        public decimal TransactionTax { get; set; } // transaction_tax
        public bool IsReconciled { get; set; } // is_reconciled (Y/N)
        public string TransactionNotes { get; set; } // transaction_notes
        public string PaymentType { get; set; } // payment_type
        public string PaymentReference { get; set; } // payment_reference
        public string BalanceTransactionId { get; set; } // balance_transaction_id
        public string PayoutId { get; set; } // payout_id
        public string AccountId { get; set; } // account_id
        public string PoNumber { get; set; } // po_number
        public int MemberId { get; set; } // member_id
        public int HistoryId { get; set; } // history_id
        public int DonationId { get; set; } // donation_id
        public int ScheduleId { get; set; } // schedule_id
        public int BillingId { get; set; } // billing_id
        public int PaymentId { get; set; } // payment_id
        public int SaleId { get; set; } // sale_id
        public int RaffleId { get; set; } // raffle_id
        public int EventId { get; set; } // event_id
        public int PageId { get; set; } // page_id
        public int EventPageId { get; set; } // event_page_id
        public int RelatedTransactionId { get; set; } // related_transaction_id
        public string GlCode1 { get; set; } // gl_code1
        public string GlCode2 { get; set; } // gl_code2
        public string FbPaymentId { get; set; } // fb_payment_id
        public string CrmTransactionId { get; set; } // crm_transaction_id
        public bool FunraisinSynced { get; set; } // funraisin_synced (Y/N)
        public bool GiftaidClaimed { get; set; } // giftaid_claimed (Y/N)
        public DateTime DateCreated { get; set; } // date_created
    }
}
