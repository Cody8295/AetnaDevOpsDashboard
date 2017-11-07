using System;
using System.Collections.Generic;

namespace Aetna.DevOps.Dashboard.UIWeb.Models
{
    /// <summary>
    /// Data structure to hold the details of an end-user.
    /// </summary>
    public class UserDetail
    {
        /// <summary>
        /// Gets or sets the Aetna user id.
        /// </summary>
        public String AetnaUserId { get; set; }

        /// <summary>
        /// Gets or sets the e-mail address of the Aetna user.
        /// </summary>
        public String EmailAddress { get; set; }

        /// <summary>
        /// Gets or sets the first name.
        /// </summary>
        public String FirstName { get; set; }

        /// <summary>
        /// Gets or sets the last name.
        /// </summary>
        public String LastName { get; set; }

        /// <summary>
        /// Gets or sets whether the current user is an admin.
        /// </summary>
        public Boolean IsAdmin { get; set; } = false;

        /// <summary>
        /// Gets or sets a collection of groups to which the user belongs.
        /// </summary>
        public List<String> DomainGroups { get; set; } = new List<String>();
    }
}