namespace StudentContingent.Domain.Model;

/// <summary>«Документ, удостоверяющий личность».</summary>
public class IdentityDocument : IEntity
{
    public Guid Id { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string Series { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; }
    public string IssuedBy { get; set; } = string.Empty;

    public Guid StudentId { get; set; }
    public Student Student { get; set; } = null!;
}
