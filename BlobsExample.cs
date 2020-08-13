using Rgd.AzureAbstractions.CloudStructures;
using System.IO;
using System.Threading.Tasks;

namespace Rgd.AzureAbstractions.Tests
{
    class Program3
    {
        static void Main(string[] args)
        {

            new Tester().UsingAzureBlobs().GetAwaiter().GetResult();

        }

        class Tester
        {
            public async Task UsingAzureBlobs()
            {
                //Using the environment variable 'AzureWebJobsStorage' for storage authentication
                //AzureBlobs blobs = new AzureBlobs("samplecontainer");

                //Using literal connection string for authentication
                AzureBlobs blobs = new AzureBlobs("samplecontainer", "my_connection_string", null);

                //Creating "samplecontainer". Loading it if already exists.
                blobs.CreateOrLoadStructure();

                //Set this true to deactivate inner logs.
                blobs.LoggingDisabled = false;

                //Sending a blob to the container from an absolute path
                await blobs.UploadBlob("C:\\Users\\foo\\Desktop\\image.png", "wallpaper.png");

                //Sending a blob to the container from an relative path. In this case the name can be the same for both
                await blobs.UploadBlob("reports.json");

                //Downloading a blob as Strean
                Stream stream = await blobs.DownloadBlobAsStream("wallpaper.png");

                //Downloading a blob as a local file
                await blobs.DownloadBlobAsFile("wallpaper.png", "C:\\Users\\foo\\Desktop\\image-copy.png");


            }
        }
    }
}