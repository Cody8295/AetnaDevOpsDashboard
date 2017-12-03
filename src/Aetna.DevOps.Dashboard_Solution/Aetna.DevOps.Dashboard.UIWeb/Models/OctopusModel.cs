namespace Aetna.DevOps.Dashboard.UIWeb.Models
{
    public interface OctopusModel<T>
    {
        bool Equals(T other);
    }
}
