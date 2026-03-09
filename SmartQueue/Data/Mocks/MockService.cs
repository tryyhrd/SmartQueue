using SmartQueue.Data.Interfaces;
using SmartQueue.Data.Models;

namespace SmartQueue.Data.Mocks
{
    public class MockService: IService
    {
        public IEnumerable<Service> Services
        {
            get
            {
                return new List<Service>()
                {
                    new Service()
                    {
                        Id = 1,
                        Name = "Подача документов",
                        Description = "adawda",
                        Code = "001"
                    }
                };
            }
        }
    }
}
