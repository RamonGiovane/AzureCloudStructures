
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
///     2020 - Version 1.3 
///     Developed by: Ramon Giovane Dias
/// </summary>
namespace Rgd.AzureAbstractions.CloudStructures
{
    /// <summary>
    ///  Manipulates a cloud table from the Azure storage.
    ///  An instance of this class can only handle one table, which must be specified by it's name on the constructor.
    ///  With this class is possible to create, load or delete a table, and insert to, query or delete an entity from a table.
    ///  The generic type defined by TEntity is the class entity to be stored to or retrieved from a table. It must implement
    ///  the Azure's interface to define the corresponding row and partition key of a table entry.
    /// </summary>
    public class AzureTable<TEntity> : CloudStructure
        where TEntity : ITableEntity, new()

    {

        private CloudTable _table;

        private TableBatchOperation _batch;

        private bool _IsCreated;

        private Queue<TableBatchOperation> _batchQueue;

        public static readonly int OPERATIONS_LIMIT = 100;

        public AzureTable(string tableName) : this(tableName, log: null) { }

        
        public AzureTable(string tableName, ILogger log) : base(tableName, log){ }

        public AzureTable(string tableName, string storageConnectionString)
            : this(tableName, storageConnectionString, log: null) { }


        public AzureTable(string tableName, string storageConnectionString, ILogger log)
            : base(tableName, storageConnectionString, log) { }

        public override bool IsCreated()
        {

            if (_IsCreated) return true;

            return _table != null && Task.Run(() => _table.ExistsAsync()).Result;
        }

        /// <summary>
        ///     Insert a new row in a partition of the table. 
        ///     The row (identified by the RowKey) may not exist already on the table.
        ///     The partition (identified by the PartitionKey) will be created if doesn't exists.
        ///     The inserted rows by this method will be waiting to be commited. Use CommitAsync().
        /// </summary>
        /// <param name="entity"></param>
        public void Insert(TEntity entity)
        {
            if (_batch == null) 
                _batch = new TableBatchOperation();
            

            //If the batch operation has more than the instructions limit, then the batch will be saved in a queue
            else if(_batch.Count == OPERATIONS_LIMIT)
            {
                TryLog($"Warning: {StructureName} now contains more than {OPERATIONS_LIMIT} operations (recommended limit) to be commited in a single batch.");

                if(_batchQueue == null)
                    _batchQueue = new Queue<TableBatchOperation>();

                _batchQueue.Enqueue(_batch);
               
                _batch = new TableBatchOperation();
            }

            _batch.InsertOrReplace(entity);

        }

        /// <summary>
        ///     Performs chages made (such as Insert or Delete an entity) in the table.
        /// </summary>
        /// <returns></returns>
        public async Task CommitAsync()
        {
            
            while (true)
            {
                await CommitAsync(_batch);

                if (_batchQueue == null || _batchQueue.Count == 0)
                    break;
                
                _batch = _batchQueue.Dequeue();
            }

        }

        private async Task CommitAsync(TableBatchOperation batch)
        {
            TryLog(StructureName + " Batch count: " + batch.Count);
            if (batch.Count == 0) return;

            
            else
            {
                await _table.ExecuteBatchAsync(batch);
                TryLog("Batch executed in " + StructureName);
            }

            batch.Clear();
        }




        /// <summary>
        ///  Opens the connection with an Azure Table on the Storage.
        ///  The table will be created if it doesn't exists.
        /// </summary>
        /// <returns></returns>
        public override bool CreateOrLoadStructure()
        {

            if (!IsCreated())
            {

                Task.Run(() => _table.CreateIfNotExistsAsync().Result);

                _IsCreated = IsCreated();

                TryLog(StructureName + (_IsCreated ? " was created. " : " could not be created."));

                return _IsCreated;
            }

            TryLog(StructureName + " was loaded.");

            return true;


        }


        public override bool DeleteStructure()
        {
            return _table != null && Task.Run(() => _table.DeleteIfExistsAsync()).Result;
        }

        public async Task<List<TEntity>> RetriveAll(string partitionKey, string extraFilter = null, string extraOperation = TableOperators.And)
        {
            var keyCondition = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey);

            var condition = extraFilter == null ? keyCondition : TableQuery.CombineFilters(keyCondition, extraOperation, extraFilter);

            TryLog(StructureName + " will retrieve data with condition: " + condition);

            var query = new TableQuery<TEntity>().Where(condition);

            // Print the fields for each customer.
            TableContinuationToken token = null;

            TableQuerySegment<TEntity> resultSegment;

            try
            {
                do
                {
                    resultSegment = await _table.ExecuteQuerySegmentedAsync(query, token);

                    token = resultSegment.ContinuationToken;

                    foreach (var entity in resultSegment.Results)
                    {
                        TryLog(StructureName + $" is retrieving: {entity.PartitionKey}\t{entity.RowKey}");

                    }
                } while (token != null);

                return resultSegment.Results;
            }
            catch (Exception e)
            {
                TryLogError("Exception ocurred: " + e);
            }
            return new List<TEntity>();
        }

        public async Task<TEntity> Retrieve(string partitionKey, string rowKey)
        {

            // Create a retrieve operation that expects a customer entity.
            TableOperation retrieveOperation =
                TableOperation.Retrieve<TEntity>(partitionKey ?? "", rowKey ?? "");

            // Execute the operation.
            TableResult retrievedResult = await _table.ExecuteAsync(retrieveOperation);

            // Assign the result to a AssignorInvoice object.
            return retrievedResult == null ? default : (TEntity)retrievedResult.Result;
        }

        public Task<TEntity> Retrieve(TEntity invoice)
        {
            return Retrieve(invoice.PartitionKey, invoice.RowKey);
        }

        public async void Delete(TEntity entity)
        {

            // Create the Delete TableOperation and then execute it.
            if (entity != null)
            {
                TableOperation deleteOperation = TableOperation.Delete(entity);

                // Execute the operation.
                await _table.ExecuteAsync(deleteOperation);

                TryLog("Entity deleted from " + StructureName + " with RK: " + entity.RowKey);
            }

            else
                TryLogError("Cannot delete a null entity from " + StructureName);
        }

    }
}
