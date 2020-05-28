using System;
using Pacco.Services.Orders.Framework;

namespace Pacco.Services.Orders.Core
{
    public static class Events
    {
        public class OrderCreated : IDomainEvent
        {
            public OrderCreated(AggregateId orderId, Guid customerId, DateTime createdAt)
            {
                OrderId = orderId;
                CustomerId = customerId;
                CreatedAt = createdAt;
            }

            public AggregateId OrderId { get; set; }
            public Guid CustomerId { get; set; } 
            public DateTime CreatedAt { get; set; }
        }
        
        public class TotalPriceChanged : IDomainEvent
        {
            public TotalPriceChanged(AggregateId orderId, decimal totalPrice)
            {
                OrderId = orderId;
                TotalPrice = totalPrice;
            }

            public AggregateId OrderId { get; set; }
            public decimal TotalPrice { get; set; }
        }

        public class VehicleAssignedToOrder : IDomainEvent
        {
            public VehicleAssignedToOrder(AggregateId orderId, Guid vehicleId)
            {
                OrderId = orderId;
                VehicleId = vehicleId;
            }

            public AggregateId OrderId { get; set; }
            public  Guid VehicleId { get; set; }
        }
        
        public class OrderDeliveryDateChanged : IDomainEvent
        {
            public OrderDeliveryDateChanged(AggregateId orderId, DateTime date)
            {
                OrderId = orderId;
                Date = date;
            }

            public AggregateId OrderId { get; set; }
            public DateTime Date { get; set; }
        }
        
        public class ParcelAddedToOrder : IDomainEvent
        {
            public ParcelAddedToOrder(AggregateId orderId, Guid parcelId, string name, string variant, string size)
            {
                OrderId = orderId;
                ParcelId = parcelId;
                Name = name;
                Variant = variant;
                Size = size;
            }

            public AggregateId OrderId { get; set; }
            public Guid ParcelId { get; set; }
            public string Name { get; set; }
            public string Variant { get; set; }
            public string Size { get; set; }
        }
        
        public class ParcelRemovedFromOrder : IDomainEvent
        {
            public ParcelRemovedFromOrder(AggregateId orderId, Guid parcelId)
            {
                OrderId = orderId;
                ParcelId = parcelId;
            }

            public AggregateId OrderId { get; set; }
            public Guid ParcelId { get; set; }
        }
        
        public class OrderApproved : IDomainEvent
        {
            public OrderApproved(AggregateId orderId)
            {
                OrderId = orderId;
            }

            public AggregateId OrderId { get; set; }
        }
        
        public class OrderCancelled : IDomainEvent
        {
            public OrderCancelled(AggregateId orderId, string reason)
            {
                OrderId = orderId;
                Reason = reason;
            }

            public AggregateId OrderId { get; set; }
            public string Reason { get; set; }
        }
        
        public class OrderCompleted : IDomainEvent
        {
            public OrderCompleted(AggregateId orderId, Guid customerId)
            {
                OrderId = orderId;
                CustomerId = customerId;
            }

            public AggregateId OrderId { get; set; }
            public Guid CustomerId { get; set; }
        }
        
        public class OrderDeliveringStarted: IDomainEvent
        {
            public OrderDeliveringStarted(AggregateId orderId)
            {
                OrderId = orderId;
            }

            public AggregateId OrderId { get; set; }
        }
        
        public class OrderDeleted: IDomainEvent
        {
            public OrderDeleted(AggregateId orderId)
            {
                OrderId = orderId;
            }

            public AggregateId OrderId { get; set; }
        }
    }
}