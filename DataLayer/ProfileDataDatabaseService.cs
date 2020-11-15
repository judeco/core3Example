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
using Interfaces.Models;
using static Interfaces.Constants.HttpConstants;

namespace DataLayer
{
    public interface IProfileDataService
    {
        Task<List<UserProfile>> Get();

        Task<UserProfile?> Add(UserProfile userProfile, UserAuthentication? userAuthentication);

        Task<UserAuthentication?> GetAuthentication(int userId);

        Task<UserProfile?> GetById(int id);

        Task<int> DeleteById(int id);

        Task<int> Update(int id, JsonPatchDocument<UserProfile>? userProfilePatch);

        Task<UserProfile?> GetByUsername(string username);
    }

    public class ProfileDataDatabaseService : IProfileDataService
    {
        private readonly ILogger _logger;
        private readonly IMapper _profileMapper;
        private readonly IMapper _authenticationMapper;
        private readonly string _connectionString;

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
            UserProfileDto userProfileDto = await connection.GetAsync<UserProfileDto>(id);
            if (userProfileDto == null) return null;
            return _profileMapper.Map<UserProfile>(userProfileDto);
        }

        public async Task<UserProfile?> GetByUsername(string username)
        {
            using IDbConnection connection = GetConnection();
            var parameters = new { UserName = username };
            var sql = "select * from UserProfiles where userName = @UserName";
            UserProfileDto userProfileDto = (await connection.QueryAsync<UserProfileDto>(sql, parameters)).ToList().FirstOrDefault();
            if (userProfileDto == null) return null;
            return _profileMapper.Map<UserProfile>(userProfileDto);
        }

        public async Task<int> DeleteById(int id)
        {
            using IDbConnection connection = GetConnection();
            using var transaction = connection.BeginTransaction();
            var userProfileDto = await connection.GetAsync<UserProfileDto>(id, transaction);
            if (userProfileDto == null) return HttpBadRequest;
            var numberRecordsDeleted = await connection.DeleteAsync<UserProfileDto>(id, transaction);
            if (numberRecordsDeleted != 1)
            {
                return HttpInternalServerError;
            }

            return HttpOk;
        }

        public async Task<int> Update(int id, JsonPatchDocument<UserProfile>? userProfilePatch)
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

        public async Task<UserProfile?> Add(UserProfile? userProfile, UserAuthentication? userAuthentication)
        {
            if (userProfile == null) return null;
            UserProfileDto userProfileDto = _profileMapper.Map<UserProfileDto>(userProfile);
            UserAuthenticationDto userAuthenticationDto =
                _authenticationMapper.Map<UserAuthenticationDto>(userAuthentication);
            using IDbConnection conn = GetConnection();
            int? id = null;
            IDbTransaction transaction;

            using (transaction = conn.BeginTransaction())
            {
                try
                {
                    id = await conn.InsertAsync(userProfileDto, transaction);
                    if (id != null)
                    {
                        userAuthenticationDto.UserId = id;
                        await conn.InsertAsync(userAuthenticationDto, transaction);
                    }
                    transaction.Commit();
                }
                catch (Exception e)
                {
                    _logger.Error(e, "failed to add user profile: {Profile}", userProfile);
                    transaction.Rollback();
                }
            }
            if (id == null)
            {
                return null;
            }
            return await GetById(id.Value);
        }

        public async Task<UserAuthentication?> GetAuthentication(int userId)
        {
            var parameters = new { UserId = userId };
            var sql = "select * from UserAuthentication where userId = @UserId";
            using IDbConnection connection = GetConnection();
            UserAuthenticationDto userAuthenticationDto = (await connection.QueryAsync<UserAuthenticationDto>(sql, parameters)).ToList().FirstOrDefault();
            if (userAuthenticationDto == null) return null;
            return _authenticationMapper.Map<UserAuthentication>(userAuthenticationDto);
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