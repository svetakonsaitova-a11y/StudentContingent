namespace StudentContingent.Domain.Model;

/// <summary>
/// Общий признак сущности доменной модели: глобально уникальный идентификатор.
/// </summary>
public interface IEntity
{
    Guid Id { get; set; }
}
