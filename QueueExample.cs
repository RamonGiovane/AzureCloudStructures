using BankOne.Automa.Consts;
using Rgd.AzureAbstractions.CloudStructures;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace Rgd.AzureAbstractions.Tests
{
    class Program2
    {
        static void Main(string[] args)
        {

            new Tester().UsingAzureQueue().GetAwaiter().GetResult();

        }

        private class Tester
        {
            public async Task UsingAzureQueue()
            {

                //Using the environment variable 'AzureWebJobsStorage' for storage authentication
                AzureQueue queue = new AzureQueue("samplequeue");

                //Using literal connection string for authentication
                //AzureQueue queue = new AzureQueue("samplequeue", "my_connection_string", null);

                //Creating "samplequeue". Loading it if already exists.
                queue.CreateOrLoadStructure();

                //Set this true to deactivate inner logs.
                queue.LoggingDisabled = false;

                //Inserting 3 random messages
                for (int i = 1; i <= 3; i++)
                    await queue.SendAsync("Message " + i);


                //Processing and deleting from the queue ALL messages. Sometimes Azure services may struggle with delays.
                List<string> messages = await queue.GetMessages(); 

                foreach (var m in messages)
                    Console.WriteLine(m);

                //Inserting an encoded message
                await queue.SendAsBase64Async("Hello World!");

                //Processing and deleting the encoded message
                Console.WriteLine(await queue.GetMessage());


            }

        }
    }
}
