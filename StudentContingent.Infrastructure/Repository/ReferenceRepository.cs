using Microsoft.EntityFrameworkCore;
using StudentContingent.Domain.Model;
using StudentContingent.Infrastructure.Data;

namespace StudentContingent.Infrastructure.Repository;

/// <summary>
/// Обобщённый репозиторий для простых справочников без собственных правил:
/// подразделения, образовательные программы, льготные категории.
/// </summary>
public class ReferenceRepository<T> where T : class, IEntity
{
    private readonly Context _context;

    public ReferenceRepository(Context context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<List<T>> GetAllAsync()
    {
        return await _context.Set<T>().AsNoTracking().ToListAsync();
    }

    public async Task<T?> GetByIdAsync(Guid id)
    {
        return await _context.Set<T>().FindAsync(id);
    }

    public async Task AddAsync(T entity)
    {
        _context.Set<T>().Add(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> UpdateAsync(T entity)
    {
        var existEntity = await _context.Set<T>().FindAsync(entity.Id);
        if (existEntity == null)
        {
            return false;
        }
        _context.Entry(existEntity).CurrentValues.SetValues(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await _context.Set<T>().FindAsync(id);
        if (entity == null)
        {
            return false;
        }

        _context.Remove(entity);
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            _context.ChangeTracker.Clear();
            throw new InvalidOperationException("Запись нельзя удалить: на неё ссылаются другие данные.");
        }
        return true;
    }

    public void ChangeTrackerClear()
    {
        _context.ChangeTracker.Clear();
    }
}
