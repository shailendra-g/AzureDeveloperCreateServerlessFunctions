using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServerlessFuncsApi
{
    public class ToDo
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("n");
        public DateTime CreatedTime { get; set; } = DateTime.UtcNow;
        public string TaskDescription { get; set; }
        public bool IsCompleted { get; set; }
    }

    public class ToDoCreateModel
    {
        public string TaskDescription { get; set; }
    }

    public class ToDoUpdateModel
    {
        public string TaskDescription { get; set; }
        public bool IsCompleted { get; set; }
    }

    public class ToDoTableEntity : TableEntity
    {
        public DateTime CreatedTime { get; set; } = DateTime.UtcNow;
        public string TaskDescription { get; set; }
        public bool IsCompleted { get; set; }
    }

    public static class Mappings
    {
        public static ToDoTableEntity ToTableEntity(this ToDo todo)
        {
            return new ToDoTableEntity
            {
                PartitionKey = "TODO",
                RowKey = todo.Id,
                CreatedTime = todo.CreatedTime,
                TaskDescription = todo.TaskDescription,
                IsCompleted = todo.IsCompleted
            };
        }

        public static ToDo ToToDo(this ToDoTableEntity todo)
        {
            return new ToDo
            {
                Id = todo.RowKey,
                CreatedTime = todo.CreatedTime,
                TaskDescription = todo.TaskDescription,
                IsCompleted = todo.IsCompleted
            };
        }
    }
}
