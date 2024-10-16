using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using A3_G4.Models;

namespace A3_G4.Data
{
    public class A3Context : DbContext
    {
        public A3Context (DbContextOptions<A3Context> options)
            : base(options)
        {
        }

        public DbSet<A3_G4.Models.EnrolledMember> EnrolledMember { get; set; } = default!;
        public DbSet<A3_G4.Models.EventCoach> EventCoach { get; set; } = default!;
        public DbSet<A3_G4.Models.Account> Account { get; set; } = default!;
    }
}
