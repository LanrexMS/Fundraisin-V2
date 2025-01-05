// Decompiled with JetBrains decompiler
// Type: FundraisinApp_Integration.Plugins.FundraisinIntegration
// Assembly: FundraisinApp-Integration.Plugin, Version=1.0.0.0, Culture=neutral, PublicKeyToken=be17da884c09fb48
// MVID: A80D178E-91E0-4361-9810-5F6936033CCA
// Assembly location: C:\Users\Nico Benito\Downloads\NicoTestSolution_1_0_0_2\PluginAssemblies\FundraisinApp-IntegrationPlugin-AD40DC3F-2002-4C54-899F-1599020D0AFB\FundraisinApp-IntegrationPlugin.dll

using FundraisinApp_Integration.Plugins.Service;
using Microsoft.Xrm.Sdk;
using System;
using System.Reflection;

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
            try
            {
                object inputParameter = ((DataCollection<string, object>)((IExecutionContext)service1).InputParameters)["lrx_fundraisinIntegrationJSONParams"];
                new Fundraising_APIService(organizationService, service1, service2, inputParameter).GetFundraisinEventRecords();
                new Fundraising_APIService(organizationService, service1, service2, inputParameter).GetFundraisinParticipantRecords();
                new Fundraising_APIService(organizationService, service1, service2, inputParameter).GetFundraisinTicketRecords();
                new Fundraising_APIService(organizationService, service1, service2, inputParameter).GetRegistrationFromParticipantEventRecord();
                new Fundraising_APIService(organizationService, service1, service2, inputParameter).GetFundraisinTicketHolderRecord();
                new Fundraising_APIService(organizationService, service1, service2, inputParameter).GetFundRaisinProductRecord();
                new Fundraising_APIService(organizationService, service1, service2, inputParameter).GetFundRaisinProductOptionsRecord();
                new Fundraising_APIService(organizationService, service1, service2, inputParameter).GetFundraisinDonationRecords();
            }
            catch (Exception ex)
            {
                service2.Trace("Exception: " + ex.Message, Array.Empty<object>());
            }
        }
    }
}