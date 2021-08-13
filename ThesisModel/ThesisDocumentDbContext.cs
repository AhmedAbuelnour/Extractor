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
            //optionsBuilder.UseSqlite(new SqlConnectionStringBuilder
            //{
            //    DataSource = @"localhost\SQLEXPRESS",
            //    InitialCatalog = "ThesisDocument",
            //    IntegratedSecurity = true

            //}.ConnectionString);

            optionsBuilder.UseSqlite("Data Source=ThesisDocuments.db");
            base.OnConfiguring(optionsBuilder);
        }
    }
}
