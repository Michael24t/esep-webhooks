using System.Text;
using Amazon.Lambda.Core;
using Newtonsoft.Json;
using System.Net.Http;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
namespace EsepWebhook;
public class Function
{
    private static readonly HttpClient client = new HttpClient();
    public string FunctionHandler(object input, ILambdaContext context)
    {
        context.Logger.LogInformation("Webhook triggered");
        context.Logger.LogInformation($"Input received: {input}");

        try
        {
            dynamic json = JsonConvert.DeserializeObject<dynamic>(input.ToString());

            string issueUrl = json.issue.html_url;
            context.Logger.LogInformation($"Parsed issue URL: {issueUrl}");

            string slackMessage = $"{{\"text\": \"issue: {issueUrl}\"}}";
            var webhookUrl = Environment.GetEnvironmentVariable("SLACK_URL");
            if (string.IsNullOrEmpty(webhookUrl))
            {
                context.Logger.LogError("SLACK_URL enviro not set");
                return "Slack URL not configured.";
            }

           var request = new HttpRequestMessage(HttpMethod.Post, webhookUrl)
            {
                Content = new StringContent(slackMessage, Encoding.UTF8, "application/json")
            };

         var response = client.Send(request);
            context.Logger.LogInformation($"Slack response: {response.StatusCode}");

            return "Webhook processed successfully.";
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"Exception: {ex.Message}");
            return $"Error: {ex.Message}";
        }
    }
}
