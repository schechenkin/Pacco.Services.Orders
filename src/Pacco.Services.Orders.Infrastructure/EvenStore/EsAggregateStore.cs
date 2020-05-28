using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using Newtonsoft.Json;
using Pacco.Services.Orders.Core;
using Pacco.Services.Orders.Framework;

namespace Pacco.Services.Orders.Infrastructure.EvenStore
{
    public class EsAggregateStore : IAggregateStore
    {
        private readonly IEventStoreConnection _connection;

        public EsAggregateStore(IEventStoreConnection connection) => _connection = connection;

        public async Task Save<T>(T aggregate) where T : AggregateRoot
        {
            if (aggregate == null)
                throw new ArgumentNullException(nameof(aggregate));

            var changes = aggregate.Events
                .Select(@event =>
                    new EventData(
                        eventId: Guid.NewGuid(),
                        type: @event.GetType().Name,
                        isJson: true,
                        data: Serialize(@event),
                        metadata: Serialize(new EventMetadata {ClrType = @event.GetType().AssemblyQualifiedName})
                    ))
                .ToArray();

            if (!changes.Any()) return;

            var streamName = GetStreamName<T>(aggregate);

            await _connection.AppendToStreamAsync(
                streamName,
                aggregate.Version,
                changes);

            //aggregate.ClearEvents();
        }

        public async Task<T> Load<T>(AggregateId aggregateId)
            where T : AggregateRoot
        {
            if (aggregateId == null)
                throw new ArgumentNullException(nameof(aggregateId));

            var stream = GetStreamName<T>(aggregateId);
            var aggregate = (T) Activator.CreateInstance(typeof(T), true);

            var page = await _connection.ReadStreamEventsForwardAsync(
                stream, 0, 1024, false);

            aggregate.Load(page.Events.Select(resolvedEvent =>
            {
                var meta = JsonConvert.DeserializeObject<EventMetadata>(
                    Encoding.UTF8.GetString(resolvedEvent.Event.Metadata));
                var dataType = Type.GetType(meta.ClrType);
                var jsonData = Encoding.UTF8.GetString(resolvedEvent.Event.Data);
                var data = JsonConvert.DeserializeObject(jsonData, dataType);
                return (IDomainEvent)data;
            }).ToArray());

            return aggregate;
        }

        public async Task<bool> Exists<T>(AggregateId aggregateId)
        {
            var stream = GetStreamName<T>(aggregateId);
            var result = await _connection.ReadEventAsync(stream, 1, false);
            return result.Status != EventReadStatus.NoStream;
        }

        private static byte[] Serialize(object data)
            => Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data));

        private static string GetStreamName<T>(AggregateId aggregateId)
            => $"{typeof(T).Name}-{aggregateId.ToString()}";

        private static string GetStreamName<T>(T aggregate)
            where T : AggregateRoot
            => $"{typeof(T).Name}-{aggregate.Id.ToString()}";

        private class EventMetadata
        {
            public string ClrType { get; set; }
        }
    }
}