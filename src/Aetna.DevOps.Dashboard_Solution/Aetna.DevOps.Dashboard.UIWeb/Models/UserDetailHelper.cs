using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;

namespace Aetna.DevOps.Dashboard.UIWeb.Models
{
    public class UserDetailHelper
    {
        public UserDetail GetAuthHeaderDetails(HttpRequestMessage request)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            String aetUser = String.Empty;
            String mail = String.Empty;
            String firstName = String.Empty;
            String lastName = String.Empty;
            List<String> groups = new List<String>();
            Boolean isAdmin = false;

            IEnumerable<String> values;
            if (request.Headers.TryGetValues("AET_USER", out values))
            {
                // Running in IIS with SiteMinder available
                aetUser = request.Headers.GetValues("AET_USER").FirstOrDefault();
                mail = request.Headers.GetValues("MAIL").FirstOrDefault();
                firstName = request.Headers.GetValues("FIRST_NAME").FirstOrDefault();
                lastName = request.Headers.GetValues("LAST_NAME").FirstOrDefault();
                groups.AddRange(GetGroupsFromHeaders(request.Headers.GetValues("GROUPS").FirstOrDefault()));
                isAdmin = DetermineIfAdmin(groups);
            }

            if (String.IsNullOrEmpty(aetUser))
            {
                // Running locally within Visual Studio.
                aetUser = "a213395";
                mail = "sederr@aetna.com";
                firstName = "Robert (ws)";
                lastName = "Seder";
                isAdmin = true;
                groups.Add("webeng"); // For testing.
            }

            groups.Sort();

            return new UserDetail()
            {
                AetnaUserId = aetUser,
                EmailAddress = mail,
                FirstName = firstName,
                LastName = lastName,
                IsAdmin = isAdmin,
                DomainGroups = groups
            };
        }

        public static Boolean DetermineIfAdmin(IEnumerable<String> groups)
        {
            String adminGroupsName = ConfigurationManager.AppSettings["adminGroups"];

            String[] adminGroups = adminGroupsName.Split(',');

            foreach (String adminGroup in adminGroups)
            {
                if (groups.Any(item => item.Equals(adminGroup, StringComparison.CurrentCultureIgnoreCase)))
                    return true;
            }

            return false;
        }

        private IEnumerable<String> GetGroupsFromHeaders(String groupsHeader)
        {
            if (String.IsNullOrWhiteSpace(groupsHeader))
                yield return null;

            // Example: CN=BD79F7F4-R,OU=BulkDistribution,OU=Messaging,OU=InfraServers,DC=aeth,DC=aetna,DC=com^

            String[] distinguishedNames = groupsHeader.Split('^');

            foreach (String distinguishedName in distinguishedNames)
            {
                String[] dnParts = distinguishedName.Split(',');
                foreach (String dnPart in dnParts)
                {
                    String[] keyValuePair = dnPart.Split('=');

                    String key = keyValuePair[0];
                    String value = keyValuePair[1];

                    if (key.Equals("CN", StringComparison.CurrentCultureIgnoreCase))
                        yield return value;
                }
            }
        }
    }
}