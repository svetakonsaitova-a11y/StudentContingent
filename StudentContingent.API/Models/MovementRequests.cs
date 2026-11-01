using StudentContingent.Domain.Model;

namespace StudentContingent.API.Models;

/// <summary>Запрос на проведение события для одного студента.</summary>
public class MovementRequest
{
    public Guid OrderId { get; set; }
    public MovementType EventType { get; set; }
    public DateTime EffectiveDate { get; set; }
    /// <summary>Новая группа: нужна для зачисления, перевода, выхода из отпуска и восстановления.</summary>
    public Guid? ToGroupId { get; set; }
}

/// <summary>Строка приказа: событие для конкретного студента.</summary>
public class OrderMovementRequest
{
    public Guid StudentId { get; set; }
    public MovementType EventType { get; set; }
    public DateTime EffectiveDate { get; set; }
    public Guid? ToGroupId { get; set; }
}
