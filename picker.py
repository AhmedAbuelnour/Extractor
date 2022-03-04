#Install the following
#pip install keybert
#pip install nltk
#pip install setuptools wheel
#pip -m spacy download en_core_web_lg

# Importing NLTK Library
from ntpath import join
import nltk 
nltk.download('wordnet')
nltk.download('punkt')
# Import System agruments
import sys
import json
from keybert import KeyBERT
# Import spacy for text similarity
import spacy
nlp = spacy.load("en_core_web_lg")

def convertTuple(tup):
    st = ' '.join(map(str, tup))
    return st
###############################################################
abstract = sys.argv[1]
SciBert_futurework = sys.argv[2]
RoBERTa_futurework = sys.argv[3]

from nltk.stem import WordNetLemmatizer 
lemmatizer = WordNetLemmatizer()
key_model = KeyBERT()

# lemmatizer for abstract
abstract_list = nltk.word_tokenize(abstract)
lemmatized_abstract = ' '.join([lemmatizer.lemmatize(w,pos='v') for w in abstract_list])
abstract_keywords = ' '.join([x[0] for x in key_model.extract_keywords(lemmatized_abstract)])

# lemmatizer for SciBert futurework
SciBert_futurework_list = nltk.word_tokenize(SciBert_futurework)
lemmatized_SciBert_futurework = ' '.join([lemmatizer.lemmatize(w,pos='v') for w in SciBert_futurework_list])
SciBert_futurework_keywords = ' '.join([x[0] for x in key_model.extract_keywords(lemmatized_SciBert_futurework)])
# lemmatizer for RoBERTa futurework
RoBERTa_futurework_list = nltk.word_tokenize(RoBERTa_futurework)
lemmatized_RoBERTa_futurework = ' '.join([lemmatizer.lemmatize(w,pos='v') for w in RoBERTa_futurework_list])
RoBERTa_futurework_keywords = ' '.join([x[0] for x in key_model.extract_keywords(lemmatized_RoBERTa_futurework)])
# Text similarity 
doc1 = nlp(abstract_keywords)
doc2 = nlp(SciBert_futurework_keywords)
doc3 = nlp(RoBERTa_futurework_keywords)

text_similarity_abstract_to_SciBERT = doc1.similarity(doc2)
text_similarity_abstract_to_RoBERTa = doc1.similarity(doc3)


if(text_similarity_abstract_to_SciBERT > text_similarity_abstract_to_RoBERTa):
    print(json.dumps({
            "Model": "SciBERT",
            "Keywords": SciBert_futurework_keywords
            }))
elif (text_similarity_abstract_to_RoBERTa > text_similarity_abstract_to_SciBERT):
      print(json.dumps({
            "Model": "RoBERTa",
            "Keywords": RoBERTa_futurework_keywords
            }))
else:
  print(json.dumps({
            "Model": "Any",
            "Keywords": abstract_keywords
       }))



