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
using System.Collections.Generic;
using Newtonsoft.Json;
using Microsoft.AspNet.SignalR;

namespace Aetna.DevOps.Dashboard.UIWeb.Controllers
{
    [Microsoft.AspNet.SignalR.Hubs.HubName("deployAction")]
    public class DeployAction : Hub
    {
        private static LiveDeploys currentState = new LiveDeploys();
        private static Random rnd = new Random();
        private static System.Timers.Timer timer = new Sysyem.Timers.Timer(400);
        [Microsoft.AspNet.SignalR.Hubs.HubMethodName("onAction")]
        public void onAction()
        {

        }
    }

    public class LiveDeploys
    {

    }

    #region "JSON API Classes"
    public class Machine
    {
        public string id;
        public string name;
        public string url;
        public System.Collections.Generic.List<string> environs;
        public string status;
        public string statusSummary;
        public string isInProcess;
        public Machine(string Id, string Name, string Url, System.Collections.Generic.List<string> Environs, string Status, string StatusSummary, string IsInProcess)
        {
            id = Id;
            name = Name;
            url = Url;
            environs = Environs;
            status = Status;
            statusSummary = StatusSummary;
            isInProcess = IsInProcess;
        }
    }

    public class MachineList
    {
        public System.Collections.Generic.List<Machine> machines;
        public MachineList() { machines = new System.Collections.Generic.List<Machine>(); }
        
        public void add(Machine m) { machines.Add(m); }
    }

    public class Environment
    {
        public string id;
        public string name;
        public string description;
        public MachineList machines;

        public Environment(string Id, string Name, string Description, MachineList Machines)
        {
            id = Id;
            name = Name;
            description = Description;
            machines = Machines;
        }

        public override string ToString()
        {
            return name + ":" + machines.machines.Count;
        }
    }
    /*
    public class Environment
    {
        public string name;
        public string id;
        public string numMachines;

        public Environment(string name, string id, string numMachines)
        {
            this.name = name;
            this.id = id;
            this.numMachines = numMachines;
        }


        public override string ToString()
        {
            return name + ":" + numMachines;
        }
    }
    */
    public class EnvironmentList
    {
        public System.Collections.Generic.List<Environment> environments;
        public EnvironmentList() { environments = new System.Collections.Generic.List<Environment>(); }
        public void add(Environment e) { environments.Add(e); }
    }

    public class Deploy
    {
        public long TimeAndDate;
        public string Message;
        public System.Collections.Generic.List<string> RelatedDocs;
        public string Category;
        public System.Collections.Generic.List<Environment> Environs;

        public Deploy(long timeAndDate, string msg, System.Collections.Generic.List<string> related, string category)
        {
            TimeAndDate = timeAndDate;
            Message = msg;
            RelatedDocs = related;
            Category = category;
            Environs = new System.Collections.Generic.List<Environment>();
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

    public class Project
    {
        public string groupId;
        public string name;
        public string lifecycle;
        public string deploymentProcess;
        public List<string> relatedDocs;

        public Project(string groupId, string name, string lifecycle, string deploymentProcess)
        {
            this.groupId = groupId;
            this.name = name;
            this.lifecycle = lifecycle;
            this.deploymentProcess = deploymentProcess;
        }
        public string getGroupId() { return groupId; }
    }

    public class ProjectList
    {
        public int count;
        public List<Project> projects;


        public ProjectList() { projects = new List<Project>(); count = 0; }

        public void Add(Project p) { projects.Add(p); count++; }
    }

    public class ProjectGroup
    {
        public string groupName;
        public string groupId;
        public ProjectList projectList;

        public ProjectGroup (string groupName, string groupId)
        {
            this.groupName = groupName;
            this.groupId = groupId;
            projectList = new ProjectList();
        }

        public void AddProject(Project project)
        {
            projectList.Add(project);
        }
    }

    public class ProjectGroupDictionary
    {
        public Dictionary<string, ProjectGroup> pGroupDictionary;

        public ProjectGroupDictionary()
        {
            pGroupDictionary = new Dictionary<string, ProjectGroup>();
        }

        public void AddProjectGroup (string groupId, ProjectGroup pGroup)
        {
            pGroupDictionary.Add(groupId, pGroup);
        }

        public void addProject (string groupId, Project project)
        {
            pGroupDictionary[groupId].AddProject(project);
        }

        public List<ProjectGroup> getProjectGroups()
        {
            return new List<ProjectGroup>(pGroupDictionary.Values);
        }
    }
    #endregion

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
            machines = 5
        }

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

        private Dictionary<string, string> getNumberEnviroments(string jsonTxt)
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

        private Dictionary<string, int> getNumberMachines(string jsonTxt)
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

        private EnvironmentList makeEnvironmentList()
        {
            EnvironmentList el = new EnvironmentList();
            Dictionary<string, int> numMachines = getNumberMachines(GetResponse(APIdatum.machines));
            Dictionary<string, string> enviromnents = getNumberEnviroments(GetResponse(APIdatum.environments));

            foreach (string key in enviromnents.Keys)
            {
                el.add(new Environment(enviromnents[key], key, numMachines[enviromnents[key]].ToString(), getMachines(enviromnents[key])));
            }

            return el;
        }

        private List<Project> makeProjectList()
        {
            List<Project> pl = new List<Project>();
            string jsonTxt = GetResponse(APIdatum.projects);

            dynamic jsonDeser = JsonConvert.DeserializeObject(jsonTxt);

            foreach (dynamic o in jsonDeser.Items)
            {
                pl.Add(new Project(o.ProjectGroupId.ToString(), o.Name.ToString(), o.LifecycleId.ToString(), o.DeploymentProcessId.ToString()));
            }

            return pl;
        }

        private List<ProjectGroup> sortProjectGroups()
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

        #region "Datetime functions"
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
                DateTime parsedDt = Convert.ToDateTime(o.Occurred.ToString());
                long occur = epochFromDateTime(parsedDt);
                if(DateTime.Now.AddDays(-1) > parsedDt) { continue; } // ignore events that took place more than 1 day ago
                Deploy d = new Deploy(occur, o.Message.ToString(),
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

        private MachineList getMachines(string envName)
        {
            string machineResponse = GetResponse(APIdatum.machines, envName);
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

        private Environment getEnviron(string envName)
        {
            string environData = GetResponse(APIdatum.environments, envName);
            dynamic env = JsonConvert.DeserializeObject(environData);
            Environment e = new Environment(env.Id.ToString(), env.Name.ToString(), env.Description.ToString(), getMachines(envName));
            return e;
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
                return Ok (getFirstInt(GetResponse(APIdatum.environments)));
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
