using Microsoft.EntityFrameworkCore;

namespace ThesisModel
{
    public class ThesisDocumentDbContext : DbContext
    {
        public DbSet<SummarizedThesis> SummarizedThesis { get; set; }
        public DbSet<Thesis> Theses { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=ThesisDocument.db");
            base.OnConfiguring(optionsBuilder);
        }
    }
}
