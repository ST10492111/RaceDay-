using Microsoft.EntityFrameworkCore;
using RaceDay.API.Models;

namespace RaceDay.API.Data
{
    public class RaceDayDbContext : DbContext
    {
        public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options) : base(options)
        {
        }

        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Enrolment> Enrolments { get; set; }
        public DbSet<Result> Results { get; set; }
        public DbSet<EventImage> EventImages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // AppUser
            modelBuilder.Entity<AppUser>(entity =>
            {
                entity.ToTable("AppUser");
                entity.HasKey(e => e.UserId);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Role).IsRequired().HasMaxLength(20);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);
            });

            // Event
            modelBuilder.Entity<Event>(entity =>
            {
                entity.ToTable("Event");
                entity.HasKey(e => e.EventId);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.EventType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Distance).HasColumnType("decimal(8,2)");
                entity.HasOne(e => e.Organiser)
                      .WithMany()
                      .HasForeignKey(e => e.OrganiserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Category
            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("Category");
                entity.HasKey(e => e.CategoryId);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Distance).HasColumnType("decimal(8,2)");
                entity.Property(e => e.EntryFee).HasColumnType("decimal(10,2)");
                entity.HasOne(e => e.Event)
                      .WithMany(e => e.Categories)
                      .HasForeignKey(e => e.EventId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Enrolment
            modelBuilder.Entity<Enrolment>(entity =>
            {
                entity.ToTable("Enrolment");
                entity.HasKey(e => e.EnrolmentId);
                entity.HasOne(e => e.Participant)
                      .WithMany()
                      .HasForeignKey(e => e.ParticipantId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Event)
                      .WithMany()
                      .HasForeignKey(e => e.EventId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Category)
                      .WithMany()
                      .HasForeignKey(e => e.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(e => new { e.ParticipantId, e.EventId }).IsUnique();
            });

            // Result
            modelBuilder.Entity<Result>(entity =>
            {
                entity.ToTable("Result");
                entity.HasKey(e => e.ResultId);
                entity.HasOne(e => e.Enrolment)
                      .WithOne(e => e.Result)
                      .HasForeignKey<Result>(e => e.EnrolmentId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(e => e.EnrolmentId).IsUnique();
            });

            // EventImage
            modelBuilder.Entity<EventImage>(entity =>
            {
                entity.ToTable("EventImage");
                entity.HasKey(e => e.ImageId);
                entity.HasOne(e => e.Event)
                      .WithMany(e => e.Images)
                      .HasForeignKey(e => e.EventId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}