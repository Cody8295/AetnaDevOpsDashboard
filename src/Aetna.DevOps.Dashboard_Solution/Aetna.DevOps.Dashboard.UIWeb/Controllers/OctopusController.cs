﻿using System;
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
    public class OctopusController : ApiController
    {

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
            }catch(WebException)
            {
                return ""; // server didnt respond, send blank response
            }
            Stream dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string serverResponse = "";
            try
            {
                serverResponse = reader.ReadToEnd();
            }catch(IOException ioe)
            {
                serverResponse = ""; // server force closed connection for some reason
            }
            reader.Close();
            response.Close();
            return serverResponse;
        }
        #endregion

        #region "Get Active Projects by Environment"
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
        private static EnvironmentList MakeEnvironmentList()
        {
            EnvironmentList el = new EnvironmentList();
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
        private List<Release> GetReleaseList(string response)
        {
            dynamic releases = JsonConvert.DeserializeObject(response);
            ReleaseList rl = new ReleaseList();
            if (String.IsNullOrEmpty(response)) { return rl.Releases; } // if response is empty, do not proceed

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
            return rl.Releases;
        }
        #endregion

        #region "Get First Int"
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

        #region "Datetime functions"

        #region "ISO to Datetime"
        private string isoToDateTime(string iso)
        {
            DateTime dateTime = DateTime.Parse(iso).ToLocalTime();
            return dateTime.ToString();
        }
        #endregion

        private DateTime DateTimeFromEpoch(long time)
        {
            return epoch.AddSeconds(time);
        }

        private long EpochFromDateTime(DateTime dt)
        {
            TimeSpan epochSpan = dt.ToUniversalTime() - epoch;
            return (long)Math.Floor(epochSpan.TotalSeconds);
        }
        #endregion

        #region "Format deploys for graphing"
        private DeployList GraphDeployments(string jsonTxt)
        {
            if (String.IsNullOrEmpty(jsonTxt)) { return new DeployList(); } // if response is empty, do not proceed
            DeployList dl = new DeployList();
            dynamic jsonDeser = JsonConvert.DeserializeObject(jsonTxt);
            foreach (dynamic o in jsonDeser.Items)
            {
                string occured = o.Occurred.ToString(); //isoToDateTime(o.Occurred.ToString());
                DateTime parsedDt = Convert.ToDateTime(occured);
                string occuredISO = parsedDt.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fZ");
                if (DateTime.Now.AddDays(-1) > parsedDt) { continue; } // ignore events that took place more than 1 day ago

                dynamic deployLinks = JsonConvert.DeserializeObject(o.RelatedDocumentIds.ToString());
                string webUrl = "";
                foreach(string str in deployLinks)
                {
                    if (str.StartsWith("Deployments-")) { webUrl = API_URL.TrimEnd("/api/".ToCharArray()) + "/app#/deployments/" + str; break; }
                }

                Deploy d = new Deploy(occuredISO, o.Message.ToString(),
                    JsonConvert.DeserializeObject<System.Collections.Generic.List<string>>(o.RelatedDocumentIds.ToString()), // nested list element
                    o.Category.ToString(), webUrl);
                if (o.Category == "DeploymentStarted") { dl.Add(d); }
                if (o.Category == "DeploymentQueued") { dl.Add(d); }
                if (o.Category == "DeploymentSucceeded") { dl.Add(d); }
                if (o.Category == "DeploymentFailed") { dl.Add(d); }
            }
            return dl;
        }
        #endregion

        #region "Get Machines"
        private static MachineList GetMachines(string envId)
        {
            string machineResponse = GetResponse(APIdatum.machines, envId);
            if (String.IsNullOrEmpty(machineResponse)) { return new MachineList(); } // if response is empty, do not proceed
            dynamic mach = JsonConvert.DeserializeObject(machineResponse);
            MachineList m = new MachineList();
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
        private Environment GetEnviron(string envName)
        {
            string environData = GetResponse(APIdatum.environments, envName);
            if (String.IsNullOrEmpty(environData)) { return new Environment("","","",new MachineList()); } // if response is empty, do not proceed
            dynamic env = JsonConvert.DeserializeObject(environData);
            Environment e = new Environment(env.Id.ToString(), env.Name.ToString(), env.Description.ToString(), GetMachines(envName));
            return e;
        }
        #endregion

        #endregion

        #region "SignalR UpdateDataState"
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
                state.ProjectGroups = SortProjectGroups();
                state.IsChanged["ProjectGroups"] = true;
                anyChange = true;
            }

            List<Project> pl = MakeProjectList();
            if (state.Projects == null || state.Projects != pl)
            {
                state.Projects = pl;
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

            List<Environment> env = MakeEnvironmentList().Environments;
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
                return Ok<List<Machine>>(GetMachines(envId).Machines);
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
                EnvironmentList el = MakeEnvironmentList();
                return Ok<List<Environment>>(el.Environments);
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
                DeployList dl = GraphDeployments(GetResponse(APIdatum.deploys));
                for (int x = 0; x < dl.Deploys.Count; x++)
                {
                    if (dl.Deploys[x].RelatedDocs.Count > 0)
                    {
                        foreach (string docID in dl.Deploys[x].RelatedDocs)
                        {
                            if (docID.Contains("Environments"))
                            {
                                dl.Deploys[x].Environs.Add(GetEnviron(docID));
                            }
                        }
                    }
                }
                return Ok<System.Collections.Generic.List<Deploy>>(dl.Deploys);
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
