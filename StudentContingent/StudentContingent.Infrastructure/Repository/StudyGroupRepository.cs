using Microsoft.EntityFrameworkCore;
using StudentContingent.Domain.Model;
using StudentContingent.Infrastructure.Data;

namespace StudentContingent.Infrastructure.Repository;

/// <summary>Репозиторий академических групп.</summary>
public class StudyGroupRepository
{
    private readonly Context _context;

    public StudyGroupRepository(Context context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<List<StudyGroup>> GetAllAsync()
    {
        return await _context.StudyGroups
            .Include(g => g.StudyProgram)
            .OrderBy(g => g.Number)
            .ToListAsync();
    }

    public async Task<StudyGroup?> GetByIdAsync(Guid id)
    {
        return await _context.StudyGroups
            .Include(g => g.StudyProgram)
            .FirstOrDefaultAsync(g => g.Id == id);
    }

    /// <summary>Группы, закреплённые за куратором.</summary>
    public async Task<List<StudyGroup>> GetByCuratorAsync(Guid curatorUserId)
    {
        return await _context.StudyGroups
            .Where(g => g.CuratorUserId == curatorUserId)
            .OrderBy(g => g.Number)
            .ToListAsync();
    }

    public async Task AddAsync(StudyGroup group)
    {
        // Состав группы формируется только событиями движения.
        group.Students.Clear();
        _context.StudyGroups.Add(group);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> UpdateAsync(StudyGroup group)
    {
        var existGroup = await _context.StudyGroups.FindAsync(group.Id);
        if (existGroup == null)
        {
            return false;
        }
        _context.Entry(existGroup).CurrentValues.SetValues(group);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var group = await _context.StudyGroups.FindAsync(id);
        if (group == null)
        {
            return false;
        }

        var isUsed = await _context.Students.AnyAsync(s => s.StudyGroupId == id)
            || await _context.MovementEvents.AnyAsync(m => m.FromGroupId == id || m.ToGroupId == id);
        if (isUsed)
        {
            throw new InvalidOperationException("Группу нельзя удалить: в ней есть студенты или на неё ссылается история движения.");
        }

        _context.Remove(group);
        await _context.SaveChangesAsync();
        return true;
    }

    public void ChangeTrackerClear()
    {
        _context.ChangeTracker.Clear();
    }
}
