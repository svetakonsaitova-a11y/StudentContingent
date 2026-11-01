using StudentContingent.Domain.Model;

namespace StudentContingent.Tests;

/// <summary>Тесты правил доменной модели. База данных не нужна.</summary>
public class TestStudentDomain
{
    [Theory]
    [InlineData(StudentStatus.NotEnrolled, MovementType.Enrollment, true)]
    [InlineData(StudentStatus.NotEnrolled, MovementType.GroupTransfer, false)]
    [InlineData(StudentStatus.Studying, MovementType.GroupTransfer, true)]
    [InlineData(StudentStatus.Studying, MovementType.AcademicLeave, true)]
    [InlineData(StudentStatus.Studying, MovementType.Reinstatement, false)]
    [InlineData(StudentStatus.AcademicLeave, MovementType.ReturnFromLeave, true)]
    [InlineData(StudentStatus.AcademicLeave, MovementType.GroupTransfer, false)]
    [InlineData(StudentStatus.Expelled, MovementType.Reinstatement, true)]
    [InlineData(StudentStatus.Expelled, MovementType.GroupTransfer, false)]
    [InlineData(StudentStatus.Graduated, MovementType.Reinstatement, false)]
    public void TestCanApply(StudentStatus status, MovementType type, bool expected)
    {
        var student = new Student { Status = status };

        Assert.Equal(expected, student.CanApply(type));
    }

    [Fact]
    public void TestApplyGroupTransfer()
    {
        var groupA = Guid.NewGuid();
        var groupB = Guid.NewGuid();
        var student = new Student { Id = Guid.NewGuid(), Status = StudentStatus.Studying, StudyGroupId = groupA };
        var movement = new MovementEvent { EventType = MovementType.GroupTransfer, ToGroupId = groupB };

        student.Apply(movement);

        Assert.Equal(groupB, student.StudyGroupId);
        Assert.Equal(groupA, movement.FromGroupId);
        Assert.Equal(student.Id, movement.StudentId);
        Assert.Single(student.MovementEvents);
    }

    [Fact]
    public void TestTransferToSameGroupIsRejected()
    {
        var groupA = Guid.NewGuid();
        var student = new Student { Status = StudentStatus.Studying, StudyGroupId = groupA };

        Assert.Throws<InvalidOperationException>(() =>
            student.Apply(new MovementEvent { EventType = MovementType.GroupTransfer, ToGroupId = groupA }));
    }

    [Fact]
    public void TestEnrollmentWithoutGroupIsRejected()
    {
        var student = new Student();

        Assert.Throws<InvalidOperationException>(() =>
            student.Apply(new MovementEvent { EventType = MovementType.Enrollment }));
        Assert.Equal(StudentStatus.NotEnrolled, student.Status);
        Assert.Empty(student.MovementEvents);
    }

    [Fact]
    public void TestExpulsionClearsGroup()
    {
        var groupA = Guid.NewGuid();
        var student = new Student { Status = StudentStatus.Studying, StudyGroupId = groupA };
        var movement = new MovementEvent { EventType = MovementType.Expulsion, ToGroupId = Guid.NewGuid() };

        student.Apply(movement);

        Assert.Equal(StudentStatus.Expelled, student.Status);
        Assert.Null(student.StudyGroupId);
        Assert.Null(movement.ToGroupId);
        Assert.Equal(StudentStatus.Studying, movement.PreviousStatus);
    }
}
