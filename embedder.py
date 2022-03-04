# Import  Sentence Transformer from HuggingFace
from sentence_transformers import SentenceTransformer
import sys

#msmarco-distilbert-base-dot-prod-v3: for asymmetric search
#paraphrase-MiniLM-L6-v2 : for symmetric search
model = SentenceTransformer('msmarco-distilbert-base-dot-prod-v3')
###############################################################
sentence = sys.argv[1]
# SCIBert Summarizer
print(embeddings = model.encode(sentence))


