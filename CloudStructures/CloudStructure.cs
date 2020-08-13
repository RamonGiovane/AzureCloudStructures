
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Rgd.AzureAbstractions.Logging;
using System;
using System.Threading.Tasks;

/// <summary>
///     2020 - Version 1.4.0 
///     Developed by: Ramon Giovane Dias
/// </summary>
namespace Rgd.AzureAbstractions.CloudStructures
{{
    
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
        protected readonly string ConnectionString;
        // protected readonly CloudStorageAccount storageAccount;

        /// <summary>
        ///     Instantiates a new cloud structure. 
        /// </summary>
        /// <param name="structureName"></param>
        /// <param name="logger"></param>
        public CloudStructure(string structureName) : this(structureName: structureName, logger: null) { }


        /// <summary>
        ///     Instantiates a new cloud structure. 
        /// </summary>
        /// <param name="structureName"></param>
        /// <param name="logger"></param>
        public CloudStructure(string structureName, ILogger logger) : base(logger)
        {

            ConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");

            StructureName = structureName;
        }

        /// <summary>
        ///     Instantiates a new cloud structure. 
        /// </summary>
        /// <param name="structureName"></param>
        /// <param name="logger"></param>
        public CloudStructure(string structureName, string connectionString, ILogger logger) : base(logger)
        {

            ConnectionString = connectionString;

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


        protected bool LogAzureResponse(string operation, Response response)
        {
            string log = new StringBuilder().Append(StructureName).Append(": Attempt to: ").
                Append(operation).Append(" | Result: ").
                Append(response.ReasonPhrase).
                Append(" - ").
                Append(response.Status).ToString();

            if (response.Status >= 200 && response.Status < 300)
            {
                TryLog(log);
                return true;
            }
            TryLogError(log);
            return false;
        }



    }
}

}
