namespace StudentContingent.Domain.Model;

/// <summary>«Контактная информация» студента.</summary>
public class ContactInfo : IEntity
{
    public Guid Id { get; set; }
    public ContactType Type { get; set; }
    public string Value { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }

    public Guid StudentId { get; set; }
    public Student Student { get; set; } = null!;
}
