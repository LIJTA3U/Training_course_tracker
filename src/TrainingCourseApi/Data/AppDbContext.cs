using Microsoft.EntityFrameworkCore;
using TrainingCourseApi.Models;

namespace TrainingCourseApi.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Course> Courses => Set<Course>();

    public DbSet<Lesson> Lessons => Set<Lesson>();

    public DbSet<Enrollment> Enrollments => Set<Enrollment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Course>(entity =>
        {
            entity.ToTable("courses");
            entity.HasKey(c => c.Id);

            entity.Property(c => c.Title)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(c => c.Description)
                .HasMaxLength(2000);

            entity.Property(c => c.Instructor)
                .HasMaxLength(150)
                .IsRequired();

            // Храним enum как текст, чтобы значения оставались читаемыми в БД.
            entity.Property(c => c.Category)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            entity.HasIndex(c => c.Title);
            entity.HasIndex(c => c.Category);
        });

        modelBuilder.Entity<Lesson>(entity =>
        {
            entity.ToTable("lessons");
            entity.HasKey(l => l.Id);

            entity.Property(l => l.Title)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(l => l.Content)
                .HasMaxLength(4000);

            entity.HasOne(l => l.Course)
                .WithMany(c => c.Lessons)
                .HasForeignKey(l => l.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(l => l.CourseId);
        });

        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.ToTable("enrollments");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.StudentName)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(e => e.StudentEmail)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(e => e.Status)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            entity.HasOne(e => e.Course)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.CourseId);
        });
    }
}