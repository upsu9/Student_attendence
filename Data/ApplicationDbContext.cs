using Microsoft.EntityFrameworkCore;
using Student_attendence.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Student_attendence.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }
        public DbSet<Student> StudentMasterData { get; set; }
        public DbSet<AdminUser> AdminUserData { get; set; }
        public DbSet<RegisteredAttendence> RegisteredAttendenceData { get; set; }
        public DbSet<UnRegisteredAttendence> UnRegisteredAttendenceData { get; set; }
        public DbSet<Note> NotesData { get; set; }
        public DbSet<Feedback> FeedbacksData { get; set; }
    }
}
