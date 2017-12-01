using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Description;
using System.IO;
using System.Web.Http;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Aetna.DevOps.Dashboard.UIWeb.Models;
using Environment = Aetna.DevOps.Dashboard.UIWeb.Models.Environment;
using Swashbuckle.Swagger.Annotations;

namespace Aetna.DevOps.Dashboard.UIWeb.Controllers
{
    /// <summary>
    /// Handles GET requests to Octopus API, parsing, (de)serializing JSON, and live updates with SignalR
    /// and exposes information over internal Web API
    /// </summary>
    public class OctopusController : ApiController
    {

        #region "API Setup"

        #region "Constants"
        private const string API_URL = "http://ec2-18-220-206-192.us-east-2.compute.amazonaws.com:81/api/";
        private const String API_KEY = "API-A5I5VUHAOV0VJJN6LQ6MXCPSMS";
        #endregion

        #region "API Datum Enum"
        /// <summary>
        /// Enumerator for differentiating how the internal API communicates to the external Octopus API
        /// </summary>
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
        /// <summary>
        /// Used for querying the Octopus API
        /// </summary>
        /// <param name="apid">API Datum Enum specifies what information will be asked for</param>
        /// <param name="param">For specifying things like Environment or Project names, taken in by the internal API GET request
        /// and passed along to the Octopus API for appropriate filtering of API results</param>
        /// <returns>A JSON string</returns>
        private static string GetResponse(APIdatum apid, string param = "")
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
                    reqString = "environments/" + (param == String.Empty ? "" : param) + "?";
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
            WebResponse response;
            try
            {
                response = request.GetResponse();
            } catch (WebException)
            {
                return ""; // server didnt respond, send blank response
            }
            Stream dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string serverResponse = "";
            try
            {
                serverResponse = reader.ReadToEnd();
            } catch (IOException)
            {
                serverResponse = ""; // server force closed connection for some reason
            }
            reader.Close();
            response.Close();
            return serverResponse;
        }
        #endregion

        #region "Get Active Projects by Environment"
        /// <summary>
        /// Gives the current deploys for some Environment
        /// </summary>
        /// <param name="envId">ID for some Environment</param>
        /// <returns>A generic list of ActiveDeploy</returns>
        private static List<ActiveDeploy> GetEnvironmentProjects(string envId)
        {
            List<ActiveDeploy> projList = new List<ActiveDeploy>();
            List<Project> projects = MakeProjectList();
            string response = GetResponse(APIdatum.dashboard);
            if (String.IsNullOrEmpty(response)) { return projList; } // if response is empty, do not proceed
            dynamic jsonDeser = JsonConvert.DeserializeObject(response);
            foreach (dynamic p in jsonDeser.Items)
            {
                if (p.EnvironmentId == envId)
                {
                    string projName = "";
                    foreach (Project proj in projects) { if (proj.Id == p.ProjectId.ToString()) { projName = proj.Name; } }
                    projList.Add(new ActiveDeploy(p.Id.ToString(), p.ProjectId.ToString(),
                    p.ReleaseId.ToString(), p.TaskId.ToString(), p.ChannelId.ToString(), p.ReleaseVersion.ToString(),
                    p.Created.ToString(), p.QueueTime.ToString(), p.CompletedTime.ToString(), p.State.ToString(),
                    p.HasWarningsOrErrors.ToString(), p.ErrorMessage.ToString(), p.Duration.ToString(), p.IsCurrent.ToString(),
                    p.IsCompleted.ToString(), projName, API_URL.TrimEnd("/api/".ToCharArray()) + "/app#/deployments/" + p.Id.ToString()));
                }
            }
            return projList;
        }
        #endregion

        #region "Get Number of Environments"
        /// <summary>
        /// 
        /// </summary>
        /// <param name="jsonTxt">A JSON string</param>
        /// <returns>Dictionary of two strings</returns>
        private static Dictionary<string, string> GetNumberEnviroments(string jsonTxt)
        {
            dynamic jsonDeser = JsonConvert.DeserializeObject(jsonTxt);
            Dictionary<string, string> environments = new Dictionary<string, string>();
            if (String.IsNullOrEmpty(jsonTxt)) { return environments; } // if response is empty, do not proceed

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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="jsonTxt">A JSON string</param>
        /// <returns>Dictionary of string, int</returns>
        private static Dictionary<string, int> GetNumberMachines(string jsonTxt)
        {
            dynamic jsonDeser = JsonConvert.DeserializeObject(jsonTxt);
            Dictionary<string, int> machines = new Dictionary<string, int>();
            if (String.IsNullOrEmpty(jsonTxt)) { return machines; } // if response is empty, do not proceed

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
        /// <summary>
        /// Uses GetNumberMachines and GetNumberEnvironments to construct a list of Environments with their respective
        /// number of machines
        /// </summary>
        /// <returns>EnvironmentList</returns>
        private static List<Environment> MakeEnvironmentList()
        {
            List<Environment> el = new List<Environment>();
            Dictionary<string, int> numMachines = GetNumberMachines(GetResponse(APIdatum.machines));
            Dictionary<string, string> environments = GetNumberEnviroments(GetResponse(APIdatum.environments));

            foreach (string key in environments.Keys)
            {
                el.Add(new Environment(environments[key], key, (numMachines.ContainsKey(environments[key]) ? numMachines[environments[key]].ToString() : "0"), GetMachines(environments[key])));
            }

            return el;
        }
        #endregion

        #region "Make Project List"
        /// <summary>
        /// Gets the list of projects
        /// </summary>
        /// <returns>Generic List of Project</returns>
        private static List<Project> MakeProjectList()
        {
            List<Project> pl = new List<Project>();
            string jsonTxt = GetResponse(APIdatum.projects);
            if (String.IsNullOrEmpty(jsonTxt)) { return pl; } // if response is empty, do not proceed
            dynamic jsonDeser = JsonConvert.DeserializeObject(jsonTxt);
            foreach (dynamic o in jsonDeser.Items)
            {
                pl.Add(new Project(o.Id.ToString(), o.ProjectGroupId.ToString(), o.Name.ToString(), o.LifecycleId.ToString(), o.DeploymentProcessId.ToString()));
            }
            return pl;
        }
        #endregion

        #region "Sort Project List"
        private static List<ProjectGroup> SortProjectGroups()
        {
            List<ProjectGroup> pg = new List<ProjectGroup>();
            ProjectGroupDictionary pgd = new ProjectGroupDictionary();

            string jsonTxt = GetResponse(APIdatum.projectGroups);
            if (String.IsNullOrEmpty(jsonTxt)) { return pg; } // if response is empty, do not proceed

            dynamic jsonDeser = JsonConvert.DeserializeObject(jsonTxt);

            foreach (dynamic o in jsonDeser.Items)
            {
                pgd.AddProjectGroup(o.Id.ToString(), new ProjectGroup(o.Name.ToString(), o.Id.ToString()));
            }

            List<Project> projects = MakeProjectList();

            foreach (Project p in projects)
            {
                pgd.addProject(p.GetGroupId(), p);
            }

            pg = pgd.getProjectGroups();
            return pg;
        }
        #endregion

        #region "Get Release List"
        /// <summary>
        /// Gets the releases and deploys for each release of a specific project
        /// </summary>
        /// <param name="response">A JSON string representing some project from Octopus API/progression</param>
        /// <returns>A Generic List of Release</returns>
        private List<Release> GetReleaseList(string response)
        {
            dynamic releases = JsonConvert.DeserializeObject(response);
            List<Release> rl = new List<Release>();
            if (String.IsNullOrEmpty(response)) { return rl; } // if response is empty, do not proceed

            foreach (dynamic r in releases.Releases)
            {
                List<ActiveDeploy> releaseDeploys = new List<ActiveDeploy>();
                foreach (dynamic env in r.Deployments)
                {
                    foreach (dynamic de in env)
                    {
                        foreach (dynamic d in de)
                        {
                            ActiveDeploy ad = new ActiveDeploy(d.Id.ToString(), d.ProjectId.ToString(), d.ReleaseId.ToString(),
                                d.TaskId.ToString(), d.ChannelId.ToString(), d.ReleaseVersion.ToString(),
                                d.Created.ToString(), d.QueueTime.ToString(), d.CompletedTime.ToString(),
                                d.State.ToString(), d.HasWarningsOrErrors.ToString(), d.ErrorMessage.ToString(),
                                d.Duration.ToString(), d.IsCurrent.ToString(), d.IsCompleted.ToString(), "", API_URL.TrimEnd("/api/".ToCharArray()) + "/app#/deployments/" + d.Id.ToString());
                            releaseDeploys.Add(ad);
                        }
                    }
                }

                dynamic releaseLinks = JsonConvert.DeserializeObject(r.Release.Links.ToString());
                string webUrl = releaseLinks.Web.ToString();
                Release re = new Release(r.Release.Id.ToString(), r.Release.Version.ToString(), r.Release.ProjectId.ToString(),
                    r.Release.ChannelId.ToString(), isoToDateTime(r.Release.Assembled.ToString()), r.Release.ReleaseNotes.ToString(),
                    releaseDeploys, API_URL.TrimEnd("/api/".ToCharArray()) + webUrl);
                rl.Add(re);
            }
            return rl;
        }
        #endregion

        #region "Get First Int"
        /// <summary>
        /// Gets the first integer found in some string
        /// Generated by txt2re.com
        /// </summary>
        /// <param name="haystack">Some string to search for an integer in</param>
        /// <returns>String</returns>
        private static string GetFirstInt(string haystack) // credits to txt2re.com
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

        #region "ISO to Datetime"
        /// <summary>
        /// Converts the ISO standard datetime into the C# DateTime which is then converted to a string
        /// </summary>
        /// <param name="iso">ISO 8601 datetime string</param>
        /// <returns>string</returns>
        private string isoToDateTime(string iso)
        {
            DateTime dateTime = DateTime.Parse(iso).ToLocalTime();
            return dateTime.ToString();
        }
        #endregion

        #region "Format deploys for graphing"
        /// <summary>
        /// Transforms JSON from Octopus API/Events into a list of Deploys ready to be used in ChartJS
        /// </summary>
        /// <param name="jsonTxt">JSON string</param>
        /// <returns>DeployList</returns>
        private List<Deploy> GraphDeployments(string jsonTxt)
        {
            if (String.IsNullOrEmpty(jsonTxt)) { return new List<Deploy>(); } // if response is empty, do not proceed
            List<Deploy> dl = new List<Deploy>();
            dynamic jsonDeser = JsonConvert.DeserializeObject(jsonTxt);
            foreach (dynamic o in jsonDeser.Items)
            {
                string occured = o.Occurred.ToString();
                DateTime parsedDt = Convert.ToDateTime(occured);
                string occuredISO = parsedDt.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fZ");
                if (DateTime.Now.AddDays(-1) > parsedDt) { continue; } // ignore events that took place more than 1 day ago

                dynamic deployLinks = JsonConvert.DeserializeObject(o.RelatedDocumentIds.ToString());
                string webUrl = "";
                foreach (string str in deployLinks)
                {
                    if (str.StartsWith("Deployments-")) { webUrl = API_URL.TrimEnd("/api/".ToCharArray()) + "/app#/deployments/" + str; break; }
                }

                Deploy d = new Deploy(occuredISO, o.Message.ToString(),
                    JsonConvert.DeserializeObject<System.Collections.Generic.List<string>>(o.RelatedDocumentIds.ToString()), // nested list element
                    o.Category.ToString(), webUrl);
                if (d.Category == "DeploymentSucceeded" || d.Category == "DeploymentFailed" ||
                    d.Category == "DeploymentStarted" || d.Category == "DeploymentQueued")
                {
                    dl.Add(d);
                }
            }
            return dl;
        }
        #endregion

        #region "Get Machines"
        /// <summary>
        /// Gets the list of machines for a specific Environment
        /// </summary>
        /// <param name="envId">ID of some Environment</param>
        /// <returns>MachineList</returns>
        private static List<Machine> GetMachines(string envId)
        {
            string machineResponse = GetResponse(APIdatum.machines, envId);
            if (String.IsNullOrEmpty(machineResponse)) { return new List<Machine>(); } // if response is empty, do not proceed
            dynamic mach = JsonConvert.DeserializeObject(machineResponse);
            List <Machine> m = new List<Machine>();
            foreach (dynamic mac in mach.Items)
            {
                System.Collections.Generic.List<string> el = new System.Collections.Generic.List<string>();
                foreach (dynamic env in mac.EnvironmentIds)
                {
                    //Environment e = getEnviron(envName);
                    el.Add(env.ToString());
                }
                //Machine m = new Machine()
                Machine machine = new Machine(mac.Id.ToString(), mac.Name.ToString(), mac.Uri.ToString(), el, mac.Status.ToString(), mac.StatusSummary.ToString(), mac.IsInProcess.ToString());
                m.Add(machine);
            }
            return m;
        }
        #endregion

        #region "Get Environment"
        /// <summary>
        /// Gets an Environment object for some Octopus environment by ID
        /// </summary>
        /// <param name="envName">Some Environment ID</param>
        /// <returns>Environment</returns>
        private Environment GetEnviron(string envName)
        {
            string environData = GetResponse(APIdatum.environments, envName);
            if (String.IsNullOrEmpty(environData)) { return new Environment("","","",new List<Machine>()); } // if response is empty, do not proceed
            dynamic env = JsonConvert.DeserializeObject(environData);
            Environment e = new Environment(env.Id.ToString(), env.Name.ToString(), env.Description.ToString(), GetMachines(envName));
            return e;
        }
        #endregion

        #endregion

        #region "SignalR UpdateDataState"
        /// <summary>
        /// Checks if any Octopus API data has changed from last state send to client
        /// </summary>
        /// <param name="state">The state of data being sent to client</param>
        /// <returns>Boolean</returns>
        public static Boolean UpdateDataState(DataState state)
        {
            Boolean anyChange = false; // debugging: should be false by default, set true on change

            // Used to notify user when data has changed
            state.IsChanged = new Dictionary<string, bool>(){ // debugging: should be false by default, set true on change
                { "ProjectGroups", false },
                { "Projects", false },
                { "Lifecycles", false },
                { "Environments", false },
                { "Deploys", false }
             };

            // Get New Data

            List<ProjectGroup> pg = SortProjectGroups();
            if (state.ProjectGroups == null || state.ProjectGroups != pg)
            {
                state.ProjectGroups = new List<ProjectGroup>();
                foreach (ProjectGroup group in pg)
                {
                    state.ProjectGroups.Add(group.Clone());
                }
                state.IsChanged["ProjectGroups"] = true;
                anyChange = true;
            }

            List<Project> pl = MakeProjectList();
            if (state.Projects == null || state.Projects != pl)
            {
                state.Projects = new List<Project>();
                foreach (Project project in pl)
                {
                    state.Projects.Add(project.Clone());
                }
                state.IsChanged["Projects"] = true;
                anyChange = true;
            }

            // Temporary until Lifecycle object is added
            int nlc = 0;
            Int32.TryParse(GetFirstInt(GetResponse(APIdatum.lifecycles)), out nlc);
            if (state.Lifecycles != nlc)
            {
                state.Lifecycles = nlc;
                state.IsChanged["Lifecycles"] = true;
                anyChange = true;
            }

            List<Environment> env = MakeEnvironmentList().Clone();
            if (state.Environments == null || state.Environments != env)
            {
                state.Environments = env;
                state.IsChanged["Environments"] = true;
                anyChange = true;
            }

            List<Deploy> dp = null; // Temporary
            if (state.Deploys != dp)
            {
                state.Deploys = dp;
                state.IsChanged["Deploys"] = true;
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
                return Ok(GetFirstInt(GetResponse(APIdatum.projectGroups)));
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
                return Ok(GetFirstInt(GetResponse(APIdatum.lifecycles)));
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
                return Ok<List<Project>>(MakeProjectList());
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
                return Ok<List<ActiveDeploy>>(GetEnvironmentProjects(envId));
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
                return Ok(GetFirstInt(GetResponse(APIdatum.projects)));
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
                return Ok<List<Release>>(GetReleaseList(GetResponse(APIdatum.projectProgression, project)));
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
                return Ok<List<Machine>>(GetMachines(envId));
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
                return Ok(GetFirstInt(GetResponse(APIdatum.environments)));
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
                List<Environment> el = MakeEnvironmentList();
                return Ok<List<Environment>>(el);
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
                List<ProjectGroup> pg = SortProjectGroups();
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
                List<Deploy> dl = GraphDeployments(GetResponse(APIdatum.deploys));
                for (int x = 0; x < dl.Count; x++)
                {
                    if (dl[x].RelatedDocs.Count > 0)
                    {
                        foreach (string docID in dl[x].RelatedDocs)
                        {
                            if (docID.Contains("Environments"))
                            {
                                dl[x].Environs.Add(GetEnviron(docID));
                            }
                        }
                    }
                }
                return Ok<System.Collections.Generic.List<Deploy>>(dl);
            }
            catch (Exception exception)
            {
                return InternalServerError(exception);
            }
        }
        #endregion

        #endregion

    }
}
