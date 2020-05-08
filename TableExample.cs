using Microsoft.WindowsAzure.Storage.Table;
using Rgd.AzureAbstractions.CloudStructures;
using System;
using System.Threading.Tasks;

namespace Rgd.AzureAbstractions.Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            new Tester().UsingAzureTable().GetAwaiter().GetResult();
        }

        class Tester
        {
            public async Task UsingAzureTable()
            {
                //Using environment variable
                AzureTable<EntityExample> table = new AzureTable<EntityExample>(tableName: "sampletable", log: null);

                //Using literal conntection string
                //AzureTable<EntityExample> table = new AzureTable<EntityExample>("sampletable", "my_connection_string", null);

                //Creating "sampletable". Loading it if already exists.
                table.CreateOrLoadStructure(); 

                //Inserting 200 random items
                for (int i = 0; i <= 200; i++)
                    table.Insert(new EntityExample("partitionkey", i.ToString(), 10));

                //Uploading table modifications to the cloud
                await table.CommitAsync();
                
            }
        }

        //Entity capable to be inserted in an AzureTable
        class EntityExample : TableEntity
        {
            public int FooProperty { get; set; }
            
            public EntityExample() { }

            public EntityExample(string partitionKey, string rowKey, int foo)
            {
                PartitionKey = partitionKey;
                RowKey = rowKey;
                FooProperty = foo;
            }
        }
    }
}
