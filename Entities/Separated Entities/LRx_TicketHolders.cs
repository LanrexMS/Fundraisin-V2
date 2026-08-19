using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CrmEarlyBound;

namespace DataverseModel
{

    /// <summary>
    /// Status of the Ticket Holders
    /// </summary>
    [System.Runtime.Serialization.DataContractAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Dataverse Model Builder", "2.0.0.16")]
    public enum lrx_ticketholders_statecode
    {

        [System.Runtime.Serialization.EnumMemberAttribute()]
        Active = 0,

        [System.Runtime.Serialization.EnumMemberAttribute()]
        Inactive = 1,
    }

    /// <summary>
    /// Reason for the status of the Ticket Holders
    /// </summary>
    [System.Runtime.Serialization.DataContractAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Dataverse Model Builder", "2.0.0.16")]
    public enum lrx_ticketholders_statuscode
    {

        [System.Runtime.Serialization.EnumMemberAttribute()]
        Active = 1,

        [System.Runtime.Serialization.EnumMemberAttribute()]
        Inactive = 2,
    }

    [System.Runtime.Serialization.DataContractAttribute()]
    [Microsoft.Xrm.Sdk.Client.EntityLogicalNameAttribute("lrx_ticketholders")]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Dataverse Model Builder", "2.0.0.16")]
    public partial class lrx_TicketHolders : Microsoft.Xrm.Sdk.Entity, System.ComponentModel.INotifyPropertyChanging, System.ComponentModel.INotifyPropertyChanged
    {

        /// <summary>
        /// Available fields, a the time of codegen, for the lrx_ticketholders entity
        /// </summary>
        public partial class Fields
        {
            public const string CreatedBy = "createdby";
            public const string CreatedOn = "createdon";
            public const string CreatedOnBehalfBy = "createdonbehalfby";
            public const string ImportSequenceNumber = "importsequencenumber";
            public const string lrx_EmergencyContact = "lrx_emergencycontact";
            public const string lrx_EmergencyContactNumber = "lrx_emergencycontactnumber";
            public const string lrx_EmergencyContactType = "lrx_emergencycontacttype";
            public const string lrx_Event = "lrx_event";
            public const string lrx_FundraisinGuestID = "lrx_fundraisinguestid";
            public const string lrx_GuardianEmail = "lrx_guardianemail";
            public const string lrx_GuardianName = "lrx_guardianname";
            public const string lrx_GuardianPhone = "lrx_guardianphone";
            public const string lrx_GuardianRelationship = "lrx_guardianrelationship";
            public const string lrx_Name = "lrx_name";
            public const string lrx_ParentRegistration = "lrx_parentregistration";
            public const string lrx_TickerHolder = "lrx_tickerholder";
            public const string lrx_TicketHoldersId = "lrx_ticketholdersid";
            public const string Id = "lrx_ticketholdersid";
            public const string lrx_Wave = "lrx_wave";
            public const string ModifiedBy = "modifiedby";
            public const string ModifiedOn = "modifiedon";
            public const string ModifiedOnBehalfBy = "modifiedonbehalfby";
            public const string OverriddenCreatedOn = "overriddencreatedon";
            public const string OwnerId = "ownerid";
            public const string OwningBusinessUnit = "owningbusinessunit";
            public const string OwningTeam = "owningteam";
            public const string OwningUser = "owninguser";
            public const string statecode = "statecode";
            public const string statuscode = "statuscode";
            public const string TimeZoneRuleVersionNumber = "timezoneruleversionnumber";
            public const string UTCConversionTimeZoneCode = "utcconversiontimezonecode";
            public const string VersionNumber = "versionnumber";
        }

        /// <summary>
        /// Default Constructor.
        /// </summary>
        public lrx_TicketHolders() :
                base(EntityLogicalName)
        {
        }

        public const string EntityLogicalName = "lrx_ticketholders";

        public const string EntityLogicalCollectionName = "lrx_ticketholderses";

        public const string EntitySetName = "lrx_ticketholderses";

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public event System.ComponentModel.PropertyChangingEventHandler PropertyChanging;

        private void OnPropertyChanged(string propertyName)
        {
            if ((this.PropertyChanged != null))
            {
                this.PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }

        private void OnPropertyChanging(string propertyName)
        {
            if ((this.PropertyChanging != null))
            {
                this.PropertyChanging(this, new System.ComponentModel.PropertyChangingEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Unique identifier of the user who created the record.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("createdby")]
        public Microsoft.Xrm.Sdk.EntityReference CreatedBy
        {
            get
            {
                return this.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>("createdby");
            }
        }

        /// <summary>
        /// Date and time when the record was created.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("createdon")]
        public System.Nullable<System.DateTime> CreatedOn
        {
            get
            {
                return this.GetAttributeValue<System.Nullable<System.DateTime>>("createdon");
            }
        }

        /// <summary>
        /// Unique identifier of the delegate user who created the record.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("createdonbehalfby")]
        public Microsoft.Xrm.Sdk.EntityReference CreatedOnBehalfBy
        {
            get
            {
                return this.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>("createdonbehalfby");
            }
        }

        /// <summary>
        /// Sequence number of the import that created this record.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("importsequencenumber")]
        public System.Nullable<int> ImportSequenceNumber
        {
            get
            {
                return this.GetAttributeValue<System.Nullable<int>>("importsequencenumber");
            }
            set
            {
                this.OnPropertyChanging("ImportSequenceNumber");
                this.SetAttributeValue("importsequencenumber", value);
                this.OnPropertyChanged("ImportSequenceNumber");
            }
        }

        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("lrx_emergencycontact")]
        public string lrx_EmergencyContact
        {
            get
            {
                return this.GetAttributeValue<string>("lrx_emergencycontact");
            }
            set
            {
                this.OnPropertyChanging("lrx_EmergencyContact");
                this.SetAttributeValue("lrx_emergencycontact", value);
                this.OnPropertyChanged("lrx_EmergencyContact");
            }
        }

        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("lrx_emergencycontactnumber")]
        public string lrx_EmergencyContactNumber
        {
            get
            {
                return this.GetAttributeValue<string>("lrx_emergencycontactnumber");
            }
            set
            {
                this.OnPropertyChanging("lrx_EmergencyContactNumber");
                this.SetAttributeValue("lrx_emergencycontactnumber", value);
                this.OnPropertyChanged("lrx_EmergencyContactNumber");
            }
        }

        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("lrx_emergencycontacttype")]
        public string lrx_EmergencyContactType
        {
            get
            {
                return this.GetAttributeValue<string>("lrx_emergencycontacttype");
            }
            set
            {
                this.OnPropertyChanging("lrx_EmergencyContactType");
                this.SetAttributeValue("lrx_emergencycontacttype", value);
                this.OnPropertyChanged("lrx_EmergencyContactType");
            }
        }

        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("lrx_event")]
        public Microsoft.Xrm.Sdk.EntityReference lrx_Event
        {
            get
            {
                return this.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>("lrx_event");
            }
            set
            {
                this.OnPropertyChanging("lrx_Event");
                this.SetAttributeValue("lrx_event", value);
                this.OnPropertyChanged("lrx_Event");
            }
        }

        /// <summary>
        /// Contains Guest ID from API response
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("lrx_fundraisinguestid")]
        public System.Nullable<int> lrx_FundraisinGuestID
        {
            get
            {
                return this.GetAttributeValue<System.Nullable<int>>("lrx_fundraisinguestid");
            }
            set
            {
                this.OnPropertyChanging("lrx_FundraisinGuestID");
                this.SetAttributeValue("lrx_fundraisinguestid", value);
                this.OnPropertyChanged("lrx_FundraisinGuestID");
            }
        }

        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("lrx_guardianemail")]
        public string lrx_GuardianEmail
        {
            get
            {
                return this.GetAttributeValue<string>("lrx_guardianemail");
            }
            set
            {
                this.OnPropertyChanging("lrx_GuardianEmail");
                this.SetAttributeValue("lrx_guardianemail", value);
                this.OnPropertyChanged("lrx_GuardianEmail");
            }
        }

        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("lrx_guardianname")]
        public string lrx_GuardianName
        {
            get
            {
                return this.GetAttributeValue<string>("lrx_guardianname");
            }
            set
            {
                this.OnPropertyChanging("lrx_GuardianName");
                this.SetAttributeValue("lrx_guardianname", value);
                this.OnPropertyChanged("lrx_GuardianName");
            }
        }

        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("lrx_guardianphone")]
        public string lrx_GuardianPhone
        {
            get
            {
                return this.GetAttributeValue<string>("lrx_guardianphone");
            }
            set
            {
                this.OnPropertyChanging("lrx_GuardianPhone");
                this.SetAttributeValue("lrx_guardianphone", value);
                this.OnPropertyChanged("lrx_GuardianPhone");
            }
        }

        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("lrx_guardianrelationship")]
        public string lrx_GuardianRelationship
        {
            get
            {
                return this.GetAttributeValue<string>("lrx_guardianrelationship");
            }
            set
            {
                this.OnPropertyChanging("lrx_GuardianRelationship");
                this.SetAttributeValue("lrx_guardianrelationship", value);
                this.OnPropertyChanged("lrx_GuardianRelationship");
            }
        }

        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("lrx_name")]
        public string lrx_Name
        {
            get
            {
                return this.GetAttributeValue<string>("lrx_name");
            }
            set
            {
                this.OnPropertyChanging("lrx_Name");
                this.SetAttributeValue("lrx_name", value);
                this.OnPropertyChanged("lrx_Name");
            }
        }

        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("lrx_parentregistration")]
        public Microsoft.Xrm.Sdk.EntityReference lrx_ParentRegistration
        {
            get
            {
                return this.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>("lrx_parentregistration");
            }
            set
            {
                this.OnPropertyChanging("lrx_ParentRegistration");
                this.SetAttributeValue("lrx_parentregistration", value);
                this.OnPropertyChanged("lrx_ParentRegistration");
            }
        }

        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("lrx_tickerholder")]
        public Microsoft.Xrm.Sdk.EntityReference lrx_TickerHolder
        {
            get
            {
                return this.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>("lrx_tickerholder");
            }
            set
            {
                this.OnPropertyChanging("lrx_TickerHolder");
                this.SetAttributeValue("lrx_tickerholder", value);
                this.OnPropertyChanged("lrx_TickerHolder");
            }
        }

        /// <summary>
        /// Unique identifier for entity instances
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("lrx_ticketholdersid")]
        public System.Nullable<System.Guid> lrx_TicketHoldersId
        {
            get
            {
                return this.GetAttributeValue<System.Nullable<System.Guid>>("lrx_ticketholdersid");
            }
            set
            {
                this.OnPropertyChanging("lrx_TicketHoldersId");
                this.SetAttributeValue("lrx_ticketholdersid", value);
                if (value.HasValue)
                {
                    base.Id = value.Value;
                }
                else
                {
                    base.Id = System.Guid.Empty;
                }
                this.OnPropertyChanged("lrx_TicketHoldersId");
            }
        }

        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("lrx_ticketholdersid")]
        public override System.Guid Id
        {
            get
            {
                return base.Id;
            }
            set
            {
                this.lrx_TicketHoldersId = value;
            }
        }

        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("lrx_wave")]
        public Microsoft.Xrm.Sdk.EntityReference lrx_Wave
        {
            get
            {
                return this.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>("lrx_wave");
            }
            set
            {
                this.OnPropertyChanging("lrx_Wave");
                this.SetAttributeValue("lrx_wave", value);
                this.OnPropertyChanged("lrx_Wave");
            }
        }

        /// <summary>
        /// Unique identifier of the user who modified the record.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("modifiedby")]
        public Microsoft.Xrm.Sdk.EntityReference ModifiedBy
        {
            get
            {
                return this.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>("modifiedby");
            }
        }

        /// <summary>
        /// Date and time when the record was modified.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("modifiedon")]
        public System.Nullable<System.DateTime> ModifiedOn
        {
            get
            {
                return this.GetAttributeValue<System.Nullable<System.DateTime>>("modifiedon");
            }
        }

        /// <summary>
        /// Unique identifier of the delegate user who modified the record.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("modifiedonbehalfby")]
        public Microsoft.Xrm.Sdk.EntityReference ModifiedOnBehalfBy
        {
            get
            {
                return this.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>("modifiedonbehalfby");
            }
        }

        /// <summary>
        /// Date and time that the record was migrated.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("overriddencreatedon")]
        public System.Nullable<System.DateTime> OverriddenCreatedOn
        {
            get
            {
                return this.GetAttributeValue<System.Nullable<System.DateTime>>("overriddencreatedon");
            }
            set
            {
                this.OnPropertyChanging("OverriddenCreatedOn");
                this.SetAttributeValue("overriddencreatedon", value);
                this.OnPropertyChanged("OverriddenCreatedOn");
            }
        }

        /// <summary>
        /// Owner Id
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("ownerid")]
        public Microsoft.Xrm.Sdk.EntityReference OwnerId
        {
            get
            {
                return this.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>("ownerid");
            }
            set
            {
                this.OnPropertyChanging("OwnerId");
                this.SetAttributeValue("ownerid", value);
                this.OnPropertyChanged("OwnerId");
            }
        }

        /// <summary>
        /// Unique identifier for the business unit that owns the record
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("owningbusinessunit")]
        public Microsoft.Xrm.Sdk.EntityReference OwningBusinessUnit
        {
            get
            {
                return this.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>("owningbusinessunit");
            }
        }

        /// <summary>
        /// Unique identifier for the team that owns the record.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("owningteam")]
        public Microsoft.Xrm.Sdk.EntityReference OwningTeam
        {
            get
            {
                return this.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>("owningteam");
            }
        }

        /// <summary>
        /// Unique identifier for the user that owns the record.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("owninguser")]
        public Microsoft.Xrm.Sdk.EntityReference OwningUser
        {
            get
            {
                return this.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>("owninguser");
            }
        }

        /// <summary>
        /// Status of the Ticket Holders
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("statecode")]
        public virtual lrx_ticketholders_statecode? statecode
        {
            get
            {
                return ((lrx_ticketholders_statecode?)(EntityOptionSetEnum.GetEnum(this, "statecode")));
            }
            set
            {
                this.OnPropertyChanging("statecode");
                this.SetAttributeValue("statecode", value.HasValue ? new Microsoft.Xrm.Sdk.OptionSetValue((int)value) : null);
                this.OnPropertyChanged("statecode");
            }
        }

        /// <summary>
        /// Reason for the status of the Ticket Holders
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("statuscode")]
        public virtual lrx_ticketholders_statuscode? statuscode
        {
            get
            {
                return ((lrx_ticketholders_statuscode?)(EntityOptionSetEnum.GetEnum(this, "statuscode")));
            }
            set
            {
                this.OnPropertyChanging("statuscode");
                this.SetAttributeValue("statuscode", value.HasValue ? new Microsoft.Xrm.Sdk.OptionSetValue((int)value) : null);
                this.OnPropertyChanged("statuscode");
            }
        }

        /// <summary>
        /// For internal use only.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("timezoneruleversionnumber")]
        public System.Nullable<int> TimeZoneRuleVersionNumber
        {
            get
            {
                return this.GetAttributeValue<System.Nullable<int>>("timezoneruleversionnumber");
            }
            set
            {
                this.OnPropertyChanging("TimeZoneRuleVersionNumber");
                this.SetAttributeValue("timezoneruleversionnumber", value);
                this.OnPropertyChanged("TimeZoneRuleVersionNumber");
            }
        }

        /// <summary>
        /// Time zone code that was in use when the record was created.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("utcconversiontimezonecode")]
        public System.Nullable<int> UTCConversionTimeZoneCode
        {
            get
            {
                return this.GetAttributeValue<System.Nullable<int>>("utcconversiontimezonecode");
            }
            set
            {
                this.OnPropertyChanging("UTCConversionTimeZoneCode");
                this.SetAttributeValue("utcconversiontimezonecode", value);
                this.OnPropertyChanged("UTCConversionTimeZoneCode");
            }
        }

        /// <summary>
        /// Version Number
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("versionnumber")]
        public System.Nullable<long> VersionNumber
        {
            get
            {
                return this.GetAttributeValue<System.Nullable<long>>("versionnumber");
            }
        }
    }
}
