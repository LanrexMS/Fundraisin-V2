using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper.Configuration;

namespace FundraisinApp_IntegrationPlugin.FundraisinApp_Integration.Plugins.Entities.Integration_Data_Model
{
    public class ProductSalesModelMap : ClassMap<ProductSales>
    {
        public ProductSalesModelMap()
        {
            Map(m => m.total_records).Name("total_records");
            Map(m => m.sale_id).Name("sale_id");
            Map(m => m.member_id).Name("member_id");
            Map(m => m.history_id).Name("history_id");
            Map(m => m.charity_id).Name("charity_id");
            Map(m => m.po_number).Name("po_number");
            Map(m => m.donation_amount).Name("donation_amount");
            Map(m => m.total_fee).Name("total_fee");
            Map(m => m.optin_fees_rate).Name("optin_fees_rate");
            Map(m => m.optin_fees).Name("optin_fees");
            Map(m => m.sale_type).Name("sale_type");
            Map(m => m.voucher_id).Name("voucher_id");
            Map(m => m.voucher).Name("voucher");
            Map(m => m.title).Name("title");
            Map(m => m.first_name).Name("first_name");
            Map(m => m.last_name).Name("last_name");
            Map(m => m.last_name_prefix).Name("last_name_prefix");
            Map(m => m.email).Name("email");
            Map(m => m.s_language).Name("s_language");
            Map(m => m.mobile).Name("mobile");
            Map(m => m.mobile_suffix).Name("mobile_suffix");
            Map(m => m.phone).Name("phone");
            Map(m => m.unit).Name("unit");
            Map(m => m.number).Name("number");
            Map(m => m.street).Name("street");
            Map(m => m.address_2).Name("address_2");
            Map(m => m.postcode).Name("postcode");
            Map(m => m.suburb).Name("suburb");
            Map(m => m.state).Name("state");
            Map(m => m.country).Name("country");
            Map(m => m.kw_address).Name("kw_address");
            Map(m => m.address_dpid).Name("address_dpid");
            Map(m => m.address_barcode).Name("address_barcode");
            Map(m => m.age).Name("age");
            Map(m => m.gender).Name("gender");
            Map(m => m.company).Name("company");
            Map(m => m.sub_total).Name("sub_total");
            Map(m => m.gst).Name("gst");
            Map(m => m.delivery).Name("delivery");
            Map(m => m.total).Name("total");
            Map(m => m.total_paid_ticket).Name("total_paid_ticket");
            Map(m => m.date_paid).Name("date_paid");
            Map(m => m.promo_id).Name("promo_id");
            Map(m => m.tax_ref).Name("tax_ref");
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
            Map(m => m.notes).Name("notes");
            Map(m => m.card_type).Name("card_type");
            Map(m => m.card_country).Name("card_country");
            Map(m => m.card_brand).Name("card_brand");
            Map(m => m.card_name).Name("card_name");
            Map(m => m.card_number).Name("card_number");
            Map(m => m.card_expiry).Name("card_expiry");
            Map(m => m.referral).Name("referral");
            Map(m => m.deliver_to_billing).Name("deliver_to_billing");
            Map(m => m.delivery_unit).Name("delivery_unit");
            Map(m => m.delivery_number).Name("delivery_number");
            Map(m => m.delivery_street).Name("delivery_street");
            Map(m => m.delivery_address_2).Name("delivery_address_2");
            Map(m => m.delivery_suburb).Name("delivery_suburb");
            Map(m => m.delivery_postcode).Name("delivery_postcode");
            Map(m => m.delivery_country).Name("delivery_country");
            Map(m => m.delivery_state).Name("delivery_state");
            Map(m => m.delivery_notes).Name("delivery_notes");
            Map(m => m.shipped).Name("shipped");
            Map(m => m.shipped_date).Name("shipped_date");
            Map(m => m.shipped_tracking).Name("shipped_tracking");
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
            Map(m => m.crm_shop_id).Name("crm_shop_id");
            Map(m => m.s_optout_email).Name("s_optout_email");
            Map(m => m.s_optout_sms).Name("s_optout_sms");
            Map(m => m.shopify_order_id).Name("shopify_order_id");
            Map(m => m.last_updated).Name("last_updated");
            Map(m => m.date_created).Name("date_created");
        }
    }

}
