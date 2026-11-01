using StudentContingent.Domain.Model;

namespace StudentContingent.Client.Services;

/// <summary>Русские подписи для перечислений доменной модели.</summary>
public static class Labels
{
    public static string Of(StudentStatus status) => status switch
    {
        StudentStatus.NotEnrolled => "Не зачислен",
        StudentStatus.Studying => "Обучается",
        StudentStatus.AcademicLeave => "Академический отпуск",
        StudentStatus.Expelled => "Отчислен",
        StudentStatus.Graduated => "Выпускник",
        _ => status.ToString()
    };

    public static string Of(MovementType type) => type switch
    {
        MovementType.Enrollment => "Зачисление",
        MovementType.CourseTransfer => "Перевод на следующий курс",
        MovementType.GroupTransfer => "Перевод в другую группу",
        MovementType.AcademicLeave => "Академический отпуск",
        MovementType.ReturnFromLeave => "Выход из академического отпуска",
        MovementType.Expulsion => "Отчисление",
        MovementType.Reinstatement => "Восстановление",
        MovementType.Graduation => "Выпуск",
        _ => type.ToString()
    };

    public static string Of(FundingBasis basis) => basis switch
    {
        FundingBasis.Budget => "Бюджет",
        FundingBasis.Contract => "Договор",
        FundingBasis.Targeted => "Целевое",
        _ => basis.ToString()
    };

    public static string Of(ContactType type) => type switch
    {
        ContactType.Phone => "Телефон",
        ContactType.Email => "E-mail",
        ContactType.Address => "Адрес",
        _ => type.ToString()
    };

    /// <summary>CSS-класс плашки статуса.</summary>
    public static string Css(StudentStatus status) => "badge badge-" + status.ToString().ToLowerInvariant();

    /// <summary>Понятное сообщение для ошибок сети (API не запущен и т.п.).</summary>
    public static string Error(Exception ex) => ex is ApiException
        ? ex.Message
        : "Не удалось связаться с API. Проверьте, что проект StudentContingent.API запущен. (" + ex.Message + ")";
}
