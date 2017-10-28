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
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Aetna.DevOps.Dashboard.UIWeb.Controllers
{
    #region "API Classes"
    public class Deploy
    {
        public DateTime TimeAndDate;
        public string Message;
        public System.Collections.Generic.List<string> RelatedDocs;
        public string Category;
        public Deploy(DateTime timeAndDate, string msg, System.Collections.Generic.List<string> related, string category)
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

    public class EnvironmentList
    {
        public List<Environment> environments;

        public EnvironmentList() { environments = new List<Environment>(); }

        public void addEnvironment(Environment environment) { environments.Add(environment); }
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
            deploys = 4,
            machines = 5,
        }

        private static string GetResponse(APIdatum apid)
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
                    reqString = "environments?";
                    break;
                case APIdatum.deploys:
                    reqString = "events?take=1000&";
                    break;
                case APIdatum.machines:
                    reqString = "machines?take=100000&";
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
                el.addEnvironment(new Environment(key, enviromnents[key], numMachines[enviromnents[key]].ToString()));
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

        private DeployList graphDeployments(string jsonTxt)
        {
            DeployList dl = new DeployList();
            dynamic jsonDeser = JsonConvert.DeserializeObject(jsonTxt);
            foreach(dynamic o in jsonDeser.Items)
            {
                if(DateTime.Now.AddDays(-1) > Convert.ToDateTime(o.Occurred)) { continue; } // ignore events that took place more than 1 day ago
                Deploy d = new Deploy(Convert.ToDateTime(o.Occurred.ToString()), o.Message.ToString(),
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
