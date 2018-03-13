using Lunar.Auth.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Lunar.Auth.Data
{
    public interface IApplicationDbContext
    {
        DbSet<IdentityUserRole<string>> UserRoles { get; set; }
        DbSet<IdentityRole> Roles { get; set; }
        DbSet<IdentityRoleClaim<string>> RoleClaims { get; set; }
        DbSet<ApplicationUser> Users { get; set; }
        DbSet<IdentityUserClaim<string>> UserClaims { get; set; }
        DbSet<IdentityUserLogin<string>> UserLogins { get; set; }
        DbSet<IdentityUserToken<string>> UserTokens { get; set; }
        DatabaseFacade Database { get; }
        IModel Model { get; }
        
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
        }
    }
}