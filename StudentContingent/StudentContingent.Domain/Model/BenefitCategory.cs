namespace StudentContingent.Domain.Model;

/// <summary>«Льготная категория». Связь со студентами — многие ко многим.</summary>
public class BenefitCategory : IEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public List<Student> Students { get; set; } = new List<Student>();
}
