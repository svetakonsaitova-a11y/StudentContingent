using StudentContingent.Domain.Model;
using StudentContingent.Infrastructure.Repository;

namespace StudentContingent.Tests;

/// <summary>
/// Тесты репозитория на тестовой базе в LocalDB (SQL Server Express LocalDB
/// ставится вместе с Visual Studio, рабочая нагрузка «ASP.NET и веб-разработка»).
/// </summary>
public class TestStudentRepository
{
    [Fact]
    //Тест, проверяющий, что база данных создалась и заполнилась
    public async Task VoidTest()
    {
        var testHelper = new TestHelper();
        var studentRepository = testHelper.StudentRepository;

        Assert.Single(await studentRepository.GetAllAsync());
    }

    [Fact]
    public async Task TestAdd()
    {
        var testHelper = new TestHelper();
        var studentRepository = testHelper.StudentRepository;
        var student = TestHelper.NewStudent("Петров", "26-0001");
        student.Benefits.Add(new BenefitCategory { Id = testHelper.BenefitId });

        await studentRepository.AddAsync(student);
        //Запрещаем отслеживание сущностей (разрываем связи с БД)
        studentRepository.ChangeTrackerClear();

        Assert.Equal(2, (await studentRepository.GetAllAsync()).Count);
        var saved = await studentRepository.GetByRecordBookNumberAsync("26-0001");
        Assert.NotNull(saved);
        Assert.Equal(StudentStatus.NotEnrolled, saved.Status);
        Assert.Null(saved.StudyGroupId);
        Assert.Single(saved.Benefits);
    }

    [Fact]
    public async Task TestAddWithStatusIsRejected()
    {
        var testHelper = new TestHelper();
        var studentRepository = testHelper.StudentRepository;
        var student = TestHelper.NewStudent("Петров", "26-0001");
        student.Status = StudentStatus.Studying;

        await Assert.ThrowsAsync<InvalidOperationException>(() => studentRepository.AddAsync(student));
    }

    [Fact]
    public async Task TestAddDuplicateRecordBookIsRejected()
    {
        var testHelper = new TestHelper();
        var studentRepository = testHelper.StudentRepository;

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => studentRepository.AddAsync(TestHelper.NewStudent("Петров", "21-0001")));
    }

    [Fact]
    public async Task TestUpdateAdd()
    {
        var testHelper = new TestHelper();
        var studentRepository = testHelper.StudentRepository;
        var student = (await studentRepository.GetByRecordBookNumberAsync("21-0001"))!;
        //Запрещаем отслеживание сущностей (разрываем связи с БД)
        studentRepository.ChangeTrackerClear();
        student.LastName = "Иванов-Петров";
        student.AddContact(new ContactInfo { Type = ContactType.Address, Value = "г. Москва, ул. Ленина, 1" });

        await studentRepository.UpdateAsync(student);
        studentRepository.ChangeTrackerClear();

        var updated = (await studentRepository.GetByRecordBookNumberAsync("21-0001"))!;
        Assert.Equal("Иванов-Петров", updated.LastName);
        Assert.Equal(3, updated.ContactCount);
    }

    [Fact]
    public async Task TestUpdateDelete()
    {
        var testHelper = new TestHelper();
        var studentRepository = testHelper.StudentRepository;
        var student = (await studentRepository.GetByRecordBookNumberAsync("21-0001"))!;
        studentRepository.ChangeTrackerClear();
        student.RemoveContactAt(0);

        await studentRepository.UpdateAsync(student);
        studentRepository.ChangeTrackerClear();

        Assert.Equal(1, (await studentRepository.GetByRecordBookNumberAsync("21-0001"))!.ContactCount);
    }

    [Fact]
    public async Task TestUpdateCannotChangeStatusDirectly()
    {
        var testHelper = new TestHelper();
        var studentRepository = testHelper.StudentRepository;
        var student = (await studentRepository.GetByRecordBookNumberAsync("21-0001"))!;
        studentRepository.ChangeTrackerClear();
        student.Status = StudentStatus.Graduated;
        student.StudyGroupId = null;

        await studentRepository.UpdateAsync(student);
        studentRepository.ChangeTrackerClear();

        var updated = (await studentRepository.GetByRecordBookNumberAsync("21-0001"))!;
        Assert.Equal(StudentStatus.Studying, updated.Status);
        Assert.Equal(testHelper.GroupAId, updated.StudyGroupId);
    }

    [Fact]
    public async Task TestDelete()
    {
        var testHelper = new TestHelper();
        var studentRepository = testHelper.StudentRepository;
        var student = (await studentRepository.GetByRecordBookNumberAsync("21-0001"))!;
        studentRepository.ChangeTrackerClear();

        Assert.True(await studentRepository.DeleteAsync(student.Id));
        studentRepository.ChangeTrackerClear();

        Assert.Empty(await studentRepository.GetAllAsync());
    }

    [Fact]
    public async Task TestEnrollment()
    {
        var testHelper = new TestHelper();
        var studentRepository = testHelper.StudentRepository;
        var student = TestHelper.NewStudent("Петров", "26-0001");
        await studentRepository.AddAsync(student);
        studentRepository.ChangeTrackerClear();

        await studentRepository.ApplyMovementAsync(
            testHelper.Movement(student.Id, MovementType.Enrollment, testHelper.GroupBId));
        studentRepository.ChangeTrackerClear();

        var enrolled = (await studentRepository.GetByIdAsync(student.Id))!;
        Assert.Equal(StudentStatus.Studying, enrolled.Status);
        Assert.Equal(testHelper.GroupBId, enrolled.StudyGroupId);
    }

    [Fact]
    public async Task TestGroupTransfer()
    {
        var testHelper = new TestHelper();
        var studentRepository = testHelper.StudentRepository;
        var student = (await studentRepository.GetByRecordBookNumberAsync("21-0001"))!;
        studentRepository.ChangeTrackerClear();

        await studentRepository.ApplyMovementAsync(
            testHelper.Movement(student.Id, MovementType.GroupTransfer, testHelper.GroupBId));
        studentRepository.ChangeTrackerClear();

        Assert.Equal(testHelper.GroupBId, (await studentRepository.GetByIdAsync(student.Id))!.StudyGroupId);
        var history = await studentRepository.GetHistoryAsync(student.Id);
        var movement = Assert.Single(history);
        Assert.Equal(testHelper.GroupAId, movement.FromGroupId);
        Assert.Equal(testHelper.GroupBId, movement.ToGroupId);
        Assert.Equal(StudentStatus.Studying, movement.PreviousStatus);
        Assert.Equal(StudentStatus.Studying, movement.NewStatus);
    }

    [Fact]
    public async Task TestInvalidMovementIsNotSaved()
    {
        var testHelper = new TestHelper();
        var studentRepository = testHelper.StudentRepository;
        var student = (await studentRepository.GetByRecordBookNumberAsync("21-0001"))!;
        studentRepository.ChangeTrackerClear();

        //Восстановить можно только отчисленного студента
        await Assert.ThrowsAsync<InvalidOperationException>(() => studentRepository.ApplyMovementAsync(
            testHelper.Movement(student.Id, MovementType.Reinstatement, testHelper.GroupBId)));

        Assert.Empty(await studentRepository.GetHistoryAsync(student.Id));
        Assert.Equal(StudentStatus.Studying, (await studentRepository.GetByIdAsync(student.Id))!.Status);
    }

    [Fact]
    public async Task TestExpelAndReinstate()
    {
        var testHelper = new TestHelper();
        var studentRepository = testHelper.StudentRepository;
        var student = (await studentRepository.GetByRecordBookNumberAsync("21-0001"))!;
        studentRepository.ChangeTrackerClear();

        await studentRepository.ApplyMovementAsync(testHelper.Movement(student.Id, MovementType.Expulsion));
        studentRepository.ChangeTrackerClear();
        var expelled = (await studentRepository.GetByIdAsync(student.Id))!;
        Assert.Equal(StudentStatus.Expelled, expelled.Status);
        Assert.Null(expelled.StudyGroupId);
        studentRepository.ChangeTrackerClear();

        await studentRepository.ApplyMovementAsync(
            testHelper.Movement(student.Id, MovementType.Reinstatement, testHelper.GroupBId));
        studentRepository.ChangeTrackerClear();
        var reinstated = (await studentRepository.GetByIdAsync(student.Id))!;
        Assert.Equal(StudentStatus.Studying, reinstated.Status);
        Assert.Equal(testHelper.GroupBId, reinstated.StudyGroupId);
        Assert.Equal(2, (await studentRepository.GetHistoryAsync(student.Id)).Count);
    }

    [Fact]
    public async Task TestOrderIsAllOrNothing()
    {
        var testHelper = new TestHelper();
        var studentRepository = testHelper.StudentRepository;
        var ivanov = (await studentRepository.GetByRecordBookNumberAsync("21-0001"))!;
        var petrov = TestHelper.NewStudent("Петров", "26-0001");
        await studentRepository.AddAsync(petrov);
        studentRepository.ChangeTrackerClear();

        //Зачисление Петрова допустимо, восстановление обучающегося Иванова — нет
        var movements = new List<MovementEvent>
        {
            testHelper.Movement(petrov.Id, MovementType.Enrollment, testHelper.GroupAId),
            testHelper.Movement(ivanov.Id, MovementType.Reinstatement, testHelper.GroupBId)
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() => studentRepository.ApplyMovementsAsync(movements));

        Assert.Equal(StudentStatus.NotEnrolled, (await studentRepository.GetByIdAsync(petrov.Id))!.Status);
    }

    [Fact]
    public async Task TestSearchByGroup()
    {
        var testHelper = new TestHelper();
        var studentRepository = testHelper.StudentRepository;
        await studentRepository.AddAsync(TestHelper.NewStudent("Петров", "26-0001"));
        studentRepository.ChangeTrackerClear();

        var inGroupA = await studentRepository.SearchAsync(new StudentFilter { StudyGroupId = testHelper.GroupAId });
        var notEnrolled = await studentRepository.SearchAsync(new StudentFilter { Status = StudentStatus.NotEnrolled });

        Assert.Equal("Иванов", Assert.Single(inGroupA).LastName);
        Assert.Equal("Петров", Assert.Single(notEnrolled).LastName);
    }

    [Fact]
    public async Task TestSummary()
    {
        var testHelper = new TestHelper();
        var studentRepository = testHelper.StudentRepository;

        var summary = await studentRepository.GetSummaryAsync();

        var row = Assert.Single(summary);
        Assert.Equal(2, row.Course);
        Assert.Equal(StudentStatus.Studying, row.Status);
        Assert.Equal(1, row.Count);
    }

    [Fact]
    public async Task TestGroupWithStudentsCannotBeDeleted()
    {
        var testHelper = new TestHelper();
        var groupRepository = testHelper.StudyGroupRepository;

        await Assert.ThrowsAsync<InvalidOperationException>(() => groupRepository.DeleteAsync(testHelper.GroupAId));
        Assert.True(await groupRepository.DeleteAsync(testHelper.GroupBId));
    }
}
