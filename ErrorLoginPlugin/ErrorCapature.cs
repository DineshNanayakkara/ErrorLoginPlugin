using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ErrorHandling;

namespace ErrorLoginPlugin
{
    public class ErrorCapature : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            // Extract the tracing service for use in debugging sandboxed plug-ins.
            // If you are not registering the plug-in in the sandbox, then you do
            // not have to add any tracing service related code.
            ITracingService tracingService =
                (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            // Obtain the execution context from the service provider.
            IPluginExecutionContext context = (IPluginExecutionContext)
                serviceProvider.GetService(typeof(IPluginExecutionContext));

            // The InputParameters collection contains all the data passed in the message request.
            if (context.InputParameters.Contains("Target") &&
                context.InputParameters["Target"] is Entity)
            {
                // Obtain the target entity from the input parameters.
                Entity entity = (Entity)context.InputParameters["Target"];

                // Verify that the target entity represents an entity type you are expecting. 
                // For example, an account. If not, the plug-in was not registered correctly.
                if (entity.LogicalName != "contact")
                    return;

                // Obtain the organization service reference which you will need for
                // web service calls.
                IOrganizationServiceFactory serviceFactory =
                    (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
                
                try
                {
                    throw new Exception();
                }
                catch (Exception ex)
                {
                    //create a new error log record

                    var executeMultiple = new ExecuteMultipleRequest 
                                                {
                                                    Settings = new ExecuteMultipleSettings()
                                                    {
                                                        ContinueOnError = false,
                                                        ReturnResponses = true
                                                    },
                                                    // Create an empty organization request collection.
                                                    Requests = new OrganizationRequestCollection() 
                                                };

                    new_errorlog errorLog = new new_errorlog();
                    errorLog.new_name = "Error by Plugin";
                    errorLog.new_description = "Error Description by plugin";

                    
                    EntityCollection input = new EntityCollection();
                    input.Entities.Add(errorLog.ToEntity<new_errorlog>());
                    
                    // Add a CreateRequest for each entity to the request collection.
                    foreach (var inputEntity in input.Entities)
                    {
                        CreateRequest createRequest = new CreateRequest { Target = inputEntity };
                        executeMultiple.Requests.Add(createRequest);
                    }

                    // Execute all the requests in the request collection using a single web method call.
                    ExecuteMultipleResponse responseWithResults =
                        (ExecuteMultipleResponse)service.Execute(executeMultiple);


                }
            }
        }
    }
}
