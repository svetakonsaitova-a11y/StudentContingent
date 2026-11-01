using Microsoft.AspNetCore.Mvc;
using StudentContingent.API.Models;
using StudentContingent.Domain.Model;
using StudentContingent.Infrastructure.Repository;

namespace StudentContingent.API.Controllers;

[ApiController]
[Route("api/students")]
public class StudentsController : ControllerBase
{
    private readonly StudentRepository _repository;

    public StudentsController(StudentRepository repository)
    {
        _repository = repository;
    }

    /// <summary>Поиск и фильтрация студентов.</summary>
    [HttpGet]
    public async Task<ActionResult<List<Student>>> Search([FromQuery] StudentFilter filter)
    {
        return await _repository.SearchAsync(filter);
    }

    /// <summary>Карточка студента со всеми сведениями.</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Student>> GetById(Guid id)
    {
        var student = await _repository.GetByIdAsync(id);
        if (student == null)
        {
            return NotFound();
        }
        return student;
    }

    [HttpGet("by-record-book/{recordBookNumber}")]
    public async Task<ActionResult<Student>> GetByRecordBookNumber(string recordBookNumber)
    {
        var student = await _repository.GetByRecordBookNumberAsync(recordBookNumber);
        if (student == null)
        {
            return NotFound();
        }
        return student;
    }

    /// <summary>История движения студента.</summary>
    [HttpGet("{id:guid}/history")]
    public async Task<ActionResult<List<MovementEvent>>> GetHistory(Guid id)
    {
        return await _repository.GetHistoryAsync(id);
    }

    /// <summary>Сводка по контингенту.</summary>
    [HttpGet("summary")]
    public async Task<ActionResult<List<ContingentSummaryRow>>> GetSummary([FromQuery] Guid? studyProgramId)
    {
        return await _repository.GetSummaryAsync(studyProgramId);
    }

    /// <summary>Новая карточка (статус «не зачислен», без группы).</summary>
    [HttpPost]
    public async Task<ActionResult<Student>> Create(Student student)
    {
        await _repository.AddAsync(student);
        return CreatedAtAction(nameof(GetById), new { id = student.Id }, student);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, Student student)
    {
        if (id != student.Id)
        {
            return BadRequest("Id в адресе и в теле запроса не совпадают.");
        }
        return await _repository.UpdateAsync(student) ? NoContent() : NotFound();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        return await _repository.DeleteAsync(id) ? NoContent() : NotFound();
    }

    /// <summary>Проведение события движения по приказу.</summary>
    [HttpPost("{id:guid}/movements")]
    public async Task<IActionResult> ApplyMovement(Guid id, MovementRequest request)
    {
        await _repository.ApplyMovementAsync(new MovementEvent
        {
            StudentId = id,
            OrderId = request.OrderId,
            EventType = request.EventType,
            EffectiveDate = request.EffectiveDate,
            ToGroupId = request.ToGroupId
        });
        return NoContent();
    }
}
