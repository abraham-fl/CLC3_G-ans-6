using System;
using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime;
using ServiceStack.Text;

namespace TestFunctionApp
{
    public class UploadBlobTrigger
    {
        [FunctionName("UploadBlobTrigger")]
        public async Task RunAsync([BlobTrigger("images/{name}", Connection = "")]Stream myBlob, string name, ILogger log)
        {
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");

            subScriptionKey = "";

            // create a ComputerVisionClient using SubscriptionKey and Endpoint
            var client = new ComputerVisionClient(new ApiKeyServiceClientCredentials(Config.SubscriptionKey))
            {
                Endpoint = Config.Endpoint
            };

            // specify desired AI features
            var features = new List<VisualFeatureTypes?> { VisualFeatureTypes.Tags };

            // Process the blob
            var result = await client.AnalyzeImageWithHttpMessagesAsync(image.Uri.ToString(), features);
            log.LogInformation($"Got response from Computer Vision with status ({result.Response.StatusCode})");

            // extract tags and store them as Dictionary<string,string>
            var tags = result.Body.Tags
                .Select((tag, index) => new { index = $"tag_{index}", tag })
                .ToDictionary(x => x.index, x => x.tag.Name);

            // store tags using custom blob metadata
            await image.SetMetadataAsync(tags);
        }
    }
}
