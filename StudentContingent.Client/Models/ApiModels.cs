using StudentContingent.Domain.Model;

namespace StudentContingent.Client.Models;

/// <summary>Запрос на проведение события (совпадает с MovementRequest в API).</summary>
public class MovementRequest
{
    public Guid OrderId { get; set; }
    public MovementType EventType { get; set; }
    public DateTime EffectiveDate { get; set; }
    public Guid? ToGroupId { get; set; }
}

/// <summary>Строка сводки (совпадает с ContingentSummaryRow в API).</summary>
public class SummaryRow
{
    public int? Course { get; set; }
    public StudentStatus Status { get; set; }
    public FundingBasis FundingBasis { get; set; }
    public int Count { get; set; }
}

/// <summary>Тело ответа об ошибке (ProblemDetails).</summary>
public class ProblemInfo
{
    public string? Title { get; set; }
    public string? Detail { get; set; }
}
