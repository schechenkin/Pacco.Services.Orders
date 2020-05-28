using System.Linq;
using System.Threading.Tasks;
using Convey.CQRS.Commands;
using Pacco.Services.Orders.Application.Exceptions;
using Pacco.Services.Orders.Application.Services;
using Pacco.Services.Orders.Core.Entities;
using Pacco.Services.Orders.Core.Repositories;
using Pacco.Services.Orders.Framework;

namespace Pacco.Services.Orders.Application.Commands.Handlers
{
    public class DeleteParcelFromOrderHandler : ICommandHandler<DeleteParcelFromOrder>
    {
        private readonly IAggregateStore _aggregateStore;
        private readonly IAppContext _appContext;
        private readonly IMessageBroker _messageBroker;
        private readonly IEventMapper _eventMapper;

        public DeleteParcelFromOrderHandler(IAggregateStore aggregateStore, IAppContext appContext,
            IMessageBroker messageBroker, IEventMapper eventMapper)
        {
            _aggregateStore = aggregateStore;
            _appContext = appContext;
            _messageBroker = messageBroker;
            _eventMapper = eventMapper;
        }

        public async Task HandleAsync(DeleteParcelFromOrder command)
        {
            var order = await _aggregateStore.Load<Order>(command.OrderId);
            if (order is null || order.Deleted)
            {
                throw new OrderNotFoundException(command.OrderId);
            }

            var identity = _appContext.Identity;
            if (identity.IsAuthenticated && identity.Id != order.CustomerId && !identity.IsAdmin)
            {
                throw new UnauthorizedOrderAccessException(order.Id, identity.Id);
            }

            order.DeleteParcel(command.ParcelId);
            await _aggregateStore.Save(order);
            var events = _eventMapper.MapAll(order.Events);
            await _messageBroker.PublishAsync(events.ToArray());
        }
    }
}