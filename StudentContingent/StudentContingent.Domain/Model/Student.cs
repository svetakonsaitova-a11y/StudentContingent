namespace StudentContingent.Domain.Model;

/// <summary>
/// «Студент» — корень агрегата. Статус и группа меняются только через
/// событие движения контингента (метод <see cref="Apply"/>).
/// </summary>
public class Student : IEntity
{
    public Guid Id { get; set; }
    public string LastName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string MiddleName { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string Citizenship { get; set; } = string.Empty;
    /// <summary>Номер зачётной книжки, уникален в пределах вуза.</summary>
    public string RecordBookNumber { get; set; } = string.Empty;
    public StudentStatus Status { get; set; } = StudentStatus.NotEnrolled;
    public FundingBasis FundingBasis { get; set; } = FundingBasis.Budget;

    /// <summary>Текущая группа; отсутствует у не зачисленных, в академическом отпуске, отчисленных и выпускников.</summary>
    public Guid? StudyGroupId { get; set; }
    public StudyGroup? StudyGroup { get; set; }

    public List<ContactInfo> Contacts { get; set; } = new List<ContactInfo>();
    public List<IdentityDocument> IdentityDocuments { get; set; } = new List<IdentityDocument>();
    public List<PreviousEducation> PreviousEducations { get; set; } = new List<PreviousEducation>();
    public List<Representative> Representatives { get; set; } = new List<Representative>();
    public List<BenefitCategory> Benefits { get; set; } = new List<BenefitCategory>();
    public List<MovementEvent> MovementEvents { get; set; } = new List<MovementEvent>();

    /// <summary>ФИО одной строкой (в базу не сохраняется).</summary>
    public string FullName => string.IsNullOrWhiteSpace(MiddleName)
        ? $"{LastName} {FirstName}"
        : $"{LastName} {FirstName} {MiddleName}";

    public int ContactCount => Contacts.Count;

    public void AddContact(ContactInfo contact)
    {
        Contacts.Add(contact);
    }

    public void RemoveContactAt(int index)
    {
        Contacts.RemoveAt(index);
    }

    /// <summary>Допустимо ли событие при текущем статусе студента.</summary>
    public bool CanApply(MovementType type) => GetTargetStatus(Status, type) != null;

    /// <summary>
    /// Таблица допустимых переходов статуса (раздел «Концептуальные ограничения»).
    /// Возвращает новый статус или null, если переход недопустим.
    /// </summary>
    public static StudentStatus? GetTargetStatus(StudentStatus current, MovementType type) => (current, type) switch
    {
        (StudentStatus.NotEnrolled, MovementType.Enrollment) => StudentStatus.Studying,
        (StudentStatus.Studying, MovementType.CourseTransfer) => StudentStatus.Studying,
        (StudentStatus.Studying, MovementType.GroupTransfer) => StudentStatus.Studying,
        (StudentStatus.Studying, MovementType.AcademicLeave) => StudentStatus.AcademicLeave,
        (StudentStatus.AcademicLeave, MovementType.ReturnFromLeave) => StudentStatus.Studying,
        (StudentStatus.Studying, MovementType.Expulsion) => StudentStatus.Expelled,
        (StudentStatus.AcademicLeave, MovementType.Expulsion) => StudentStatus.Expelled,
        (StudentStatus.Expelled, MovementType.Reinstatement) => StudentStatus.Studying,
        (StudentStatus.Studying, MovementType.Graduation) => StudentStatus.Graduated,
        _ => null
    };

    /// <summary>Требует ли событие указания новой группы.</summary>
    public static bool RequiresTargetGroup(MovementType type) =>
        type is MovementType.Enrollment
            or MovementType.CourseTransfer
            or MovementType.GroupTransfer
            or MovementType.ReturnFromLeave
            or MovementType.Reinstatement;

    /// <summary>
    /// Применяет событие движения: проверяет допустимость, заполняет событие
    /// (статус и группа «до» и «после»), меняет статус и группу студента
    /// и добавляет событие в историю.
    /// </summary>
    /// <exception cref="InvalidOperationException">Переход недопустим.</exception>
    public void Apply(MovementEvent movement)
    {
        ArgumentNullException.ThrowIfNull(movement);

        var target = GetTargetStatus(Status, movement.EventType)
            ?? throw new InvalidOperationException(
                $"Событие {movement.EventType} недопустимо для студента {FullName} со статусом {Status}.");

        Guid? newGroupId = null;
        if (RequiresTargetGroup(movement.EventType))
        {
            if (movement.ToGroupId is null)
            {
                throw new InvalidOperationException($"Для события {movement.EventType} нужно указать новую группу.");
            }
            if (movement.ToGroupId == StudyGroupId)
            {
                throw new InvalidOperationException("Новая группа совпадает с текущей.");
            }
            newGroupId = movement.ToGroupId;
        }

        movement.StudentId = Id;
        movement.PreviousStatus = Status;
        movement.NewStatus = target;
        movement.FromGroupId = StudyGroupId;
        movement.ToGroupId = newGroupId;

        Status = target;
        StudyGroupId = newGroupId;
        MovementEvents.Add(movement);
    }
}
