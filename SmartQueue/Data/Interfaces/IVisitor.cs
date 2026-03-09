namespace SmartQueue.Data.Interfaces
{
    public interface IVisitor
    {
        public IEnumerable<IVisitor> Visitors { get; }
    }
}
