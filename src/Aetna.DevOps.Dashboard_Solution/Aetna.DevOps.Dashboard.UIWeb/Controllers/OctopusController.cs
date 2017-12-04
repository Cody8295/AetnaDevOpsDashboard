using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Description;
using System.IO;
using System.Web.Http;
using System.Text.RegularExpressions;
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

        private DataState currentState = DataState.Instance;

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
                return Ok(currentState.ProjectGroups.Count);
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
                return Ok(currentState.Lifecycles);
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
                return Ok<List<Project>>(currentState.Projects);
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
                List<ActiveDeploy> list = null;
                foreach (Environment environment in currentState.Environments)
                {
                    if (environment.Id == envId) list = environment.ActiveDeploys;
                }
                return Ok<List<ActiveDeploy>>(list);
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
                return Ok(currentState.Projects.Count);
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
                List<Release> releases = null;
                foreach (Project projectObj in currentState.Projects)
                {
                    if (projectObj.Id == project) releases = projectObj.Releases;
                }
                return Ok<List<Release>>(releases);
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
                List<Machine> machines = null;
                foreach (Environment environment in currentState.Environments)
                {
                    if (environment.Id == envId) machines = environment.Machines;
                }
                return Ok<List<Machine>>(machines);
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
                return Ok(currentState.Environments.Count);
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
                return Ok<List<Environment>>(currentState.Environments);
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
                return Ok<List<ProjectGroup>>(currentState.ProjectGroups);
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
                return Ok<System.Collections.Generic.List<DeployEvent>>(currentState.DeployEvents);
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
                return Ok<List<Deploy>>(currentState.LiveDeploys);
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
                return Ok<List<Deploy>>(currentState.Deploys);
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
