using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Fundraising_Engagement.Plugins.Service;

namespace Fundraising_Engagement.Plugins
{
    public class FundraisinIntegration : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            // Obtain the execution context from the service provider
            IPluginExecutionContext context = (IPluginExecutionContext)
                serviceProvider.GetService(typeof(IPluginExecutionContext));

            // Obtain Organization Service factory service from the service provider
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)
                serviceProvider.GetService(typeof(IOrganizationServiceFactory));

            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            // Obtain the tracing service
            ITracingService tracingService = (ITracingService)
                serviceProvider.GetService(typeof(ITracingService));

            try
            {
                var JSONStringParams = context.InputParameters["lrx_fundraisinIntegrationJSONParams"];

                var fundraisingService = new Fundraising_APIService(service, context, tracingService);
                fundraisingService.GetFundraisinDonationRecords();

            }
            catch (Exception ex)
            {
                tracingService.Trace($"Exception: {ex.Message}");
            }
        }
    }
}
