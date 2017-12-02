namespace Aetna.DevOps.Dashboard.UIWeb.Models
{
    public interface Clonable<T>
    {
        T Clone();

        bool Equals(T other);
    }
}
