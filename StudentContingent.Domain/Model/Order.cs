namespace StudentContingent.Domain.Model;

/// <summary>«Приказ» — основание для событий движения контингента.</summary>
public class Order : IEntity
{
    public Guid Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string OrderType { get; set; } = string.Empty;
    public string SignedBy { get; set; } = string.Empty;

    public List<MovementEvent> MovementEvents { get; set; } = new List<MovementEvent>();
}
