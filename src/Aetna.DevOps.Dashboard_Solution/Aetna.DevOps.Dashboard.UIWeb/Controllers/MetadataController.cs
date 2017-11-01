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
    public class Deploy
    {
        public long TimeAndDate;
        public string Message;
        public System.Collections.Generic.List<string> RelatedDocs;
        public string Category;
        public Deploy(long timeAndDate, string msg, System.Collections.Generic.List<string> related, string category)
        {
            TimeAndDate = timeAndDate;
            Message = msg;
            RelatedDocs = related;
            Category = category;
        }
        public override string ToString()
        {
            return Message;
        }
    }

    public class DeployList
    {
        public System.Collections.Generic.List<Deploy> deploys;
        public DeployList() { deploys = new System.Collections.Generic.List<Deploy>(); }
        public void add(Deploy d) { deploys.Add(d); }
    }

    public class MetadataController : ApiController
    {
        #region "Aetna Provided"
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
        #endregion

        #region "API Setup"
        private const string API_URL = "http://ec2-18-220-206-192.us-east-2.compute.amazonaws.com:81/api/";
        private const String API_KEY = "API-A5I5VUHAOV0VJJN6LQ6MXCPSMS";
        private readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private enum APIdatum
        {
            projectGroups = 0,
            projects = 1,
            lifecycles = 2,
            environments = 3,
            deploys = 4,
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
                case APIdatum.deploys:
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

        private DateTime dateTimeFromEpoch(long time)
        {
            return epoch.AddSeconds(time);
        }

        private long epochFromDateTime(DateTime dt)
        {
            TimeSpan epochSpan = dt.ToUniversalTime() - epoch;
            return (long)Math.Floor(epochSpan.TotalSeconds);
        }

        private DeployList graphDeployments(string jsonTxt)
        {
            DeployList dl = new DeployList();
            dynamic jsonDeser = JsonConvert.DeserializeObject(jsonTxt);
            foreach(dynamic o in jsonDeser.Items)
            {
                DateTime parsedDt = Convert.ToDateTime(o.Occurred.ToString());
                long occur = epochFromDateTime(parsedDt);
                if(DateTime.Now.AddDays(-1) > parsedDt) { continue; } // ignore events that took place more than 1 day ago
                Deploy d = new Deploy(occur, o.Message.ToString(),
                    JsonConvert.DeserializeObject<System.Collections.Generic.List<string>>(o.RelatedDocumentIds.ToString()), // nested list element
                    o.Category.ToString());
                //Console.Write(d.ToString());
                if (o.Category == "DeploymentStarted") { dl.add(d); }
                if (o.Category == "DeploymentQueued") { dl.add(d); }
                if (o.Category == "DeploymentSucceeded") { dl.add(d); }
                if (o.Category == "DeploymentFailed") { dl.add(d); }
            }
            return dl;
            //return JsonConvert.SerializeObject(dl,Formatting.Indented);
        }

        private string formatDeployTimes(ArrayList deploys)
        {
            string retVal = String.Empty;
            int[] c = new int[24]; // one for each hour
            foreach (String[] strArr in deploys)
            {
                DateTime occ = Convert.ToDateTime(strArr[1]);
                int t = int.Parse(occ.ToString("HH"));
                c[t] += (occ > DateTime.Now.AddDays(-1) && occ <= DateTime.Now ? 1 : 0);
                //retVal += strArr[0].ToString() + "," + strArr[1].ToString() + Environment.NewLine;
            }
            int endTime = int.Parse(DateTime.Now.ToString("HH"));
            string[] times = new string[24];
            for (int i = 0; i < 24; i++)
            {
                if (endTime + i + 1 <= 23)
                    times[i] = (endTime + i + 1).ToString() + ":00" + "," + c[(endTime + i + 1)].ToString() + ";";
                else
                    times[i] = (endTime + i - 23).ToString() + ":00" + "," + c[(endTime + i - 23)].ToString() + ";";
            }

            foreach (string a in times) { retVal += a; }

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
        /// Pulls information about how many deploys there are over the past 24 hours and information about each one
        /// </summary>
        /// <returns></returns>
        
        [Route("api/Octo/deploys")]
        [ResponseType(typeof(int))]
        [SwaggerResponse(200, "Ok - call was successful.", typeof(UserDetail))]
        public IHttpActionResult GetDeploys()
        {
            try
            {
                DeployList dl = graphDeployments(GetResponse(APIdatum.deploys));
                //if (dl.deploys.Count == 0) { return Ok(""); }
                return Ok<System.Collections.Generic.List<Deploy>>(dl.deploys);
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
