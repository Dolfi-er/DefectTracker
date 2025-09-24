using Microsoft.EntityFrameworkCore;
using Back.Models.Entities;

namespace Back.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        
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