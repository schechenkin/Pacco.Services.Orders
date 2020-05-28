using System.Collections.Generic;
using Pacco.Services.Orders.Core;

namespace Pacco.Services.Orders.Framework
{
    public abstract class AggregateRoot
    {
        private readonly List<IDomainEvent> _events = new List<IDomainEvent>();
        public IEnumerable<IDomainEvent> Events => _events;
        public AggregateId Id { get; protected set; }
        public int Version { get; protected set; } = -1;
        
        protected abstract void When(IDomainEvent @event);

        protected void AddEvent(IDomainEvent @event)
        {
            _events.Add(@event);
        }
        
        protected void Apply(IDomainEvent @event)
        {
            When(@event);
            EnsureValidState();
            _events.Add(@event);
        }
        
        public void Load(IEnumerable<IDomainEvent> history)
        {
            foreach (var e in history)
            {
                When(e);
                Version++;
            }
        }

        public void ClearEvents() => _events.Clear();
        
        protected abstract void EnsureValidState();
    }
}