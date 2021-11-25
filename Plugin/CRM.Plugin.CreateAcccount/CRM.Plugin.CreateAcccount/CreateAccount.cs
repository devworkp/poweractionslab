using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;

namespace CRM.Plugin.CreateAcccount
{

    public class CreateAcccount : IPlugin
    {
        string _unsecureConfig = "";
        string _secureConfig = "";
        public CreateAcccount(string unsecureConfig, string secureConfig)
        {
            if (string.IsNullOrEmpty(unsecureConfig))
            {
                throw new InvalidPluginExecutionException("Unsecure configuration missing.");
            }
            _unsecureConfig = unsecureConfig;
            _secureConfig = secureConfig;
        }
        public void Execute(IServiceProvider serviceProvider)
        {
            ITracingService tracing = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            // ExecutionContext object
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            // Organization Service factory object 
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));

            // Organization Service object
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            Entity preImageEntity = (context.PreEntityImages != null && context.PreEntityImages.Contains("PreImage")) ? (Entity)context.PreEntityImages["PreImage"] : null;

            Entity postImageEntity = (context.PostEntityImages != null && context.PostEntityImages.Contains("PostImage")) ? (Entity)context.PostEntityImages["PostImage"] : null;

            //if (postImageEntity != null)
            //    throw new Exception("Post Image foundb");

            tracing.Trace(context.Depth.ToString());
            tracing.Trace("unsecure" + _unsecureConfig);
            tracing.Trace("secure" + _secureConfig);

            throw new Exception("Test");
           
        }
    }
}
