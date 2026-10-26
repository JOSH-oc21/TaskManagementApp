using System.Text.Json;
using TaskManagementApp.Models;

namespace TaskManagementApp.Services;

public class TaskStore
{
    private readonly string _filePath;

    public TaskStore()
    {
        _filePath = Path.Combine(FileSystem.AppDataDirectory, "tasks.json");
    }

    public async Task<List<TaskItem>> GetTasksAsync()
    {
        if (!File.Exists(_filePath))
        {
            return GetSeedTasks();
        }

        try
        {
            var json = await File.ReadAllTextAsync(_filePath);
            if (string.IsNullOrWhiteSpace(json))
            {
                return GetSeedTasks();
            }

            var tasks = JsonSerializer.Deserialize<List<TaskItem>>(json);
            return tasks ?? GetSeedTasks();
        }
        catch
        {
            return GetSeedTasks();
        }
    }

    public async Task SaveTasksAsync(List<TaskItem> tasks)
    {
        var json = JsonSerializer.Serialize(tasks, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_filePath, json);
    }

    public async Task AddTaskAsync(TaskItem task)
    {
        var tasks = await GetTasksAsync();
        tasks.Insert(0, task);
        await SaveTasksAsync(tasks);
    }

    public async Task UpdateTaskAsync(TaskItem updatedTask)
    {
        var tasks = await GetTasksAsync();
        var index = tasks.FindIndex(task => task.Id == updatedTask.Id);
        if (index >= 0)
        {
            tasks[index] = updatedTask;
            await SaveTasksAsync(tasks);
        }
    }

    public async Task DeleteTaskAsync(Guid id)
    {
        var tasks = await GetTasksAsync();
        var filtered = tasks.Where(task => task.Id != id).ToList();
        await SaveTasksAsync(filtered);
    }

    public async Task ToggleTaskAsync(Guid id)
    {
        var tasks = await GetTasksAsync();
        var task = tasks.FirstOrDefault(item => item.Id == id);
        if (task != null)
        {
            task.IsCompleted = !task.IsCompleted;
            await SaveTasksAsync(tasks);
        }
    }

    private static List<TaskItem> GetSeedTasks()
    {
        return new List<TaskItem>
        {
            new TaskItem
            {
                Title = "Plan sprint tasks",
                Description = "Review the current sprint and assign work for the week.",
                Category = "Work",
                Priority = "High",
                DueDate = DateTime.Today.AddDays(2),
                IsCompleted = false
            },
            new TaskItem
            {
                Title = "Prepare groceries",
                Description = "Buy ingredients for the weekend meals and snacks.",
                Category = "Personal",
                Priority = "Medium",
                DueDate = DateTime.Today.AddDays(1),
                IsCompleted = true
            }
        };
    }
}
