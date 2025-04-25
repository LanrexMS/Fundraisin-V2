using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FundraisinApp_IntegrationPlugin.FundraisinApp_Integration.Plugins.Entities.Integration_Data_Model
{
    public class ParticipantModelMap : ClassMap<ParticipantModel>
    {
        public ParticipantModelMap()
        {
            // Mapping only the necessary properties for the provided fields
            Map(m => m.MFname).Name("m_fname"); // Maps to "firstname"
            Map(m => m.MLname).Name("m_lname"); // Maps to "lastname"
            Map(m => m.MEmail).Name("m_email"); // Maps to "emailaddress1"
            Map(m => m.MPhoneHome).Name("m_phone_home"); // Maps to "telephone1"
            Map(m => m.MPhoneMobile).Name("m_phone_mobile"); // Maps to "mobilephone"
            Map(m => m.MAddressStreet).Name("m_address_street"); // Maps to "address1_line1"
            Map(m => m.MAddressSuburb).Name("m_address_suburb"); // Maps to "address1_city"
            Map(m => m.MAddressPCode).Name("m_address_pcode"); // Maps to "address1_postalcode"
            Map(m => m.MAddressState).Name("m_address_state"); // Maps to "address1_stateorprovince"
            Map(m => m.MAddressCountry).Name("m_address_country"); // Maps to "address1_country"
            Map(m => m.MemberId).Name("member_id"); // Maps to "lrx_fundraisinmemberid"
            Map(m => m.MEmergencyContact).Name("m_emergency_contact");
            Map(m => m.MEmergencyPhone).Name("m_emergency_phone");
        }
    }
}
