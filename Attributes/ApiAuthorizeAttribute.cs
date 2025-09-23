using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;


namespace MyApi.Attributes
{
    [AttributeUsage(validOn: AttributeTargets.Class)]
    public class ApiAuthorizeAttribute : Attribute, IAsyncActionFilter
    {
        private const string APIKEYNAME = "ApiKey";
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!context.HttpContext.Request.Query.TryGetValue(APIKEYNAME, out var extractedApiKey))
            {
                context.Result = new ContentResult()
                {
                    StatusCode = 401,
                    Content = "Api Key was not provided"
                };
                return;
            }

            try
            {
                var ipAddress = context.HttpContext.Request.HttpContext.Connection.RemoteIpAddress;
                bool passAuth = false;
                var appSettings = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
                //var apiKey = appSettings.GetValue<string>(APIKEYNAME);
                if (appSettings.GetConnectionString("API_DB") != null)
                {
                    using (SqlConnection connection = new SqlConnection(appSettings.GetConnectionString("API_DB")))
                    {
                        connection.Open();
                        string sql = "SELECT * FROM SYS_AUTHORIZATION where AccessIP='" + ipAddress + "' and ApiKey='" + extractedApiKey + "'";
                        using (SqlCommand command = new SqlCommand(sql, connection))
                        {
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    passAuth = true;
                                }
                            }
                        }
                        
                        sql = "INSERT INTO SYS_ACCESS_LOG (AccessIP,ApiKey) values ('" + ipAddress + "','" + extractedApiKey + "')";
                        using (SqlCommand command1 = new SqlCommand(sql, connection))
                        {
                            command1.ExecuteReader();
                        }                           
                    }

                    if (!passAuth)
                    {                        
                        context.Result = new ContentResult()
                        {
                            StatusCode = 401,
                            Content = "Api Key is not valid"
                        };
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                context.Result = new ContentResult()
                {
                    StatusCode = 401,
                    Content = ex.Message + "<br />" + ex.StackTrace
                };
                return;
            }
            await next();
        }

        private const string HttpContext = "MS_HttpContext";
        private const string RemoteEndpointMessage = "System.ServiceModel.Channels.RemoteEndpointMessageProperty";

        private string GetIP(HttpRequestMessage request)
        {
            if (request.Properties.ContainsKey(HttpContext))
            {
                dynamic ctx = request.Properties[HttpContext];
                if (ctx != null)
                {
                    return ctx.Request.UserHostAddress;
                }
            }

            if (request.Properties.ContainsKey(RemoteEndpointMessage))
            {
                dynamic remoteEndpoint = request.Properties[RemoteEndpointMessage];
                if (remoteEndpoint != null)
                {
                    return remoteEndpoint.Address;
                }
            }

            return null;
        }
    }
}
