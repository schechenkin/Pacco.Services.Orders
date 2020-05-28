using System.Linq;
using System.Threading.Tasks;
using Convey.CQRS.Commands;
using Pacco.Services.Orders.Application.Services;
using Pacco.Services.Orders.Core.Entities;
using Pacco.Services.Orders.Core.Exceptions;
using Pacco.Services.Orders.Core.Repositories;
using Pacco.Services.Orders.Framework;

namespace Pacco.Services.Orders.Application.Commands.Handlers
{
    public class CreateOrderHandler : ICommandHandler<CreateOrder>
    {
        private readonly IAggregateStore _aggregateStore;
        //private readonly ICustomerRepository _customerRepository;
        private readonly IMessageBroker _messageBroker;
        private readonly IEventMapper _eventMapper;
        private readonly IDateTimeProvider _dateTimeProvider;

        public CreateOrderHandler(IAggregateStore aggregateStore,
            IMessageBroker messageBroker, IEventMapper eventMapper, IDateTimeProvider dateTimeProvider)
        {
            _aggregateStore = aggregateStore;
            _messageBroker = messageBroker;
            _eventMapper = eventMapper;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task HandleAsync(CreateOrder command)
        {
            /*if (!(await _customerRepository.ExistsAsync(command.CustomerId)))
            {
                throw new CustomerNotFoundException(command.CustomerId);
            }*/

            var order = Order.Create(command.OrderId, command.CustomerId, _dateTimeProvider.Now);
            await _aggregateStore.Save(order);
            var events = _eventMapper.MapAll(order.Events);
            await _messageBroker.PublishAsync(events.ToArray());
        }
    }
}