using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;

using CloudBlobClient = Microsoft.WindowsAzure.Storage.Blob.CloudBlobClient;
using CloudBlobContainer = Microsoft.WindowsAzure.Storage.Blob.CloudBlobContainer;
using CloudBlockBlob = Microsoft.WindowsAzure.Storage.Blob.CloudBlockBlob;
using System.Drawing;
using System.Drawing.Imaging;
using SixLabors.ImageSharp.ColorSpaces;
using System.Runtime.InteropServices;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace TestFunctionApp
{
    public class Function1
    {
        [FunctionName("Function1")]
        public async Task RunAsync([QueueTrigger("queue-tagging-done", Connection = "")] string myQueueItem, [Queue("queue-histogram-flattening-done", Connection = "")] ICollector<string> outputQueueItem, ILogger log)
        {
            log.LogInformation($"Tagging done of : {myQueueItem}");

            string connectionString = "<Enter Your Connection String Here>";

            string containerNameDownload = "images";

            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerNameDownload);
            BlobClient blobClient = containerClient.GetBlobClient(myQueueItem);

            BlobProperties prop = await blobClient.GetPropertiesAsync();

            BlobDownloadResult blob = blobClient.DownloadContent();
            

            Bitmap bmp = new Bitmap(new MemoryStream(blob.Content.ToArray()));

            log.LogInformation($"pixel at 0,0: {bmp.GetPixel(0, 0)}");
            log.LogInformation($"pixel at 1,1: {bmp.GetPixel(1, 1)}");
            log.LogInformation($"dimensions: h:{bmp.Height}, w:{bmp.Width}");

            Bitmap img = bmp;
            int w = img.Width;
            int h = img.Height;
            BitmapData sd = img.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            int bytes = sd.Stride * sd.Height;
            byte[] buffer = new byte[bytes];
            byte[] result = new byte[bytes];
            Marshal.Copy(sd.Scan0, buffer, 0, bytes);
            img.UnlockBits(sd);
            int current = 0;
            double[] pn = new double[256];
            for (int p = 0; p < bytes; p += 4)
            {
                pn[buffer[p]]++;
            }
            for (int prob = 0; prob < pn.Length; prob++)
            {
                pn[prob] /= (w * h);
            }
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    current = y * sd.Stride + x * 4;
                    double sum = 0;
                    for (int i = 0; i < buffer[current]; i++)
                    {
                        sum += pn[i];
                    }
                    for (int c = 0; c < 3; c++)
                    {
                        result[current + c] = (byte)Math.Floor(255 * sum);
                    }
                    result[current + 3] = 255;
                }
            }
            Bitmap res = new Bitmap(w, h);
            BitmapData rd = res.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(result, 0, rd.Scan0, bytes);
            res.UnlockBits(rd);

            string containerNameUpload = "processed";

            BlobContainerClient containerClientUpload = blobServiceClient.GetBlobContainerClient(containerNameUpload);
            BlobClient blobClientUpload = containerClientUpload.GetBlobClient(myQueueItem);


            MemoryStream ms = new MemoryStream();
            res.Save(ms, ImageFormat.Jpeg);
            ms.Position = 0;
            await blobClientUpload.UploadAsync(ms, true);
            ms.Close();

            log.LogInformation($"{myQueueItem} is uploaded into ");

            await blobClientUpload.SetMetadataAsync(prop.Metadata);

            outputQueueItem.Add(myQueueItem);
        }
    }
}
