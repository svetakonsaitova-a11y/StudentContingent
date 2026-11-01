namespace StudentContingent.Domain.Model;

/// <summary>Статус студента.</summary>
public enum StudentStatus
{
    /// <summary>Не зачислен (карточка создана, приказа о зачислении ещё нет).</summary>
    NotEnrolled,
    /// <summary>Обучается.</summary>
    Studying,
    /// <summary>Академический отпуск.</summary>
    AcademicLeave,
    /// <summary>Отчислен.</summary>
    Expelled,
    /// <summary>Выпускник.</summary>
    Graduated
}

/// <summary>Тип события движения контингента.</summary>
public enum MovementType
{
    /// <summary>Зачисление.</summary>
    Enrollment,
    /// <summary>Перевод на следующий курс.</summary>
    CourseTransfer,
    /// <summary>Перевод в другую группу.</summary>
    GroupTransfer,
    /// <summary>Предоставление академического отпуска.</summary>
    AcademicLeave,
    /// <summary>Выход из академического отпуска.</summary>
    ReturnFromLeave,
    /// <summary>Отчисление.</summary>
    Expulsion,
    /// <summary>Восстановление.</summary>
    Reinstatement,
    /// <summary>Выпуск.</summary>
    Graduation
}

/// <summary>Основа обучения.</summary>
public enum FundingBasis
{
    /// <summary>Бюджет.</summary>
    Budget,
    /// <summary>Договор об оказании платных услуг.</summary>
    Contract,
    /// <summary>Целевое обучение.</summary>
    Targeted
}

/// <summary>Вид контактной информации.</summary>
public enum ContactType
{
    Phone,
    Email,
    Address
}
