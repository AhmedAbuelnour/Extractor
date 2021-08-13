using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ThesisModel
{
    public class ThesisDocumentDbContext :DbContext
    {
        public DbSet<Thesis> Theses { get; set; }
        public DbSet<Keyword> Keywords { get; set; }
        public DbSet<Author> Authors { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(new SqlConnectionStringBuilder
            {
                DataSource = @"localhost\SQLEXPRESS",
                InitialCatalog = "ThesisDocument",
                IntegratedSecurity = true

            }.ConnectionString);
            base.OnConfiguring(optionsBuilder);
        }
    }
}
