using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ThesisModel
{
    public class ThesisDocumentDbContext :DbContext
    {
        public DbSet<SummarizedThesis> SummarizedThesis { get; set; }
        public DbSet<Thesis> Theses { get; set; }
        public DbSet<Author> Authors { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //optionsBuilder.UseSqlite(new SqlConnectionStringBuilder
            //{
            //    DataSource = @"localhost\SQLEXPRESS",
            //    InitialCatalog = "ThesisDocument",
            //    IntegratedSecurity = true

            //}.ConnectionString);

            optionsBuilder.UseSqlite("Data Source=ThesisDocument.db");
            base.OnConfiguring(optionsBuilder);
        }
    }
}
