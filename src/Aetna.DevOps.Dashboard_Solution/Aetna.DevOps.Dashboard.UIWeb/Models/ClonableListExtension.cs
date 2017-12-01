using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Aetna.DevOps.Dashboard.UIWeb.Models
{
    public static class ClonableListExtension
    {
        public static List<T> Clone<T> (this List<T> list) where T : Clonable<T>
        {
            List<T> newList = new List<T>();
            foreach (T element in list)
            {
                newList.Add(element.Clone());
            }
            return newList;
        }
    }
}