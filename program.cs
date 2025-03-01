using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
var app = builder.Build();

// Middleware for logging requests
app.Use(async (context, next) =>
{
    Console.WriteLine($"Request: {context.Request.Method} {context.Request.Path}");
    await next.Invoke();
});

app.MapControllers();
app.Run();

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private static List<User> users = new List<User>();

    [HttpGet]
    public IActionResult GetUsers() => Ok(users);

    [HttpGet("{id}")]
    public IActionResult GetUser(int id)
    {
        var user = users.FirstOrDefault(u => u.Id == id);
        return user == null ? NotFound() : Ok(user);
    }

    [HttpPost]
    public IActionResult CreateUser([FromBody] User user)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        
        user.Id = users.Count + 1;
        users.Add(user);
        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
    }

    [HttpPut("{id}")]
    public IActionResult UpdateUser(int id, [FromBody] User user)
    {
        var existingUser = users.FirstOrDefault(u => u.Id == id);
        if (existingUser == null)
            return NotFound();
        
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        
        existingUser.Name = user.Name;
        existingUser.Email = user.Email;
        return NoContent();
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteUser(int id)
    {
        var user = users.FirstOrDefault(u => u.Id == id);
        if (user == null)
            return NotFound();
        
        users.Remove(user);
        return NoContent();
    }
}

public class User
{
    public int Id { get; set; }
    
    [Required]
    [MinLength(2)]
    public string Name { get; set; }
    
    [Required]
    [EmailAddress]
    public string Email { get; set; }
}
