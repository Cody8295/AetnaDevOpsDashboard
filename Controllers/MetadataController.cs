using System;
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using Aetna.DevOps.Dashboard.UIWeb.Models;
using Swashbuckle.Swagger.Annotations;
using System.Text.RegularExpressions;
using System.Net;

namespace Aetna.DevOps.Dashboard.UIWeb.Controllers
{
    public class MetadataController : ApiController
    {
        public MetadataController() : this(new UserDetailHelper())
        {
        }

        public MetadataController(UserDetailHelper userDetailHelper)
        {
            if (userDetailHelper == null)
                throw new ArgumentNullException(nameof(userDetailHelper));

            UserHelper = userDetailHelper;
        }

        public UserDetailHelper UserHelper { get; private set; }

        #region "API Setup"
        private enum APIdatum
        {
            projectGroups = 0,
            projects = 1,
            lifecycles = 2,
            environments = 3,
        }

        private static string GetResponse(APIdatum apid)
        {
            WebRequest request;
            string reqString = String.Empty;
            switch (apid)
            {
                case APIdatum.projectGroups:
                    //request = WebRequest.Create("http://ec2-18-220-206-192.us-east-2.compute.amazonaws.com:81/api/projectgroups?apikey=API-837MQRR6IWHPNTQ9H5FLZSQJ4Y");
                    reqString = "projectGroups";
                    break;
                case APIdatum.projects:
                    reqString = "projects";
                    break;
                case APIdatum.lifecycles:
                    reqString = "lifecycles";
                    break;
                case APIdatum.environments:
                    reqString = "environments";
                    break;
                default: break;
            }
            request = WebRequest.Create("http://ec2-18-220-206-192.us-east-2.compute.amazonaws.com:81/api/"+ reqString +"?apikey=API-837MQRR6IWHPNTQ9H5FLZSQJ4Y");

            request.Credentials = CredentialCache.DefaultCredentials;
            WebResponse response = request.GetResponse();
            //Console.WriteLine(((HttpWebResponse)response).StatusDescription); 
            Stream dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd(); 
            //Console.WriteLine(responseFromServer);
            reader.Close();
            response.Close();
            return responseFromServer;
        }
        #endregion

        #region "API Calls"
        /// <summary>
        /// Pulls information about how many project groups there are
        /// </summary>
        /// <returns></returns>
        [Route("api/Octo/projectGroups")]
        [ResponseType(typeof(int))]
        [SwaggerResponse(200, "Ok - call was successful.", typeof(UserDetail))]
        public IHttpActionResult GetProjectGroups()
        {
            try
            {
                string re1 = ".*?"; // Non-greedy match on filler
                string re2 = "(\\d+)";  // Integer Number 1
                Regex r = new Regex(re1 + re2, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                Match m = r.Match(GetResponse(APIdatum.projectGroups));
                if (m.Success)
                {
                    String int1 = m.Groups[1].ToString();
                    return Ok(int1);
                }
                return InternalServerError();
            }
            catch (Exception exception)
            {
                return InternalServerError(exception);
            }
        }

        /// <summary>
        /// Pulls information about how many lifecycles there are
        /// </summary>
        /// <returns></returns>
        [Route("api/Octo/lifecycles")]
        [ResponseType(typeof(int))]
        [SwaggerResponse(200, "Ok - call was successful.", typeof(UserDetail))]
        public IHttpActionResult GetLifecycles()
        {
            try
            {
                string re1 = ".*?"; // Non-greedy match on filler
                string re2 = "(\\d+)";  // Integer Number 1
                Regex r = new Regex(re1 + re2, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                Match m = r.Match(GetResponse(APIdatum.lifecycles));
                if (m.Success)
                {
                    String int1 = m.Groups[1].ToString();
                    return Ok(int1);
                }
                return InternalServerError();
            }
            catch (Exception exception)
            {
                return InternalServerError(exception);
            }
        }

        /// <summary>
        /// Pulls information about how many projects there are
        /// </summary>
        /// <returns></returns>
        [Route("api/Octo/projects")]
        [ResponseType(typeof(int))]
        [SwaggerResponse(200, "Ok - call was successful.", typeof(UserDetail))]
        public IHttpActionResult GetProjects()
        {
            try
            {
                string re1 = ".*?"; // Non-greedy match on filler
                string re2 = "(\\d+)";  // Integer Number 1
                Regex r = new Regex(re1 + re2, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                Match m = r.Match(GetResponse(APIdatum.projects));
                if (m.Success)
                {
                    String int1 = m.Groups[1].ToString();
                    return Ok(int1);
                }
                return InternalServerError();
            }
            catch (Exception exception)
            {
                return InternalServerError(exception);
            }
        }

        /// <summary>
        /// Pulls information about how many enviornments there are
        /// </summary>
        /// <returns></returns>
        [Route("api/Octo/environments")]
        [ResponseType(typeof(int))]
        [SwaggerResponse(200, "Ok - call was successful.", typeof(UserDetail))]
        public IHttpActionResult GetEnvironments()
        {
            try
            {
                string re1 = ".*?"; // Non-greedy match on filler
                string re2 = "(\\d+)";  // Integer Number 1
                Regex r = new Regex(re1 + re2, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                Match m = r.Match(GetResponse(APIdatum.environments));
                if (m.Success)
                {
                    String int1 = m.Groups[1].ToString();
                    return Ok(int1);
                }
                return InternalServerError();
            }
            catch (Exception exception)
            {
                return InternalServerError(exception);
            }
        }
        #endregion

        /// <summary>
        /// Gets information about the current user.
        /// </summary>
        /// <returns></returns>
        [Route("api/Metadata/UserDetail")]
        [ResponseType(typeof(UserDetail))]
        [SwaggerResponse(200, "Ok - call was successful.", typeof(UserDetail))]
        public IHttpActionResult GetUserDetail()
        {
            try
            {
                UserDetail model = UserHelper.GetAuthHeaderDetails(Request);

                HttpContext.Current.Session["currentUser"] = model;

                return Ok(model);
            }
            catch (Exception exception)
            {
                return InternalServerError(exception);
            }
        }

        /// <summary>
        /// Gets information about the current operating environment.
        /// </summary>
        /// <returns></returns>
        [Route("api/Metadata/Environment")]
        [ResponseType(typeof(OperatingEnvironment))]
        [SwaggerResponse(200, "Ok - call was successful.", typeof(OperatingEnvironment))]
        public IHttpActionResult GetEnvironment()
        {
            try
            {
                String showEnvironment = System.Configuration.ConfigurationManager.AppSettings["showEnvironment"];
                String environmentName = System.Configuration.ConfigurationManager.AppSettings["environmentName"];
                String cssClass = System.Configuration.ConfigurationManager.AppSettings["cssClass"];

                DateTime buildDate = File.GetCreationTime(Assembly.GetExecutingAssembly().Location);
                Version version = Assembly.GetExecutingAssembly().GetName().Version;

                OperatingEnvironment model = new OperatingEnvironment()
                {
                    ShowEnvironment = showEnvironment,
                    EnvironmentName = environmentName,
                    CssClass = cssClass,
                    BuildDate = buildDate,
                    Version = "v" + version.ToString(4)
                };

                return Ok(model);
            }
            catch (Exception exception)
            {
                return InternalServerError(exception);
            }
        }
    }
}
