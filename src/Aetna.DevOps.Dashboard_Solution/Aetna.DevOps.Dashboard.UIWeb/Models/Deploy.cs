using System.Collections.Generic;

namespace Aetna.DevOps.Dashboard.UIWeb.Models
{
    public class Deploy : OctopusModel<Deploy>
    {
        public string TimeAndDate;
        public string Message;
        public List<string> RelatedDocs;
        public string Category;
        public List<Environment> Environs;
        public string WebUrl;

        public Deploy(string timeAndDate, string msg, List<string> related, string category, string webUrl)
        {
            TimeAndDate = timeAndDate;
            Message = msg;
            RelatedDocs = related;
            Category = category;
            Environs = new List<Environment>();
            WebUrl = webUrl;
        }
        public override string ToString()
        {
            return Message;
        }

        public bool Equals(Deploy other)
        {
            return (TimeAndDate == other.TimeAndDate && Message == other.Message && RelatedDocs.Equals(other.RelatedDocs) && Category == other.Category && Environs.Equals(other.Environs) && WebUrl == other.WebUrl);
        }
    }
}