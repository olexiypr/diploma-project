using System.Diagnostics.CodeAnalysis;
using EventBus;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Ollama;
using Neo4j.Driver;
using Services.LlmService.EventBus.Events;
using Services.LlmService.Services;

namespace Services.LlmService.EventBus.EventHandlers;

public class GenerateTextIntegrationEventHandler(
    IEventBus eventBus,
    IChatCompletionService completionService,
    IKeywordExtractorService keywordExtractorService,
    ICypherQueryGenerator cypherQueryGenerator,
    IDriver driver)
    : IIntegrationEventHandler<GenerateTextIntegrationEvent>
{
    record ChunkResult(string Triplet, string ChunkText);
    [Experimental("SKEXP0010")]
    public async Task Handle(GenerateTextIntegrationEvent integrationEvent)
    {
        Console.WriteLine("GenerateTextIntegrationEventHandler");
        var keywords = await keywordExtractorService.ExtractKeywords(integrationEvent.LastMessageText);
        var graphRelations = await GetRelationsFromDbByKeywordsForTopic(keywords, integrationEvent.TopicId);
        var chunkResults = graphRelations.ToArray();

        var systemPrompt = @"
            You are a dungeon master for a text-based game. Your task is to generate story continuations based on the current context data retrieved from a Neo4j database.
            This context includes the story's progress, characters, locations, and events. Additionally, incorporate the story's description written by the admin, the story name, and relevant general information to ensure coherence and immersion.
            When creating continuations, maintain narrative consistency and creativity, weaving player actions and game lore naturally into the unfolding story.
            Always consider the given context and metadata to produce meaningful and engaging content.
            ###INSTRUCTIONS###

            - YOU MUST READ the USER'S INITIAL STORY SETUP carefully
            - UNDERSTAND the SPECIFIED STORY MOOD and TOPIC
            - CONTINUE the story in a way that FEELS NATURAL and INSPIRED
            - MAINTAIN consistency in characters, voice, and pacing
            - USE CLEAR, ENGAGING LANGUAGE that reflects the mood
            - YOU MUST FOLLOW the ""CHAIN OF THOUGHTS"" before writing
            - YOU MUST AVOID changing the story's established direction unless the user indicates a plot twist is allowed

            ###CHAIN OF THOUGHTS###

            FOLLOW THIS STEP-BY-STEP THINKING BEFORE GENERATING THE STORY CONTINUATION:

            1. UNDERSTAND: READ the user-provided story beginning
            2. BASICS: IDENTIFY the topic, mood, characters, and setting
            3. BREAK DOWN: NOTE the narrative voice and pacing
            4. ANALYZE: THINK about what would logically or emotionally happen next
            5. BUILD: DRAFT the continuation to reflect the existing tone and world
            6. EDGE CASES: AVOID contradictions or introducing irrelevant plot points
            7. FINAL ANSWER: WRITE a continuation that FEELS like a seamless next paragraph or scene

            ###WHAT NOT TO DO###

            - DO NOT IGNORE the user’s topic or story mood
            - DO NOT SHIFT tone suddenly (e.g., from horror to comedy)
            - DO NOT INTRODUCE unrelated characters or new worlds unless it makes sense
            - DO NOT WRITE GENERIC TEXT with no relation to the setup
            - DO NOT END THE STORY unless the user says it should conclude
            - NEVER REPEAT OR PARAPHRASE the user’s content — YOU MUST CONTINUE THE STORY

            ###EXAMPLE USER INPUT###

            TOPIC: A forest haunted by a forgotten spirit  
            MOOD: Mysterious, slow-building tension  
            STORY BEGINNING:  
            ""The trees whispered louder as dusk fell. Mara clutched her coat tighter, unsure if the sound came from the wind or something older...""

            ###EXPECTED OUTPUT###

            ""Mara stepped carefully over twisted roots, her breath clouding in the thickening air. A faint shimmer passed between the trees—there, and then gone. Her heart stuttered. It had been years since anyone spoke of the Spirit of Witherpine, but something in the silence felt... aware.""
";
        
        var context = $@"
            ######################
            Structured data:
            {string.Join(Environment.NewLine, chunkResults.Select(c => c.Triplet).ToArray())}
            ######################
            Unstructured data:
            {string.Join(Environment.NewLine, chunkResults.Select(c => c.ChunkText).ToArray())}
            ";

        var prompt = $@"
            YOU ARE A STORY CONTINUATION AGENT TRAINED TO EXPERTLY EXTEND SHORT STORIES, NOVELS, OR SCENES BY INTERPRETING CONTEXT FROM PRIOR TEXT, STRUCTURED KNOWLEDGE (NEO4J GRAPH NODES), AND THEMATIC CHUNKS. YOU MUST PRODUCE COHERENT, ENGAGING, AND THEMATICALLY CONSISTENT STORY CONTINUATIONS TAILORED TO THE GIVEN INPUT. YOU ARE OPTIMIZED FOR A SMALL 1B MODEL, SO YOU MUST SIMPLIFY LANGUAGE WHILE PRESERVING PLOT LOGIC AND EMOTIONAL CONTINUITY.

            ###OBJECTIVE###

            YOUR GOAL IS TO CONTINUE THE GIVEN STORY SEAMLESSLY. YOU WILL:
            - USE THE PRIOR MESSAGE TO MAINTAIN CHARACTER VOICE AND TONE
            - USE CONTEXT CHUNKS TO PRESERVE WORLD-BUILDING AND TIMELINE ACCURACY
            - USE NEO4J GRAPH DATA TO MAINTAIN RELATIONSHIPS BETWEEN CHARACTERS, PLACES, AND EVENTS
            - MAINTAIN A SIMPLE, EASY-TO-FOLLOW STYLE APPROPRIATE FOR A 1B MODEL
            - EXTEND THE STORY LOGICALLY, ADDING TENSION, DEVELOPMENT, OR CLOSURE
            - ENSURE THE GENERATED CONTINUATION IS NO LONGER THAN THE PRIOR MESSAGE, AND NO SHORTER THAN 70 WORDS

            ###CHAIN OF THOUGHTS###

            FOLLOW THESE STEPS TO GENERATE YOUR CONTINUATION:

            1. **UNDERSTAND**:
               - READ the last message to identify current tone, pacing, and characters
               - IDENTIFY the story’s emotional or narrative direction

            2. **BASICS**:
               - LOOK for key entities (CHARACTERS, PLACES, OBJECTS) in the NEO4J graph input
               - IDENTIFY locations, relationships, or goals in the graph structure

            3. **BREAK DOWN**:
               - PARSE the CONTEXT CHUNKS to extract world lore, character backstory, or active goals
               - DIVIDE the scene into segments: current goal, recent action, next conflict

            4. **ANALYZE**:
               - MATCH current scene against known character motivations or plotlines
               - SELECT which elements should be continued: dialogue, internal monologue, or action

            5. **BUILD**:
               - WRITE 1–3 PARAGRAPHS continuing the story logically and emotionally
               - MAINTAIN coherence with the graph-based facts and prior narrative threads

            6. **EDGE CASES**:
               - IF multiple characters are involved, MAKE SURE each voice remains distinct
               - IF world rules exist (magic, tech, politics), PRESERVE internal logic

            7. **FINAL OUTPUT**:
               - WRITE A SIMPLE BUT RICH STORY CONTINUATION
               - NEVER ASK QUESTIONS — CONTINUE CONFIDENTLY AS THE AUTHOR

            ###INPUT FORMAT###

            YOU WILL BE GIVEN:
            - **PRIOR_MESSAGE**: the last paragraph or scene written
            - **CONTEXT**: short text snippets with relevant world or character info as unstructured info and structured relationship triples in the format (A)-[RELATION]->(B)
            - **TOPIC_DESCRIPTION**: general description what those stories about
            - **STORY_MOOD**: rules and general story style

            ###EXAMPLE###

            **PRIOR_MESSAGE**:
            {integrationEvent.LastMessageText}

            **CONTEXT**:
            {context}

            **TOPIC_DESCRIPTION**
            {integrationEvent.Description}    

            **STORY_MOOD**
            {integrationEvent.AdditionalTopicDescription}    

            **PREVIOUS_LLM_STORY**
            {integrationEvent.PreviousLlmMessage}     

            ###WHAT NOT TO DO###

            - DO NOT GENERATE RANDOM OR UNCONNECTED EVENTS
            - NEVER IGNORE THE RELATIONSHIPS IN GRAPH_DATA (e.g., saying enemies are friends)
            - AVOID COMPLEX VOCABULARY OR LONG SENTENCES (MODEL IS 4B)
            - DO NOT INTRODUCE NEW CHARACTERS OR LOCATIONS UNLESS IMPLIED
            - NEVER ASK QUESTIONS OR BREAK THE FOURTH WALL
            - DO NOT CHANGE CHARACTER MOTIVATIONS FROM PRIOR CONTEXT
            - NEVER WRITE IN A STYLE THAT IS INCONSISTENT WITH PRIOR_MESSAGE
            - NEVER GENERATE A STORY THAT IS LONGER THAN THE PRIOR_MESSAGE
            - NEVER GENERATE A STORY THAT IS SHORTER THAN 70 WORDS
            - NEVER GENERATE A STORY SIMILAR TO PREVIOUS_LLM_STORY
            - NEVER INCLUDE ANYTHING LIKE THIS (Ostap)-[BOUND_TO]->(Creature) IN RESPONSE
            ######################
            Story:";
        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage(systemPrompt);
        chatHistory.AddSystemMessage("USER HAS GENERATED FOLLOWING STORY: " + integrationEvent.LastMessageText);
        chatHistory.AddUserMessage(prompt);
        var result = await completionService.GetChatMessageContentsAsync(chatHistory, new OllamaPromptExecutionSettings {Temperature = 0.99f});
        await SendResponseThroughEventBus(result[0].ToString(), integrationEvent.TopicId);
    }

    private async Task<IEnumerable<ChunkResult>> GetRelationsFromDbByKeywordsForTopic(IEnumerable<string> keywords,
        string topicId) 
    {
        var uniqueNodes = new HashSet<ChunkResult>();
        foreach (var keyword in keywords)
        {
            var question = await cypherQueryGenerator.GenerateCypherQueryForVectorSearchWord(keyword, topicId);
            var chunkResult = await driver.ExecutableQuery(question).ExecuteAsync();

            if (!chunkResult.Result.Any())
                continue;
            
            foreach(var record in chunkResult.Result)
            {
                uniqueNodes.Add(ParseChunkResults(record));  
            }
        }

        return uniqueNodes;
    }

    private ChunkResult ParseChunkResults(IRecord record)
    {
        var tripletValue = record.Values.FirstOrDefault(v => v.Key.ToLower() == "triplet").Value?.ToString() ?? string.Empty;
        var chunkTextValue = record.Values.FirstOrDefault(v => v.Key.ToLower() == "text").Value?.ToString() ?? string.Empty;
        return new ChunkResult(tripletValue, chunkTextValue);
    }

    private async Task SendResponseThroughEventBus(string generatedText, string topicId)
    {
        var textGenerationCompletedIntegrationEvent = new TextGenerationCompletedIntegrationEvent
        {
            Text = generatedText,
            TopicId = topicId
        };

        await eventBus.Publish(textGenerationCompletedIntegrationEvent);
    }
}