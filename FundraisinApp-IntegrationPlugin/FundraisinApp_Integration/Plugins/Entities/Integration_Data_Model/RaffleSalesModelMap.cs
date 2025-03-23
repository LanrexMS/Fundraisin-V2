using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FundraisinApp_IntegrationPlugin.FundraisinApp_Integration.Plugins.Entities.Integration_Data_Model
{
    public class RaffleSalesModelMap : ClassMap<RaffleSalesModel>
    {
        public RaffleSalesModelMap()
        {
            Map(m => m.total_records).Name("total_records");
            Map(m => m.sale_id).Name("sale_id");
            Map(m => m.raffle_id).Name("raffle_id");
            Map(m => m.vip_id).Name("vip_id");
            Map(m => m.vip_member_id).Name("vip_member_id");
            Map(m => m.charity_id).Name("charity_id");
            Map(m => m.option_id).Name("option_id");
            Map(m => m.number_tickets).Name("number_tickets");
            Map(m => m.ticket_start).Name("ticket_start");
            Map(m => m.ticket_end).Name("ticket_end");
            Map(m => m.title).Name("title");
            Map(m => m.first_name).Name("first_name");
            Map(m => m.last_name).Name("last_name");
            Map(m => m.last_name_prefix).Name("last_name_prefix");
            Map(m => m.email).Name("email");
            Map(m => m.s_language).Name("s_language");
            Map(m => m.optin_fees).Name("optin_fees");
            Map(m => m.po_number).Name("po_number");
            Map(m => m.donation_amount).Name("donation_amount");
            Map(m => m.donation_amount_local).Name("donation_amount_local");
            Map(m => m.total_fee).Name("total_fee");
            Map(m => m.optin_fees_rate).Name("optin_fees_rate");
            Map(m => m.mobile).Name("mobile");
            Map(m => m.mobile_suffix).Name("mobile_suffix");
            Map(m => m.phone).Name("phone");
            Map(m => m.address_unit).Name("address_unit");
            Map(m => m.address_number).Name("address_number");
            Map(m => m.address_street).Name("address_street");
            Map(m => m.address_2).Name("address_2");
            Map(m => m.address_postcode).Name("address_postcode");
            Map(m => m.address_suburb).Name("address_suburb");
            Map(m => m.address_state).Name("address_state");
            Map(m => m.address_country).Name("address_country");
            Map(m => m.kw_address).Name("kw_address");
            Map(m => m.dob).Name("dob");
            Map(m => m.gender).Name("gender");
            Map(m => m.company).Name("company");
            Map(m => m.agree_terms).Name("agree_terms");
            Map(m => m.optin).Name("optin");
            Map(m => m.optin_text).Name("optin_text");
            Map(m => m.external_optout).Name("external_optout");
            Map(m => m.optin_email).Name("optin_email");
            Map(m => m.optin_sms).Name("optin_sms");
            Map(m => m.optin_post).Name("optin_post");
            Map(m => m.optin_phone).Name("optin_phone");
            Map(m => m.sub_total).Name("sub_total");
            Map(m => m.gst).Name("gst");
            Map(m => m.total).Name("total");
            Map(m => m.date_paid).Name("date_paid");
            Map(m => m.tax_ref).Name("tax_ref");
            Map(m => m.card_type).Name("card_type");
            Map(m => m.card_country).Name("card_country");
            Map(m => m.card_brand).Name("card_brand");
            Map(m => m.card_name).Name("card_name");
            Map(m => m.card_number).Name("card_number");
            Map(m => m.card_expiry).Name("card_expiry");
            Map(m => m.payment_method).Name("payment_method");
            Map(m => m.s_refund).Name("s_refund");
            Map(m => m.s_refund_date).Name("s_refund_date");
            Map(m => m.s_refund_amount).Name("s_refund_amount");
            Map(m => m.s_refund_reason).Name("s_refund_reason");
            Map(m => m.email_sent).Name("email_sent");
            Map(m => m.email_sent_date).Name("email_sent_date");
            Map(m => m.sale_lng).Name("sale_lng");
            Map(m => m.sale_lat).Name("sale_lat");
            Map(m => m.sale_ip).Name("sale_ip");
            Map(m => m.sale_useragent).Name("sale_useragent");
            Map(m => m.payment_intent_id).Name("payment_intent_id");
            Map(m => m.gateway_customer_ref).Name("gateway_customer_ref");
            Map(m => m.s_optin).Name("s_optin");
            Map(m => m.s_external_optout).Name("s_external_optout");
            Map(m => m.s_external_optout_date).Name("s_external_optout_date");
            Map(m => m.s_optin_email).Name("s_optin_email");
            Map(m => m.s_optin_sms).Name("s_optin_sms");
            Map(m => m.s_optin_post).Name("s_optin_post");
            Map(m => m.s_optin_phone).Name("s_optin_phone");
            Map(m => m.s_optin_text).Name("s_optin_text");
            Map(m => m.utm_campaign).Name("utm_campaign");
            Map(m => m.utm_source).Name("utm_source");
            Map(m => m.utm_medium).Name("utm_medium");
            Map(m => m.utm_content).Name("utm_content");
            Map(m => m.utm_term).Name("utm_term");
            Map(m => m.is_mobile).Name("is_mobile");
            Map(m => m.crm_sale_id).Name("crm_sale_id");
            Map(m => m.funraisin_synced).Name("funraisin_synced");
            Map(m => m.last_updated).Name("last_updated");
            Map(m => m.date_created).Name("date_created");
        }
    }
}
