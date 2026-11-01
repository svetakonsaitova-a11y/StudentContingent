using Microsoft.AspNetCore.Mvc;
using StudentContingent.Domain.Model;
using StudentContingent.Infrastructure.Repository;

namespace StudentContingent.API.Controllers;

/// <summary>Общий CRUD для простых справочников.</summary>
[ApiController]
public abstract class ReferenceController<T> : ControllerBase where T : class, IEntity
{
    private readonly ReferenceRepository<T> _repository;

    protected ReferenceController(ReferenceRepository<T> repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<ActionResult<List<T>>> GetAll()
    {
        return await _repository.GetAllAsync();
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<T>> GetById(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
        {
            return NotFound();
        }
        return entity;
    }

    [HttpPost]
    public async Task<ActionResult<T>> Create(T entity)
    {
        await _repository.AddAsync(entity);
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, T entity)
    {
        if (id != entity.Id)
        {
            return BadRequest("Id в адресе и в теле запроса не совпадают.");
        }
        return await _repository.UpdateAsync(entity) ? NoContent() : NotFound();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        return await _repository.DeleteAsync(id) ? NoContent() : NotFound();
    }
}

[Route("api/departments")]
public class DepartmentsController : ReferenceController<Department>
{
    public DepartmentsController(ReferenceRepository<Department> repository) : base(repository)
    {
    }
}

[Route("api/programs")]
public class StudyProgramsController : ReferenceController<StudyProgram>
{
    public StudyProgramsController(ReferenceRepository<StudyProgram> repository) : base(repository)
    {
    }
}

[Route("api/benefits")]
public class BenefitCategoriesController : ReferenceController<BenefitCategory>
{
    public BenefitCategoriesController(ReferenceRepository<BenefitCategory> repository) : base(repository)
    {
    }
}
