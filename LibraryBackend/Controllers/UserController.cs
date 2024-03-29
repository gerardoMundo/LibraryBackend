﻿using AutoMapper;
using LibraryBackend.context;
using LibraryBackend.DTO.Users;
using LibraryBackend.Entities;
using LibraryBackend.Utilities.Enums;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryBackend.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDBContext Context;
        private readonly IMapper Mapper;

        public UserController(ApplicationDBContext context, IMapper mapper)
        {
            this.Context = context;
            this.Mapper = mapper;
        }

        [HttpGet, Route("students")]
        public async Task<ActionResult<List<StudentDTO>>> GetStudents()
        {
            var students = await Context.Users.Where(u => u.Type == UserTypes.Student).ToListAsync();
            return Mapper.Map<List<StudentDTO>>(students);
        }

        [HttpGet, Route("professors")]
        public async Task<ActionResult<List<ProfessorDTO>>> GetProfessors()
        {
            var professors = await Context.Users.Where(u => u.Type == UserTypes.Professor).ToListAsync();
            return Mapper.Map<List<ProfessorDTO>>(professors);
        }

        [HttpGet, Route("administratives")]
        public async Task<ActionResult<List<AdministrativeDTO>>> GetAdministratives()
        {
            var administratives = await Context.Users.Where(u => u.Type == UserTypes.Administrative).ToListAsync();
            return Mapper.Map<List<AdministrativeDTO>>(administratives);
        }

        [AllowAnonymous]
        [HttpGet, Route("users")]
        public async Task<ActionResult<List<UserDTO>>> GetAllUsers()
        {
            var users = await Context.Users.ToListAsync();
            return Mapper.Map<List<UserDTO>>(users);
        }

        [HttpGet("{id:int}", Name = "getUserById")]
        public async Task<ActionResult<StudentDTO>> GetUserById(int id)
        {
            var student = await Context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (student == null) { return NotFound(); }

            return Mapper.Map<StudentDTO>(student);
        }

        [HttpPost]
        public async Task<ActionResult> PostUser(UserCreationDTO userCreation)
        {
            bool userExist = await Context.Users.AnyAsync(u =>
                                     u.EnrollmentNum == userCreation.EnrollmentNum && u.EmployeeKey == userCreation.EmployeeKey);

            bool validCreadentials = userCreation.EnrollmentNum?.Length < 8
               && userCreation.EmployeeKey?.Length < 10;

            if (userExist)
            {
                return BadRequest("El usuario ya se encuentra registrado");
            }
            else if (validCreadentials)
            {
                return BadRequest("No es una matrícula o numero de empleado válido");
            }

            User user = Mapper.Map<User>(userCreation);
            Context.Add(user);
            await Context.SaveChangesAsync();
            return CreatedAtRoute("getUserById", new { id = user.Id }, user);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> PutUser(int id, UserCreationDTO userCreation)
        {
            var user = await Context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) { return NotFound(); }

            user = Mapper.Map(userCreation, user);
            await Context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPatch("{id:int}")]
        public async Task<ActionResult> PatchUser(int id, JsonPatchDocument<UserPatchDTO> patchDocument)
        {
            if (patchDocument == null) { return BadRequest(); }
            var userDB = await Context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (userDB == null) { return NotFound(nameof(userDB)); }

            var userDTO = Mapper.Map<UserPatchDTO>(userDB);
            patchDocument.ApplyTo(userDTO);

            var isValid = TryValidateModel(userDTO);
            if (!isValid) { return BadRequest(ModelState); }

            Mapper.Map(userDTO, userDB);
            await Context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "isAdmin")]
        public async Task<ActionResult> DeleteUser(int id)
        {
            bool studentExist = await Context.Users.AnyAsync(u => u.Id == id);

            if (!studentExist) { return NotFound($"El usuario con el ID: {id} no existe"); }

            Context.Remove(new User() { Id = id });
            await Context.SaveChangesAsync();
            return NoContent();
        }
    }
}
