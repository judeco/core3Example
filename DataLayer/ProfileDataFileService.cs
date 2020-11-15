using Microsoft.AspNetCore.JsonPatch;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Common.Models;
using Utilities.Extensions;
using static Common.Constants.HttpConstants;

namespace DataLayer
{
    public class ProfileDataFileService : IProfileDataService
    {
        private static string _filePath = "";

        public ProfileDataFileService()
        {
            _filePath = Path.Combine(Assembly.GetExecutingAssembly().Location, "Profiles.json");
        }

        public async Task<UserProfileResponse> Add(UserProfile userProfile, UserAuthentication? userAuthentication)
        {
            if (!File.Exists(_filePath))
            {
                File.Create(_filePath);
            }
            string fileAsJson;
            using (StreamReader r = new StreamReader(_filePath))
            {
                fileAsJson = await r.ReadToEndAsync();
            }
            List<UserProfile> profiles = new List<UserProfile>();
            if (fileAsJson.IsNotBlank())
            {
                profiles = JsonConvert.DeserializeObject<List<UserProfile>>(fileAsJson);
            }
            var match = profiles.FirstOrDefault(p => p.Equals(userProfile));
            if (match == null)
            {
                profiles.Add(userProfile);
            }
            var profile =  await Task.FromResult(userProfile);
            return new UserProfileResponse(HttpOk, profile);
        }

        public Task<int> DeleteById(int id)
        {
            throw new NotImplementedException();
        }

        public Task<List<UserProfile>> Get()
        {
            throw new NotImplementedException();
        }

        public Task<UserAuthentication?> GetAuthentication(int userId)
        {
            throw new NotImplementedException();
        }

        public Task<UserProfile?> GetById(int id)
        {
            throw new NotImplementedException();
        }

        public Task<int> Update(UserProfile userProfile)
        {
            // TODO
            throw new NotImplementedException();
        }

        public Task<UserProfile?> GetByUsername(string username)
        {
            throw new NotImplementedException();
        }

        public Task<int> Patch(int id, JsonPatchDocument<UserProfile>? userProfilePatch)
        {
            throw new NotImplementedException();
        }
    }
}