using System.Threading.Tasks;
using Convey.CQRS.Commands;
using Pacco.Services.Orders.Application.Events;
using Pacco.Services.Orders.Application.Exceptions;
using Pacco.Services.Orders.Application.Services;
using Pacco.Services.Orders.Core.Entities;
using Pacco.Services.Orders.Core.Exceptions;
using Pacco.Services.Orders.Core.Repositories;
using Pacco.Services.Orders.Framework;

namespace Pacco.Services.Orders.Application.Commands.Handlers
{
    public class DeleteOrderHandler : ICommandHandler<DeleteOrder>
    {
        private readonly IAggregateStore _aggregateStore;
        private readonly IAppContext _appContext;
        private readonly IMessageBroker _messageBroker;

        public DeleteOrderHandler(IAggregateStore aggregateStore, IAppContext appContext,
            IMessageBroker messageBroker)
        {
            _aggregateStore = aggregateStore;
            _appContext = appContext;
            _messageBroker = messageBroker;
        }

        public async Task HandleAsync(DeleteOrder command)
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

            if (!order.CanBeDeleted)
            {
                throw new CannotDeleteOrderException(command.OrderId);
            }
            
            order.Delete();

            await _aggregateStore.Save(order);
            await _messageBroker.PublishAsync(new OrderDeleted(command.OrderId));
        }
    }
}