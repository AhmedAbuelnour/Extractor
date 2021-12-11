using System.ComponentModel.DataAnnotations.Schema;

namespace ThesisModel
{
    public class Thesis
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string FutureWork { get; set; }
        public string PublishedDate { get; set; }
        public string Abstract { get; set; }
        public int SummarizedThesisId { get; set; }
        [ForeignKey(nameof(SummarizedThesisId))] public SummarizedThesis SummarizedThesis { get; set; }
    }
}
