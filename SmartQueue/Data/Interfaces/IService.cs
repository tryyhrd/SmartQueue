using SmartQueue.Data.Models;

namespace SmartQueue.Data.Interfaces
{
    public interface IService
    {
        public IEnumerable<Service> Services { get; }
    }
}
