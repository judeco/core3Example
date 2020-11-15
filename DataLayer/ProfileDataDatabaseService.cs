using AutoMapper;
using Dapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Common.Models;
using static Common.Constants.HttpConstants;

namespace DataLayer
{
    public interface IProfileDataService
    {
        Task<List<UserProfile>> Get();

        Task<UserProfileResponse> Add(UserProfile userProfile, UserAuthentication? userAuthentication);

        Task<UserAuthentication?> GetAuthentication(int userId);

        Task<UserProfile?> GetById(int id);

        Task<int> DeleteById(int id);

        Task<int> Patch(int id, JsonPatchDocument<UserProfile>? userProfilePatch);

        Task<int> Update(UserProfile userProfile);

        Task<UserProfile?> GetByUsername(string username);
    }

    public class ProfileDataDatabaseService : IProfileDataService
    {
        private readonly ILogger _logger;
        private readonly IMapper _profileMapper;
        private readonly IMapper _authenticationMapper;
        private readonly string _connectionString;

        private readonly string userNameUniqueConstraint = "UserProfile_UserName";

        public ProfileDataDatabaseService(ILogger logger, IMapper profileMapper, IMapper authenticationMapper, IConfiguration configuration)
        {
            _logger = logger;
            _profileMapper = profileMapper;
            _authenticationMapper = authenticationMapper;
            _connectionString = configuration.GetConnectionString("SqlConnectionString");
        }

        public async Task<List<UserProfile>> Get()
        {
            using IDbConnection connection = GetConnection();
            var userProfileDtos = await connection.GetListAsync<UserProfileDto>();
            return userProfileDtos.Select(userProfileDto => _profileMapper.Map<UserProfile>(userProfileDto)).ToList();
        }

        public async Task<UserProfile?> GetById(int id)
        {
            using IDbConnection connection = GetConnection();
            var userProfileDto = await connection.GetAsync<UserProfileDto>(id);
            return userProfileDto == null ? null : _profileMapper.Map<UserProfile>(userProfileDto);
        }

        public async Task<int> Update(UserProfile userProfile)
        {
            if (userProfile.Id == null)
            {
                return HttpBadRequest;
            }
            using IDbConnection connection = GetConnection();
            var userProfileDto = await connection.GetAsync<UserProfileDto>(userProfile.Id);
            if (userProfileDto == null) return HttpBadRequest;
            var updateProfileDto = _profileMapper.Map<UserProfileDto>(userProfile);
            var numberRecordsUpdated = await connection.UpdateAsync(updateProfileDto);
            return numberRecordsUpdated;
        }

        public async Task<UserProfile?> GetByUsername(string username)
        {
            using IDbConnection connection = GetConnection();
            var parameters = new { UserName = username };
            var userProfileDto = (await connection.QueryAsync<UserProfileDto>("select * from UserProfiles where userName = @UserName", parameters)).ToList().FirstOrDefault();
            return userProfileDto == null ? null : _profileMapper.Map<UserProfile>(userProfileDto);
        }

        public async Task<int> DeleteById(int id)
        {
            using IDbConnection connection = GetConnection();
            using var transaction = connection.BeginTransaction();
            var userProfileDto = await connection.GetAsync<UserProfileDto>(id, transaction);
            if (userProfileDto == null) return HttpBadRequest;
            var numberRecordsDeleted = await connection.DeleteAsync<UserProfileDto>(id, transaction);
            return numberRecordsDeleted != 1 ? HttpInternalServerError : HttpOk;
        }

        public async Task<int> Patch(int id, JsonPatchDocument<UserProfile>? userProfilePatch)
        {
            using IDbConnection connection = GetConnection();
            using var transaction = connection.BeginTransaction();
            var userProfileDto = await connection.GetAsync<UserProfileDto>(id, transaction);
            if (userProfileDto == null) return HttpBadRequest;
            var userProfile = _profileMapper.Map<UserProfile>(userProfileDto);
            userProfilePatch?.ApplyTo(userProfile);
            await connection.UpdateAsync(userProfileDto, transaction);
            return HttpOk;
        }

        public async Task<UserProfileResponse> Add(UserProfile? userProfile, UserAuthentication? userAuthentication)
        {
            UserProfileResponse errorResponse = new UserProfileResponse(HttpInternalServerError);
            if (userProfile == null) return errorResponse;
            var userProfileDto = _profileMapper.Map<UserProfileDto>(userProfile);
            var userAuthenticationDto = _authenticationMapper.Map<UserAuthenticationDto>(userAuthentication);
            using IDbConnection connection = GetConnection();
            int? id;
            IDbTransaction transaction;

            using var dbTransaction = transaction = connection.BeginTransaction();
            try
            {
                id = await connection.InsertAsync(userProfileDto, transaction);
                if (id != null)
                {
                    userAuthenticationDto.UserId = id;
                    await connection.InsertAsync(userAuthenticationDto, transaction);
                }
                transaction.Commit();
            }
            catch (Exception e)
            {
                transaction.Rollback();
                if (e.Message.Contains(userNameUniqueConstraint))
                {
                    _logger.Debug(e, "failed to add user profile: {Profile}. Username duplicate", userProfile);
                    return new UserProfileResponse(HttpBadRequest, "Username already in use");
                }
                _logger.Error(e, "failed to add user profile: {Profile}", userProfile);
                return errorResponse;
            }
            if (id == null)
            {
                return errorResponse;
            }
            var profile = await GetById(id.Value);
            if (profile == null)
            {
                return errorResponse;
            }

            profile.Id = id;
            return new UserProfileResponse(HttpOk, profile);
        }

        public async Task<UserAuthentication?> GetAuthentication(int userId)
        {
            var parameters = new { UserId = userId };
            var sql = "select * from UserAuthentication where userId = @UserId";
            using IDbConnection connection = GetConnection();
            UserAuthenticationDto userAuthenticationDto = (await connection.QueryAsync<UserAuthenticationDto>(sql, parameters)).ToList().FirstOrDefault();
            return userAuthenticationDto == null ? null : _authenticationMapper.Map<UserAuthentication>(userAuthenticationDto);
        }

        private IDbConnection GetConnection()
        {
            try
            {
                IDbConnection connection = new SqlConnection(_connectionString);
                connection.Open();
                return connection;
            }
            catch (Exception e)
            {
                _logger.Error(e, "Failed to connect to database with connection string " + _connectionString);
                throw;
            }
        }
    }
}