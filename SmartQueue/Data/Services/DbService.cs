using SmartQueue.Data.Interfaces;
using SmartQueue.Data.Models;

namespace SmartQueue.Data.Services
{
    public class DbService: IService
    {
        public readonly Common.SmartQueueContext _context;
        public DbService(Common.SmartQueueContext context)
        {
            _context = context;
        }
        public IEnumerable<Service> Services
        {
            get
            {
                return _context.Services;
            }
        }
    }
}
