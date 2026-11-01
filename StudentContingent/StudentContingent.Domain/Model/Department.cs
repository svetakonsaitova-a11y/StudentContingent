namespace StudentContingent.Domain.Model;

/// <summary>«Подразделение» — институт или факультет.</summary>
public class Department : IEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ShortName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;

    public List<StudyProgram> Programs { get; set; } = new List<StudyProgram>();
}
