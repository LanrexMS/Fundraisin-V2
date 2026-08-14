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

            string entityName = string.Empty;

            if (service1.InputParameters.Contains("lrx_entity") &&
                service1.InputParameters["lrx_entity"] != null)
            {
                entityName = service1.InputParameters["lrx_entity"].ToString().Trim().ToLower();
            }

            if (string.IsNullOrWhiteSpace(entityName))
            {
                service2.Trace("Entity Name was not supplied.");
                return;
            }

            service2.Trace($"Entity Name : {entityName}");

            var config = GetConfigurationRecord(organizationService, service2);

            if (config == null)
            {
                return;
            }
            Fundraising_APIService apiService = new Fundraising_APIService(organizationService, service1, service2, config, inputParameter, entityName);
            switch (entityName)
            {
                case "event":
                    apiService.GetFundraisinEventRecords();
                    break;

                case "participant":
                    apiService.GetFundraisinParticipantRecords();
                    break;

                case "organisation":
                    apiService.GetFundRaisinOrganisationRecord();
                    break;

                case "eventteam":
                    apiService.GetFundRaisinEventTeamsRecord();
                    break;

                case "ticket":
                    apiService.GetFundraisinTicketRecords();
                    break;

                case "promocode":
                    apiService.GetFundRaisinPromoCodeRecord();
                    break;

                case "registration":
                    apiService.GetRegistrationFromParticipantEventRecord();
                    break;

                case "ticketholder":
                    apiService.GetFundraisinTicketHolderRecord();
                    break;

                case "product":
                    apiService.GetFundRaisinProductRecord();
                    break;

                case "productoption":
                    apiService.GetFundRaisinProductOptionsRecord();
                    break;

                case "salesitem":
                    apiService.GetFundraisinProductSalesItem();
                    break;

                case "raffle":
                    apiService.GetFundraisinRaffleRecords();
                    break;

                case "raffleticketoption":
                    apiService.GetFundraisinRaffleTicketOptionRecords();
                    break;

                case "rafflesales":
                    apiService.GetFundraisinRaffleSalesRecords();
                    break;

                case "transaction":
                    apiService.GetFundRaisinTransactionRecord();
                    break;

                case "donation":
                    apiService.GetFundRaisinOflineDonation();
                    break;

                case "waves":
                    apiService.GetFundraisinWaveRecords();
                    break;
                case "eventproduct":
                    apiService.GetFundraisinEventProducts();
                    break;
                case "pages":
                    apiService.GetFundraisinPages();
                    break;
                default:
                    service2.Trace($"Unknown Entity Name : {entityName}");
                    break;
            }
            service2.Trace("===== Plugin Completed Successfully =====");
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
                    lrx_Configuration.Fields.lrx_FundraisinAPIURL,
                    lrx_Configuration.Fields.lrx_FundraisinAPIKey,
                    lrx_Configuration.Fields.lrx_DefaultCampaign,
                    lrx_Configuration.Fields.lrx_DefaultPaymentMethod,
                    lrx_Configuration.Fields.lrx_DefaultPrimaryDesignation,
                    lrx_Configuration.Fields.lrx_FirstNameLastNameEmail,
                    lrx_Configuration.Fields.lrx_FirstNameLastNameMobile,
                    lrx_Configuration.Fields.lrx_FirstNameLastNameDob
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