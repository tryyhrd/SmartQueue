using SmartQueue.Data.Models;

namespace SmartQueue.Data.Interfaces
{
    public interface IVisitor
    {
        public IEnumerable<Visitor> Visitors { get; }
        Task AddVisitorAsync(Visitor visitor);
    }
}
