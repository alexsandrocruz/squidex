﻿// ==========================================================================
//  UserManagementController.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Squidex.Controllers.Api.Users.Models;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Security;
using Squidex.Pipeline;
using Squidex.Read.Users;

namespace Squidex.Controllers.Api.Users
{
    [MustBeAdministrator]
    [ApiExceptionFilter]
    [SwaggerIgnore]
    public class UserManagementController : Controller
    {
        private readonly UserManager<IUser> userManager;
        private readonly IUserFactory userFactory;

        public UserManagementController(UserManager<IUser> userManager, IUserFactory userFactory)
        {
            this.userManager = userManager;
            this.userFactory = userFactory;
        }

        [HttpGet]
        [Route("user-management")]
        [ApiCosts(0)]
        public async Task<IActionResult> GetUsers([FromQuery] string query = null, [FromQuery] int skip = 0, [FromQuery] int take = 10)
        {
            var taskForUsers = userManager.QueryByEmailAsync(query, take, skip);
            var taskForCount = userManager.CountAsync(query);

            await Task.WhenAll(taskForUsers, taskForCount);

            var response = new UsersDto
            {
                Total = taskForCount.Result,
                Items = taskForUsers.Result.Select(x => SimpleMapper.Map(x, new UserDto { DisplayName = x.DisplayName(), PictureUrl = x.PictureUrl() })).ToArray()
            };

            return Ok(response);
        }

        [HttpGet]
        [Route("user-management/{id}")]
        [ApiCosts(0)]
        public async Task<IActionResult> GetUser(string id)
        {
            var entity = await userManager.FindByIdAsync(id);

            if (entity == null)
            {
                return NotFound();
            }

            var response = SimpleMapper.Map(entity, new UserDto { DisplayName = entity.DisplayName(), PictureUrl = entity.PictureUrl() });

            return Ok(response);
        }

        [HttpPost]
        [Route("user-management")]
        [ApiCosts(0)]
        public async Task<IActionResult> PostUser([FromBody] CreateUserDto request)
        {
            var user = await userManager.CreateAsync(userFactory, request.Email, request.DisplayName, request.Password);

            var response = new UserCreatedDto { Id = user.Id, PictureUrl = user.PictureUrl() };

            return Ok(response);
        }

        [HttpPut]
        [Route("user-management/{id}")]
        [ApiCosts(0)]
        public async Task<IActionResult> PutUser(string id, [FromBody] UpdateUserDto request)
        {
            await userManager.UpdateAsync(id, request.Email, request.DisplayName, request.Password);

            return NoContent();
        }

        [HttpPut]
        [Route("user-management/{id}/lock/")]
        [ApiCosts(0)]
        public async Task<IActionResult> LockUser(string id)
        {
            if (IsSelf(id))
            {
                throw new ValidationException("Locking user failed.", new ValidationError("You cannot lock yourself."));
            }

            await userManager.LockAsync(id);

            return NoContent();
        }

        [HttpPut]
        [Route("user-management/{id}/unlock/")]
        [ApiCosts(0)]
        public async Task<IActionResult> UnlockUser(string id)
        {
            if (IsSelf(id))
            {
                throw new ValidationException("Unlocking user failed.", new ValidationError("You cannot unlock yourself."));
            }

            await userManager.UnlockAsync(id);

            return NoContent();
        }

        private bool IsSelf(string id)
        {
            var subject = User.OpenIdSubject();

            return string.Equals(subject, id, StringComparison.OrdinalIgnoreCase);
        }
    }
}
