using System;
using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Azure.Storage.Blobs;
using System.Linq;

namespace TestFunctionApp
{
    public class UploadBlobTrigger
    {
        private static readonly string connectionString = Environment.GetEnvironmentVariable("ConnectionStringImageStorage");
        private static readonly string subScriptionKey = Environment.GetEnvironmentVariable("SubscriptionKey");
        private static readonly string endPoint = Environment.GetEnvironmentVariable("CognitiveServiceEndpoint");
        private static readonly string containerNameDownload = Environment.GetEnvironmentVariable("ContainerNameImage");

        [FunctionName("UploadBlobTrigger")]
        public async Task RunAsync([BlobTrigger("images/{name}", Connection = "AzureWebJobsStorage")] Stream blob, [Queue("queue-tagging-done", Connection = "AzureWebJobsStorage")] ICollector<string> outputQueueItem, string name, ILogger log)
        {
            log.LogInformation($"C# Blob trigger function started with:{name} \n");

            string tagNames = "";

            List<string> dummyTags = new List<string> (new string[] { "pc game", "person", "mountain", "building", "vehicle", "grass", "outdoor", "landscape", "cloud", "hero"});
            if (name.StartsWith("dummy_"))
            {
                List<string> tagList = dummyTags.OrderBy(arg => Guid.NewGuid()).Take(5).ToList();
                bool first = true;
                foreach (var tag in tagList)
                {
                    if (!first)
                    {
                        tagNames += ", ";
                    }
                    tagNames += tag;
                    first = false;
                }
            } else
            {
                log.LogInformation($"Going to start client");
                // create a ComputerVisionClient using SubscriptionKey and Endpoint
                var client = new ComputerVisionClient(new ApiKeyServiceClientCredentials(subScriptionKey))
                {
                    Endpoint = endPoint
                };
                log.LogInformation($"Client Started");
                // specify desired AI features
                var features = new List<VisualFeatureTypes?> { VisualFeatureTypes.Tags };

                var result = await client.AnalyzeImageInStreamWithHttpMessagesAsync(blob, features);
                // Process the blob

                log.LogInformation($"Got response from Computer Vision with status ({result.Response.StatusCode})");

                // extract tags and store them as Dictionary<string,string>

                bool first = true;
                foreach (var tag in result.Body.Tags)
                {
                    if (!first)
                    {
                        tagNames += ", ";
                    }
                    tagNames += $"{tag.Name}";
                    first = false;
                }

            }

            Dictionary<string, string> tags = new Dictionary<string, string> { { "tags", tagNames } };
            
            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerNameDownload);
            BlobClient blobClient = containerClient.GetBlobClient(name);

            // store tags using custom blob metadata
            await blobClient.SetMetadataAsync(tags);

            outputQueueItem.Add(name);
        }
    }
}
