using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OSWTF.Models;
using OSWTF.Server.Models;

namespace OSWTF.Web.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {            
        }

        public DbSet<TestingOptions.Test> Tests { get; set; }
        public DbSet<Template> Templates { get; set; }
        public DbSet<UrlRule> UrlRules { get; set; }
        public DbSet<UrlRuleType> UrlRuleTypes { get; set; }
    }
}
