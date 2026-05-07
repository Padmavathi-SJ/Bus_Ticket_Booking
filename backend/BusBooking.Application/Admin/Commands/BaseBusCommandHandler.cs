// Application/Commands/BaseBusCommandHandler.cs
using BusBooking.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BusBooking.Application.Commands
{
    public abstract class BaseBusCommandHandler<TCommand> : IRequestHandler<TCommand, Guid>
        where TCommand : BaseBusCommand
    {
        protected readonly IAppDbContext _context;
        
        protected BaseBusCommandHandler(IAppDbContext context)
        {
            _context = context;
        }
        
        // Template method pattern - common handler logic
        public async Task<Guid> Handle(TCommand request, CancellationToken cancellationToken)
        {
            // Step 1: Pre-validation (hook for derived classes)
            await BeforeCreateAsync(request, cancellationToken);
            
            // Step 2: Create bus entity using template method
            var bus = request.ToBusEntity();
            
            // Step 3: Post-creation logic (hook for derived classes)
            await AfterCreateAsync(bus, request, cancellationToken);
            
            // Step 4: Save to database
            _context.Buses.Add(bus);
            await _context.SaveChangesAsync(cancellationToken);
            
            // Step 5: Post-save logic (hook for derived classes)
            await AfterSaveAsync(bus, request, cancellationToken);
            
            return bus.Id;
        }
        
        // Virtual methods - can override if needed, but not required
        protected virtual Task BeforeCreateAsync(TCommand request, CancellationToken ct) 
            => Task.CompletedTask;
            
        protected virtual Task AfterCreateAsync(Bus bus, TCommand request, CancellationToken ct) 
            => Task.CompletedTask;
            
        protected virtual Task AfterSaveAsync(Bus bus, TCommand request, CancellationToken ct) 
            => Task.CompletedTask;
    }
}