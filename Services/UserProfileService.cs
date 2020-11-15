using DataLayer;
using Interfaces.ExtensionMethods;
using Interfaces.Models;
using Microsoft.AspNetCore.JsonPatch;
using Serilog;
using Services.Models.Responses;
using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading.Tasks;
using Utilities.Extensions;
using static Interfaces.Constants.HttpConstants;
using static Interfaces.Constants.Logging;
using static Interfaces.Constants.UserFeedback;

namespace Services
{
    public interface IProfileService
    {
        Task<UserProfileListResponse> Get();

        Task<UserProfileResponse> Add(UserProfile? userProfile);

        Task<UserProfileResponse> GetById(int? id);

        Task<UserProfileResponse> DeleteById(int? id);

        Task<UserProfileResponse> Update(int? id, JsonPatchDocument<UserProfile>? userProfilePatch);

        Task<UserProfileResponse> Login(UserProfile? inputUserProfile);
    }

    public class UserProfileService : IProfileService
    {
        private readonly ILogger _logger;
        private readonly IProfileDataService _profileDataService;
        private readonly IPasswordService _passwordService;

        public UserProfileService(
            ILogger logger, IProfileDataService profileDataService, IPasswordService passwordService)
        {
            _logger = logger;
            _profileDataService = profileDataService;
            _passwordService = passwordService;
        }

        public async Task<UserProfileListResponse> Get()
        {
            try
            {
                var userProfiles = await _profileDataService.Get();
                return new UserProfileListResponse(HttpOk, userProfiles);
            }
            catch (Exception e)
            {
                _logger.Error(e, FailedToGetList, nameof(UserProfile));
                return new UserProfileListResponse(HttpInternalServerError, InternalServerErrorMsg);
            }
        }

        public async Task<UserProfileResponse> GetById(int? id)
        {
            if (id.IsNotPositiveInt())
            {
                _logger.Warning(NullInput, nameof(id));
                return new UserProfileResponse(HttpBadRequest, BadRequestMsg);
            }

            try
            {
                Debug.Assert(id != null, nameof(id) + " != null");
                var user = await _profileDataService.GetById(id.Value);
                if (user == null)
                {
                    _logger.Error(FailedToGetId, nameof(UserProfile), id);
                    return new UserProfileResponse(HttpBadRequest, BadRequestMsg);
                }
                return new UserProfileResponse(HttpOk, user);
            }
            catch (Exception e)
            {
                _logger.Error(e, FailedToGetId, nameof(UserProfile), id);
                return new UserProfileResponse(HttpInternalServerError, InternalServerErrorMsg);
            }
        }

        public async Task<UserProfileResponse> DeleteById(int? id)
        {
            if (id.IsNotPositiveInt())
            {
                _logger.Warning("null " + nameof(id));
                return new UserProfileResponse(HttpBadRequest, BadRequestMsg);
            }

            try
            {
                Debug.Assert(id != null, nameof(id) + " != null");
                var result = await _profileDataService.DeleteById(id.Value);
                return new UserProfileResponse(result);
            }
            catch (Exception e)
            {
                _logger.Error(e, FailedToDeleteId, nameof(UserProfile), id);
                return new UserProfileResponse(HttpInternalServerError, InternalServerErrorMsg);
            }
        }

        public async Task<UserProfileResponse> Update(int? id, JsonPatchDocument<UserProfile>? userProfilePatch)
        {
            if (!id.HasValue)
            {
                _logger.Warning(NullInput + nameof(id));
                return new UserProfileResponse(HttpBadRequest, BadRequestMsg);
            }

            if (IsBadUserProfilePatch(userProfilePatch)) { return new UserProfileResponse(HttpBadRequest, BadRequestMsg); }

            var updateResult = await _profileDataService.Update(id.Value, userProfilePatch);

            if (updateResult != HttpOk) return new UserProfileResponse(updateResult);

            //round trip get should be in new transaction
            return await GetUpdatedUserProfile(id);
        }

        private async Task<UserProfileResponse> GetUpdatedUserProfile(int? id)
        {
            if (id == null)
            {
                return new UserProfileResponse(400, BadRequestMsg);
            }
            var userProfile = await _profileDataService.GetById(id.Value);
            return userProfile is null
                ? new UserProfileResponse(HttpInternalServerError, InternalServerErrorMsg)
                : new UserProfileResponse(HttpOk, userProfile);
        }

        private bool IsBadUserProfilePatch(JsonPatchDocument<UserProfile>? userProfilePatch)
        {
            if (userProfilePatch == null)
            {
                _logger.Warning(NullInput, nameof(userProfilePatch));
                return true;
            }

            if (userProfilePatch.Operations?.Count == 0)
            {
                _logger.Warning(InvalidInput, nameof(userProfilePatch), userProfilePatch);
                return true;
            }

            return false;
        }

        public async Task<UserProfileResponse> Add(UserProfile? userProfile)
        {
            if (userProfile == null)
            {
                _logger.Warning(NullInput, nameof(userProfile));
                return new UserProfileResponse(HttpBadRequest);
            }
            TrimWhitespaceInUserProfile(userProfile);
            if (IsBadProfile(userProfile))
            {
                _logger.Warning(InvalidInput, nameof(userProfile), userProfile);
                return new UserProfileResponse(HttpBadRequest, BadRequestMsg);
            }
            try
            {
                var userAuthentication = _passwordService.HashPassword(userProfile.Password);
                userProfile = await _profileDataService.Add(userProfile, userAuthentication);
                if (userProfile == null)
                {
                    return new UserProfileResponse(HttpInternalServerError, InternalServerErrorMsg);
                }
                return new UserProfileResponse(HttpOk, userProfile);
            }
            catch (SqlException e)
            {
                var msg = e.Message;
                if (msg.Contains("duplicate", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (msg.Contains("email", StringComparison.InvariantCultureIgnoreCase))
                    {
                        _logger.Debug(FailedToAddDuplicate, nameof(UserProfile.Email), userProfile);
                        return new UserProfileResponse(HttpBadRequest, DuplicateEmail);
                    }
                    if (msg.Contains("username", StringComparison.InvariantCultureIgnoreCase))
                    {
                        _logger.Debug(FailedToAddDuplicate, nameof(UserProfile.Username), userProfile);
                        return new UserProfileResponse(HttpBadRequest, DuplicateUserName);
                    }
                    _logger.Error(e, FailedToAddDuplicate, "Unkonwn column", userProfile);
                    throw;
                }
                _logger.Error(e, FailedToAdd, nameof(UserProfile), userProfile);
                return new UserProfileResponse(HttpInternalServerError, InternalServerErrorMsg);
            }
            catch (Exception e)
            {
                _logger.Error(e,FailedToAdd, nameof(UserProfile), userProfile);
                return new UserProfileResponse(HttpInternalServerError, InternalServerErrorMsg);
            }
        }

        public async Task<UserProfileResponse> Login(UserProfile? inputUserProfile)
        {
            if (inputUserProfile == null)
            {
                _logger.Warning(NullInput, nameof(UserProfile));
                return new UserProfileResponse(HttpBadRequest, BadRequestMsg);
            }
            TrimWhitespaceInUserProfile(inputUserProfile);
            if (IsBadProfile(inputUserProfile))
            {
                _logger.Warning(InvalidInput, nameof(UserProfile.Username), inputUserProfile.Username);
                return new UserProfileResponse(HttpBadRequest, BadRequestMsg);
            }
            try
            {
                var storedUserProfile = await _profileDataService.GetByUsername(inputUserProfile.Username);
                if (storedUserProfile?.Id == null)
                {
                    _logger.Debug(FailedToLogin, inputUserProfile.Username);
                    return new UserProfileResponse(HttpUnauthorized, LoginFailedMsg);
                }
                var storedAuthentication = await _profileDataService.GetAuthentication(storedUserProfile.Id.Value);
                if (storedAuthentication == null)
                {
                    return new UserProfileResponse(HttpInternalServerError, InternalServerErrorMsg);
                }
                var isCorrectPassword = _passwordService.VerifyPassword(storedAuthentication.PasswordHash, inputUserProfile.Password);
                if (isCorrectPassword)
                {
                    return new UserProfileResponse(HttpOk, storedUserProfile);
                }
                return new UserProfileResponse(HttpUnauthorized, LoginFailedMsg);
            }
            catch (Exception e)
            {
                _logger.Error(e, FailedToLogin, inputUserProfile.Username);
                return new UserProfileResponse(HttpInternalServerError, InternalServerErrorMsg);
            }
        }

        private void TrimWhitespaceInUserProfile(UserProfile userProfile)
        {
#pragma warning disable CS8601 // Possible null reference assignment - SafeTrim.
            userProfile.Username = userProfile.Username.SafeTrim();
            userProfile.Email = userProfile.Email.SafeTrim();
#pragma warning restore CS8601 // Possible null reference assignment.
        }

        private bool IsBadProfile(UserProfile userProfile)
        {
            if (userProfile.Password.IsEmpty())
            {
                _logger.Warning(BlankInput, nameof(userProfile.Password));
                return true;
            }

            if (userProfile.Username.IsEmpty())
            {
                _logger.Warning(BlankInput, nameof(userProfile.Username));
                return true;
            }
            return false;
        }
    }
}