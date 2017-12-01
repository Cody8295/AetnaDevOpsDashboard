using System.Collections.Generic;

namespace Aetna.DevOps.Dashboard.UIWeb.Models
{
    public class Deploy
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

        public Deploy Clone()
        {
            List<string> relatedDocs = new List<string>(RelatedDocs.Capacity);
            foreach (string doc in RelatedDocs)
            {
                relatedDocs.Add(doc);
            }
            
            Deploy newDeploy = new Deploy(TimeAndDate, Message, relatedDocs, Category, WebUrl);

            foreach (Environment environment in Environs)
            {
                newDeploy.Environs.Add(environment.Clone());
            }

            return newDeploy;
        }
    }
}