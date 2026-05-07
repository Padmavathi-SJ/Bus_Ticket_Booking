// Application/Commands/BaseBusCommand.cs
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace BusBooking.Application.Commands
{
    public abstract class BaseBusCommand : IRequest<Guid>
    {
        // Common properties - all bus commands need these
        [Required] public Guid RouteId { get; set; }
        [Required] public string BusNumber { get; set; } = string.Empty;
        [Required] public string BusName { get; set; } = string.Empty;
        [Required] public string BusType { get; set; } = string.Empty;
        [Required] public int TotalSeats { get; set; }
        [Required] public int FemaleSeats { get; set; }
        [Required] public int MaleSeats { get; set; }
        [Required] public double BasePrice { get; set; }
        public string Amenities { get; set; } = string.Empty;
        
        // Template method - each derived class provides its own logic
        protected abstract BusStatus GetInitialStatus();
        protected abstract bool GetInitialAvailability();
        protected abstract Guid? GetOperatorId();
        
        // Factory method - creates Bus from common properties
        public Bus ToBusEntity()
        {
            return new Bus
            {
                Id = Guid.NewGuid(),
                RouteId = this.RouteId,
                BusNumber = this.BusNumber,
                BusName = this.BusName,
                BusType = this.BusType,
                TotalSeats = this.TotalSeats,
                FemaleSeats = this.FemaleSeats,
                MaleSeats = this.MaleSeats,
                BasePrice = this.BasePrice,
                Amenities = this.Amenities,
                OperatorId = GetOperatorId(),
                Status = GetInitialStatus(),
                IsAvailable = GetInitialAvailability(),
                CreatedAt = DateTime.UtcNow
            };
        }
    }
}