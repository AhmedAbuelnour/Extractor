using System.ComponentModel.DataAnnotations.Schema;

namespace ThesisModel
{
    public class Keyword
    {
        public int Id { get; set; }

        public string Term { get; set; }

        public int ThesisId { get; set; }
        [ForeignKey(nameof(ThesisId))] public Thesis Thesis { get; set; }
    }
}
