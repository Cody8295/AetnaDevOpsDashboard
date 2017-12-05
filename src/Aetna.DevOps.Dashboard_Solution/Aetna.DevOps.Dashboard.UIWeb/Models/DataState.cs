using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using Aetna.DevOps.Dashboard.UIWeb.Controllers;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Aetna.DevOps.Dashboard.UIWeb.Models
{
    public sealed class DataState
    {

        #region Private Attributes

        private static DataState instance;
        private static readonly object SingletonLock = new object();

        private List<ProjectGroup> projectGroups;
        private List<Project> projects;
        private int lifecycles;
        private List<Environment> environments;
        private List<DeployEvent> deployEvents;
        private List<Deploy> liveDeploys;
        private List<Deploy> deploys;
        private string jsonSerialization;
        private Dictionary<string, bool> isChanged;

        private readonly JsonSerializerSettings jsonCamelCaseSettings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };
       
        #region Constants
        private const string ApiUrl = "http://ec2-18-220-206-192.us-east-2.compute.amazonaws.com:81/api/";
        private const String ApiKey = "API-A5I5VUHAOV0VJJN6LQ6MXCPSMS";
        #endregion

        #endregion

        #region Accessors

        public List<ProjectGroup> ProjectGroups => projectGroups;
        public List<Project> Projects => projects;
        public int Lifecycles => lifecycles;
        public List<Environment> Environments => environments;
        public List<DeployEvent> DeployEvents => deployEvents;
        public List<Deploy> LiveDeploys => liveDeploys;
        public List<Deploy> Deploys => deploys;
        public string JsonSerialization => jsonSerialization;
        public Dictionary<string, bool> IsChanged => isChanged;

        public static DataState Instance
        {
            get
            {
                lock (SingletonLock)
                {
                    if (instance != null) return instance;

                    return instance = new DataState();
                }
            }
        }

        #endregion

        private DataState()
        {
            UpdateAll();
        }
        
        #region Update DataState
        /// <summary>
        /// Checks if any Octopus API data has changed from last state send to client
        /// </summary>
        /// <param name="state">The state of data being sent to client</param>
        /// <returns>Boolean</returns>
        public void UpdateAll()
        {

            UpdateProjectGroups();
            UpdateProjects();
            UpdateLifecycles();
            UpdateEnvironments();
            UpdateDeployEvents();
            UpdateDeploys();
            UpdateLiveDeploys();

        }

        public string UpdateProjectGroups()
        {
            List<ProjectGroup> pg = MakeProjectGroupList();
            if (ProjectGroups == null || !ProjectGroups.DeepEquals<ProjectGroup>(pg))
            {
                projectGroups = pg;
                return JsonConvert.SerializeObject(projectGroups, jsonCamelCaseSettings);
            }
            return "noChange";
        }

        public string UpdateProjects()
        {
            List<Project> pl = MakeProjectList();
            if (Projects == null || !Projects.DeepEquals<Project>(pl))
            {
                projects = pl;
                return JsonConvert.SerializeObject(projects, jsonCamelCaseSettings);
            }
            return "noChange";
        }

        public string UpdateLifecycles()
        {
            int nlc = 0;
            Int32.TryParse(GetFirstInt(GetResponse(ApiDatum.Lifecycles)), out nlc);
            if (Lifecycles != nlc)
            {
                lifecycles = nlc;
                return nlc.ToString();
            }
            return "noChange";
        }

        public string UpdateEnvironments()
        {
            List<Environment> env = MakeEnvironmentList();
            if (Environments == null || !Environments.DeepEquals<Environment>(env))
            {
                environments = env;
                return JsonConvert.SerializeObject(environments, jsonCamelCaseSettings);
            }
            return "noChange";

        }

        public string UpdateDeployEvents()
        {
            List<DeployEvent> dep = MakeDeployEventList();
            if (DeployEvents == null || !DeployEvents.DeepEquals<DeployEvent>(dep))
            {
                deployEvents = dep;
                return JsonConvert.SerializeObject(deployEvents, jsonCamelCaseSettings);
            }
            return "noChange";
        }

        public string UpdateDeploys()
        {
            List<Deploy> dp = MakeDeployList();
            if (Deploys == null || !Deploys.DeepEquals<Deploy>(dp))
            {
                deploys = dp;
                return JsonConvert.SerializeObject(deploys, jsonCamelCaseSettings);
            }
            return "noChange";
        }

        public string UpdateLiveDeploys()
        {
            List<Deploy> ldp = MakeDeployList("Executing");
            if (LiveDeploys == null || !LiveDeploys.DeepEquals<Deploy>(ldp))
            {
                liveDeploys = ldp;
                return JsonConvert.SerializeObject(liveDeploys, jsonCamelCaseSettings);
            }
            return "noChange";
        }

        #endregion

        #region Data Retrieval Methods

        #region Utility functions

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
        private string GetResponse(ApiDatum apid, string param = "")
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
                    else { reqString = "deployments/?taskState=" + param + "&take=250&"; }
                    break;
                case ApiDatum.Deploy:
                    reqString = "deployments/" + param + "?";
                    break;
                case ApiDatum.Task:
                    reqString = "tasks/" + param + "?";
                    break;
                default: break;
            }
            request = WebRequest.Create(ApiUrl + reqString + "apikey=" + ApiKey);
            request.Credentials = CredentialCache.DefaultCredentials;
            WebResponse response;
            try
            {
                response = request.GetResponse();
            }
            catch (WebException)
            {
                return ""; // server didnt respond, send blank response
            }
            Stream dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string serverResponse = "";
            try
            {
                serverResponse = reader.ReadToEnd();
            }
            catch (IOException)
            {
                serverResponse = ""; // server force closed connection for some reason
            }
            reader.Dispose();
            response.Dispose();
            return serverResponse;
        }
        #endregion

        #region Get First Int
        /// <summary>
        /// Gets the first integer found in some string
        /// Generated by txt2re.com
        /// </summary>
        /// <param name="haystack">Some string to search for an integer in</param>
        /// <returns>String</returns>
        private string GetFirstInt(string haystack) // credits to txt2re.com
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

        #endregion

        #region Octopus API Calls

        #region Make Environment List

        #region Additional methods used in MakeEnvironmentList()

        /// <summary>
        /// Gives the actively deployed release instances for some Environment
        /// </summary>
        /// <param name="envId">ID for some Environment</param>
        /// <returns>A generic list of ActiveDeploy</returns>
        private List<ActiveDeploy> GetActiveDeploysByEnvironment(string envId)
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
                        p.IsCompleted.ToString(), projName, ApiUrl.TrimEnd("/api/".ToCharArray()) + "/app#/deployments/" + p.Id.ToString()));
                }
            }
            return projList;
        }

        /// <summary>
        /// Gets the list of machines for a specific Environment
        /// </summary>
        /// <param name="envId">ID of some Environment</param>
        /// <returns>MachineList</returns>
        private List<Machine> GetMachines(string envId)
        {
            string machineResponse = GetResponse(ApiDatum.Machines, envId);
            if (String.IsNullOrEmpty(machineResponse)) { return new List<Machine>(); } // if response is empty, do not proceed
            dynamic mach = JsonConvert.DeserializeObject(machineResponse);
            List<Machine> m = new List<Machine>();
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

        /// <summary>
        ///  Gets Environment information from Octopus and returns a dictionary that maps each environment's name to its ID.
        /// </summary>
        /// <param name="jsonTxt">A JSON string</param>
        /// <returns>Dictionary of two strings containing the Name and Id of each Environment</returns>
        private Dictionary<string, string> GetEvironmentListAsStringDictionary(string jsonTxt)
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
        
        /// <summary>
        /// Uses GetNumberMachines and GetEvironmentListAsStringDictionary to construct a list of Environments with their respective
        /// number of machines
        /// </summary>
        /// <returns>EnvironmentList</returns>
        private List<Environment> MakeEnvironmentList()
        {
            List<Environment> el = new List<Environment>();
            Dictionary<string, string> environments = GetEvironmentListAsStringDictionary(GetResponse(ApiDatum.Environments));

            foreach (KeyValuePair<string, string> element in environments)
            {
                List<Machine> machines = GetMachines(element.Value);
                el.Add(new Environment(element.Value, element.Key, machines.Count.ToString(), machines, GetActiveDeploysByEnvironment(element.Value)));
            }

            return el;
        }
        #endregion

        #region Make Project List
        /// <summary>
        /// Gets the list of projects
        /// </summary>
        /// <returns>Generic List of Project</returns>
        private List<Project> MakeProjectList()
        {
            List<Project> pl = new List<Project>();
            string jsonTxt = GetResponse(ApiDatum.Projects);
            if (String.IsNullOrEmpty(jsonTxt)) { return pl; } // if response is empty, do not proceed
            dynamic jsonDeser = JsonConvert.DeserializeObject(jsonTxt);
            foreach (dynamic o in jsonDeser.Items)
            {
                pl.Add(new Project(o.Id.ToString(), o.ProjectGroupId.ToString(), o.Name.ToString(), o.LifecycleId.ToString(), o.DeploymentProcessId.ToString(),GetReleaseList(o.Id.ToString())));
            }
            return pl;
        }
        #endregion

        #region Make Project Group List
        private List<ProjectGroup> MakeProjectGroupList()
        {
            List<ProjectGroup> pg = new List<ProjectGroup>();
            Dictionary<string, ProjectGroup> pgd = new Dictionary<string, ProjectGroup>();

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
        /// <param name="project">Project to get releases from</param>
        /// <returns>A Generic List of Releases</returns>
        private List<Release> GetReleaseList(string project)
        {
            string response = GetResponse(ApiDatum.Releases, project);
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
                                d.Duration.ToString(), d.IsCurrent.ToString(), d.IsCompleted.ToString(), "", ApiUrl.TrimEnd("/api/".ToCharArray()) + "/app#/deployments/" + d.Id.ToString());
                            releaseDeploys.Add(ad);
                        }
                    }
                }

                dynamic releaseLinks = JsonConvert.DeserializeObject(r.Release.Links.ToString());
                string webUrl = releaseLinks.Web.ToString();
                Release re = new Release(r.Release.Id.ToString(), r.Release.Version.ToString(), r.Release.ProjectId.ToString(),
                    r.Release.ChannelId.ToString(), isoToDateTime(r.Release.Assembled.ToString()), r.Release.ReleaseNotes.ToString(),
                    releaseDeploys, ApiUrl.TrimEnd("/api/".ToCharArray()) + webUrl);
                rl.Add(re);
            }
            return rl;
        }
        #endregion


        #region Make DeployEvent List, Formatted for Graphing
        /// <summary>
        /// Transforms JSON from Octopus API/Events into a list of DeployEvents ready to be used in ChartJS
        /// </summary>
        /// <param name="jsonTxt">JSON string</param>
        /// <returns>DeployList</returns>
        private List<DeployEvent> MakeDeployEventList()
        {
            string jsonTxt = GetResponse(ApiDatum.DeployEvents);
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
                            webUrl = ApiUrl.TrimEnd("/api/".ToCharArray()) + "/app#/deployments/" + str;
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

        #region Make Deploy List
        /// <summary>
        /// Retrieves a list of all deploys from Octopus API
        /// </summary>
        /// <param name="jsonTxt">JSON string</param>
        /// <returns>DeployList</returns>
        private List<Deploy> MakeDeployList()
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

        #endregion

        #region Data Filter Methods

        #region Get Environment
        /// <summary>
        /// Gets an Environment object for some Octopus environment by ID
        /// </summary>
        /// <param name="envName">Some Environment ID</param>
        /// <returns>Environment</returns>
        private Environment GetEnvironment(string envName)
        {
            foreach (Environment environment in Environments) if (environment.Name == envName) return environment;
            return null;
        }
        #endregion
        
        #region Get Deploy
        /// <summary>
        /// Transforms JSON from Octopus API/Events into a list of DeployEvents ready to be used in ChartJS
        /// </summary>
        /// <param name="id">Deploy Id</param>
        /// <returns>Deploy</returns>
        private Deploy GetDeploy(string id)
        {
            foreach (Deploy deploy in Deploys) if (deploy.Id == id) return deploy;
            return null;
        }
        #endregion
        
        #region Make Deploy List by State
        /// <summary>
        /// Filters list of Deploys to only retrieve those with a certain state
        /// </summary>
        /// <param name="state">state</param>
        /// <returns>DeployList</returns>
        private List<Deploy> MakeDeployList(string state)
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

            List<Deploy> deployList = new List<Deploy>();

            foreach (Deploy deploy in Deploys)
            {
                if (deploy.TaskState == status) deployList.Add(deploy);
            }

            return deployList;
        }
        #endregion

        #endregion

        #endregion
    }
}