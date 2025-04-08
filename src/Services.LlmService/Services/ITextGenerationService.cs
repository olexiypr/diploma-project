using Services.LlmService.ResponseModels;

namespace Services.LlmService.Services;

public interface ITextGenerationService
{
    GeneratedTextResponseModel GenerateText();
}