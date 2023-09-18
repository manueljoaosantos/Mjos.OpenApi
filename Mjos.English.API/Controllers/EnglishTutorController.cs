using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using Mjos.English.API.Models;
using System.Text.Json;
using System.Text;
using OpenAI_API;
using OpenAI_API.Completions;

namespace Mjos.English.API.Controllers;

[ApiController]
[Route("api/english-tutor")]
public class EnglishTutorController : ControllerBase
{
    public readonly HttpClient _httpClient;
    public EnglishTutorController(HttpClient httpClient)
    {
        _httpClient= httpClient;
    }
    [HttpGet]
    public async Task<IActionResult> Get(string text, [FromServices] IConfiguration configuration)
    {
        var token = configuration.GetValue<string>("OpenAIServiceOptions:ApiKey");

        _httpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);

        var model = new ChatGptInputModel(text);
        var requestBody = JsonSerializer.Serialize(model);
        var content = new StringContent(requestBody, Encoding.UTF8, "application/json");


        var response = await _httpClient.PostAsync("https://api.openai.com/v1/completions", content);

        var result = await response.Content.ReadFromJsonAsync<ChatGptViewModel>();
        var promptResponse = result.choices.First();
        return Ok(promptResponse.text.Replace("\n", "").Replace("\t", ""));
    }

    [HttpGet]
    [Route("UseChatGPT")]
    public async Task<IActionResult> UseChatGPT(string query, [FromServices] IConfiguration configuration)
    {

        var token = configuration.GetValue<string>("OpenAIServiceOptions:ApiKey");
        string outputResult = "";
        var openai = new OpenAIAPI(token);
        CompletionRequest completionRequest = new CompletionRequest();
        completionRequest.Prompt = query;
        completionRequest.Model = OpenAI_API.Models.Model.DavinciText;
        completionRequest.MaxTokens = 1024;

        var completions = await openai.Completions.CreateCompletionAsync(completionRequest);

        foreach (var completion in completions.Completions)
        {
            outputResult += completion.Text;
        }

        return Ok(outputResult);

    }
}