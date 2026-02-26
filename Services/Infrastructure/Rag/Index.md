## Offline / Background services (Indexing pipeline)

1. Data Extraction Service : Read raw data from SQL (Products, Categories, Descriptions)
    - Raw SQL -> Domain objects (ProductDTO)
2. Document Builder Service: Convert domain data → semantic text documents
    - DTOs -> Human readable docs
3. Chunking service: Split document to chunks
    - configuration: - Size: - Overlap
    - Documents text -> list of text chunks
4. Embedding Service (Indexing): Convert chunks to vectors
    - chunks -> Embeddings + metadata
5. Vector Index service: Store embeddings in vector database, Handle upserts and deletes

## Runtime Services (Query pipeline)

1. Chat Orchestrator API (Entry point)
    - Responsibility
        - Single public endpoint for chat
        - Controls the entire flow
    - Does NOT
        - Talk directly to vector DB
        - Talk directly to LLM

2. Query Embedding Service
    - Responsibility
        - Convert user message → embedding
    - Input
        - User query<!--  -->
    - Output
        - Query vector

3. Retrieval Service
    - Responsibility
        - Query vector DB
        - Apply filters (category, price, gender)
        - Return Top-K chunks
    - Output
        - Static knowledge context
    - This is where relevance is decided.
4. Context builder Service: Bad context = bad answers
    - Responsibility
        - Combine:
            - Retrieved static chunks
            - Live SQL data (cart, availability)
            - System constraints
    - Output
        - Final prompt context
5. LLM Generation Service
    - Responsibility
        - Call the LLM
        - Generate final answer
    - Input - Prompt + context
      -Output - Natural language response
    - The LLM:
        - Does not retrieve
        - Does not reason about truth
        - Only explains
