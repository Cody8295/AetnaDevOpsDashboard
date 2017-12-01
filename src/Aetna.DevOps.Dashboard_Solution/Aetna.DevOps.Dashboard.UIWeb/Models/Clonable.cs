using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aetna.DevOps.Dashboard.UIWeb.Models
{
    public interface Clonable<T>
    {
        T Clone();
    }
}
