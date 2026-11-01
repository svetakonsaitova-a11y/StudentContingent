using StudentContingent.Domain.Model;

namespace StudentContingent.Infrastructure.Repository;

/// <summary>Условия поиска студентов. Пустое поле не участвует в отборе.</summary>
public class StudentFilter
{
    /// <summary>Начало фамилии.</summary>
    public string? LastName { get; set; }
    public string? RecordBookNumber { get; set; }
    public Guid? StudyGroupId { get; set; }
    public Guid? StudyProgramId { get; set; }
    public int? Course { get; set; }
    public StudentStatus? Status { get; set; }
    public FundingBasis? FundingBasis { get; set; }
}

/// <summary>Строка сводки по контингенту.</summary>
public class ContingentSummaryRow
{
    /// <summary>Курс; пусто для студентов без группы.</summary>
    public int? Course { get; set; }
    public StudentStatus Status { get; set; }
    public FundingBasis FundingBasis { get; set; }
    public int Count { get; set; }
}
