using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage;

namespace ServerlessFuncsApi
{
    public static class ToDoApi
    {
        //static List<ToDo> items = new List<ToDo>();

        [FunctionName("CreateToDo")]
        public static async Task<IActionResult> CreateToDo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "todo")] HttpRequest req,
            [Table("todos", Connection = "AzureWebJobsStorage")] IAsyncCollector<ToDoTableEntity> todoTable,
            ILogger log)
        {
            log.LogInformation("Creating a new todo list item.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var input = JsonConvert.DeserializeObject<ToDoCreateModel>(requestBody);

            var todo = new ToDo { TaskDescription = input.TaskDescription };
            //items.Add(todo);
            await todoTable.AddAsync(todo.ToTableEntity());

            return new OkObjectResult(todo);
        }

        [FunctionName("GetToDoList")]
        public static async Task<IActionResult> GetAllToDoItemsAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todo")] HttpRequest req,
            [Table("todos", Connection = "AzureWebJobsStorage")] CloudTable todoTable,
            ILogger log)
        {
            log.LogInformation("Get all ToDo items");
            var query = new TableQuery<ToDoTableEntity>();
            var segment = await todoTable.ExecuteQuerySegmentedAsync(query, null);
            return new OkObjectResult(segment.Select(Mappings.ToToDo));
        }

        [FunctionName("GetToDoById")]
        public static IActionResult GetToDoItemById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todo/{id}")] HttpRequest req,
            [Table("todos", "TODO", "{id}", Connection ="AzureWebJobsStorage")] ToDoTableEntity todo,
            ILogger log, string id)
        {
            log.LogInformation("Get todo item by id");

            if(todo == null)
            {
                return new NotFoundResult();
            }

            return new OkObjectResult(todo.ToToDo());
        }

        [FunctionName("UpdateToDoItem")]
        public static async Task<IActionResult> UpdateToDoItem(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "todo/{id}")] HttpRequest req,
            [Table("todos", Connection = "AzureWebJobsStorage")] CloudTable todoTable,
            ILogger log, string id)
        {
            log.LogInformation($"updating item {id}");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var input = JsonConvert.DeserializeObject<ToDoUpdateModel>(requestBody);

            var findOperation = TableOperation.Retrieve<ToDoTableEntity>("TODO", id);
            var findResult = await todoTable.ExecuteAsync(findOperation);
            if (findResult.Result == null)
            {
                return new NotFoundResult();
            }

            var existingRow = (ToDoTableEntity)findResult.Result;
            existingRow.IsCompleted = input.IsCompleted;
            if(!string.IsNullOrEmpty(input.TaskDescription))
            {
                existingRow.TaskDescription = input.TaskDescription;
            }

            var replaceOperation = TableOperation.Replace(existingRow);
            await todoTable.ExecuteAsync(replaceOperation);
            return new OkObjectResult(existingRow.ToToDo()); 
        }

        [FunctionName("DeleteToDoItem")]
        public static IActionResult DeleteToDoItem(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "todo/{id}")] HttpRequest req,
            [Table("todos", Connection ="AzureWebJobsStorage")] CloudTable todoTable,
            ILogger log, string id)
        {
            log.LogInformation($"Deleting the id {id}");
            var deleteOperation = TableOperation.Delete(new TableEntity() { PartitionKey = "TODO", RowKey = id, ETag = "*" });

            try
            {
                var deleteResult = todoTable.ExecuteAsync(deleteOperation);
            }
            catch(StorageException e) when (e.RequestInformation.HttpStatusCode == 404)
            {
                return new NotFoundResult();
            }

            return new OkResult();
        }
    }
}
