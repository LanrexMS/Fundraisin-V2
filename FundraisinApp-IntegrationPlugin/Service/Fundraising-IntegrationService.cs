using CrmEarlyBound;
using FundraisinApp_Integration.Plugins;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.PluginTelemetry;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlTypes;
using System.IdentityModel.Metadata;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;

namespace Fundraising_Integration.Plugins.Service
{
    public class FundraisingService
    {
        private readonly IOrganizationService _service;
        private readonly IPluginExecutionContext _context;
        private readonly ITracingService _tracingService;


        public FundraisingService(IOrganizationService service, IPluginExecutionContext context, ITracingService tracingService)
        {
            _service = service;
            _context = context;
            _tracingService = tracingService;
        }

        public void UpdateRegistrationAmountRaised(MsnFp_Transaction transaction)
        {
            if (transaction == null)
            {
                _tracingService.Trace("Transaction is null.");
                return;
            }

            // Get lrx_registrations lookup value
            if (!transaction.Attributes.Contains("lrx_registrations") || transaction.Attributes["lrx_registrations"] == null)
            {
                _tracingService.Trace("Transaction does not have an associated lrx_registrations.");
                return;
            }

            EntityReference registrationRef = (EntityReference)transaction.Attributes["lrx_registrations"];

            if (registrationRef == null) { 
                return;
            }

            // Query all transactions related to this registration
            QueryExpression query = new QueryExpression("msnfp_transaction")
            {
                ColumnSet = new ColumnSet("msnfp_amount"),
                Criteria =
                {
                    Conditions =
                    {
                        new ConditionExpression("lrx_registrations", ConditionOperator.Equal, registrationRef.Id)
                    }
                }
            };

            EntityCollection transactions = _service.RetrieveMultiple(query);

            if (transactions.Entities.Count == 0)
            {
                _tracingService.Trace("No transactions found for this registration.");
                return;
            }

            _tracingService.Trace($"Found {transactions.Entities.Count} transactions related to registration.");

            // Compute total msnfp_amount
            decimal totalAmount = 0;
            foreach (Entity trans in transactions.Entities)
            {
                if (trans.Contains("msnfp_amount") && trans["msnfp_amount"] != null)
                {
                    totalAmount += ((Money)trans["msnfp_amount"]).Value;
                }
            }

            _tracingService.Trace($"Total computed amount: {totalAmount}");

            // Update lrx_amountraised field on registration entity
            Entity registrationUpdate = new Entity("lrx_registrations")
            {
                Id = registrationRef.Id
            };
            registrationUpdate["lrx_amountraised"] = new Money(totalAmount);

            _service.Update(registrationUpdate);
            _tracingService.Trace("Updated lrx_amountraised field successfully.");
        }
    }
}
