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
using System.Collections;
using Newtonsoft.Json;

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
        const string API_URL = "http://ec2-18-220-206-192.us-east-2.compute.amazonaws.com:81/api/";
        const String API_KEY = "API-A5I5VUHAOV0VJJN6LQ6MXCPSMS";

        private enum APIdatum
        {
            projectGroups = 0,
            projects = 1,
            lifecycles = 2,
            environments = 3,
            builds = 4,
        }

        private static string GetResponse(APIdatum apid)
        {
            WebRequest request;
            string reqString = String.Empty;
            switch (apid)
            {
                case APIdatum.projectGroups:
                    reqString = "projectGroups?";
                    break;
                case APIdatum.projects:
                    reqString = "projects?";
                    break;
                case APIdatum.lifecycles:
                    reqString = "lifecycles?";
                    break;
                case APIdatum.environments:
                    reqString = "environments?";
                    break;
                case APIdatum.builds:
                    reqString = "events?take=1000&";
                    break;
                default: break;
            }
            request = WebRequest.Create(API_URL + reqString + "apikey=" + API_KEY);
            request.Credentials = CredentialCache.DefaultCredentials;
            WebResponse response = request.GetResponse();
            Stream dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string serverResponse = reader.ReadToEnd(); 
            reader.Close();
            response.Close();
            return serverResponse;
        }

        private string getFirstInt(string haystack) // credits to txt2re.com
        {
            string re1 = ".*?"; // Non-greedy match on filler
            string re2 = "(\\d+)";  // Integer Number 1
            Regex r = new Regex(re1 + re2, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            Match m = r.Match(haystack);
            if (m.Success)
            {
                String int1 = m.Groups[1].ToString();
                return int1;
            }
            return "API error";
        }

        private string graphDeployments(string jsonTxt)
        {
            ArrayList deploysStarted = new ArrayList();
            dynamic jsonDeser = JsonConvert.DeserializeObject(jsonTxt);

            DateTime yesterday = DateTime.Now.AddDays(-1); // get yesterday
            string date = yesterday.ToString("yyyy-MM-ddTHH");

            foreach(dynamic o in jsonDeser.Items)
            {
                //deploysStarted.Add(new String[] { o.Message, o.Occurred });
                if (o.Category== "DeploymentStarted" || o.Category=="DeploymentFailed" || o.Category== "DeploymentSucceeded")
                {
                    // API date format is yyyy-MM-ddTHH:mm:ss
                    string occurred = o.Occurred;
                    string[] timeCheck = occurred.Split(':');
                    
                    if(date.CompareTo(timeCheck[0]) > 0)
                    {
                        string[] returnHour = timeCheck[0].Split(' '); // split between date and time
                        int h = int.Parse(returnHour[1]);
                        deploysStarted.Add(new String[] { o.Category, h.ToString() }); // select hour
                    }
                }
            }
            string retVal = String.Empty;
            foreach(String[] strArr in deploysStarted)
            {
                if (retVal == String.Empty)
                {
                    retVal += strArr[0] + ":" + strArr[1];
                }
                else
                {
                    retVal += "," + strArr[0] + ":" + strArr[1];
                }
            }
            return retVal;
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
                return Ok(getFirstInt(GetResponse(APIdatum.projectGroups)));
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
                return Ok(getFirstInt(GetResponse(APIdatum.lifecycles)));
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
                return Ok(getFirstInt(GetResponse(APIdatum.projects)));
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
                return Ok(getFirstInt(GetResponse(APIdatum.environments)));
            }
            catch (Exception exception)
            {
                return InternalServerError(exception);
            }
        }

        /// <summary>
        /// Pulls information about how many builds there are over the past 24 hours
        /// </summary>
        /// <returns></returns>
        [Route("api/Octo/builds")]
        [ResponseType(typeof(int))]
        [SwaggerResponse(200, "Ok - call was successful.", typeof(UserDetail))]
        public IHttpActionResult GetBuilds()
        {
            try
            {
                return Ok(graphDeployments(GetResponse(APIdatum.builds)));
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
