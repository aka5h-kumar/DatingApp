

using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Authorize]
    public class AccountController:BaseApiController
    {
        private readonly DataContext _context;
        private readonly ITokenServices _tokenService;
        public AccountController(DataContext context, ITokenServices tokenService)
        {
            this._tokenService = tokenService;
            this._context = context;
            
        }

        [HttpPost("register")] //POST:api/Account/register
        public async Task<ActionResult<UserDtos>> Register(RegisterDTOs registerDt ) {

            if(await UserExists(registerDt.Username)){
                return BadRequest("User Name already exists");
            }
            using var hmac = new HMACSHA512();

            var user = new AppUser{
                UserName = registerDt.Username.ToLower(),
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDt.Password)),
                PasswordSalt = hmac.Key
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return new UserDtos{
                Username = user.UserName,
                Token = _tokenService.CreateToken(user)
            };
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<UserDtos>> Login(LoginDto loginDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x=>x.UserName == loginDto.Username);

            if(user ==null) return Unauthorized("Invalid user");
            var hmac = new HMACSHA512(user.PasswordSalt);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

            for(int i=0;i<computedHash.Length;i++){
                if(computedHash[i] != user.PasswordHash[i]) return Unauthorized("Invalid Password");
            }
            return new UserDtos{
                Username = user.UserName,
                Token = _tokenService.CreateToken(user)
            };
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<AppUser>>> GetUser(){

            var users = await _context.Users.ToListAsync();
           
            return users;

        }

        [HttpGet("{id}")]
        private async Task<bool> UserExists(string username)
        {
            return await _context.Users.AnyAsync(x=> x.UserName == username.ToLower());
        }
    }
}