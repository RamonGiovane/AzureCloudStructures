# AzureCloudStructures
 Abstraction classes to make easier the management of the Azure cloud tables, queues and blobs (uses .Net Core 2.0).
 
 The purpose here is to use less lines of code to do simple things, such as:
 
 ### Insert data in tables
 ### Upload a blob to an Azure Storage Container
 ### Enqueue a Azure Queue
 
 
# Example
Only 3 lines of code for inserting a blob to the storage:

<img src="https://github.com/RamonGiovane/AzureCloudStructures/blob/master/example1.png?raw=true">

 
# Dependencies
Compatible with: .NetCore 2.* projects.

The following NuGet packages are necessary:
- Azure.Storage.Blobs Version >= 12.4.4
- Azure.Storage.Queues" Version >= 12.3.2
- Microsoft.Azure.Cosmos.Table >= 1.0.7
- Microsoft.Extensions.Logging.Abstractions >= 2.2.0

