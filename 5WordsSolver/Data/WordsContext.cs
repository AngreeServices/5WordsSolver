using Microsoft.EntityFrameworkCore;
namespace _5WordsSolver.Data
{
    public class WordsContext: DbContext
    {
        public DbSet<WordDB> Words { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=data.sqlite");
        }
    }
}
