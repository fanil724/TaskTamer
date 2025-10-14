using Microsoft.EntityFrameworkCore;
using TaskTamer_Logic.Models;

namespace TaskTamer_Persistence.DataAccess
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Department> Departments { get; set; }
        public DbSet<Position> Positions { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Equipment> Equipment { get; set; }
        public DbSet<RequestStatus> RequestStatuses { get; set; }
        public DbSet<RequestType> RequestTypes { get; set; }
        public DbSet<Request> Requests { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
        public DbSet<RequestHistory> RequestHistory { get; set; }
        public DbSet<AuthLog> AuthLogs { get; set; }
        public DbSet<EquipmentResponsible> EquipmentResponsibles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            
            modelBuilder.Entity<Department>()
                .HasIndex(d => d.Name)
                .IsUnique();

            modelBuilder.Entity<Position>()
                .HasIndex(p => p.Title)
                .IsUnique();

            modelBuilder.Entity<Equipment>()
                .HasIndex(e => e.SerialNumber)
                .IsUnique();

            modelBuilder.Entity<RequestStatus>()
                .HasIndex(rs => rs.Name)
                .IsUnique();

            modelBuilder.Entity<RequestType>()
                .HasIndex(rt => rt.Name)
                .IsUnique();

            modelBuilder.Entity<Role>()
                .HasIndex(r => r.Name)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.EmployeeID)
                .IsUnique();

            modelBuilder.Entity<PasswordResetToken>()
                .HasIndex(prt => prt.Token)
                .IsUnique();
            
            modelBuilder.Entity<EquipmentResponsible>()
                .HasIndex(er => new { er.EquipmentID, er.EmployeeID })
                .IsUnique();
            modelBuilder.Entity<Request>()
                .HasOne(r => r.Equipment)
                .WithMany(e => e.Requests)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RequestStatus>()
                .HasMany(r => r.Requests)
                .WithOne(r => r.RequestStatus)
                .HasForeignKey(r => r.RequestStatusID);

            modelBuilder.Entity<RequestStatus>()
                .HasMany(rs => rs.HistoryRecords)
                .WithOne(rh => rh.Status)
                .HasForeignKey(rh => rh.RequestStatusID);
            
            modelBuilder.Entity<Employee>()
                .HasMany(e => e.CreatedRequests) 
                .WithOne(r => r.Author) 
                .HasForeignKey(r => r.AuthorID); 
            
            modelBuilder.Entity<Employee>()
                .HasMany(e => e.ExecutedRequests) 
                .WithOne(r => r.Executor) 
                .HasForeignKey(r => r.ExecutorID); 
            
            modelBuilder.Entity<Request>()
                .HasMany(r => r.History) 
                .WithOne(h => h.Request) 
                .HasForeignKey(h => h.RequestID); 
                

            modelBuilder.Entity<AuthLog>(entity =>
            { 
                entity.HasKey(al => al.LogID);
            });
        }
    }
}