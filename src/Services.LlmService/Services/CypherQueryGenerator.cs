using Services.LlmService.Entities;
using Services.LlmService.Neo4j;

namespace Services.LlmService.Services;

public class CypherQueryGenerator(IEmbeddingGenerationService embeddingGenerationService) : ICypherQueryGenerator
{
    private const string DEFAULT_RELATION_WITH_TOPIC_NAME = "BELONGS_TO";
    private const string DEFAULT_TOPIC_VARIABLE_NAME = "Document";
    private const string DEFAULT_CHUNK_VARIABLE_NAME = "Document_Chunk";
    private const string DEFAULT_ENTITY_VARIABLE_NAME = "ENTITY";
    private const string DEFAULT_VECTOR_INDEX_NAME = "TEXT_EMBEDDING";
    private const string DEFAULT_EMBEDDING_PROPERTY_NAME = "embedding";

    public async IAsyncEnumerable<IEnumerable<string>> GenerateCypherQueryFromTripletRowsForRow(string topicId, string chunkText, IEnumerable<TripletRow> tripletRows)
    {
        foreach (var tripletRow in tripletRows)
        {
            yield return await GenerateCypherQueryForTripletRowWithChunks(topicId, chunkText, tripletRow);
        }
    }

    private async Task<IEnumerable<string>> GenerateCypherQueryForTripletRowWithChunks(string topicId, string chunkText, TripletRow triplet)
    {
        var entityCypherText = new List<string>
        {
            $"MERGE ({DEFAULT_TOPIC_VARIABLE_NAME}:DOCUMENT {{ id: '{topicId}', type:'DOCUMENT'}})",
            $"MERGE ({DEFAULT_CHUNK_VARIABLE_NAME}:DOCUMENT_CHUNK {{ topicId: '{topicId}', type:'DOCUMENT_CHUNK', text: '{chunkText}'}})",
            $"MERGE ({DEFAULT_CHUNK_VARIABLE_NAME})-[:{DEFAULT_RELATION_WITH_TOPIC_NAME}]->({DEFAULT_TOPIC_VARIABLE_NAME})"
        };
        var prefix = 1;
        var pcHead = Neo4jHelpers.CreateName(triplet.head);
        var pcTail = Neo4jHelpers.CreateName(triplet.tail);
        if (pcHead == pcTail)
        {
            pcHead += prefix;
            prefix++;
            pcTail += prefix;
        }
        else
        {
            pcHead += prefix;
            pcTail += prefix;
        }
            
        var relationName = triplet.relation.Replace(" ", "_").Replace("-","_");
        if (string.IsNullOrEmpty(relationName))
        {
            relationName = "RELATED_TO";
        }
        
        entityCypherText.Add($"MERGE ({pcHead}{prefix}: {DEFAULT_ENTITY_VARIABLE_NAME} {{name: \'{triplet.head}\', type: \'{triplet.head_type}\'}})-[:{relationName}]->({pcTail}{prefix}: {DEFAULT_ENTITY_VARIABLE_NAME} {{name: \'{triplet.tail}\', type: \'{triplet.tail_type}\'}})");
        
        entityCypherText.Add($"MERGE ({pcHead}{prefix})-[:{DEFAULT_RELATION_WITH_TOPIC_NAME}]->({DEFAULT_CHUNK_VARIABLE_NAME})");
        entityCypherText.Add($"MERGE ({pcTail}{prefix})-[:{DEFAULT_RELATION_WITH_TOPIC_NAME}]->({DEFAULT_CHUNK_VARIABLE_NAME})");
        
        var headEmbedding = await embeddingGenerationService.GenerateEmbedding(triplet.head);
        var tailEmbedding = await embeddingGenerationService.GenerateEmbedding(triplet.tail);
        
        entityCypherText.Add($"WITH {pcHead}{prefix}, [{string.Join(", ", headEmbedding.ToArray())}] AS {pcHead}{prefix}embedding");
        entityCypherText.Add($"CALL db.create.setNodeVectorProperty({pcHead}{prefix}, '{DEFAULT_EMBEDDING_PROPERTY_NAME}', {pcHead}{prefix}embedding)");
        
        entityCypherText.Add($"MERGE ({pcTail}{prefix}: {DEFAULT_ENTITY_VARIABLE_NAME} {{name: \'{triplet.tail}\', type: \'{triplet.tail_type}\'}})");
        entityCypherText.Add($"WITH {pcTail}{prefix}, [{string.Join(", ", tailEmbedding.ToArray())}] AS {pcTail}{prefix}embedding");
        entityCypherText.Add($"CALL db.create.setNodeVectorProperty({pcTail}{prefix}, '{DEFAULT_EMBEDDING_PROPERTY_NAME}', {pcTail}{prefix}embedding)");

        return entityCypherText;
    }

    public async Task<string> GenerateCypherQueryForVectorSearchWord(string word)
    {
        var keywordEmbedding = await embeddingGenerationService.GenerateEmbedding(word);
        var query = $@"
                               WITH [{string.Join(", ", keywordEmbedding.ToArray())}] AS question_embedding
                                   CALL db.index.vector.queryNodes(
                                       '{DEFAULT_VECTOR_INDEX_NAME}',
                                       10, 
                                       question_embedding
                                       ) 
                                   YIELD node AS e1, score
                                   MATCH (e1)-[r]-(e2:ENTITY)-[r2:{DEFAULT_RELATION_WITH_TOPIC_NAME}]->(dc)
                                   RETURN '(' + COALESCE(e1.name,'') + ')-[:' + COALESCE(type(r),'') + ']->(' + COALESCE(e2.name,'') + ')' as triplet, dc.text as text
                               ";
        return query;
    }
    
    
    public string GenerateCypherQueryToCreateVectorIndex()
    {
        var createVectorIndex = $@"CREATE VECTOR INDEX {DEFAULT_VECTOR_INDEX_NAME} IF NOT EXISTS
                                    FOR (e:{DEFAULT_ENTITY_VARIABLE_NAME}) ON e.{DEFAULT_EMBEDDING_PROPERTY_NAME}
                                    OPTIONS {{indexConfig: {{
                                        `vector.dimensions`: 768,
                                        `vector.similarity_function`: 'cosine'
                                    }}}}";
        return createVectorIndex;
    }
}