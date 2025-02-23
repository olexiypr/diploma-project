using Microsoft.AspNetCore.Mvc;
using Services.LLM.ResponseModels;

namespace Services.LLM.Controllers;

[ApiController]
[Route("[controller]")]
public class TextGenerationController : ControllerBase
{
    [HttpPost]
    public async Task<GeneratedTextResponseModel> GenerateStoryContinuation()
    {
        await Task.Delay(TimeSpan.FromSeconds(20));
        return new GeneratedTextResponseModel {Text = "Story continuation!"};
    }
}