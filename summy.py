# Import Transformer from HuggingFace
from transformers import AutoTokenizer, AutoConfig, AutoModel
import sys
import json
from summarizer import Summarizer
###############################################################
futurework = sys.argv[1]
# SCIBert Summarizer

# Load model, model config and tokenizer via Transformers
custom_SCIBert_config = AutoConfig.from_pretrained('allenai/scibert_scivocab_uncased')
custom_SCIBert_config.output_hidden_states=True
custom_SCIBert_tokenizer = AutoTokenizer.from_pretrained('allenai/scibert_scivocab_uncased')
custom_SCIBert_model = AutoModel.from_pretrained('allenai/scibert_scivocab_uncased', config=custom_SCIBert_config)
SCIBert_model = Summarizer(custom_model=custom_SCIBert_model, custom_tokenizer=custom_SCIBert_tokenizer)

# RoBERTa Summarizer
custom_RoBERTa_config = AutoConfig.from_pretrained('roberta-base')
custom_RoBERTa_config.output_hidden_states=True
custom_RoBERTa_tokenizer = AutoTokenizer.from_pretrained('roberta-base')
custom_RoBERTa_model = AutoModel.from_pretrained('roberta-base', config=custom_RoBERTa_config)
RoBERTa_model = Summarizer(custom_model=custom_RoBERTa_model, custom_tokenizer=custom_RoBERTa_tokenizer)

# Result
print(json.dumps({
  "SCIBert": SCIBert_model(futurework),
  "RoBERTa": RoBERTa_model(futurework)
}))


