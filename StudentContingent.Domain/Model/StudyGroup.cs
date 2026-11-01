namespace StudentContingent.Domain.Model;

/// <summary>«Академическая группа» — основная единица учёта студентов.</summary>
public class StudyGroup : IEntity
{
    public Guid Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public int Course { get; set; }
    public int EnrollmentYear { get; set; }

    public Guid StudyProgramId { get; set; }
    public StudyProgram StudyProgram { get; set; } = null!;

    /// <summary>Учётная запись куратора (может отсутствовать).</summary>
    public Guid? CuratorUserId { get; set; }

    /// <summary>Текущий состав группы.</summary>
    public List<Student> Students { get; set; } = new List<Student>();
}
