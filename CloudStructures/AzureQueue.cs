
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

/// <summary>
///     2020 - Version 1.4.0 
///     Developed by: Ramon Giovane Dias
/// </summary>
namespace Rgd.AzureAbstractions.CloudStructures
{

    public class AzureQueue : CloudStructure
    {
        private QueueClient _queue;

        public AzureQueue(string structureName) : base(structureName) { }

        public AzureQueue(string structureName, ILogger logger) : base(structureName, logger) { }

        public AzureQueue(string structureName, string connectionString, ILogger logger) : base(structureName, connectionString, logger) { }

        public override bool CreateOrLoadStructure()
        {
            _queue = new QueueClient(ConnectionString, StructureName);

            var response = Task.Run(() => _queue.CreateIfNotExistsAsync()).Result;

            if (response == null)
            {
                TryLog(StructureName + " is already created.");
                return true;
            }

            return LogAzureResponse("Create", response);
        }


        /// <summary>
        ///    Enqueue a string as a message.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task<bool> SendAsync(string message)
        {
            var resp = (await _queue.SendMessageAsync(message)).GetRawResponse();

            return LogAzureResponse("Send Message", resp);
        }

        /// <summary>
        ///    Encodes a UTF-8 string to a Base-64 string and enqueues it as message.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task<bool> SendAsBase64Async(string message)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            message = Convert.ToBase64String(buffer);

            var resp = (await _queue.SendMessageAsync(message)).GetRawResponse();

            return LogAzureResponse("Send Message", resp);
        }


        public override bool DeleteStructure()
        {
            return _queue != null && Task.Run(() => _queue.DeleteIfExistsAsync()).Result;

        }

        public override bool IsCreated()
        {
            return Task.Run(() => _queue.ExistsAsync()).Result;
        }


        /// <summary>
        ///    Pops and returns a message from the queue.
        /// </summary>
        /// <returns>The message text or null if there is no messages to return</returns>
        public async Task<string> GetMessage()
        {
            QueueMessage[] messages = await _queue.ReceiveMessagesAsync();

            if (messages.Length == 0) return null;


            var m = messages[0];

            string msg = m.MessageText;

            _queue.DeleteMessage(m.MessageId, m.PopReceipt);

            return msg;

        }

        /// <summary>
        ///    Pops and returns ALL messages from the queue as a List of strings.
        /// </summary>
        /// <returns>A list of strings or an empty list if there is no messages to return</returns>
        public async Task<List<string>> GetMessages()
        {
            List<string> list = new List<string>();

            QueueMessage[] messages = await _queue.ReceiveMessagesAsync();

            foreach (var m in messages)
            {
                list.Add(m.MessageText);

                _queue.DeleteMessage(m.MessageId, m.PopReceipt);
            }

            return list;
        }
    }
}
