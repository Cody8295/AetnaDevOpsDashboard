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

        #region API Setup

        #region Constants
        private const string API_URL = "http://ec2-18-220-206-192.us-east-2.compute.amazonaws.com:81/api/";
        private const String API_KEY = "API-A5I5VUHAOV0VJJN6LQ6MXCPSMS";
        #endregion

        #region API Datum Enum
        /// <summary>
        /// Enumerator for differentiating how the internal API communicates to the external Octopus API
        /// </summary>
        private enum ApiDatum
        {
            ProjectGroups = 0,
            Projects = 1,
            Lifecycles = 2,
            Environments = 3,
            DeployEvents = 4,
            Machines = 5,
            Releases = 6,
            Dashboard = 7,
            Deploys = 8,
            Deploy = 9,
            Task = 10
        }
        #endregion

        #region HTTP Request Constructor
        /// <summary>
        /// Used for querying the Octopus API
        /// </summary>
        /// <param name="apid">API Datum Enum specifies what information will be asked for</param>
        /// <param name="param">For specifying things like Environment or Project names, taken in by the internal API GET request
        /// and passed along to the Octopus API for appropriate filtering of API results</param>
        /// <returns>A JSON string</returns>
        private static string GetResponse(ApiDatum apid, string param = "")
        {
            WebRequest request;
            string reqString = String.Empty;
            switch (apid)
            {
                case ApiDatum.ProjectGroups:
                    reqString = "projectGroups?take=1000&";
                    break;
                case ApiDatum.Projects:
                    reqString = "projects?";
                    break;
                case ApiDatum.Lifecycles:
                    reqString = "lifecycles?";
                    break;
                case ApiDatum.Environments:
                    reqString = "environments/" + (param == String.Empty ? "" : param) + "?";
                    break;
                case ApiDatum.DeployEvents:
                    reqString = "events?eventCategories=DeploymentSucceeded&eventCategories=DeploymentQueued&eventCategories=DeploymentFailed&eventCategories=DeploymentStarted&eventCategories=TaskCanceled&take=250&";
                    break;
                case ApiDatum.Machines:
                    if (param == string.Empty) // all machines
                    {
                        reqString = "machines?take=100000&";
                    }
                    else // asking for info on machines about specific environment
                    {
                        reqString = "environments/" + param + "/machines?";
                    }
                    break;
                case ApiDatum.Releases:
                    if (param == String.Empty) { return ""; } // can't find progression of project without it's name
                    reqString = "progression/" + param + "?";
                    break;
                case ApiDatum.Dashboard:
                    reqString = "dashboard?";
                    break;
                case ApiDatum.Deploys:
                    if (param == String.Empty) { reqString = "deployments/?take=250&"; }
                    else { reqString = "deployments/?taskState=" + param + "&take=250&";}
                    break;
                case ApiDatum.Deploy:
                    reqString = "deployments/" + param + "?";
                    break;
                case ApiDatum.Task:
                    reqString = "tasks/" + param + "?";
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

        #region Get Active DeployEvents by Environment
        /// <summary>
        /// Gives the current deploys for some Environment
        /// </summary>
        /// <param name="envId">ID for some Environment</param>
        /// <returns>A generic list of ActiveDeploy</returns>
        private static List<ActiveDeploy> GetActiveDeploysByEnvironment(string envId)
        {
            List<ActiveDeploy> projList = new List<ActiveDeploy>();
            List<Project> projects = MakeProjectList();
            string response = GetResponse(ApiDatum.Dashboard);
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

        #region Get Number of Environments
        /// <summary>
        /// 
        /// </summary>
        /// <param name="jsonTxt">A JSON string</param>
        /// <returns>Dictionary of two strings</returns>
        private static Dictionary<string, string> GetNumberEnvironments(string jsonTxt)
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

        #region Get Number of Machines
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

        #region Make Environment List
        /// <summary>
        /// Uses GetNumberMachines and GetNumberEnvironments to construct a list of Environments with their respective
        /// number of machines
        /// </summary>
        /// <returns>EnvironmentList</returns>
        private static List<Environment> MakeEnvironmentList()
        {
            List<Environment> el = new List<Environment>();
            Dictionary<string, int> numMachines = GetNumberMachines(GetResponse(ApiDatum.Machines));
            Dictionary<string, string> environments = GetNumberEnvironments(GetResponse(ApiDatum.Environments));

            foreach (KeyValuePair<string, string> element in environments)
            {
                el.Add(new Environment(element.Value, element.Key, (numMachines.ContainsKey(element.Value) ? numMachines[element.Value].ToString() : "0"), GetMachines(element.Value)));
            }

            return el;
        }
        #endregion

        #region Make Project List
        /// <summary>
        /// Gets the list of projects
        /// </summary>
        /// <returns>Generic List of Project</returns>
        private static List<Project> MakeProjectList()
        {
            List<Project> pl = new List<Project>();
            string jsonTxt = GetResponse(ApiDatum.Projects);
            if (String.IsNullOrEmpty(jsonTxt)) { return pl; } // if response is empty, do not proceed
            dynamic jsonDeser = JsonConvert.DeserializeObject(jsonTxt);
            foreach (dynamic o in jsonDeser.Items)
            {
                pl.Add(new Project(o.Id.ToString(), o.ProjectGroupId.ToString(), o.Name.ToString(), o.LifecycleId.ToString(), o.DeploymentProcessId.ToString()));
            }
            return pl;
        }
        #endregion

        #region Make Project Group List
        private static List<ProjectGroup> MakeProjectGroupList()
        {
            List<ProjectGroup> pg = new List<ProjectGroup>();
            Dictionary<string,ProjectGroup> pgd = new Dictionary<string, ProjectGroup>();

            string jsonTxt = GetResponse(ApiDatum.ProjectGroups);
            if (String.IsNullOrEmpty(jsonTxt)) { return pg; } // if response is empty, do not proceed

            dynamic jsonDeser = JsonConvert.DeserializeObject(jsonTxt);

            foreach (dynamic o in jsonDeser.Items)
            {
                pgd.Add(o.Id.ToString(), new ProjectGroup(o.Name.ToString(), o.Id.ToString()));
            }

            List<Project> projects = MakeProjectList();

            foreach (Project p in projects)
            {
                pgd.AddProject(p.GetGroupId(), p);
            }

            pg = pgd.ToList();
            return pg;
        }
        #endregion

        #region Get Release List
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

        #region Get First Int
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

        #region ISO to Datetime
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

        #region Make DeployEvent List, Formatted for Graphing
        /// <summary>
        /// Transforms JSON from Octopus API/Events into a list of DeployEvents ready to be used in ChartJS
        /// </summary>
        /// <param name="jsonTxt">JSON string</param>
        /// <returns>DeployList</returns>
        private static List<DeployEvent> MakeDeployEventList(string jsonTxt)
        {
            if (String.IsNullOrEmpty(jsonTxt)) { return new List<DeployEvent>(); } // if response is empty, do not proceed
            List<DeployEvent> dl = new List<DeployEvent>();
            dynamic jsonDeser = JsonConvert.DeserializeObject(jsonTxt);
            foreach (dynamic o in jsonDeser.Items)
            {
                if (o.Category.ToString().StartsWith("Deployment") || o.Category.ToString() == "TaskCanceled")
                {
                    string occured = o.Occurred.ToString();
                    DateTime parsedDt = Convert.ToDateTime(occured);
                    string occuredISO = parsedDt.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fZ");
                    if (DateTime.Now.AddDays(-1) > parsedDt) { continue; } // ignore events that took place more than 1 day ago

                    string deployId = "", envId = "";
                    dynamic deployLinks = JsonConvert.DeserializeObject(o.RelatedDocumentIds.ToString());
                    string webUrl = "";
                    foreach (string str in deployLinks)
                    {
                        if (str.StartsWith("Deployments-"))
                        {
                            webUrl = API_URL.TrimEnd("/api/".ToCharArray()) + "/app#/deployments/" + str;
                            deployId = str;
                        }
                        else if (str.StartsWith("Environments"))
                        {
                            envId = str;
                        }
                    }

                    DeployEvent d = new DeployEvent(occuredISO, o.Message.ToString(),
                        JsonConvert.DeserializeObject<System.Collections.Generic.List<string>>(o.RelatedDocumentIds.ToString()), // nested list element
                        o.Category.ToString(), webUrl, deployId);
                    if (!String.IsNullOrEmpty(deployId))
                    {
                        dl.Add(d);
                        if (envId != "") d.Environs.Add(GetEnvironment(envId));
                        if (d.Category == "TaskCanceled") d.Category = "DeploymentFailed";
                    }
                    
                }
                
            }

            return dl;
        }
        #endregion

        #region Make Deploy List by State
        /// <summary>
        /// Retrieves a list of all deploys with a given state from Octopus API
        /// </summary>
        /// <param name="state">state</param>
        /// <returns>DeployList</returns>
        private static List<Deploy> MakeDeployList(string state)
        {
            Deploy.State status;
            switch (state)
            {
                case "Success":
                    status = Deploy.State.Success;
                    break;
                case "Failed":
                    status = Deploy.State.Failed;
                    break;
                case "Queued":
                    status = Deploy.State.Queued;
                    break;
                case "Executing":
                    status = Deploy.State.Executing;
                    break;
                case "Canceled":
                    status = Deploy.State.Canceled;
                    break;
                default:
                    status = Deploy.State.Unknown;
                    break;
            }


            List<Deploy> deploys = new List<Deploy>();
            string jsonTxt = GetResponse(ApiDatum.Deploys,state);
            if (!String.IsNullOrEmpty(jsonTxt)) // if response is empty, do not proceed
            {
                dynamic jsonDeser = JsonConvert.DeserializeObject(jsonTxt);
                foreach (dynamic o in jsonDeser.Items)
                {
                    deploys.Add(new Deploy(o.Id.ToString(), o.ProjectId.ToString(), o.ReleaseId.ToString(), o.EnvironmentId.ToString(), o.Links.Web.ToString(), o.Created.ToString(), status));
                }

            }

            return deploys;
        }
        #endregion

        #region Make Deploy List
        /// <summary>
        /// Retrieves a list of all deploys from Octopus API
        /// </summary>
        /// <param name="jsonTxt">JSON string</param>
        /// <returns>DeployList</returns>
        private static List<Deploy> MakeDeployList()
        {
            List<Deploy> deploys = new List<Deploy>();
            string jsonTxt = GetResponse(ApiDatum.Deploys);
            if (!String.IsNullOrEmpty(jsonTxt)) // if response is empty, do not proceed
            {
                dynamic jsonDeser = JsonConvert.DeserializeObject(jsonTxt);
                foreach (dynamic o in jsonDeser.Items)
                {
                    Deploy.State state = Deploy.State.Unknown;
                    string taskJson = GetResponse(ApiDatum.Task, o.TaskId.ToString());
                    if (!String.IsNullOrEmpty(taskJson))
                    {
                        dynamic taskDeserialization = JsonConvert.DeserializeObject(taskJson);
                        switch (taskDeserialization.ToString())
                        {
                            case "Success":
                                state = Deploy.State.Success;
                                break;
                            case "Failed":
                                state = Deploy.State.Failed;
                                break;
                            case "Queued":
                                state = Deploy.State.Queued;
                                break;
                            case "Executing":
                                state = Deploy.State.Executing;
                                break;
                            case "Canceled":
                                state = Deploy.State.Canceled;
                                break;
                        }
                    }
                    deploys.Add(new Deploy(o.Id.ToString(), o.ProjectId.ToString(), o.ReleaseId.ToString(), o.EnvironmentId.ToString(), o.Links.Web.ToString(), o.Created.ToString(), state));
                }

            }

            return deploys;
        }
        #endregion

        #region Get Deploy
        /// <summary>
        /// Transforms JSON from Octopus API/Events into a list of DeployEvents ready to be used in ChartJS
        /// </summary>
        /// <param name="jsonTxt">JSON string</param>
        /// <returns>DeployList</returns>
        private static Deploy GetDeploy(string id)
        {
            if (String.IsNullOrEmpty(id)) return null;

            Deploy deploy;
            string jsonTxt = GetResponse(ApiDatum.Deploy, id);
            if (!String.IsNullOrEmpty(jsonTxt)) // if response is empty, do not proceed
            {
                dynamic jsonDeser = JsonConvert.DeserializeObject(jsonTxt);
                Deploy.State state = Deploy.State.Unknown;
                string taskJson = GetResponse(ApiDatum.Task, jsonDeser.TaskId.ToString());
                if (!String.IsNullOrEmpty(taskJson))
                {
                    dynamic taskDeserialization = JsonConvert.DeserializeObject(taskJson);
                    switch (taskDeserialization.ToString())
                    {
                        case "Success":
                            state = Deploy.State.Success;
                            break;
                        case "Failed":
                            state = Deploy.State.Failed;
                            break;
                        case "Queued":
                            state = Deploy.State.Queued;
                            break;
                        case "Executing":
                            state = Deploy.State.Executing;
                            break;
                        case "Canceled":
                            state = Deploy.State.Canceled;
                            break;
                    }
                }
                deploy = new Deploy(id, jsonDeser.ProjectId.ToString(),
                    jsonDeser.ReleaseId.ToString(), jsonDeser.EnvironmentId.ToString(), jsonDeser.Links.Web.ToString(),
                    jsonDeser.Created.ToString(), state);

            }
            else
            {
                deploy = null;
            }

            return deploy;
        }
        #endregion

        #region Get Machines
        /// <summary>
        /// Gets the list of machines for a specific Environment
        /// </summary>
        /// <param name="envId">ID of some Environment</param>
        /// <returns>MachineList</returns>
        private static List<Machine> GetMachines(string envId)
        {
            string machineResponse = GetResponse(ApiDatum.Machines, envId);
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

        #region Get Environment
        /// <summary>
        /// Gets an Environment object for some Octopus environment by ID
        /// </summary>
        /// <param name="envName">Some Environment ID</param>
        /// <returns>Environment</returns>
        private static Environment GetEnvironment(string envName)
        {
            string environData = GetResponse(ApiDatum.Environments, envName);
            if (String.IsNullOrEmpty(environData)) { return new Environment("","","",new List<Machine>()); } // if response is empty, do not proceed
            dynamic env = JsonConvert.DeserializeObject(environData);
            Environment e = new Environment(env.Id.ToString(), env.Name.ToString(), env.Description.ToString(), GetMachines(envName) );
            return e;
        }
        #endregion

        #endregion

        #region SignalR UpdateDataState
        /// <summary>
        /// Checks if any Octopus API data has changed from last state send to client
        /// </summary>
        /// <param name="state">The state of data being sent to client</param>
        /// <returns>Boolean</returns>
        public static bool UpdateDataState(DataState state)
        {
            bool anyChange = false;

            // Used to notify user when data has changed
            state.IsChanged = new Dictionary<string, bool>(){
                { "ProjectGroups", false },
                { "Projects", false },
                { "Lifecycles", false },
                { "Environments", false },
                { "DeployEvents", false },
                { "Deploys", false }
             };

            // Get New Data

            List<ProjectGroup> pg = MakeProjectGroupList();
            if (state.ProjectGroups == null || !state.ProjectGroups.DeepEquals<ProjectGroup>(pg))
            {
                state.ProjectGroups = pg;
                state.IsChanged["ProjectGroups"] = true;
                anyChange = true;
            }

            List<Project> pl = MakeProjectList();
            if (state.Projects == null || !state.Projects.DeepEquals<Project>(pl))
            {
                state.Projects = pl;
                state.IsChanged["Projects"] = true;
                anyChange = true;
            }

            // Temporary until Lifecycle object is added
            int nlc = 0;
            Int32.TryParse(GetFirstInt(GetResponse(ApiDatum.Lifecycles)), out nlc);
            if (state.Lifecycles != nlc)
            {
                state.Lifecycles = nlc;
                state.IsChanged["Lifecycles"] = true;
                anyChange = true;
            }

            List<Environment> env = MakeEnvironmentList();
            if (state.Environments == null || !state.Environments.DeepEquals<Environment>(env))
            {
                state.Environments = env;
                state.IsChanged["Environments"] = true;
                anyChange = true;
            }

            List<DeployEvent> dep = MakeDeployEventList(GetResponse(ApiDatum.DeployEvents)); 
            if (state.DeployEvents == null || !state.DeployEvents.DeepEquals<DeployEvent>(dep))
            {
                state.DeployEvents = dep;
                state.IsChanged["DeployEvents"] = true;
                anyChange = true;
            }

            List<Deploy> dp = MakeDeployList();
            List<Deploy> ldp = MakeDeployList("Executing");
            if (state.LiveDeploys == null || !state.LiveDeploys.DeepEquals<Deploy>(ldp) 
                || state.Deploys == null || !state.Deploys.DeepEquals<Deploy>(dp))
            {
                state.Deploys = dp;
                state.LiveDeploys = ldp;
                state.IsChanged["Deploys"] = true;
                anyChange = true;
            }

            return anyChange;
        }
        #endregion

        #region API Calls

        #region Number of Project Groups
        /// <summary>
        /// Pulls information about how many project groups there are
        /// </summary>
        /// <returns></returns>
        [Route("api/Octo/numProjectGroups")]
        [ResponseType(typeof(int))]
        [SwaggerResponse(200, "Ok - call was successful.", typeof(int))]
        public IHttpActionResult GetProjectGroups()
        {
            try
            {
                return Ok(GetFirstInt(GetResponse(ApiDatum.ProjectGroups)));
            }
            catch (Exception exception)
            {
                return InternalServerError(exception);
            }
        }
        #endregion

        #region Lifecycles
        /// <summary>
        /// Pulls information about how many lifecycles there are
        /// </summary>
        /// <returns></returns>
        [Route("api/Octo/lifecycles")]
        [ResponseType(typeof(int))]
        [SwaggerResponse(200, "Ok - call was successful.", typeof(int))]
        public IHttpActionResult GetLifecycles()
        {
            try
            {
                return Ok(GetFirstInt(GetResponse(ApiDatum.Lifecycles)));
            }
            catch (Exception exception)
            {
                return InternalServerError(exception);
            }
        }
        #endregion

        #region Projects
        /// <summary>
        /// Pulls information about projects
        /// </summary>
        /// <returns></returns>
        [Route("api/Octo/projects")]
        [ResponseType(typeof(int))]
        [SwaggerResponse(200, "Ok - call was successful.", typeof(List<Project>))]
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

        #region Active Deploys by Environment
        /// <summary>
        /// Pulls information about projects by environment
        /// </summary>
        /// <returns></returns>
        [Route("api/Octo/activeDeploysByEnvironment")]
        [ResponseType(typeof(int))]
        [SwaggerResponse(200, "Ok - call was successful.", typeof(List<ActiveDeploy>))]
        public IHttpActionResult GetEnvProjects(string envId)
        {
            try
            {
                return Ok<List<ActiveDeploy>>(GetActiveDeploysByEnvironment(envId));
            }
            catch (Exception exception)
            {
                return InternalServerError(exception);
            }
        }
        #endregion

        #region Project Count
        /// <summary>
        /// Pulls information about how many projects there are
        /// </summary>
        /// <returns></returns>
        [Route("api/Octo/numProjects")]
        [ResponseType(typeof(int))]
        [SwaggerResponse(200, "Ok - call was successful.", typeof(int))]
        public IHttpActionResult GetProjects()
        {
            try
            {
                return Ok(GetFirstInt(GetResponse(ApiDatum.Projects)));
            }
            catch (Exception exception)
            {
                return InternalServerError(exception);
            }
        }
        #endregion

        #region Project Releases
        /// <summary>
        /// Pulls information about how a project has progressed in respect to releases
        /// </summary>
        /// <returns></returns>
        [Route("api/Octo/releasesByProject")]
        [ResponseType(typeof(int))]
        [SwaggerResponse(200, "Ok - call was successful.", typeof(List<Release>))]
        public IHttpActionResult GetProjectProgression(string project)
        {
            try
            {
                return Ok<List<Release>>(GetReleaseList(GetResponse(ApiDatum.Releases, project)));
            }
            catch (Exception exception)
            {
                return InternalServerError(exception);
            }
        }
        #endregion

        #region Machines by Environment
        /// <summary>
        /// Pulls information about how each machine for a specified environment
        /// </summary>
        /// <returns></returns>
        [Route("api/Octo/machinesByEnvironment")]
        [ResponseType(typeof(int))]
        [SwaggerResponse(200, "Ok - call was successful.", typeof(List<Machine>))]
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

        #region Number of Environments
        /// <summary>
        /// Pulls information about how many enviornments there are
        /// </summary>
        /// <returns></returns>
        [Route("api/Octo/numEnvironments")]
        [ResponseType(typeof(int))]
        [SwaggerResponse(200, "Ok - call was successful.", typeof(int))]
        public IHttpActionResult GetEnvironments()
        {
            try
            {
                return Ok(GetFirstInt(GetResponse(ApiDatum.Environments)));
            }
            catch (Exception exception)
            {
                return InternalServerError(exception);
            }
        }
        #endregion

        #region Environment List
        /// <summary>
        /// Pulls information about how many enviornments there are
        /// </summary>
        /// <returns></returns>
        [Route("api/Octo/environments")]
        [ResponseType(typeof(int))]
        [SwaggerResponse(200, "Ok - call was successful.", typeof(List<Environment>))]
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

        #region Project Groups
        /// <summary>
        /// Pulls information about how many enviornments there are
        /// </summary>
        /// <returns></returns>
        [Route("api/Octo/projectGroups")]
        [ResponseType(typeof(int))]
        [SwaggerResponse(200, "Ok - call was successful.", typeof(List<ProjectGroup>))]
        public IHttpActionResult GetProjectList()
        {
            try
            {
                List<ProjectGroup> pg = MakeProjectGroupList();
                return Ok<List<ProjectGroup>>(pg);
            }
            catch (Exception exception)
            {
                return InternalServerError(exception);
            }
        }
        #endregion

        #region Deploy Events
        /// <summary>
        /// Pulls information about how many deploys there are over the past 24 hours and information about each one
        /// </summary>
        /// <returns></returns>
        [Route("api/Octo/deployEvents")]
        [ResponseType(typeof(int))]
        [SwaggerResponse(200, "Ok - call was successful.", typeof(List<DeployEvent>))]
        public IHttpActionResult GetDeployEvents()
        {
            try
            {
                List<DeployEvent> dl = MakeDeployEventList(GetResponse(ApiDatum.DeployEvents));
                return Ok<System.Collections.Generic.List<DeployEvent>>(dl);
            }
            catch (Exception exception)
            {
                return InternalServerError(exception);
            }
        }
        #endregion

        #region Live Deploys
        /// <summary>
        /// Pulls list of all currently executing deploys
        /// </summary>
        /// <returns></returns>
        [Route("api/Octo/deploysByStatus")]
        [ResponseType(typeof(int))]
        [SwaggerResponse(200, "Ok - call was successful.", typeof(List<Deploy>))]
        public IHttpActionResult GetLiveDeploys(string status)
        {
            try
            {
                return Ok<List<Deploy>>(MakeDeployList(status));
            }
            catch (Exception exception)
            {
                return InternalServerError(exception);
            }
        }
        #endregion

        #region Deploys
        /// <summary>
        /// Pulls list of all currently executing deploys
        /// </summary>
        /// <returns></returns>
        [Route("api/Octo/deploys")]
        [ResponseType(typeof(int))]
        [SwaggerResponse(200, "Ok - call was successful.", typeof(List<Deploy>))]
        public IHttpActionResult GetDeploys()
        {
            try
            {
                return Ok<List<Deploy>>(MakeDeployList());
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
