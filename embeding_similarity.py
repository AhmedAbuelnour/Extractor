# Import Transformer from HuggingFace
import numpy as np
from scipy import spatial
import sys
###############################################################

keyword_embedding =  np.load(sys.argv[1])
sentence_embedding = np.load(sys.argv[2])
# calculate embeding similarity
result = 1 - spatial.distance.cosine(keyword_embedding, sentence_embedding)

print(result)

