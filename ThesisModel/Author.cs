using System.ComponentModel.DataAnnotations.Schema;

namespace ThesisModel
{
    public class Author
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public string Department { get; set; }
        public string Institution { get; set; }
        public string Country { get; set; }
        public int ThesisId { get; set; }
        [ForeignKey(nameof(ThesisId))] public Thesis Thesis { get; set; }
    }
}
