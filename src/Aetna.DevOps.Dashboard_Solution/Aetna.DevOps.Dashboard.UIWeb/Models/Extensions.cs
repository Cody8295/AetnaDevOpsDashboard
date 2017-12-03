using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Aetna.DevOps.Dashboard.UIWeb.Models
{
    public static class Extensions
    {
        #region Deep Equals
        #region List
        public static bool Equals<T>(this List<T> list, List<T> other) where T : OctopusModel<T>
        {
            if (list == null) return other == null;
            if (list.Count != other.Count) return false;

            for (int i = 0; i < list.Count; i++)
            {
                if (!list[i].Equals(other[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public static bool Equals(this List<string> list, List<string> other)
        {
            if (list == null) return other == null;
            if (list.Count != other.Count) return false;
            for (int i = 0; i < list.Count; i++)
            {
                if (!list[i].Equals(other[i])) return false;
            }
            return true;
        }
        #endregion
        #region Dictionary
        public static bool Equals<T>(this Dictionary<string, T> dictionary, Dictionary<string, T> other) where T : OctopusModel<T>
        {
            if (dictionary == null) return other == null;
            if (dictionary.Count != other.Count) return false;
            foreach (KeyValuePair<string, T> element in dictionary)
            {
                if (!element.Value.Equals(other[element.Key])) return false;
            }
            return true;
        }

        public static bool Equals (this Dictionary<string, string> dictionary, Dictionary<string, string> other)
        {
            if (dictionary == null) return other == null;
            if (dictionary.Count != other.Count) return false;
            foreach (KeyValuePair<string, string> element in dictionary)
            {
                if (!element.Value.Equals(other[element.Key])) return false;
            }
            return true;
        }
        #endregion
        #endregion

        public static List<T> ToList<T>(this Dictionary<string, T> dictionary) where T : OctopusModel<T>
        {
            return new List<T>(dictionary.Values);
        }

        public static void AddProject(this Dictionary<string, ProjectGroup> dictionary, string groupId, Project newProject)
        {
            dictionary[groupId].AddProject(newProject);
        }
    }
}