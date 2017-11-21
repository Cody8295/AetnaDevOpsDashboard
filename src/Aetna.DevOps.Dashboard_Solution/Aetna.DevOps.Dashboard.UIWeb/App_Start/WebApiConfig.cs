using System.Configuration;
using System.Diagnostics;
using System.Net.Http.Formatting;
using System.Web.Configuration;
using System.Web.Http;
using Newtonsoft.Json.Serialization;

namespace Aetna.DevOps.Dashboard.UIWeb
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{Id}",
                defaults: new { id = RouteParameter.Optional }
            );


            config.Formatters.Clear();
            //config.Formatters.Add(new JilFormatter());

            //config.Formatters.RemoveAt(0);
            //config.Formatters.Insert(0, new JilFormatter());

            GlobalConfiguration.Configuration.Formatters.Add(new JsonMediaTypeFormatter()
            {
                Indent = true
            });

            config.Formatters.JsonFormatter.Indent = true;
            config.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling =
                Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            config.Formatters.JsonFormatter.SerializerSettings.ContractResolver =
                new CamelCasePropertyNamesContractResolver();

            //GlobalConfiguration.Configuration.Formatters.JsonFormatter.MediaTypeMappings.Add(
            //    new QueryStringMapping("type", "json", new MediaTypeHeaderValue("application/json")));

            //GlobalConfiguration.Configuration.Formatters.XmlFormatter.MediaTypeMappings.Add(
            //    new QueryStringMapping("type", "xml", new MediaTypeHeaderValue("application/xml")));

            var customErrors = (CustomErrorsSection)ConfigurationManager
                .GetSection("system.web/customErrors");

            IncludeErrorDetailPolicy errorDetailPolicy;

            switch (customErrors.Mode)
            {
                case CustomErrorsMode.RemoteOnly:
                    errorDetailPolicy = IncludeErrorDetailPolicy.LocalOnly;
                    break;
                case CustomErrorsMode.On:
                    errorDetailPolicy = IncludeErrorDetailPolicy.Never;
                    break;
                case CustomErrorsMode.Off:
                    errorDetailPolicy = IncludeErrorDetailPolicy.Always;
                    break;
                default:
                    errorDetailPolicy = IncludeErrorDetailPolicy.Never;
                    break;
            }

            Debug.WriteLine(
                "Web API Startup: Setting .IncludeErrorDetailPolicy to '{0}', because customErrors set to '{1}'.",
                errorDetailPolicy, customErrors.Mode);
            GlobalConfiguration.Configuration.IncludeErrorDetailPolicy = errorDetailPolicy;

            // Per OWASP: https://www.owasp.org/index.php/Exception_Handling
            //GlobalConfiguration.Configuration.Filters.Add(new LogExceptionFilterAttribute());
            //GlobalConfiguration.Configuration.Services.Add(typeof(IExceptionLogger), new DefaultExceptionLogger());
        }
    }
}
