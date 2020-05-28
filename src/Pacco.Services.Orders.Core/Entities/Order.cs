using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using Pacco.Services.Orders.Core.Exceptions;
using Pacco.Services.Orders.Framework;

namespace Pacco.Services.Orders.Core.Entities
{
    public class Order : AggregateRoot
    {
        private ISet<Parcel> _parcels = new HashSet<Parcel>();
        public Guid CustomerId { get; private set; }
        public Guid? VehicleId { get; private set; }
        public OrderStatus Status { get; private set; }
        
        public bool Deleted { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? DeliveryDate { get; private set; }
        public decimal TotalPrice { get; private set; }
        public string CancellationReason { get; private set; }
        public bool CanBeDeleted => Status == OrderStatus.New;
        public bool CanAssignVehicle => Status == OrderStatus.New || Status == OrderStatus.Canceled;
        public bool HasParcels => Parcels.Any();

        public IEnumerable<Parcel> Parcels
        {
            get => _parcels;
            private set => _parcels = new HashSet<Parcel>(value);
        }
        
        public Order(AggregateId id, Guid customerId, DateTime createdAt)
        {
            Apply(new Events.OrderCreated(id, customerId, createdAt));
           
            /*Id = id;
            CustomerId = customerId;
            Status = status;
            CreatedAt = createdAt;
            Parcels = parcels ?? Enumerable.Empty<Parcel>();
            if (vehicleId.HasValue)
            {
                SetVehicle(vehicleId.Value);
            }

            if (deliveryDate.HasValue)
            {
                SetDeliveryDate(deliveryDate.Value);
            }

            TotalPrice = totalPrice;
            CancellationReason = string.Empty;*/
        }

        public static Order Create(AggregateId id, Guid customerId, DateTime createdAt)
        {
            return new Order(id, customerId, createdAt);
        }

        public void SetTotalPrice(decimal totalPrice)
        {
            if (Status != OrderStatus.New)
            {
                throw new CannotChangeOrderPriceException(Id);
            }

            if (totalPrice < 0)
            {
                throw new InvalidOrderPriceException(Id, totalPrice);
            }
            
            Apply(new Events.TotalPriceChanged(Id, totalPrice));

            //TotalPrice = totalPrice;
        }

        public void SetVehicle(Guid vehicleId)
        {
            Apply(new Events.VehicleAssignedToOrder(Id, vehicleId));
            
            //VehicleId = vehicleId;
        }

        public void SetDeliveryDate(DateTime deliveryDate)
        {
            Apply(new Events.OrderDeliveryDateChanged(Id, deliveryDate));
            
            //DeliveryDate = deliveryDate.Date;
        }

        public void AddParcel(Parcel parcel)
        {
            if (!_parcels.Add(parcel))
            {
                throw new ParcelAlreadyAddedToOrderException(Id, parcel.Id);
            }

            Apply(new Events.ParcelAddedToOrder(Id, parcel.Id, parcel.Name, parcel.Variant, parcel.Size));
            
            //AddEvent(new ParcelAdded(this, parcel));
        }

        public void DeleteParcel(Guid parcelId)
        {
            var parcel = _parcels.SingleOrDefault(p => p.Id == parcelId);
            if (parcel is null)
            {
                throw new OrderParcelNotFoundException(parcelId, Id);
            }

            Apply(new Events.ParcelRemovedFromOrder(Id, parcel.Id));
            
            //AddEvent(new ParcelDeleted(this, parcel));
        }

        public void Approve()
        {
            if (Status != OrderStatus.New && Status != OrderStatus.Canceled)
            {
                throw new CannotChangeOrderStateException(Id, Status, OrderStatus.Approved);
            }

            /*Status = OrderStatus.Approved;
            CancellationReason = string.Empty;
            AddEvent(new OrderStateChanged(this));*/
            
            Apply(new Events.OrderApproved(Id));
        }

        public void Cancel(string reason)
        {
            if (Status == OrderStatus.Completed || Status == OrderStatus.Canceled)
            {
                throw new CannotChangeOrderStateException(Id, Status, OrderStatus.Canceled);
            }

            /*Status = OrderStatus.Canceled;
            CancellationReason = reason ?? string.Empty;
            AddEvent(new OrderStateChanged(this));*/
            
            Apply(new Events.OrderCancelled(Id, reason));
        }

        public void Complete()
        {
            if (Status != OrderStatus.Delivering)
            {
                throw new CannotChangeOrderStateException(Id, Status, OrderStatus.Completed);
            }

            /*Status = OrderStatus.Completed;
            AddEvent(new OrderStateChanged(this));*/
            
            Apply(new Events.OrderCompleted(Id, CustomerId));
        }

        public void SetDelivering()
        {
            if (Status != OrderStatus.Approved)
            {
                throw new CannotChangeOrderStateException(Id, Status, OrderStatus.Delivering);
            }

            /*Status = OrderStatus.Delivering;
            AddEvent(new OrderStateChanged(this));*/
            
            Apply(new Events.OrderDeliveringStarted(Id));
        }

        public void Delete()
        {
            if (CanBeDeleted)
            {
                Apply(new Events.OrderDeleted(Id));
            }
        }

        protected override void When(IDomainEvent @event)
        {
            switch (@event)
            {
                case Events.OrderCreated e:
                    Id = e.OrderId;
                    CustomerId = e.CustomerId;
                    Status = OrderStatus.New;
                    Parcels = Enumerable.Empty<Parcel>();
                    CancellationReason = string.Empty;
                    break;
                case Events.TotalPriceChanged e:
                    TotalPrice = e.TotalPrice;
                    break;
                case Events.VehicleAssignedToOrder e:
                    VehicleId = e.VehicleId;
                    break;
                case Events.OrderDeliveryDateChanged e:
                    DeliveryDate = e.Date;
                    break;
                case Events.ParcelAddedToOrder e:
                    _parcels.Add(new Parcel(e.ParcelId, e.Name, e.Variant, e.Size));
                    break;
                case Events.ParcelRemovedFromOrder e:
                    var parcel = _parcels.Single(p => p.Id == e.ParcelId);
                    _parcels.Remove(parcel);
                    break;
                case Events.OrderApproved e:
                    Status = OrderStatus.Approved;
                    CancellationReason = String.Empty;
                    break;
                case Events.OrderCancelled e:
                    Status = OrderStatus.Canceled;
                    CancellationReason = e.Reason ?? string.Empty;
                    break;
                case Events.OrderCompleted e:
                    Status = OrderStatus.Completed;
                    CancellationReason = String.Empty;
                    break;
                case Events.OrderDeliveringStarted e:
                    Status = OrderStatus.Delivering;
                    break;
                case Events.OrderDeleted e:
                    Deleted = true;
                    break;
            }
        }

        protected override void EnsureValidState()
        {
            
        }
        
        protected Order()
        {
            
        }
    }
}