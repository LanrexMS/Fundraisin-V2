using CrmEarlyBound;
using FundraisinApp_Integration.Plugins.Service;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FundraisinApp_IntegrationPlugin.FundraisinApp_Integration.Plugins
{
    public class HistoricalDataFunRaisin : IPlugin
    {
        static HistoricalDataFunRaisin()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                if (args.Name.Contains("CsvHelper"))
                {
                    // Replace with the correct path to your embedded resource
                    string resourceName = "FundraisinApp_IntegrationPlugin.CsvHelper.dll";

                    using (var stream = Assembly.GetExecutingAssembly()
                            .GetManifestResourceStream(resourceName))
                    {
                        if (stream == null)
                        {
                            throw new InvalidOperationException($"Embedded resource '{resourceName}' not found.");
                        }

                        byte[] assemblyData = new byte[stream.Length];
                        stream.Read(assemblyData, 0, assemblyData.Length);
                        return Assembly.Load(assemblyData);
                    }
                }
                if (args.Name.Contains("Microsoft.Bcl.HashCode"))
                {
                    using (var stream = Assembly.GetExecutingAssembly()
                            .GetManifestResourceStream("FundraisinApp_IntegrationPlugin.Microsoft.Bcl.HashCode.dll"))
                    {
                        byte[] assemblyData = new byte[stream.Length];
                        stream.Read(assemblyData, 0, assemblyData.Length);
                        return Assembly.Load(assemblyData);
                    }
                }
                return null;
            };
        }

        public async void Execute(IServiceProvider serviceProvider)
        {
            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IPluginExecutionContext service1 = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationService organizationService = ((IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory))).CreateOrganizationService(new Guid?(((IExecutionContext)service1).UserId));
            ITracingService service2 = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                var targetEntity = (Entity)context.InputParameters["Target"];

                if (targetEntity.LogicalName == LRx_FunRaisinLogs.EntityLogicalName)
                {
                    LRx_FunRaisinLogs funRaisinLogs = targetEntity.ToEntity<LRx_FunRaisinLogs>();

                    if (!funRaisinLogs.Attributes.Contains(LRx_FunRaisinLogs.Fields.LRx_JSonParam) || funRaisinLogs.Attributes[LRx_FunRaisinLogs.Fields.LRx_JSonParam] == null)
                    {
                        return;
                    }
                    string jsonInput = (string)funRaisinLogs.Attributes[LRx_FunRaisinLogs.Fields.LRx_JSonParam];

                    switch (context.MessageName)
                    {
                        case "Create":
                            object inputParameter = (object)jsonInput;
                            //await new Fundraising_APIService(organizationService, service1, service2, inputParameter).GetAllFundraisinEventRecords();
                            //await new Fundraising_APIService(organizationService, service1, service2, inputParameter).GetFundraisinParticipantRecords();
                            //await new Fundraising_APIService(organizationService, service1, service2, inputParameter).GetFundRaisinOrganisationRecord();
                            //await new Fundraising_APIService(organizationService, service1, service2, inputParameter).GetFundRaisinEventTeamsRecord();
                            //await new Fundraising_APIService(organizationService, service1, service2, inputParameter).GetFundraisinTicketRecords();
                            //await new Fundraising_APIService(organizationService, service1, service2, inputParameter).GetFundRaisinPromoCodeRecord(); // for comment
                            //await new Fundraising_APIService(organizationService, service1, service2, inputParameter).GetRegistrationFromParticipantEventRecord();
                            //await new Fundraising_APIService(organizationService, service1, service2, inputParameter).GetFundraisinTicketHolderRecord();
                            await new Fundraising_APIService(organizationService, service1, service2, inputParameter).GetFundRaisinProductRecord();
                            await new Fundraising_APIService(organizationService, service1, service2, inputParameter).GetFundRaisinProductOptionsRecord();
                            await new Fundraising_APIService(organizationService, service1, service2, inputParameter).GetFundraisinProductSalesItem();
                            await new Fundraising_APIService(organizationService, service1, service2, inputParameter).GetFundraisinRaffleRecords(); // for comment
                            await new Fundraising_APIService(organizationService, service1, service2, inputParameter).GetFundraisinRaffleTicketOptionRecords(); // for comment
                            await new Fundraising_APIService(organizationService, service1, service2, inputParameter).GetFundraisinRaffleSalesRecords(); // for comment
                            await new Fundraising_APIService(organizationService, service1, service2, inputParameter).GetFundRaisinTransactionRecord();

                            LRx_FunRaisinLogs logs = new LRx_FunRaisinLogs
                            {
                                Id = funRaisinLogs.Id,
                                LRx_IntegrationResult = "Integration Successfull"
                            };
                            organizationService.Update(logs);
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }
}
