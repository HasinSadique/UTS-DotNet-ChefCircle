using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public UsersController(ApplicationDbContext context)
    {
        _context = context;
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(int id)
    {
        // Fetch user data from the database
        var user = await _context.Users.FindAsync(id);
        
        if (user == null)
        {
            return NotFound(); // Return 404 if user not found
        }
        
        // Return the user data in JSON format
        return Ok(new
        {
            UID = user.UID,
            Fullname = user.Fullname,
            Email = user.Email,
            Phone = user.Phone,
            Address = user.Address,
            Password = user.Password,
            Role = user.Role
        });
    }
}
