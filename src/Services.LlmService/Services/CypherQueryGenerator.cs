using Services.LlmService.Entities;
using Services.LlmService.Helpers;

namespace Services.LlmService.Services;

public class CypherQueryGenerator : ICypherQueryGenerator
{
    private const string DEFAULT_RELATION_WITH_TOPIC_NAME = "BELONGS_TO";
    private const string DEFAULT_TOPIC_VARIABLE_NAME = "Document";
    public IEnumerable<string> GenerateCypherQueryFromTripletRowsForTopicMessage(string topicId, IEnumerable<TripletRow> tripletRows)
    {
        var entityCypherText = new List<string>();
        entityCypherText.Add($"MERGE ({DEFAULT_TOPIC_VARIABLE_NAME}:DOCUMENT {{ id: '{topicId}', type:'DOCUMENT''}})"); 
        var relationships = new HashSet<string>();
        foreach (var triplet in tripletRows)
        {
            var pcHead = Neo4jHelpers.CreateName(triplet.head);
            var pcTail = Neo4jHelpers.CreateName(triplet.tail);
            var relationName = triplet.relation.Replace(" ", "_").Replace("-","_");
            if (string.IsNullOrEmpty(relationName))
            {
                relationName = "RELATED_TO";
            }
            entityCypherText.Add($"MERGE ({pcHead}:ENTITY {{name: \'{triplet.head}\', type: \'{triplet.head_type}\'}})-[:{relationName}]->({pcTail}: ENTITY {{name: \'{triplet.tail}\', type: \'{triplet.tail_type}\'}})");
            entityCypherText.Add($"MERGE ({pcHead})-[:{DEFAULT_RELATION_WITH_TOPIC_NAME}]->({DEFAULT_TOPIC_VARIABLE_NAME})");
            entityCypherText.Add($"MERGE ({pcTail})-[:{DEFAULT_RELATION_WITH_TOPIC_NAME}]->({DEFAULT_TOPIC_VARIABLE_NAME})");
        }

        return entityCypherText;
    }
}