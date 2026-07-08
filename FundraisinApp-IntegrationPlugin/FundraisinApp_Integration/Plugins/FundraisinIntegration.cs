// Decompiled with JetBrains decompiler
// Type: FundraisinApp_Integration.Plugins.FundraisinIntegration
// Assembly: FundraisinApp-Integration.Plugin, Version=1.0.0.0, Culture=neutral, PublicKeyToken=be17da884c09fb48
// MVID: A80D178E-91E0-4361-9810-5F6936033CCA
// Assembly location: C:\Users\Nico Benito\Downloads\NicoTestSolution_1_0_0_2\PluginAssemblies\FundraisinApp-IntegrationPlugin-AD40DC3F-2002-4C54-899F-1599020D0AFB\FundraisinApp-IntegrationPlugin.dll

using FundraisinApp_Integration.Plugins.Service;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Reflection;
using static CrmEarlyBound.SiFund_Package;

#nullable disable
namespace FundraisinApp_Integration.Plugins
{
    public class FundraisinIntegration : IPlugin
    {
        static FundraisinIntegration()
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
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext service1 = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationService organizationService = ((IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory))).CreateOrganizationService(new Guid?(((IExecutionContext)service1).UserId));
            ITracingService service2 = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            service2.Trace("===== Fundraisin Plugin Started =====");
            if (!service1.InputParameters.Contains("lrx_fundraisinAppIntegrationPluginJSONParams") ||
    service1.InputParameters["lrx_fundraisinAppIntegrationPluginJSONParams"] == null)
            {
                service2.Trace("Input parameter 'lrx_fundraisinAppIntegrationPluginJSONParams' not found or null.");
                return;
            }

            object inputParameter = ((DataCollection<string, object>)((IExecutionContext)service1).InputParameters)["lrx_fundraisinAppIntegrationPluginJSONParams"];
            service2.Trace("Input Parameter Type: " + inputParameter.GetType().FullName);
            service2.Trace("Input Parameter Value:");
            service2.Trace(inputParameter.ToString());
            service2.Trace("===== Plugin Completed Successfully =====");


            var config = GetConfigurationRecord(organizationService, service2);

            if (config == null)
            {
                return;
            }
            Fundraising_APIService apiService = new Fundraising_APIService(organizationService,service1,service2,config, inputParameter);

            if (config.lrx_GetFundraisinEventRecords == true)
            {
                service2.Trace("Executing GetFundraisinEventRecords");
                apiService.GetFundraisinEventRecords();
            }
            else
            {
                service2.Trace("Skipped GetFundraisinEventRecords");
            }

            if (config.lrx_GetFundraisinParticipantRecords == true)
            {
                service2.Trace("Executing GetFundraisinParticipantRecords");
                apiService.GetFundraisinParticipantRecords();
            }
            else
            {
                service2.Trace("Skipped GetFundraisinParticipantRecords");
            }

            if (config.lrx_GetFundRaisinOrganisationRecord == true)
            {
                service2.Trace("Executing GetFundRaisinOrganisationRecord");
                apiService.GetFundRaisinOrganisationRecord();
            }
            else
            {
                service2.Trace("Skipped GetFundRaisinOrganisationRecord");
            }

            if (config.lrx_GetFundRaisinEventTeamsRecord == true)
            {
                service2.Trace("Executing GetFundRaisinEventTeamsRecord");
                apiService.GetFundRaisinEventTeamsRecord();
            }
            else
            {
                service2.Trace("Skipped GetFundRaisinEventTeamsRecord");
            }

            if (config.lrx_GetFundraisinTicketRecords == true)
            {
                service2.Trace("Executing GetFundraisinTicketRecords");
                apiService.GetFundraisinTicketRecords();
            }
            else
            {
                service2.Trace("Skipped GetFundraisinTicketRecords");
            }

            if (config.lrx_GetFundRaisinPromoCodeRecord == true)
            {
                service2.Trace("Executing GetFundRaisinPromoCodeRecord");
                apiService.GetFundRaisinPromoCodeRecord();
            }
            else
            {
                service2.Trace("Skipped GetFundRaisinPromoCodeRecord");
            }

            if (config.lrx_GetRegistrationFromParticipantEventRecord == true)
            {
                service2.Trace("Executing GetRegistrationFromParticipantEventRecord");
                apiService.GetRegistrationFromParticipantEventRecord();
            }
            else
            {
                service2.Trace("Skipped GetRegistrationFromParticipantEventRecord");
            }

            if (config.lrx_GetFundraisinTicketHolderRecord == true)
            {
                service2.Trace("Executing GetFundraisinTicketHolderRecord");
                apiService.GetFundraisinTicketHolderRecord();
            }
            else
            {
                service2.Trace("Skipped GetFundraisinTicketHolderRecord");
            }

            if (config.lrx_GetFundRaisinProductRecord == true)
            {
                service2.Trace("Executing GetFundRaisinProductRecord");
                apiService.GetFundRaisinProductRecord();
            }
            else
            {
                service2.Trace("Skipped GetFundRaisinProductRecord");
            }

            if (config.lrx_GetFundRaisinProductOptionsRecord == true)
            {
                service2.Trace("Executing GetFundRaisinProductOptionsRecord");
                apiService.GetFundRaisinProductOptionsRecord();
            }
            else
            {
                service2.Trace("Skipped GetFundRaisinProductOptionsRecord");
            }

            if (config.lrx_GetFundraisinProductSalesItem == true)
            {
                service2.Trace("Executing GetFundraisinProductSalesItem");
                apiService.GetFundraisinProductSalesItem();
            }
            else
            {
                service2.Trace("Skipped GetFundraisinProductSalesItem");
            }

            if (config.lrx_GetFundraisinRaffleRecords == true)
            {
                service2.Trace("Executing GetFundraisinRaffleRecords");
                apiService.GetFundraisinRaffleRecords();
            }
            else
            {
                service2.Trace("Skipped GetFundraisinRaffleRecords");
            }

            if (config.lrx_GetFundraisinRaffleTicketOptionRecords == true)
            {
                service2.Trace("Executing GetFundraisinRaffleTicketOptionRecords");
                apiService.GetFundraisinRaffleTicketOptionRecords();
            }
            else
            {
                service2.Trace("Skipped GetFundraisinRaffleTicketOptionRecords");
            }

            if (config.lrx_GetFundraisinRaffleSalesRecords == true)
            {
                service2.Trace("Executing GetFundraisinRaffleSalesRecords");
                apiService.GetFundraisinRaffleSalesRecords();
            }
            else
            {
                service2.Trace("Skipped GetFundraisinRaffleSalesRecords");
            }

            if (config.lrx_GetFundRaisinTransactionRecord == true)
            {
                service2.Trace("Executing GetFundRaisinTransactionRecord");
                apiService.GetFundRaisinTransactionRecord();
            }
            else
            {
                service2.Trace("Skipped GetFundRaisinTransactionRecord");
            }
            return;
        }


        private static lrx_Configuration GetConfigurationRecord(
    IOrganizationService service,
    ITracingService tracingService)
        {
            var query = new QueryExpression(lrx_Configuration.EntityLogicalName)
            {
                TopCount = 1,
                ColumnSet = new ColumnSet(
                    lrx_Configuration.Fields.lrx_GetFundraisinEventRecords,
                    lrx_Configuration.Fields.lrx_GetFundraisinParticipantRecords,
                    lrx_Configuration.Fields.lrx_GetFundRaisinOrganisationRecord,
                    lrx_Configuration.Fields.lrx_GetFundRaisinEventTeamsRecord,
                    lrx_Configuration.Fields.lrx_GetFundraisinTicketRecords,
                    lrx_Configuration.Fields.lrx_GetFundRaisinPromoCodeRecord,
                    lrx_Configuration.Fields.lrx_GetRegistrationFromParticipantEventRecord,
                    lrx_Configuration.Fields.lrx_GetFundraisinTicketHolderRecord,
                    lrx_Configuration.Fields.lrx_GetFundRaisinProductRecord,
                    lrx_Configuration.Fields.lrx_GetFundRaisinProductOptionsRecord,
                    lrx_Configuration.Fields.lrx_GetFundraisinProductSalesItem,
                    lrx_Configuration.Fields.lrx_GetFundraisinRaffleRecords,
                    lrx_Configuration.Fields.lrx_GetFundraisinRaffleTicketOptionRecords,
                    lrx_Configuration.Fields.lrx_GetFundraisinRaffleSalesRecords,
                    lrx_Configuration.Fields.lrx_GetFundRaisinTransactionRecord,
                    lrx_Configuration.Fields.statecode,
                    lrx_Configuration.Fields.lrx_FundraisinAPIURL,
                    lrx_Configuration.Fields.lrx_FundraisinAPIKey,
                    lrx_Configuration.Fields.lrx_DefaultCampaign,
                    lrx_Configuration.Fields.lrx_DefaultPaymentMethod
                )
            };

            query.Criteria.AddCondition(lrx_Configuration.Fields.statecode, ConditionOperator.Equal, 0);

            var result = service.RetrieveMultiple(query);
            if (result.Entities.Count == 0)
            {
                tracingService.Trace("No active configuration record found.");
                return null;
            }

            return result.Entities[0].ToEntity<lrx_Configuration>();
        }
    }
}