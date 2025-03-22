using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2.DocumentModel;
using Microsoft.Extensions.Options;
using Services.MessagesService.Entities;
using Services.MessagesService.Exceptions;
using Services.MessagesService.Mappers;
using Services.MessagesService.RequestModels;
using Services.MessagesService.ResponseModels;
using Services.MessagesService.Settings;

namespace Services.MessagesService.Repositories;

public class DynamoDbMessagesRepository(IMessageMapper messageMapper, IOptions<DynamoDbSettings> dynamoDbSettings, IAmazonDynamoDB amazonDynamoDb) : IMessagesRepository
{
    public async Task<bool> Create(MessageEntity messageEntity)
    {
        var requestAsJson = JsonSerializer.Serialize(messageEntity);
        var requestAsDocument = Document.FromJson(requestAsJson);
        var requestAsAttributes = requestAsDocument.ToAttributeMap();
        var putItemRequest = new PutItemRequest
        {
            TableName = dynamoDbSettings.Value.TableName,
            Item = requestAsAttributes
        };
        var response = await amazonDynamoDb.PutItemAsync(putItemRequest);
        return response.HttpStatusCode == System.Net.HttpStatusCode.OK;
    }

    public async Task<MessageEntity> GetById(string topicId, string id)
    {
        var hashKeyPropertyName = GetAttributeJsonPropertyName(typeof(MessageEntity), nameof(DynamoDBHashKeyAttribute));
        var rangeKeyPropertyName =
            GetAttributeJsonPropertyName(typeof(MessageEntity), nameof(DynamoDBRangeKeyAttribute));
        var getItemRequest = new GetItemRequest
        {
            TableName = dynamoDbSettings.Value.TableName,
            Key = new Dictionary<string, AttributeValue>
            {
                {hashKeyPropertyName, new AttributeValue {N = topicId}},
                {rangeKeyPropertyName, new AttributeValue {S = id}}
            }
        };
        var response = await amazonDynamoDb.GetItemAsync(getItemRequest);
        if (response.Item.Count < 1)
        {
            throw new MessageNotFoundException();
        }
        
        return JsonSerializer.Deserialize<MessageEntity>(Document.FromAttributeMap(response.Item).ToJson())!;
    }

    public async Task<IEnumerable<MessageEntity>> GetMessagesByTopicId(string topicId)
    {
        var queryRequest = new QueryRequest
        {
            TableName = dynamoDbSettings.Value.TableName,
            Limit = 10,
            Select = Select.ALL_ATTRIBUTES,
            KeyConditionExpression = $"{GetAttributeJsonPropertyName(typeof(MessageEntity), nameof(DynamoDBHashKeyAttribute))} = :pkValue",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":pkValue", new AttributeValue {S = topicId}}
            }
        };
        var response = await amazonDynamoDb.QueryAsync(queryRequest);
        if (response.Count < 1)
        {
            throw new MessageNotFoundException();
        }
        
        return response.Items.Select(i => JsonSerializer.Deserialize<MessageEntity>(Document.FromAttributeMap(i).ToJson()))!;
    }

    private string GetAttributeJsonPropertyName(Type type, string attributeName)
    {
        var properties = type.GetProperties();
        var propertyInfo = properties.FirstOrDefault(p => 
            p.GetCustomAttributes()
                .FirstOrDefault(a => a.GetType().Name == attributeName) != null);
        var attribute = (JsonPropertyNameAttribute) propertyInfo?.GetCustomAttributes().FirstOrDefault(a => a.GetType().Name == nameof(JsonPropertyNameAttribute))!;
        if (attribute is null)
        {
            throw new ArgumentException();
        }

        return attribute.Name;
    }
}