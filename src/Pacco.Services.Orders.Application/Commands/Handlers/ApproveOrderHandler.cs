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
    public class ApproveOrderHandler : ICommandHandler<ApproveOrder>
    {
        private readonly IAggregateStore _aggregateStore;
        private readonly IMessageBroker _messageBroker;
        private readonly IEventMapper _eventMapper;
        private readonly IAppContext _appContext;

        public ApproveOrderHandler(IAggregateStore aggregateStore, IMessageBroker messageBroker,
            IEventMapper eventMapper, IAppContext appContext)
        {
            _aggregateStore = aggregateStore;
            _messageBroker = messageBroker;
            _eventMapper = eventMapper;
            _appContext = appContext;
        }

        public async Task HandleAsync(ApproveOrder command)
        {
            var order = await _aggregateStore.Load<Order>(command.OrderId);
            if (order is null || order.Deleted)
            {
                throw new OrderNotFoundException(command.OrderId);
            }

            var identity = _appContext.Identity;
            if (identity.IsAuthenticated && identity.Id != order.CustomerId && !identity.IsAdmin)
            {
                throw new UnauthorizedOrderAccessException(command.OrderId, identity.Id);
            }
            
            order.Approve();
            await _aggregateStore.Save(order);
            var events = _eventMapper.MapAll(order.Events);
            await _messageBroker.PublishAsync(events.ToArray());
        }
    }
}