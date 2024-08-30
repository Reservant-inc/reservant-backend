using Microsoft.EntityFrameworkCore;

namespace LogsViewer.Data;

/// <summary>
/// Database context for the logs database
/// </summary>
internal class LogDbContext(DbContextOptions<LogDbContext> options) : DbContext(options)
{
    public DbSet<LogMessage> Log { get; set; }
}
