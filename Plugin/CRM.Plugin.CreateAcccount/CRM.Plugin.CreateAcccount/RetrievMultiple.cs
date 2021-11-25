using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace CRM.Plugin.RetrievMultiple
{

    public class RetrievMultiple : IPlugin
    {
        //string _unsecureConfig = "";
        //string _secureConfig = "";
        //public RetrievMultiple(string unsecureConfig, string secureConfig)
        //{
        //    if (string.IsNullOrEmpty(unsecureConfig))
        //    {
        //        throw new InvalidPluginExecutionException("Unsecure configuration missing.");
        //    }
        //    _unsecureConfig = unsecureConfig;
        //    _secureConfig = secureConfig;
        //}
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

         

            if (context.MessageName != "RetrieveMultiple" || context.Stage != 20 || context.Mode != 0 ||
                  !context.InputParameters.Contains("Query") || !(context.InputParameters["Query"] is QueryExpression))
            {
                tracing.Trace("Not expected context");
                return;
            }

            QueryExpression query = (QueryExpression)context.InputParameters["Query"];
            if (ReplaceRegardingCondition( query, tracing))
            {
              //  throw new Exception("Found");
                context.InputParameters["Query"] = query;
            }           
        }
        

     private static bool ReplaceRegardingCondition( QueryExpression query , ITracingService tracer)
        {
            if (query.EntityName != "activitypointer" || query.Criteria == null || query.Criteria.Conditions == null || query.Criteria.Conditions.Count < 2)
            {
                tracer.Trace("Not expected query");
                return false;
            }

            ConditionExpression nullCondition = null;
            ConditionExpression regardingCondition = null;

            tracer.Trace("Checking criteria for expected conditions");
            foreach (ConditionExpression cond in query.Criteria.Conditions)
            {
                if (cond.AttributeName == "activityid" && cond.Operator == ConditionOperator.Null)
                {
                    tracer.Trace("Found triggering null condition");
                    nullCondition = cond;
                }
                else if (cond.AttributeName == "regardingobjectid" && cond.Operator == ConditionOperator.Equal && cond.Values.Count == 1 && cond.Values[0] is Guid)
                {
                    tracer.Trace("Found condition for regardingobjectid");
                    regardingCondition = cond;
                }
                else
                {
                    tracer.Trace("Disregarding condition for {cond.AttributeName}");
                }
            }
            if (nullCondition == null || regardingCondition == null)
            {
                tracer.Trace("Missing expected null condition or regardingobjectid condition");
                return false;
            }
            var regardingId = (Guid)regardingCondition.Values[0];
            tracer.Trace("Found regarding id: {regardingId}");

            tracer.Trace("Removing triggering conditions");
            query.Criteria.Conditions.Remove(nullCondition);
            query.Criteria.Conditions.Remove(regardingCondition);

            tracer.Trace("Adding link-entity and condition for activity party");
            //var leActivityparty = query.AddLink("activityparty", "activityid", "activityid");
            var leActivityparty = query.AddLink("contact", "regardingobjectid", "contactid");
            leActivityparty.LinkCriteria.AddCondition("parentcustomerid", ConditionOperator.Equal, regardingId);
            return true;
        }



        /*
         
          if (context.MessageName != "RetrieveMultiple" || context.Stage != 20 || context.Mode != 0 ||
                  !context.InputParameters.Contains("Query") || !(context.InputParameters["Query"] is QueryExpression))
            {
                tracing.Trace("Not expected context");
                return;
            }

            QueryExpression query = (QueryExpression)context.InputParameters["Query"];
            if (ReplaceRegardingCondition( query, tracing))
            {
                context.InputParameters["Query"] = query;
            }
        }
        

     private static bool ReplaceRegardingCondition( QueryExpression query , ITracingService tracer)
        {
            if (query.EntityName != "activitypointer" || query.Criteria == null || query.Criteria.Conditions == null || query.Criteria.Conditions.Count < 2)
            {
                tracer.Trace("Not expected query");
                return false;
            }

            ConditionExpression nullCondition = null;
            ConditionExpression regardingCondition = null;

            tracer.Trace("Checking criteria for expected conditions");
            foreach (ConditionExpression cond in query.Criteria.Conditions)
            {
                if (cond.AttributeName == "activityid" && cond.Operator == ConditionOperator.Null)
                {
                    tracer.Trace("Found triggering null condition");
                    nullCondition = cond;
                }
                else if (cond.AttributeName == "regardingobjectid" && cond.Operator == ConditionOperator.Equal && cond.Values.Count == 1 && cond.Values[0] is Guid)
                {
                    tracer.Trace("Found condition for regardingobjectid");
                    regardingCondition = cond;
                }
                else
                {
                    tracer.Trace("Disregarding condition for {cond.AttributeName}");
                }
            }
            if (nullCondition == null || regardingCondition == null)
            {
                tracer.Trace("Missing expected null condition or regardingobjectid condition");
                return false;
            }
            var regardingId = (Guid)regardingCondition.Values[0];
            tracer.Trace("Found regarding id: {regardingId}");

            tracer.Trace("Removing triggering conditions");
            query.Criteria.Conditions.Remove(nullCondition);
            query.Criteria.Conditions.Remove(regardingCondition);

            tracer.Trace("Adding link-entity and condition for activity party");
            //var leActivityparty = query.AddLink("activityparty", "activityid", "activityid");
            var leActivityparty = query.AddLink("contact", "contactid", "regardingobjectid");
            leActivityparty.LinkCriteria.AddCondition("parentcustomerid", ConditionOperator.Equal, regardingId);
            return true;
        }
         */
    }
}
