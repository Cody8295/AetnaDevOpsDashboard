﻿using System;
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using Aetna.DevOps.Dashboard.UIWeb.Models;
using Swashbuckle.Swagger.Annotations;
using System.Text.RegularExpressions;
using System.Net;
using System.Collections.Generic;
using Newtonsoft.Json;
using Environment = Aetna.DevOps.Dashboard.UIWeb.Models.Environment;

namespace Aetna.DevOps.Dashboard.UIWeb.Controllers
{
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

        #region "Constants"
        private const string API_URL = "http://ec2-18-220-206-192.us-east-2.compute.amazonaws.com:81/api/";
        private const String API_KEY = "API-A5I5VUHAOV0VJJN6LQ6MXCPSMS";
        private readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        #endregion

        #region "API Datum Enum"
        private enum APIdatum
        {
            projectGroups = 0,
            projects = 1,
            lifecycles = 2,
            environments = 3,
            deploys = 4,
            machines = 5,
            projectProgression = 6,
            dashboard = 7
        }
        #endregion

        #region "HTTP Request Constructor"
        private static string GetResponse(APIdatum apid, string param="")
        {
            WebRequest request;
            string reqString = String.Empty;
            switch (apid)
            {
                case APIdatum.projectGroups:
                    reqString = "projectGroups?take=1000&";
                    break;
                case APIdatum.projects:
                    reqString = "projects?";
                    break;
                case APIdatum.lifecycles:
                    reqString = "lifecycles?";
                    break;
                case APIdatum.environments:
                    reqString = "environments/"+ (param==String.Empty?"":param) +"?";
                    break;
                case APIdatum.deploys:
                    reqString = "events?take=1000&";
                    break;
                case APIdatum.machines:
                    if (param == string.Empty) // all machines
                    {
                        reqString = "machines?take=100000&";
                    }
                    else // asking for info on machines about specific environment
                    {
                        reqString = "environments/" + param + "/machines?";
                    }
                    break;
                case APIdatum.projectProgression:
                    if (param == String.Empty) { return ""; } // can't find progression of project without it's name
                    reqString = "progression/" + param + "?";
                    break;
                case APIdatum.dashboard:
                    reqString = "dashboard?";
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
        #endregion

        #region "Get Active Projects by Environment"
        private static List<ActiveDeploy> getEnvProjects(string envId)
        {
            List<ActiveDeploy> projList = new List<ActiveDeploy>();
            List<Project> projects = makeProjectList();
            string response = GetResponse(APIdatum.dashboard);
            dynamic jsonDeser = JsonConvert.DeserializeObject(response);
            foreach (dynamic p in jsonDeser.Items)
            {
                if (p.EnvironmentId == envId)
                {
                    string projName = "";
                    foreach (Project proj in projects) { if (proj.id == p.ProjectId.ToString()) { projName = proj.name; } }
                    projList.Add(new ActiveDeploy(p.Id.ToString(), p.ProjectId.ToString(),
                    p.ReleaseId.ToString(), p.TaskId.ToString(), p.ChannelId.ToString(), p.ReleaseVersion.ToString(),
                    p.Created.ToString(), p.QueueTime.ToString(), p.CompletedTime.ToString(), p.State.ToString(),
                    p.HasWarningsOrErrors.ToString(), p.ErrorMessage.ToString(), p.Duration.ToString(), p.IsCurrent.ToString(),
                    p.IsCompleted.ToString(), projName));
                }
            }
            return projList;
        }
        #endregion

        #region "Get Number of Environments"
        private static Dictionary<string, string> getNumberEnviroments(string jsonTxt)
        {
            dynamic jsonDeser = JsonConvert.DeserializeObject(jsonTxt);
            Dictionary<string, string> environments = new Dictionary<string, string>();

            foreach (dynamic o in jsonDeser.Items)
            {
                if (environments.ContainsKey(o.Name.ToString()))
                    environments[o.Name.ToString()]++;
                else
                    environments.Add(o.Name.ToString(), o.Id.ToString());

            }
            return environments;
        }
        #endregion
    
        #region "Get Number of Machines"
        private static Dictionary<string, int> getNumberMachines(string jsonTxt)
        {
            dynamic jsonDeser = JsonConvert.DeserializeObject(jsonTxt);
            Dictionary<string, int> machines = new Dictionary<string, int>();

            foreach (dynamic o in jsonDeser.Items)
            {
                foreach (dynamic machine in o.EnvironmentIds)
                {
                    if (machines.ContainsKey(machine.ToString()))
                        machines[machine.ToString()]++;
                    else
                        machines.Add(machine.ToString(), 1);
                }
            }

            return machines;
        }
        #endregion
    
        #region "Make Environment List"
        private static EnvironmentList makeEnvironmentList()
        {
            EnvironmentList el = new EnvironmentList();
            Dictionary<string, int> numMachines = getNumberMachines(GetResponse(APIdatum.machines));
            Dictionary<string, string> environments = getNumberEnviroments(GetResponse(APIdatum.environments));

            foreach (string key in environments.Keys)
            {
                el.add(new Environment(environments[key], key, (numMachines.ContainsKey(environments[key]) ? numMachines[environments[key]].ToString() : "0"), getMachines(environments[key])));
            }

            return el;
        }
        #endregion
    
        #region "Make Project List"
        private static List<Project> makeProjectList()
        {
            List<Project> pl = new List<Project>();
            string jsonTxt = GetResponse(APIdatum.projects);
            dynamic jsonDeser = JsonConvert.DeserializeObject(jsonTxt);
            foreach (dynamic o in jsonDeser.Items)
            {
                pl.Add(new Project(o.Id.ToString(), o.ProjectGroupId.ToString(), o.Name.ToString(), o.LifecycleId.ToString(), o.DeploymentProcessId.ToString()));
            }
            return pl;
        }
        #endregion
    
        #region "Sort Project List"
        private static List<ProjectGroup> sortProjectGroups()
        {
            List<ProjectGroup> pg;
            ProjectGroupDictionary pgd = new ProjectGroupDictionary();

            string jsonTxt = GetResponse(APIdatum.projectGroups);

            dynamic jsonDeser = JsonConvert.DeserializeObject(jsonTxt);

            foreach (dynamic o in jsonDeser.Items)
            {
                pgd.AddProjectGroup(o.Id.ToString(), new ProjectGroup(o.Name.ToString(), o.Id.ToString()));
            }

            List<Project> projects = makeProjectList();

            foreach(Project p in projects)
            {
                pgd.addProject(p.getGroupId(), p);
            }

            pg = pgd.getProjectGroups();
            return pg;
        }
        #endregion

        #region "Get Release List"
        private List<Release> getReleaseList(string response)
        {
            dynamic releases = JsonConvert.DeserializeObject(response);
            ReleaseList rl = new ReleaseList();
            foreach (dynamic r in releases.Releases)
            {
                Release re = new Release(r.Release.Id.ToString(), r.Release.Version.ToString(), r.Release.ProjectId.ToString(),
                    r.Release.ChannelId.ToString(), isoToDateTime(r.Release.Assembled.ToString()), r.Release.ReleaseNotes.ToString());
                rl.add(re);
            }
            return rl.releaseList;
        }
        #endregion
    
        #region "Get First Int"
        private static string getFirstInt(string haystack) // credits to txt2re.com
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
        #endregion

        #region "Datetime functions"

        #region "ISO to Datetime"
        private string isoToDateTime(string iso)
        {
            DateTime dateTime = DateTime.Parse(iso).ToLocalTime();
            return dateTime.ToString();
        }
        #endregion

        private DateTime dateTimeFromEpoch(long time)
        {
            return epoch.AddSeconds(time);
        }

        private long epochFromDateTime(DateTime dt)
        {
            TimeSpan epochSpan = dt.ToUniversalTime() - epoch;
            return (long)Math.Floor(epochSpan.TotalSeconds);
        }
        #endregion

        #region "Format deploys for graphing"
        private DeployList graphDeployments(string jsonTxt)
        {
            DeployList dl = new DeployList();
            dynamic jsonDeser = JsonConvert.DeserializeObject(jsonTxt);
            foreach(dynamic o in jsonDeser.Items)
            {
                string occured = o.Occurred.ToString(); //isoToDateTime(o.Occurred.ToString());
                DateTime parsedDt = Convert.ToDateTime(occured);
                string occuredISO = parsedDt.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fZ");
                if (DateTime.Now.AddDays(-1) > parsedDt) { continue; } // ignore events that took place more than 1 day ago
                Deploy d = new Deploy(occuredISO, o.Message.ToString(),
                    JsonConvert.DeserializeObject<System.Collections.Generic.List<string>>(o.RelatedDocumentIds.ToString()), // nested list element
                    o.Category.ToString());
                if (o.Category == "DeploymentStarted") { dl.add(d); }
                if (o.Category == "DeploymentQueued") { dl.add(d); }
                if (o.Category == "DeploymentSucceeded") { dl.add(d); }
                if (o.Category == "DeploymentFailed") { dl.add(d); }
            }
            return dl;
        }
        #endregion
    
        #region "Get Machines"
        private static MachineList getMachines(string envId)
        {
            string machineResponse = GetResponse(APIdatum.machines, envId);
            dynamic mach = JsonConvert.DeserializeObject(machineResponse);
            MachineList m = new MachineList();
            foreach(dynamic mac in mach.Items)
            {
                System.Collections.Generic.List<string> el = new System.Collections.Generic.List<string>();
                foreach(dynamic env in mac.EnvironmentIds)
                {
                    //Environment e = getEnviron(envName);
                    el.Add(env.ToString());
                }
                //Machine m = new Machine()
                Machine machine = new Machine(mac.Id.ToString(), mac.Name.ToString(), mac.Uri.ToString(), el, mac.Status.ToString(), mac.StatusSummary.ToString(), mac.IsInProcess.ToString());
                m.add(machine);
            }
            return m;
        }
        #endregion

        #region "Get Environment"
        private Environment getEnviron(string envName)
        {
            string environData = GetResponse(APIdatum.environments, envName);
            dynamic env = JsonConvert.DeserializeObject(environData);
            Environment e = new Environment(env.Id.ToString(), env.Name.ToString(), env.Description.ToString(), getMachines(envName));
            return e;
        }
        #endregion

        #endregion

        #region "SignalR stuff"
        public static Boolean UpdateDataState(DataState state)
        {
            Boolean anyChange = false; // debugging: should be false by default, set true on change

            // Used to notify user when data has changed
            state.isChanged = new Dictionary<string, bool>(){ // debugging: should be false by default, set true on change
                { "ProjectGroups", false },
                { "Projects", false },
                { "Lifecycles", false },
                { "Environments", false },
                { "Deploys", false }
             };

            // Get New Data

            List<ProjectGroup> pg = sortProjectGroups();
            if (state.ProjectGroups == null || state.ProjectGroups != pg)
            {
                state.ProjectGroups = sortProjectGroups();
                state.isChanged["ProjectGroups"] = true;
                anyChange = true;
            }

            List<Project> pl = makeProjectList();
            if (state.Projects == null || state.Projects != pl)
            {
                state.Projects= pl;
                state.isChanged["Projects"] = true;
                anyChange = true;
            }

            // Temporary until Lifecycle object is added
            int nlc = 0;
            Int32.TryParse(getFirstInt(GetResponse(APIdatum.lifecycles)),out nlc);
            if (state.Lifecycles != nlc)
            {
                state.Lifecycles = nlc;
                state.isChanged["Lifecycles"] = true;
                anyChange = true;
            }

            List<Environment> env = makeEnvironmentList().environments;
            if (state.Environments == null || state.Environments != env)
            {
                state.Environments = env;
                state.isChanged["Environments"] = true;
                anyChange = true;
            }

            List<Deploy> dp = null; // Temporary
            if (state.Deploys != dp)
            {
                state.Deploys = dp;
                state.isChanged["Deploys"] = true;
                anyChange = true;
            }

            return anyChange;
        }
        #endregion

        #region "API Calls"

        #region "Project Groups"
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
        #endregion

        #region "Lifecycles"
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
        #endregion

        #region "Projects Info"
        /// <summary>
        /// Pulls information about projects
        /// </summary>
        /// <returns></returns>
        [Route("api/Octo/projectsInfo")]
        [ResponseType(typeof(int))]
        [SwaggerResponse(200, "Ok - call was successful.", typeof(UserDetail))]
        public IHttpActionResult GetProjectsInfo()
        {
            try
            {
                return Ok<List<Project>>(makeProjectList());
            }
            catch (Exception exception)
            {
                return InternalServerError(exception);
            }
        }
        #endregion

        #region "Projects by Environment"
        /// <summary>
        /// Pulls information about projects by environment
        /// </summary>
        /// <returns></returns>
        [Route("api/Octo/environmentProjects")]
        [ResponseType(typeof(int))]
        [SwaggerResponse(200, "Ok - call was successful.", typeof(UserDetail))]
        public IHttpActionResult GetEnvProjects(string envId)
        {
            try
            {
                return Ok<List<ActiveDeploy>>(getEnvProjects(envId));
            }
            catch (Exception exception)
            {
                return InternalServerError(exception);
            }
        }
        #endregion

        #region "Project Count"
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
        #endregion

        #region "Project Releases"
        /// <summary>
        /// Pulls information about how a project has progressed in respect to releases
        /// </summary>
        /// <returns></returns>
        [Route("api/Octo/projectProgression")]
        [ResponseType(typeof(int))]
        [SwaggerResponse(200, "Ok - call was successful.", typeof(UserDetail))]
        public IHttpActionResult GetProjectProgression(string project)
        {
            try
            {
                return Ok<List<Release>>(getReleaseList(GetResponse(APIdatum.projectProgression, project)));
            }
            catch (Exception exception)
            {
                return InternalServerError(exception);
            }
        }
        #endregion

        #region "Machines by Environment"
        /// <summary>
        /// Pulls information about how each machine for a specified environment
        /// </summary>
        /// <returns></returns>
        [Route("api/Octo/environmentMachines")]
        [ResponseType(typeof(int))]
        [SwaggerResponse(200, "Ok - call was successful.", typeof(UserDetail))]
        public IHttpActionResult GetEnvironmentMachines(string envId)
        {
            try
            {
                return Ok<List<Machine>>(getMachines(envId).machines);
            }
            catch (Exception exception)
            {
                return InternalServerError(exception);
            }
        }
        #endregion

        #region "Environments"
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
        #endregion

        #region "Environment List"
        /// <summary>
        /// Pulls information about how many enviornments there are
        /// </summary>
        /// <returns></returns>
        [Route("api/Octo/environmentList")]
        [ResponseType(typeof(int))]
        [SwaggerResponse(200, "Ok - call was successful.", typeof(UserDetail))]
        public IHttpActionResult GetEnvironmentList()
        {
            try
            {
                EnvironmentList el = makeEnvironmentList();
                return Ok<List<Environment>>(el.environments);
            }
            catch (Exception exception)
            {
                return InternalServerError(exception);
            }
        }
        #endregion

        #region "Project List"
        /// <summary>
        /// Pulls information about how many enviornments there are
        /// </summary>
        /// <returns></returns>
        [Route("api/Octo/ProjectList")]
        [ResponseType(typeof(int))]
        [SwaggerResponse(200, "Ok - call was successful.", typeof(UserDetail))]
        public IHttpActionResult GetProjectList()
        {
            try
            {
                List<ProjectGroup> pg = sortProjectGroups();
                return Ok<List<ProjectGroup>>(pg);
            }
            catch (Exception exception)
            {
                return InternalServerError(exception);
            }
        }
        #endregion

        #region "Deploys"
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
                for(int x = 0; x < dl.deploys.Count; x++)
                {
                    if (dl.deploys[x].RelatedDocs.Count > 0)
                    {
                        foreach(string docID in dl.deploys[x].RelatedDocs){
                            if (docID.Contains("Environments"))
                            {
                                dl.deploys[x].Environs.Add(getEnviron(docID));
                            }
                        }
                    }
                }
                return Ok<System.Collections.Generic.List<Deploy>>(dl.deploys);
            }
            catch (Exception exception)
            {
                return InternalServerError(exception);
            }
        }
        #endregion

        #endregion

        #region "Aetna Provided"
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
        #endregion
    }
}
