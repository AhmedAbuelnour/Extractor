# Import  Sentence Transformer from HuggingFace
import numpy as np
from sentence_transformers import SentenceTransformer
import sys

#msmarco-distilbert-base-dot-prod-v3: for asymmetric search
#paraphrase-MiniLM-L6-v2 : for symmetric search
model = SentenceTransformer('msmarco-distilbert-base-dot-prod-v3')
###############################################################
sentence = sys.argv[1]
directoryToSave = sys.argv[2]
thesisName = sys.argv[3]

fullPath = directoryToSave + "/" + thesisName + ".npy"
# save file
embeddingResult = model.encode(sentence)

np.save(fullPath, embeddingResult)

print(fullPath)


