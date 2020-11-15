using Interfaces.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Services;
using Services.Models.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IProfileService _profileService;

        public UserController(IProfileService profileService)
        {
            _profileService = profileService;
        }

        [HttpGet("")]
        public async Task<ActionResult<List<UserProfile>>> Get()
        {
            var response = await _profileService.Get();
            return StatusCode(response.ApiFeedback.HttpCode, response.UserProfiles);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserProfileResponse>> GetById(int? id)
        {
            var userResponse = await _profileService.GetById(id);
            return StatusCode(userResponse.ApiFeedback.HttpCode, userResponse);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteById(int? id)
        {
            var userResponse = await _profileService.DeleteById(id);
            return StatusCode(userResponse.ApiFeedback.HttpCode, userResponse.UserProfile);
        }

        [HttpPut("")]
        public async Task<ActionResult<UserProfileResponse?>> Login(UserProfile? userProfile)
        {
            var userResponse = await _profileService.Login(userProfile);
            return StatusCode(userResponse.ApiFeedback.HttpCode, userResponse);
        }

        [HttpPost("")]
        public async Task<ActionResult<UserProfileResponse?>> Add(UserProfile? userProfile)
        {
            var userResponse = await _profileService.Add(userProfile);
            return StatusCode(userResponse.ApiFeedback.HttpCode, userResponse);
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult<UserProfile?>> Patch(int? id, [FromBody] JsonPatchDocument<UserProfile> userProfilePatch)
        {
            var userResponse = await _profileService.Update(id, userProfilePatch);
            return StatusCode(userResponse.ApiFeedback.HttpCode, userResponse.UserProfile);
        }
    }
}