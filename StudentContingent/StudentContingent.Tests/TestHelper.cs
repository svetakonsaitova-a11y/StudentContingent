using Microsoft.EntityFrameworkCore;
using StudentContingent.Domain.Model;
using StudentContingent.Infrastructure.Data;
using StudentContingent.Infrastructure.Repository;

// Все тесты работают с одной тестовой базой, поэтому выполняем их последовательно.
[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace StudentContingent.Tests;

public class TestHelper
{
    private readonly Context _context;

    public Guid GroupAId { get; }
    public Guid GroupBId { get; }
    public Guid OrderId { get; }
    public Guid BenefitId { get; }

    public TestHelper()
    {
        //Используем обычную базу данных, не в памяти
        //Имя тестовой базы данных должно отличаться от базы данных проекта
        var contextOptions = new DbContextOptionsBuilder<Context>()
            .UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=StudentContingentTest;Integrated Security=true;TrustServerCertificate=true")
            .Options;

        _context = new Context(contextOptions);

        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();

        var department = new Department
        {
            Name = "Институт информационных технологий",
            ShortName = "ИИТ",
            Type = "Институт"
        };
        var program = new StudyProgram
        {
            Code = "09.03.01",
            Name = "Информатика и вычислительная техника",
            Profile = "Программное обеспечение",
            Level = "Бакалавриат",
            StudyForm = "Очная",
            DurationYears = 4,
            Department = department
        };
        var groupA = new StudyGroup { Number = "ИВТ-21", Course = 2, EnrollmentYear = 2025, StudyProgram = program };
        var groupB = new StudyGroup { Number = "ИВТ-22", Course = 2, EnrollmentYear = 2025, StudyProgram = program };
        var order = new Order
        {
            Number = "15-ст",
            Date = new DateTime(2026, 9, 1),
            OrderType = "О движении контингента",
            SignedBy = "Ректор"
        };
        var benefit = new BenefitCategory { Name = "Сирота", Description = "Дети-сироты и оставшиеся без попечения родителей" };

        //Значение идентификатора явно не задаем.
        //Используем для идентификации уникальный в рамках теста номер зачётной книжки
        var student = new Student
        {
            LastName = "Иванов",
            FirstName = "Иван",
            MiddleName = "Иванович",
            BirthDate = new DateTime(2006, 5, 14),
            Gender = "М",
            Citizenship = "РФ",
            RecordBookNumber = "21-0001",
            Status = StudentStatus.Studying,
            FundingBasis = FundingBasis.Budget,
            StudyGroup = groupA
        };
        student.AddContact(new ContactInfo { Type = ContactType.Phone, Value = "+7 900 111-11-11", IsPrimary = true });
        student.AddContact(new ContactInfo { Type = ContactType.Email, Value = "ivanov@example.ru" });

        _context.AddRange(student, groupB, order, benefit);
        _context.SaveChanges();

        GroupAId = groupA.Id;
        GroupBId = groupB.Id;
        OrderId = order.Id;
        BenefitId = benefit.Id;

        //Запрещаем отслеживание (разрываем связи с БД)
        _context.ChangeTracker.Clear();
    }

    public StudentRepository StudentRepository
    {
        get
        {
            return new StudentRepository(_context);
        }
    }

    public StudyGroupRepository StudyGroupRepository
    {
        get
        {
            return new StudyGroupRepository(_context);
        }
    }

    public OrderRepository OrderRepository
    {
        get
        {
            return new OrderRepository(_context);
        }
    }

    /// <summary>Новая карточка для тестов добавления.</summary>
    public static Student NewStudent(string lastName, string recordBookNumber)
    {
        return new Student
        {
            LastName = lastName,
            FirstName = "Пётр",
            BirthDate = new DateTime(2007, 1, 20),
            Gender = "М",
            Citizenship = "РФ",
            RecordBookNumber = recordBookNumber,
            FundingBasis = FundingBasis.Contract
        };
    }

    /// <summary>Событие движения по тестовому приказу.</summary>
    public MovementEvent Movement(Guid studentId, MovementType type, Guid? toGroupId = null)
    {
        return new MovementEvent
        {
            StudentId = studentId,
            OrderId = OrderId,
            EventType = type,
            EffectiveDate = new DateTime(2026, 10, 1),
            ToGroupId = toGroupId
        };
    }
}
