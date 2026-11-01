namespace StudentContingent.Domain.Model;

/// <summary>«Образовательная программа» — направление подготовки с профилем.</summary>
public class StudyProgram : IEntity
{
    public Guid Id { get; set; }
    /// <summary>Код направления по ОКСО, например 09.03.01.</summary>
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Profile { get; set; } = string.Empty;
    /// <summary>Уровень: бакалавриат, специалитет, магистратура.</summary>
    public string Level { get; set; } = string.Empty;
    /// <summary>Форма обучения: очная, очно-заочная, заочная.</summary>
    public string StudyForm { get; set; } = string.Empty;
    /// <summary>Нормативный срок обучения, лет.</summary>
    public double DurationYears { get; set; }

    public Guid DepartmentId { get; set; }
    public Department Department { get; set; } = null!;

    public List<StudyGroup> Groups { get; set; } = new List<StudyGroup>();
}
