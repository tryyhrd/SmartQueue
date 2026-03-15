using SmartQueue.Data.Models;

namespace SmartQueue.Data.Interfaces
{
    public interface IService
    {
        public IEnumerable<Service> Services { get; }
        Task AddServiceAsync(Service service);
        Task RemoveServiceAsync(Service service);
        Task UpdateServiceAsync(Service service);
    }
}
