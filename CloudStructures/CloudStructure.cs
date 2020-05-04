
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Rgd.AzureAbstractions.Logging;
using System;

/// <summary>
///     2020 - Version 1.2 
///     Developed by: Ramon Giovane Dias
/// </summary>
namespace Rgd.AzureAbstractions.CloudStructures
{
    /// <summary>
    ///  Manipulates a cloud structure from the Azure storage. eg. Azure blob containers, Azure Tables, Azure Queues.
    ///  An instance of this class can only handle one strucutre, which must be specified by it's name on the constructor.
    ///  This class specifies methods to create, load or delete a structure, as well as check a structure name and the existance of a 
    ///  structure.
    ///  A Logger instance may be set to trace the operations made in the structures.
    /// </summary>
    public abstract class CloudStructure : AbstractLogger
    {
        public readonly string StructureName;

        protected readonly CloudStorageAccount storageAccount;
        
        /// <summary>
        ///     Instantiates a new cloud structure. 
        ///     The environment variable "AzureWebJobsStorage" must be set with the cloud storage account connection string
        ///     in order to give this class access to the Azure Storage.
        ///     Otherwise, an access denied exception will be thrown.
        /// </summary>
        /// <param name="structureName"></param>
        /// <param name="logger"></param>
        public CloudStructure(string structureName) : this(structureName: structureName, logger: null) { }


        /// <summary>
        ///     Instantiates a new cloud structure. 
        ///     The environment variable "AzureWebJobsStorage" must be set with the cloud storage account connection string
        ///     in order to give this class access to the Azure Storage.
        ///     Otherwise, an access denied exception will be thrown.
        /// </summary>
        /// <param name="structureName"></param>
        /// <param name="logger"></param>
        public CloudStructure(string structureName, ILogger logger) : base(logger)
        {

            storageAccount = CloudStorageAccount.Parse(Environment.GetEnvironmentVariable("AzureWebJobsStorage"));

            StructureName = structureName;
        }

        /// <summary>
        ///     Instantiates a new cloud structure. 
        ///     The cloud storage account connection string must be passed
        ///     in order to give this class access to the Azure Storage.
        ///     An invalid key will result in an access denied exception.
        /// </summary>
        /// <param name="structureName"></param>
        /// <param name="logger"></param>
        public CloudStructure(string structureName, string storageConnectionString) 
            : this(structureName: structureName, storageConnectionString: storageConnectionString, logger: null) { }

        /// <summary>
        ///     Instantiates a new cloud structure. 
        ///     The cloud storage account connection string must be passed
        ///     in order to give this class access to the Azure Storage.
        ///     An invalid key will result in an access denied exception.
        /// </summary>
        /// <param name="structureName"></param>
        /// <param name="logger"></param>
        public CloudStructure(string structureName, string storageConnectionString, ILogger logger) : base(logger)
        {
            storageAccount = CloudStorageAccount.Parse(storageConnectionString);

            StructureName = structureName;
        }

        /// <summary>
        ///     Checks if the structure is created.
        /// </summary>
        /// <returns></returns>
        public abstract bool IsCreated();

        /// <summary>
        ///     Creates a new structure or loads an existing one with the given StructureName.
        /// </summary>
        /// <returns></returns>
        public abstract bool CreateOrLoadStructure();

        /// <summary>
        ///     Deletes the structure with the given StructureName.
        /// </summary>
        /// <returns></returns>
        public abstract bool DeleteStructure();


    }
}
