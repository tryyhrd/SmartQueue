namespace SmartQueue.Data.Interfaces
{
    public interface IService
    {
        public IEnumerable<IService> Services { get; }
    }
}
