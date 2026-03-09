using Microsoft.EntityFrameworkCore;
using SmartQueue.Data.Models;

namespace SmartQueue.Data.Common
{
    public class SmartQueueContext: DbContext
    {
        public SmartQueueContext(DbContextOptions<SmartQueueContext> options) : base(options){}
        public DbSet<Service> Services { get; set; }
        public DbSet<Visitor> Visitors  { get; set; }
        public DbSet<Ticket> Tickets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
