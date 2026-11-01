using Microsoft.AspNetCore.Mvc;
using StudentContingent.API.Models;
using StudentContingent.Domain.Model;
using StudentContingent.Infrastructure.Repository;

namespace StudentContingent.API.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly OrderRepository _orders;
    private readonly StudentRepository _students;

    public OrdersController(OrderRepository orders, StudentRepository students)
    {
        _orders = orders;
        _students = students;
    }

    [HttpGet]
    public async Task<ActionResult<List<Order>>> GetAll()
    {
        return await _orders.GetAllAsync();
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Order>> GetById(Guid id)
    {
        var order = await _orders.GetByIdAsync(id);
        if (order == null)
        {
            return NotFound();
        }
        return order;
    }

    /// <summary>Регистрация приказа.</summary>
    [HttpPost]
    public async Task<ActionResult<Order>> Create(Order order)
    {
        await _orders.AddAsync(order);
        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, Order order)
    {
        if (id != order.Id)
        {
            return BadRequest("Id в адресе и в теле запроса не совпадают.");
        }
        return await _orders.UpdateAsync(order) ? NoContent() : NotFound();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        return await _orders.DeleteAsync(id) ? NoContent() : NotFound();
    }

    /// <summary>
    /// Проведение приказа: события для нескольких студентов.
    /// Если хотя бы одно событие недопустимо, не проводится ни одно.
    /// </summary>
    [HttpPost("{id:guid}/movements")]
    public async Task<IActionResult> ApplyMovements(Guid id, List<OrderMovementRequest> requests)
    {
        var movements = requests
            .Select(r => new MovementEvent
            {
                OrderId = id,
                StudentId = r.StudentId,
                EventType = r.EventType,
                EffectiveDate = r.EffectiveDate,
                ToGroupId = r.ToGroupId
            })
            .ToList();

        await _students.ApplyMovementsAsync(movements);
        return NoContent();
    }
}
