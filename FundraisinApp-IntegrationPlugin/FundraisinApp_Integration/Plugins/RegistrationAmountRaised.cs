using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrmEarlyBound;
using Fundraising_Integration.Plugins.Service;
using System.Reflection;

namespace FundraisinApp_Integration.Plugins
{
    public class RegistrationAmountRaised : IPlugin
    {
        static RegistrationAmountRaised()
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
            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = serviceFactory.CreateOrganizationService(context.UserId);
            var tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            var fundraisingService = new FundraisingService(service, context, tracingService);

            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                var targetEntity = (Entity)context.InputParameters["Target"];

                if (targetEntity.LogicalName == MsnFp_Transaction.EntityLogicalName)
                {

                    MsnFp_Transaction transaction = targetEntity.ToEntity<MsnFp_Transaction>();

                    switch (context.MessageName)
                    {
                        case "Create":
                            fundraisingService.UpdateRegistrationAmountRaised(transaction);
                            break;
                        case "Update":
                            fundraisingService.UpdateRegistrationAmountRaised(transaction);
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }
}
