using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

namespace TestFunctionApp
{
    public class ResizeQueueTrigger
    {
        [FunctionName("ResizeQueueTrigger")]
        public async Task RunAsync([QueueTrigger("queue-histogram-flattening-done", Connection = "")]string myQueueItem, ILogger log)
        {
            log.LogInformation($"Histogram flattening done of : {myQueueItem}");

            string connectionString = "<Enter Your Connection String here>";

            string containerNameDownload = "processed";

            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerNameDownload);
            BlobClient blobClient = containerClient.GetBlobClient(myQueueItem);

            BlobProperties prop = await blobClient.GetPropertiesAsync();

            BlobDownloadResult blob = blobClient.DownloadContent();

            var destinationSize = 480;
            var destinationImage = new Bitmap(destinationSize, destinationSize);

            using (var graphics = Graphics.FromImage(destinationImage))
            {
                graphics.Clear(Color.White);
                using (var sourceImage = new Bitmap(new MemoryStream(blob.Content.ToArray())))
                {
                    var s = Math.Max(sourceImage.Width, sourceImage.Height);
                    var w = destinationSize * sourceImage.Width / s;
                    var h = destinationSize * sourceImage.Height / s;
                    var x = (destinationSize - w) / 2;
                    var y = (destinationSize - h) / 2;

                    // Use alpha blending in case the source image has transparencies.
                    graphics.CompositingMode = CompositingMode.SourceOver;

                    // Use high quality compositing and interpolation.
                    graphics.CompositingQuality = CompositingQuality.HighQuality;
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

                    graphics.DrawImage(sourceImage, x, y, w, h);
                }
            }

            string containerNameUpload = "thumbnails";

            BlobContainerClient containerClientUpload = blobServiceClient.GetBlobContainerClient(containerNameUpload);
            BlobClient blobClientUpload = containerClientUpload.GetBlobClient(myQueueItem);


            MemoryStream ms = new MemoryStream();
            destinationImage.Save(ms, ImageFormat.Jpeg);
            ms.Position = 0;
            await blobClientUpload.UploadAsync(ms, true);
            ms.Close();

            log.LogInformation($"{myQueueItem} is uploaded into ");

            await blobClientUpload.SetMetadataAsync(prop.Metadata);
        }
    }
}
