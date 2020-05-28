using System.Threading.Tasks;

namespace Pacco.Services.Orders.Framework
{
    public interface IAggregateStore
    {
        Task<bool> Exists<T>(AggregateId aggregateId);
        
        Task Save<T>(T aggregate) where T : AggregateRoot;
        
        Task<T> Load<T>(AggregateId aggregateId) where T : AggregateRoot;
    }
}