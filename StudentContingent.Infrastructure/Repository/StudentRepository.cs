using Microsoft.EntityFrameworkCore;
using StudentContingent.Domain.Model;
using StudentContingent.Infrastructure.Data;

namespace StudentContingent.Infrastructure.Repository;

/// <summary>
/// Репозиторий агрегата «Студент»: CRUD карточки со всеми сведениями
/// и проведение событий движения контингента.
/// </summary>
public class StudentRepository
{
    private readonly Context _context;

    public Context UnitOfWork
    {
        get
        {
            return _context;
        }
    }

    public StudentRepository(Context context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<List<Student>> GetAllAsync()
    {
        return await _context.Students
            .OrderBy(s => s.LastName)
            .ThenBy(s => s.FirstName)
            .ToListAsync();
    }

    public async Task<Student?> GetByIdAsync(Guid id)
    {
        return await WithDetails().FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<Student?> GetByRecordBookNumberAsync(string recordBookNumber)
    {
        return await WithDetails().FirstOrDefaultAsync(s => s.RecordBookNumber == recordBookNumber);
    }

    public async Task<List<Student>> SearchAsync(StudentFilter filter)
    {
        IQueryable<Student> query = _context.Students.Include(s => s.StudyGroup);

        if (!string.IsNullOrWhiteSpace(filter.LastName))
        {
            query = query.Where(s => s.LastName.StartsWith(filter.LastName));
        }
        if (!string.IsNullOrWhiteSpace(filter.RecordBookNumber))
        {
            query = query.Where(s => s.RecordBookNumber == filter.RecordBookNumber);
        }
        if (filter.StudyGroupId is not null)
        {
            query = query.Where(s => s.StudyGroupId == filter.StudyGroupId);
        }
        if (filter.StudyProgramId is not null)
        {
            query = query.Where(s => s.StudyGroup != null && s.StudyGroup.StudyProgramId == filter.StudyProgramId);
        }
        if (filter.Course is not null)
        {
            query = query.Where(s => s.StudyGroup != null && s.StudyGroup.Course == filter.Course);
        }
        if (filter.Status is not null)
        {
            query = query.Where(s => s.Status == filter.Status);
        }
        if (filter.FundingBasis is not null)
        {
            query = query.Where(s => s.FundingBasis == filter.FundingBasis);
        }

        return await query
            .OrderBy(s => s.LastName)
            .ThenBy(s => s.FirstName)
            .ToListAsync();
    }

    /// <summary>Текущий состав группы (для куратора).</summary>
    public async Task<List<Student>> GetByGroupAsync(Guid studyGroupId)
    {
        return await _context.Students
            .Where(s => s.StudyGroupId == studyGroupId)
            .Include(s => s.Contacts)
            .OrderBy(s => s.LastName)
            .ThenBy(s => s.FirstName)
            .ToListAsync();
    }

    /// <summary>История движения студента по дате вступления в силу.</summary>
    public async Task<List<MovementEvent>> GetHistoryAsync(Guid studentId)
    {
        return await _context.MovementEvents
            .Where(m => m.StudentId == studentId)
            .Include(m => m.Order)
            .Include(m => m.FromGroup)
            .Include(m => m.ToGroup)
            .OrderBy(m => m.EffectiveDate)
            .AsNoTracking()
            .ToListAsync();
    }

    /// <summary>Численность контингента по курсам, статусам и основам обучения.</summary>
    public async Task<List<ContingentSummaryRow>> GetSummaryAsync(Guid? studyProgramId = null)
    {
        IQueryable<Student> query = _context.Students;
        if (studyProgramId is not null)
        {
            query = query.Where(s => s.StudyGroup != null && s.StudyGroup.StudyProgramId == studyProgramId);
        }

        var rows = await query
            .GroupBy(s => new
            {
                Course = s.StudyGroup == null ? (int?)null : s.StudyGroup.Course,
                s.Status,
                s.FundingBasis
            })
            .Select(g => new ContingentSummaryRow
            {
                Course = g.Key.Course,
                Status = g.Key.Status,
                FundingBasis = g.Key.FundingBasis,
                Count = g.Count()
            })
            .ToListAsync();

        return rows
            .OrderBy(r => r.Course)
            .ThenBy(r => r.Status)
            .ThenBy(r => r.FundingBasis)
            .ToList();
    }

    /// <summary>
    /// Добавляет карточку. Новая карточка всегда «не зачислен» и без группы:
    /// группа и статус назначаются только приказом о зачислении.
    /// </summary>
    public async Task AddAsync(Student student)
    {
        if (student.Status != StudentStatus.NotEnrolled || student.StudyGroupId != null)
        {
            throw new InvalidOperationException(
                "Новая карточка создаётся со статусом «не зачислен» и без группы. Зачисление проводится приказом.");
        }
        if (await _context.Students.AnyAsync(s => s.RecordBookNumber == student.RecordBookNumber))
        {
            throw new InvalidOperationException($"Номер зачётной книжки {student.RecordBookNumber} уже занят.");
        }

        student.StudyGroup = null;
        student.MovementEvents.Clear();
        // Льготные категории — существующие записи справочника, берём их из базы по Id.
        student.Benefits = await LoadBenefitsAsync(student.Benefits.Select(b => b.Id).ToList());

        _context.Students.Add(student);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Обновляет карточку и синхронизирует коллекции сведений (как UpdateAsync у Person):
    /// новые элементы добавляются, существующие обновляются, отсутствующие удаляются.
    /// Статус, группа и история через этот метод не меняются.
    /// </summary>
    /// <returns>false, если студент не найден.</returns>
    public async Task<bool> UpdateAsync(Student student)
    {
        var existStudent = await GetByIdAsync(student.Id);
        if (existStudent == null)
        {
            return false;
        }

        var status = existStudent.Status;
        var groupId = existStudent.StudyGroupId;
        _context.Entry(existStudent).CurrentValues.SetValues(student);
        // Защищаем правила движения контингента от прямого изменения.
        existStudent.Status = status;
        existStudent.StudyGroupId = groupId;

        SyncChildren(existStudent.Contacts, student.Contacts,
            c => { c.StudentId = existStudent.Id; c.Student = existStudent; });
        SyncChildren(existStudent.IdentityDocuments, student.IdentityDocuments,
            d => { d.StudentId = existStudent.Id; d.Student = existStudent; });
        SyncChildren(existStudent.PreviousEducations, student.PreviousEducations,
            p => { p.StudentId = existStudent.Id; p.Student = existStudent; });
        SyncChildren(existStudent.Representatives, student.Representatives,
            r => { r.StudentId = existStudent.Id; r.Student = existStudent; });

        var benefitIds = student.Benefits.Select(b => b.Id).ToHashSet();
        existStudent.Benefits.RemoveAll(b => !benefitIds.Contains(b.Id));
        var missingIds = benefitIds.Except(existStudent.Benefits.Select(b => b.Id)).ToList();
        existStudent.Benefits.AddRange(await LoadBenefitsAsync(missingIds));

        await _context.SaveChangesAsync();
        return true;
    }

    /// <returns>false, если студент не найден.</returns>
    public async Task<bool> DeleteAsync(Guid id)
    {
        Student? student = await _context.Students.FindAsync(id);
        if (student == null)
        {
            return false;
        }
        _context.Remove(student);
        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>Проводит одно событие движения.</summary>
    public Task ApplyMovementAsync(MovementEvent movement)
    {
        return ApplyMovementsAsync(new[] { movement });
    }

    /// <summary>
    /// Проводит события движения (например, все события одного приказа).
    /// Выполняется по принципу «всё или ничего»: если хотя бы один переход
    /// недопустим, не сохраняется ни одно событие.
    /// </summary>
    public async Task ApplyMovementsAsync(IReadOnlyCollection<MovementEvent> movements)
    {
        if (movements.Count == 0)
        {
            return;
        }

        try
        {
            var orderIds = movements.Select(m => m.OrderId).Distinct().ToList();
            var foundOrders = await _context.Orders.CountAsync(o => orderIds.Contains(o.Id));
            if (foundOrders != orderIds.Count)
            {
                throw new KeyNotFoundException("Приказ-основание не найден.");
            }

            var groupIds = movements
                .Where(m => m.ToGroupId.HasValue)
                .Select(m => m.ToGroupId!.Value)
                .Distinct()
                .ToList();
            if (groupIds.Count > 0)
            {
                var foundGroups = await _context.StudyGroups.CountAsync(g => groupIds.Contains(g.Id));
                if (foundGroups != groupIds.Count)
                {
                    throw new KeyNotFoundException("Группа не найдена.");
                }
            }

            var studentIds = movements.Select(m => m.StudentId).Distinct().ToList();
            var students = await _context.Students
                .Where(s => studentIds.Contains(s.Id))
                .ToDictionaryAsync(s => s.Id);

            foreach (var movement in movements.OrderBy(m => m.EffectiveDate))
            {
                if (!students.TryGetValue(movement.StudentId, out var student))
                {
                    throw new KeyNotFoundException($"Студент {movement.StudentId} не найден.");
                }
                student.Apply(movement);
                _context.MovementEvents.Add(movement);
            }

            await _context.SaveChangesAsync();
        }
        catch
        {
            // Ничего из частично применённых изменений не должно попасть в базу позже.
            _context.ChangeTracker.Clear();
            throw;
        }
    }

    public void ChangeTrackerClear()
    {
        _context.ChangeTracker.Clear();
    }

    private IQueryable<Student> WithDetails()
    {
        return _context.Students
            .Include(s => s.StudyGroup)
            .Include(s => s.Contacts)
            .Include(s => s.IdentityDocuments)
            .Include(s => s.PreviousEducations)
            .Include(s => s.Representatives)
            .Include(s => s.Benefits)
            .AsSplitQuery();
    }

    private async Task<List<BenefitCategory>> LoadBenefitsAsync(List<Guid> ids)
    {
        if (ids.Count == 0)
        {
            return new List<BenefitCategory>();
        }
        return await _context.BenefitCategories.Where(b => ids.Contains(b.Id)).ToListAsync();
    }

    /// <summary>
    /// Синхронизирует коллекцию отслеживаемой сущности с пришедшей коллекцией по Id.
    /// </summary>
    private void SyncChildren<T>(List<T> existing, List<T> incoming, Action<T> attachToParent)
        where T : class, IEntity
    {
        foreach (var item in incoming)
        {
            var current = existing.FirstOrDefault(e => e.Id == item.Id);
            if (current == null)
            {
                attachToParent(item);
                existing.Add(item);
                _context.Entry(item).State = EntityState.Added;
            }
            else
            {
                _context.Entry(current).CurrentValues.SetValues(item);
            }
        }

        foreach (var current in existing.Where(e => incoming.All(i => i.Id != e.Id)).ToList())
        {
            _context.Remove(current);
        }
    }
}
