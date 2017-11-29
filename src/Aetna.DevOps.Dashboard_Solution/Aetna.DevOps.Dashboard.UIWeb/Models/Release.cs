﻿using System.Collections.Generic;
using System.Collections;

namespace Aetna.DevOps.Dashboard.UIWeb.Models
{
    public class Release
    {
        public string Id, Version, Projectid, Channelid, Assembled, Releasenotes, WebUrl;
        public List<ActiveDeploy> ReleaseDeploys;
        public Dictionary<string, string> Details;
        public enum Source
        {
            TFS,
            TcTFS,
            TcGHE
        }
        public Source ProjectSource;

        public Release(string id, string version, string projectId, string channelId, string assembled, string releaseNotes, List<ActiveDeploy> releaseDeploys, string webUrl)
        {
            Id = id;
            Version = version;
            Projectid = projectId;
            Channelid = channelId;
            Assembled = assembled;
            ReleaseDeploys = releaseDeploys;
            WebUrl = webUrl;
            Details = new Dictionary<string, string>();

            string[] details = releaseNotes.Split(new string[] { ": ", ", ", " - " }, System.StringSplitOptions.RemoveEmptyEntries);

            if (details.Length > 10)
            {
                for (int i = 1; i < details.Length - 2; i++)
                {
                    string[] datum = details[i].Split(new string[] { "=" }, System.StringSplitOptions.None);
                    Details.Add(datum[0], datum[1]);
                }

                Details.Add(details[details.Length - 2], details[details.Length - 1]);

                if (details[0].Equals("Aetna-TFS-Info"))
                {
                    ProjectSource = Source.TFS;
                }
                else if (Details.ContainsKey("ChangeSet"))
                {
                    ProjectSource = Source.TcTFS;
                }
                else
                {
                    ProjectSource = Source.TcGHE;
                }

                foreach (KeyValuePair<string, string> entry in Details)
                {
                    Releasenotes += entry.Key + ": " + entry.Value + "<br />";
                }
            }
            else
            {
                Releasenotes = releaseNotes;
            }
        }
    }
}