using Microsoft.EntityFrameworkCore;
using StudentContingent.Domain.Model;
using StudentContingent.Infrastructure.Data;

namespace StudentContingent.Infrastructure.Repository;

/// <summary>Репозиторий приказов.</summary>
public class OrderRepository
{
    private readonly Context _context;

    public OrderRepository(Context context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<List<Order>> GetAllAsync()
    {
        return await _context.Orders
            .OrderByDescending(o => o.Date)
            .ToListAsync();
    }

    /// <summary>Приказ вместе с проведёнными по нему событиями.</summary>
    public async Task<Order?> GetByIdAsync(Guid id)
    {
        return await _context.Orders
            .Include(o => o.MovementEvents)
                .ThenInclude(m => m.Student)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task AddAsync(Order order)
    {
        var numberTaken = await _context.Orders
            .AnyAsync(o => o.Number == order.Number && o.Date.Year == order.Date.Year);
        if (numberTaken)
        {
            throw new InvalidOperationException($"Приказ № {order.Number} за {order.Date.Year} год уже зарегистрирован.");
        }

        // События создаются только при проведении приказа.
        order.MovementEvents.Clear();
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> UpdateAsync(Order order)
    {
        var existOrder = await _context.Orders.FindAsync(order.Id);
        if (existOrder == null)
        {
            return false;
        }
        _context.Entry(existOrder).CurrentValues.SetValues(order);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null)
        {
            return false;
        }
        if (await _context.MovementEvents.AnyAsync(m => m.OrderId == id))
        {
            throw new InvalidOperationException("Приказ нельзя удалить: по нему уже проведены события движения.");
        }
        _context.Remove(order);
        await _context.SaveChangesAsync();
        return true;
    }

    public void ChangeTrackerClear()
    {
        _context.ChangeTracker.Clear();
    }
}
