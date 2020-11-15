using System.Collections.Generic;
using Common.Models;

namespace Services.Models.Responses
{
    // This could be generic but using nullable reference types with generics throws up warnings.
    // If this gets fixed in future then use generics
    public class UserProfileListResponse
    {
        public ApiFeedback ApiFeedback { get; }

        public List<UserProfile>? UserProfiles { get; }

        public UserProfileListResponse(int httpCode)
        {
            ApiFeedback = new ApiFeedback(httpCode);
        }

        public UserProfileListResponse(int httpCode, string explanation)
        {
            ApiFeedback = new ApiFeedback(httpCode, explanation);
        }

        public UserProfileListResponse(int httpCode, List<UserProfile> userProfiles)
        {
            ApiFeedback = new ApiFeedback(httpCode);
            UserProfiles = userProfiles;
        }
    }
}