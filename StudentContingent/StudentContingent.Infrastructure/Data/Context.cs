using Microsoft.EntityFrameworkCore;
using StudentContingent.Domain.Model;

namespace StudentContingent.Infrastructure.Data;

public class Context : DbContext
{
    public Context(DbContextOptions<Context> options)
        : base(options)
    {
    }

    public DbSet<Department> Departments => Set<Department>();
    public DbSet<StudyProgram> StudyPrograms => Set<StudyProgram>();
    public DbSet<StudyGroup> StudyGroups => Set<StudyGroup>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<ContactInfo> Contacts => Set<ContactInfo>();
    public DbSet<IdentityDocument> IdentityDocuments => Set<IdentityDocument>();
    public DbSet<PreviousEducation> PreviousEducations => Set<PreviousEducation>();
    public DbSet<Representative> Representatives => Set<Representative>();
    public DbSet<BenefitCategory> BenefitCategories => Set<BenefitCategory>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<MovementEvent> MovementEvents => Set<MovementEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Справочники: удалять запись, на которую есть ссылки, нельзя (Restrict).
        modelBuilder.Entity<Department>(e =>
        {
            e.Property(d => d.Name).HasMaxLength(200);
            e.Property(d => d.ShortName).HasMaxLength(20);
            e.Property(d => d.Type).HasMaxLength(50);
        });

        modelBuilder.Entity<StudyProgram>(e =>
        {
            e.Property(p => p.Code).HasMaxLength(10);
            e.Property(p => p.Name).HasMaxLength(200);
            e.Property(p => p.Level).HasMaxLength(50);
            e.Property(p => p.StudyForm).HasMaxLength(50);
            e.HasOne(p => p.Department)
                .WithMany(d => d.Programs)
                .HasForeignKey(p => p.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<StudyGroup>(e =>
        {
            e.Property(g => g.Number).HasMaxLength(20);
            e.HasOne(g => g.StudyProgram)
                .WithMany(p => p.Groups)
                .HasForeignKey(g => g.StudyProgramId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Student>(e =>
        {
            e.Property(s => s.LastName).HasMaxLength(100);
            e.Property(s => s.FirstName).HasMaxLength(100);
            e.Property(s => s.MiddleName).HasMaxLength(100);
            e.Property(s => s.RecordBookNumber).HasMaxLength(20);
            e.HasIndex(s => s.RecordBookNumber).IsUnique();

            // Перечисления храним строками, чтобы таблицы читались без расшифровки.
            e.Property(s => s.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(s => s.FundingBasis).HasConversion<string>().HasMaxLength(20);

            e.HasOne(s => s.StudyGroup)
                .WithMany(g => g.Students)
                .HasForeignKey(s => s.StudyGroupId)
                .OnDelete(DeleteBehavior.Restrict);

            // Многие ко многим: связующую таблицу BenefitCategoryStudent EF Core создаёт сам.
            e.HasMany(s => s.Benefits)
                .WithMany(b => b.Students);

            // Сведения о студенте удаляются вместе с ним (по умолчанию Cascade).
            e.HasMany(s => s.Contacts).WithOne(c => c.Student).HasForeignKey(c => c.StudentId);
            e.HasMany(s => s.IdentityDocuments).WithOne(d => d.Student).HasForeignKey(d => d.StudentId);
            e.HasMany(s => s.PreviousEducations).WithOne(p => p.Student).HasForeignKey(p => p.StudentId);
            e.HasMany(s => s.Representatives).WithOne(r => r.Student).HasForeignKey(r => r.StudentId);
        });

        modelBuilder.Entity<ContactInfo>(e =>
        {
            e.Property(c => c.Type).HasConversion<string>().HasMaxLength(20);
        });

        modelBuilder.Entity<Order>(e =>
        {
            e.Property(o => o.Number).HasMaxLength(50);
        });

        modelBuilder.Entity<MovementEvent>(e =>
        {
            e.Property(m => m.EventType).HasConversion<string>().HasMaxLength(30);
            e.Property(m => m.PreviousStatus).HasConversion<string>().HasMaxLength(20);
            e.Property(m => m.NewStatus).HasConversion<string>().HasMaxLength(20);

            e.HasOne(m => m.Student)
                .WithMany(s => s.MovementEvents)
                .HasForeignKey(m => m.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(m => m.Order)
                .WithMany(o => o.MovementEvents)
                .HasForeignKey(m => m.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            // Две связи с одной таблицей групп: «из» и «в».
            e.HasOne(m => m.FromGroup)
                .WithMany()
                .HasForeignKey(m => m.FromGroupId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(m => m.ToGroup)
                .WithMany()
                .HasForeignKey(m => m.ToGroupId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
