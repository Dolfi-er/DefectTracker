using Microsoft.EntityFrameworkCore;
using Back.Models.Entities;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Back.Data
{
    public class AppDbContext : DbContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AppDbContext(DbContextOptions<AppDbContext> options, IHttpContextAccessor httpContextAccessor) : base(options) 
        {
            _httpContextAccessor = httpContextAccessor;
        }
        
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectStatus> ProjectStatuses { get; set; }
        public DbSet<Defect> Defects { get; set; }
        public DbSet<DefectStatus> DefectStatuses { get; set; }
        public DbSet<Info> Infos { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<DefectHistory> DefectHistories { get; set; }
        public DbSet<DefectAttachment> DefectAttachments { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Конфигурация User
            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
                
            // Конфигурация Project
            modelBuilder.Entity<Project>()
                .HasOne(p => p.ProjectStatus)
                .WithMany(ps => ps.Projects)
                .HasForeignKey(p => p.ProjectStatusId)
                .OnDelete(DeleteBehavior.Restrict);
                
            // Конфигурация Defect
            modelBuilder.Entity<Defect>()
                .HasOne(d => d.Project)
                .WithMany(p => p.Defects)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);
                
            modelBuilder.Entity<Defect>()
                .HasOne(d => d.Status)
                .WithMany(ds => ds.Defects)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.Restrict);
                
            modelBuilder.Entity<Defect>()
                .HasOne(d => d.Info)
                .WithOne(i => i.Defect)
                .HasForeignKey<Defect>(d => d.InfoId)
                .OnDelete(DeleteBehavior.Cascade);
                
            modelBuilder.Entity<Defect>()
                .HasOne(d => d.Responsible)
                .WithMany(u => u.ResponsibleDefects)
                .HasForeignKey(d => d.ResponsibleId)
                .OnDelete(DeleteBehavior.SetNull);
                
            modelBuilder.Entity<Defect>()
                .HasOne(d => d.CreatedBy)
                .WithMany()
                .HasForeignKey(d => d.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);
                
            // Конфигурация Comment
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Defect)
                .WithMany(d => d.Comments)
                .HasForeignKey(c => c.DefectId)
                .OnDelete(DeleteBehavior.Cascade);
                
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);
                
            // Конфигурация DefectHistory
            modelBuilder.Entity<DefectHistory>()
                .HasOne(dh => dh.Defect)
                .WithMany(d => d.History)
                .HasForeignKey(dh => dh.DefectId)
                .OnDelete(DeleteBehavior.Cascade);
                
            modelBuilder.Entity<DefectHistory>()
                .HasOne(dh => dh.User)
                .WithMany(u => u.DefectHistories)
                .HasForeignKey(dh => dh.UserId)
                .OnDelete(DeleteBehavior.Restrict);
                
            // Конфигурация DefectAttachment
            modelBuilder.Entity<DefectAttachment>()
                .HasOne(da => da.Defect)
                .WithMany(d => d.Attachments)
                .HasForeignKey(da => da.DefectId)
                .OnDelete(DeleteBehavior.Cascade);
                
            modelBuilder.Entity<DefectAttachment>()
                .HasOne(da => da.UploadedBy)
                .WithMany()
                .HasForeignKey(da => da.UploadedById)
                .OnDelete(DeleteBehavior.Restrict);
                
            // Заполнение начальных данных
            SeedData(modelBuilder);
        }

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            var defectEntries = ChangeTracker.Entries<Defect>()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted)
                .ToList();

            var historyEntries = new List<DefectHistory>();
            var now = DateTime.UtcNow;
            var currentUserId = GetCurrentUserId();

            // First, process deleted defects and add their history entries
            foreach (var entry in defectEntries.Where(e => e.State == EntityState.Deleted))
            {
                var defect = entry.Entity;
                
                // Log defect deletion
                historyEntries.Add(new DefectHistory
                {
                    DefectId = defect.DefectId,
                    UserId = currentUserId,
                    FieldName = "Defect",
                    OldValue = "Exists",
                    NewValue = "Deleted",
                    ChangeDate = now
                });
            }

            // Add history entries for deleted defects first
            if (historyEntries.Any())
            {
                DefectHistories.AddRange(historyEntries);
                await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
                historyEntries.Clear(); // Clear the list for the next batch
            }

            // Now process added and modified defects
            foreach (var entry in defectEntries.Where(e => e.State == EntityState.Added || e.State == EntityState.Modified))
            {
                var defect = entry.Entity;

                if (entry.State == EntityState.Added)
                {
                    // Log defect creation
                    historyEntries.Add(new DefectHistory
                    {
                        DefectId = defect.DefectId,
                        UserId = currentUserId,
                        FieldName = "Defect",
                        OldValue = "",
                        NewValue = "Created",
                        ChangeDate = now
                    });

                    // Log initial field values
                    LogInitialFieldValues(defect, currentUserId, now, historyEntries);
                }
                else if (entry.State == EntityState.Modified)
                {
                    // Log field changes
                    LogFieldChanges(entry, defect, currentUserId, now, historyEntries);
                }
            }

            // Save all changes (defect modifications/deletions and history for added/modified defects)
            var result = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);

            // Add remaining history entries (for added and modified defects)
            if (historyEntries.Any())
            {
                DefectHistories.AddRange(historyEntries);
                await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            }

            return result;
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            return SaveChangesAsync(acceptAllChangesOnSuccess).GetAwaiter().GetResult();
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = _httpContextAccessor?.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
            return 1; // Fallback to admin user if not authenticated (for testing)
        }

        private void LogInitialFieldValues(Defect defect, int userId, DateTime changeDate, List<DefectHistory> historyEntries)
        {
            var fieldsToLog = new[]
            {
                new { Field = "ProjectId", Value = defect.ProjectId.ToString() },
                new { Field = "StatusId", Value = defect.StatusId.ToString() },
                new { Field = "ResponsibleId", Value = defect.ResponsibleId?.ToString() ?? "None" },
                new { Field = "CreatedById", Value = defect.CreatedById.ToString() }
            };

            foreach (var field in fieldsToLog)
            {
                historyEntries.Add(new DefectHistory
                {
                    DefectId = defect.DefectId,
                    UserId = userId,
                    FieldName = field.Field,
                    OldValue = "",
                    NewValue = field.Value,
                    ChangeDate = changeDate
                });
            }
        }

        private void LogFieldChanges(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<Defect> entry, Defect defect, int userId, DateTime changeDate, List<DefectHistory> historyEntries)
        {
            var changedProperties = entry.Properties
                .Where(p => p.IsModified && !p.Metadata.IsPrimaryKey())
                .ToList();

            foreach (var property in changedProperties)
            {
                var propertyName = property.Metadata.Name;
                var originalValue = property.OriginalValue?.ToString() ?? "null";
                var currentValue = property.CurrentValue?.ToString() ?? "null";

                // Пропускаем логирование, если значения не изменились
                if (originalValue == currentValue)
                    continue;

                historyEntries.Add(new DefectHistory
                {
                    DefectId = defect.DefectId,
                    UserId = userId,
                    FieldName = propertyName,
                    OldValue = originalValue,
                    NewValue = currentValue,
                    ChangeDate = changeDate
                });
            }

            // Логирование изменения UpdatedDate
            if (entry.Property(x => x.UpdatedDate).IsModified)
            {
                historyEntries.Add(new DefectHistory
                {
                    DefectId = defect.DefectId,
                    UserId = userId,
                    FieldName = "UpdatedDate",
                    OldValue = entry.Property(x => x.UpdatedDate).OriginalValue.ToString(),
                    NewValue = defect.UpdatedDate.ToString(),
                    ChangeDate = changeDate
                });
            }
        }
        
        private void SeedData(ModelBuilder modelBuilder)
        {
            // Роли
            modelBuilder.Entity<Role>().HasData(
                new Role { RoleId = 1, RoleName = "Observer" },
                new Role { RoleId = 2, RoleName = "Engineer" },
                new Role { RoleId = 3, RoleName = "Manager" }
            );
            
            // Статусы проектов
            modelBuilder.Entity<ProjectStatus>().HasData(
                new ProjectStatus { ProjectStatusId = 1, ProjectStatusName = "Active", ProjectStatusDescription = "Active project" },
                new ProjectStatus { ProjectStatusId = 2, ProjectStatusName = "Completed", ProjectStatusDescription = "Completed project" },
                new ProjectStatus { ProjectStatusId = 3, ProjectStatusName = "On Hold", ProjectStatusDescription = "Project on hold" }
            );
            
            // Статусы дефектов
            modelBuilder.Entity<DefectStatus>().HasData(
                new DefectStatus { Id = 1, StatusName = "New", StatusDescription = "New defect" },
                new DefectStatus { Id = 2, StatusName = "In Progress", StatusDescription = "Defect in progress" },
                new DefectStatus { Id = 3, StatusName = "Under Review", StatusDescription = "Defect under review" },
                new DefectStatus { Id = 4, StatusName = "Closed", StatusDescription = "Defect closed" },
                new DefectStatus { Id = 5, StatusName = "Cancelled", StatusDescription = "Defect cancelled" }
            );
        }
    }
}