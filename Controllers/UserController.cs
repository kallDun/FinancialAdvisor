using FinancialAdvisorTelegramBot.Models;
using FinancialAdvisorTelegramBot.Services;
using Microsoft.AspNetCore.Mvc;

namespace FinancialAdvisorTelegramBot.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly IUserService _userService;

        public UserController(ILogger<UserController> logger, IUserService userService)
        {
            _logger = logger;
            _userService = userService;
        }

        [HttpGet]
        public async Task<List<User>> GetUsers()
        {
            return await _userService.GetUsers();
        }

        [HttpPost("add")]
        public async Task<int> AddUser(User user)
        {
            return await _userService.AddUser(user);
        }

        [HttpGet("by_accounts/{accountsCount}")]
        public async Task<List<User>> GetUsersWithMoreOrEqualAccountsThan(int accountsCount)
        {
            return await _userService.GetUsersWithMoreOrEqualAccountsThan(accountsCount);
        }
    }
}