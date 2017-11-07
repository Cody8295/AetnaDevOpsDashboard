using System;

namespace Aetna.DevOps.Dashboard.UIWeb.Models
{
    /// <summary>
    /// Data structure to hold the details of an operating environment.
    /// </summary>
    public class OperatingEnvironment
    {

        public String ShowEnvironment { get; set; }

        /// <summary>
        /// Gets or sets the name of the environment. (e.g. "Dev", "QA", "Prod", etc)
        /// </summary>
        public String EnvironmentName { get; set; }

        /// <summary>
        /// Gets or sets the CSS class to use for the environment display. (e.g. "label-success")
        /// </summary>
        public String CssClass { get; set; }

        /// <summary>
        /// Gets or sets the date this application was built.
        /// </summary>
        public DateTime BuildDate { get; set; }

        /// <summary>
        /// Gets or sets the version of this application.
        /// </summary>
        public String Version { get; set; }
    }
}