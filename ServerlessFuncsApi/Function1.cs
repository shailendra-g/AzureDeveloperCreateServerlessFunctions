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

namespace ServerlessFuncsApi
{
    public static class ToDoApi
    {
        static List<ToDo> items = new List<ToDo>();

        [FunctionName("CreateToDo")]
        public static async Task<IActionResult> CreateToDo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "todo")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Creating a new todo list item.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var input = JsonConvert.DeserializeObject<ToDoCreateModel>(requestBody);

            var todo = new ToDo { TaskDescription = input.TaskDescription };
            items.Add(todo);

            return new ObjectResult(todo);
        }

        [FunctionName("GetToDoList")]
        public static IActionResult GetAllToDoItems(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todo")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Get all ToDo items");
            return new ObjectResult(items);
        }

        [FunctionName("GetToDoById")]
        public static IActionResult GetToDoItemById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todo/{id}")] HttpRequest req,
            ILogger log, string id)
        {
            log.LogInformation("Get todo item by id");

            var todo = items.FirstOrDefault(a => a.Id == id);

            if(todo == null)
            {
                return new NotFoundResult();
            }

            return new ObjectResult(todo);
        }

        [FunctionName("UpdateToDoItem")]
        public static async Task<IActionResult> UpdateToDoItem(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "todo/{id}")] HttpRequest req,
            ILogger log, string id)
        {
            log.LogInformation($"updating item {id}");

            var item = items.FirstOrDefault(a => a.Id == id);
            if (item == null)
            {
                return new NotFoundResult();
            }

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var input = JsonConvert.DeserializeObject<ToDoUpdateModel>(requestBody);

            item.IsCompleted = input.IsCompleted;
            if(!string.IsNullOrEmpty(input.TaskDescription))
            {
                item.TaskDescription = input.TaskDescription;
            }
            return new ObjectResult(item); 
        }

        [FunctionName("DeleteToDoItem")]
        public static IActionResult DeleteToDoItem(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "todo/{id}")] HttpRequest req,
            ILogger log, string id)
        {
            log.LogInformation($"Deleting the id {id}");

            var item = items.FirstOrDefault(x => x.Id == id);
            if(item == null)
            {
                return new NotFoundResult();
            }

            items.Remove(item);
            return new OkResult();
        }
    }
}
