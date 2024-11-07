using DataverseModel;
using Fundraising_Engagement.Plugins.Service;
using Microsoft.Xrm.Sdk;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fundraising_Engagement.Plugins
{
    public class Financial : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = serviceFactory.CreateOrganizationService(context.UserId);
            var tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            var fundraisingService = new FundraisingService(service, context, tracingService);

            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                var targetEntity = (Entity)context.InputParameters["Target"];

                if (targetEntity.LogicalName == LRx_FinAnaCiaL.EntityLogicalName)
                {
                    var financialID = targetEntity.Id;
                    
                    LRx_FinAnaCiaL financialRecord = new LRx_FinAnaCiaL();

                    switch (context.MessageName)
                    {
                        case "Create":
                            fundraisingService.UpdateFinancialSummary(financialID, financialRecord);
                            break;
                        case "Update":
                            fundraisingService.UpdateFinancialSummary(financialID, financialRecord);
                            break;
                        default:
                            break;
                    }
                }
            }
            else if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is EntityReference && context.MessageName == "Delete")
            {
                // Handle logic on transaction deletion
                var preImageEntity = context.PreEntityImages["FinancialPreImage"];

                if (context.PreEntityImages != null && context.PreEntityImages.Contains("FinancialPreImage"))
                {
                    var preImage = preImageEntity.ToEntity<LRx_FinAnaCiaL>();

                    if (preImage != null)
                    {
                        // Initialize a new instance of the registration record
                        LRx_FinAnaCiaL financialRecord = new LRx_FinAnaCiaL();

                        financialRecord.LRx_OpportunityToFinancial = preImage.LRx_OpportunityToFinancial;
                        financialRecord.LRx_AssetType = preImage.LRx_AssetType;

                        fundraisingService.UpdateFinancialSummary(Guid.Empty, financialRecord);
                    }

                }
            }
        }
    }
}
