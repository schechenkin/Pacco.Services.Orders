using System;
using System.Collections.Generic;
using System.Linq;
using Convey.CQRS.Events;
using Pacco.Services.Orders.Application.Events;
using Pacco.Services.Orders.Application.Services;
using Pacco.Services.Orders.Core;
using Pacco.Services.Orders.Core.Entities;

namespace Pacco.Services.Orders.Infrastructure.Services
{
    public class EventMapper : IEventMapper
    {
        public IEnumerable<IEvent> MapAll(IEnumerable<IDomainEvent> events)
            => events.Select(Map);

        public IEvent Map(IDomainEvent @event)
        {
            /*switch (@event)
            {
                case OrderStateChanged e:
                    return e.Order.Status switch
                    {
                        OrderStatus.New => new OrderCreated(e.Order.Id),
                        OrderStatus.Approved => new OrderApproved(e.Order.Id),
                        OrderStatus.Delivering => new OrderDelivering(e.Order.Id),
                        OrderStatus.Completed => new OrderCompleted(e.Order.Id, e.Order.CustomerId),
                        OrderStatus.Canceled => new OrderCanceled(e.Order.Id, e.Order.CancellationReason),
                        _ => throw new ArgumentOutOfRangeException()
                    };

                    break;
                case ParcelAdded e:
                    return new ParcelAddedToOrder(e.Order.Id, e.Parcel.Id);
                case ParcelDeleted e:
                    return new ParcelDeletedFromOrder(e.Order.Id, e.Parcel.Id);
            }*/
            
            switch (@event)
            {
                case Events.OrderCreated e:
                    return new OrderCreated(e.OrderId); 
                case Events.OrderApproved e:
                    return new OrderApproved(e.OrderId); 
                case Events.OrderDeliveringStarted e:
                    return new OrderDelivering(e.OrderId); 
                case Events.OrderCompleted e:
                    return new OrderCompleted(e.OrderId, e.CustomerId); 
                case Events.OrderCancelled e:
                    return new OrderCreated(e.OrderId); 

                    break;
                case Events.ParcelAddedToOrder e:
                    return new ParcelAddedToOrder(e.OrderId, e.ParcelId);
                case Events.ParcelRemovedFromOrder e:
                    return new ParcelDeletedFromOrder(e.OrderId, e.ParcelId);
            }

            return null;
        }
    }
}