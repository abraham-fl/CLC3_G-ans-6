using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using AForge.Imaging.Filters;
using System.Configuration;
using System;

namespace TestFunctionApp
{
    public class ContrastQueueTrigger
    {
        private static readonly string connectionString = Environment.GetEnvironmentVariable("ConnectionStringImageStorage");
        private static readonly string containerNameDownload = Environment.GetEnvironmentVariable("ContainerNameImage");
        private static readonly string containerNameUpload = Environment.GetEnvironmentVariable("ContainerNameProcessed");

        [FunctionName("ContrastQueueTrigger")]
        public async Task RunAsync([QueueTrigger("queue-tagging-done", Connection = "AzureWebJobsStorage")] string myQueueItem, [Queue("queue-histogram-flattening-done", Connection = "AzureWebJobsStorage")] ICollector<string> outputQueueItem, ILogger log)
        {
            log.LogInformation($"Tagging done of : {myQueueItem}");

            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerNameDownload);
            BlobClient blobClient = containerClient.GetBlobClient(myQueueItem);

            BlobProperties prop = await blobClient.GetPropertiesAsync();
            BlobDownloadResult blob = blobClient.DownloadContent();

            Bitmap bmp = new Bitmap(new MemoryStream(blob.Content.ToArray()));

          
            //contrast enhancement
            Bitmap sharpenImage = new Bitmap(bmp.Width, bmp.Height);

            int filterWidth = 3;
            int filterHeight = 3;
            int w = bmp.Width;
            int h = bmp.Height;

            double[,] filter = new double[filterWidth, filterHeight];

            filter[0, 0] = filter[0, 1] = filter[0, 2] = filter[1, 0] = filter[1, 2] = filter[2, 0] = filter[2, 1] = filter[2, 2] = -1;
            filter[1, 1] = 9;

            double factor = 1.0;
            double bias = 0.0;

            Color[,] result = new Color[bmp.Width, bmp.Height];

            for (int x = 0; x < w; ++x)
            {
                for (int y = 0; y < h; ++y)
                {
                    double red = 0.0, green = 0.0, blue = 0.0;

                    for (int filterX = 0; filterX < filterWidth; filterX++)
                    {
                        for (int filterY = 0; filterY < filterHeight; filterY++)
                        {
                            int imageX = (x - filterWidth / 2 + filterX + w) % w;
                            int imageY = (y - filterHeight / 2 + filterY + h) % h;

                            Color imageColor = bmp.GetPixel(imageX, imageY);


                            red += imageColor.R * filter[filterX, filterY];
                            green += imageColor.G * filter[filterX, filterY];
                            blue += imageColor.B * filter[filterX, filterY];
                        }
                        int r = Math.Min(Math.Max((int)(factor * red + bias), 0), 255);
                        int g = Math.Min(Math.Max((int)(factor * green + bias), 0), 255);
                        int b = Math.Min(Math.Max((int)(factor * blue + bias), 0), 255);

                        result[x, y] = Color.FromArgb(r, g, b);
                    }
                }
            }
            for (int i = 0; i < w; ++i)
            {
                for (int j = 0; j < h; ++j)
                {
                    sharpenImage.SetPixel(i, j, result[i, j]);
                }
            }
            log.LogInformation($"{myQueueItem} finished preprocessing");

            BlobContainerClient containerClientUpload = blobServiceClient.GetBlobContainerClient(containerNameUpload);
            BlobClient blobClientUpload = containerClientUpload.GetBlobClient(myQueueItem);

            MemoryStream ms = new MemoryStream();
            sharpenImage.Save(ms, ImageFormat.Jpeg);
            ms.Position = 0;
            await blobClientUpload.UploadAsync(ms, true);
            ms.Close();

            log.LogInformation($"{myQueueItem} is uploaded into ");

            await blobClientUpload.SetMetadataAsync(prop.Metadata);

            outputQueueItem.Add(myQueueItem);
        }
    }
}
