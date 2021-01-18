using Ex7.DAL;
using Ex7.DTOs.Requests;
using Ex7.Models;
using Ex7.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Ex7.Controllers
{
    [ApiController]
    [Route("api/students")]

    public class StudentsController : ControllerBase
    {

        private readonly IStudentDbService _dbService;
        private IConfiguration _configuration;

        public StudentsController(IStudentDbService dbService, IConfiguration configuration)
        {
            _dbService = dbService;
            _configuration = configuration;
        }

        [HttpGet]
        [Authorize]
        public IActionResult GetStudents()
        {
            var _students = _dbService.GetStudents();
            return Ok(_students);
        }

        [HttpGet("{id}")]
        [Authorize]
        public IActionResult GetStudent(string id)
        {
            var student = _dbService.GetStudent(id);
            return Ok(student);
        }

        [HttpPost]
        public IActionResult CreateStudent(Student student)
        {
            student.IndexNumber = $"s{new Random().Next(1, 20000)}";
            return Ok(student);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateStudent(int id)
        {
            if (id == 1)
            {
                return Ok("Studnet zaktualizowany");
            }
            return NotFound("Nie znaleziono studenta"); ;
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteStudent(int id)
        {
            if (id == 2)
            {
                return Ok("Student został usunięty");
            }
            return NotFound("Nie znaleziono studenta");
        }

        [HttpPost("login")]
        public IActionResult Login(LoginRequest request)
        {
            Student st = _dbService.GetStudent(request.login);

            if(request.login == st.IndexNumber && request.password == st.Password)
            {
                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, st.IndexNumber),
                    new Claim(ClaimTypes.Name, st.FirstName),
                    new Claim(ClaimTypes.Role, "admin"),
                    new Claim(ClaimTypes.Role, "student")
                };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["SecretKey"]));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken
                    (
                        issuer: "Gakko",
                        audience: "Students",
                        claims: claims,
                        expires: DateTime.Now.AddMinutes(10),
                        signingCredentials: creds
                    );

                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    refreshToken = Guid.NewGuid()
                });
            }
            else
            {
                return NotFound("Zły login lub hasło");
            }

        }
    }
}
