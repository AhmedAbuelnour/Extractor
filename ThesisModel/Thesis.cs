using System;
using System.Collections.Generic;

namespace ThesisModel
{
    public class Thesis
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Abstract { get; set; }

        public string FutureWork { get; set; }

        public string PublishedDate { get; set; }

        public virtual ICollection<Keyword> Keywords { get; set; }

        public virtual ICollection<Author> Authors { get; set; }
    }
}
