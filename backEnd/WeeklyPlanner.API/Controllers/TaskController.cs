using Microsoft.AspNetCore.Mvc;
using WeeklyPlanner.Model.Entities;
using WeeklyPlanner.Model.Repositories;

namespace WeeklyPlanner.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskController : ControllerBase
    {
        protected TaskRepository Repository { get; }

        public TaskController(TaskRepository repository)
        {
            Repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        // GET: api/task/{taskId}
        [HttpGet("{taskId}")]
        public ActionResult<PlannerTask> GetTask([FromRoute] int taskId)
        {
            var task = Repository.GetTaskById(taskId);
            if (task == null)
            {
                return NotFound($"Task with ID {taskId} not found.");
            }

            return Ok(task);
        }

        // GET: api/task
        [HttpGet]
        public ActionResult<IEnumerable<PlannerTask>> GetAllTasks()
        {
            var tasks = Repository.GetTask();
            return Ok(tasks);
        }

        // GET: api/task/getRoomiesForTask/{taskId}
        [HttpGet("getRoomiesForTask/{taskId}")]
        public ActionResult<IEnumerable<Roomie>> GetRoomiesForTask([FromRoute] int taskId)
        {
            var roomies = Repository.GetRoomiesForTask(taskId);
            if (roomies == null || !roomies.Any())
            {
                return NotFound($"No roomies found for Task ID {taskId}.");
            }

            return Ok(roomies);
        }

        // POST: api/task
        [HttpPost]
        public ActionResult CreateTask([FromBody] PlannerTask task)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (task == null)
            {
                return BadRequest("Task data is invalid.");
            }

            bool result = Repository.InsertTask(task);
            if (result)
            {
                return Ok();
            }

            return BadRequest("Failed to create task.");
        }

        // POST: api/task/addRoomiesToTask/{taskId}
        [HttpPost("addRoomiesToTask/{taskId}")]
        public ActionResult AddRoomiesToTask([FromRoute] int taskId, [FromBody] List<int> roomieIds)
        {
            if (roomieIds == null || !roomieIds.Any())
            {
                return BadRequest("No roomie IDs provided.");
            }

            foreach (var roomieId in roomieIds)
            {
                bool result = Repository.AssignRoomieToTask(taskId, roomieId);
                if (!result)
                {
                    return BadRequest($"Failed to assign Roomie ID {roomieId} to Task ID {taskId}.");
                }
            }

            return Ok("Roomies assigned successfully.");
        }

        // PUT: api/task/{taskId}
        [HttpPut("{taskId}")]
        public ActionResult UpdateTask([FromRoute] int taskId, [FromBody] PlannerTask task)
        {
            if (task == null || taskId != task.TaskId)
            {
                return BadRequest("Task data is invalid or Task IDs do not match.");
            }

            var existingTask = Repository.GetTaskById(taskId);
            if (existingTask == null)
            {
                return NotFound($"Task with ID {taskId} not found.");
            }

            bool result = Repository.UpdateTask(task);
            if (result)
            {
                return Ok();
            }

            return BadRequest("Failed to update task.");
        }

        // DELETE: api/task/{taskId}
        [HttpDelete("{taskId}")]
        public ActionResult DeleteTask([FromRoute] int taskId)
        {
            var existingTask = Repository.GetTaskById(taskId);
            if (existingTask == null)
            {
                return NotFound($"Task with ID {taskId} not found.");
            }

            bool result = Repository.DeleteTask(taskId);
            if (result)
            {
                return NoContent();
            }

            return BadRequest("Failed to delete task.");
        }

        // DELETE: api/task/removeRoomieFromTask/{taskId}/{roomieId}
        [HttpDelete("removeRoomieFromTask/{taskId}/{roomieId}")]
        public ActionResult RemoveRoomieFromTask([FromRoute] int taskId, [FromRoute] int roomieId)
        {
            bool result = Repository.RemoveRoomieFromTask(taskId, roomieId);
            if (!result)
            {
                return BadRequest($"Failed to remove Roomie ID {roomieId} from Task ID {taskId}.");
            }

            return NoContent();
        }
    }
}