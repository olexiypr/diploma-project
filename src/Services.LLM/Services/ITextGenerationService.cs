using Services.LLM.ResponseModels;

namespace Services.LLM.Services;

public interface ITextGenerationService
{
    GeneratedTextResponseModel GenerateText();
}