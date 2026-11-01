using Microsoft.AspNetCore.Mvc;
using StudentContingent.Domain.Model;
using StudentContingent.Infrastructure.Repository;

namespace StudentContingent.API.Controllers;

[ApiController]
[Route("api/groups")]
public class GroupsController : ControllerBase
{
    private readonly StudyGroupRepository _groups;
    private readonly StudentRepository _students;

    public GroupsController(StudyGroupRepository groups, StudentRepository students)
    {
        _groups = groups;
        _students = students;
    }

    /// <summary>Все группы или группы куратора.</summary>
    [HttpGet]
    public async Task<ActionResult<List<StudyGroup>>> GetAll([FromQuery] Guid? curatorUserId)
    {
        return curatorUserId is null
            ? await _groups.GetAllAsync()
            : await _groups.GetByCuratorAsync(curatorUserId.Value);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<StudyGroup>> GetById(Guid id)
    {
        var group = await _groups.GetByIdAsync(id);
        if (group == null)
        {
            return NotFound();
        }
        return group;
    }

    /// <summary>Список группы.</summary>
    [HttpGet("{id:guid}/students")]
    public async Task<ActionResult<List<Student>>> GetStudents(Guid id)
    {
        return await _students.GetByGroupAsync(id);
    }

    [HttpPost]
    public async Task<ActionResult<StudyGroup>> Create(StudyGroup group)
    {
        await _groups.AddAsync(group);
        return CreatedAtAction(nameof(GetById), new { id = group.Id }, group);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, StudyGroup group)
    {
        if (id != group.Id)
        {
            return BadRequest("Id в адресе и в теле запроса не совпадают.");
        }
        return await _groups.UpdateAsync(group) ? NoContent() : NotFound();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        return await _groups.DeleteAsync(id) ? NoContent() : NotFound();
    }
}
