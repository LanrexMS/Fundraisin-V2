using CsvHelper.Configuration;
using FundraisinApp_Integration.Plugins.Entities.Integration_Data_Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FundraisinApp_IntegrationPlugin.FundraisinApp_Integration.Plugins.Entities.Integration_Data_Model
{
    public class TransactionModelMap : ClassMap<TransactionModel>
    {
        public TransactionModelMap()
        {
            Map(m => m.Total_records).Name("total_records");
            Map(m => m.Transaction_id).Name("transaction_id");
            Map(m => m.Transaction_type).Name("transaction_type");
            Map(m => m.Charity_id).Name("charity_id");
            Map(m => m.Transaction_value).Name("transaction_value");
            Map(m => m.Currency).Name("currency");
            Map(m => m.Currency_rate).Name("currency_rate");
            Map(m => m.Transaction_fees).Name("transaction_fees");
            Map(m => m.Transaction_fees_rate).Name("transaction_fees_rate");
            Map(m => m.Transaction_fees_gateway).Name("transaction_fees_gateway");
            Map(m => m.Transaction_fees_mandatory).Name("transaction_fees_mandatory");
            Map(m => m.Transaction_tax).Name("transaction_tax");
            Map(m => m.Is_reconciled).Name("is_reconciled");
            Map(m => m.Transaction_notes).Name("transaction_notes");
            Map(m => m.Payment_type).Name("payment_type");
            Map(m => m.Payment_reference).Name("payment_reference");
            Map(m => m.Balance_transaction_id).Name("balance_transaction_id");
            Map(m => m.Payout_id).Name("payout_id");
            Map(m => m.Account_id).Name("account_id");
            Map(m => m.Po_number).Name("po_number");
            Map(m => m.Member_id).Name("member_id");
            Map(m => m.History_id).Name("history_id");
            Map(m => m.Donation_id).Name("donation_id");
            Map(m => m.Schedule_id).Name("schedule_id");
            Map(m => m.Billing_id).Name("billing_id");
            Map(m => m.Payment_id).Name("payment_id");
            Map(m => m.Sale_id).Name("sale_id");
            Map(m => m.Raffle_id).Name("raffle_id");
            Map(m => m.Event_id).Name("event_id");
            Map(m => m.Page_id).Name("page_id");
            Map(m => m.Event_page_id).Name("event_page_id");
            Map(m => m.Related_transaction_id).Name("related_transaction_id");
            Map(m => m.Gl_code1).Name("gl_code1");
            Map(m => m.Gl_code2).Name("gl_code2");
            Map(m => m.Fb_payment_id).Name("fb_payment_id");
            Map(m => m.Crm_transaction_id).Name("crm_transaction_id");
            Map(m => m.Funraisin_synced).Name("funraisin_synced");
            Map(m => m.Giftaid_claimed).Name("giftaid_claimed");
            Map(m => m.Date_created).Name("date_created");
        }
    }
}
