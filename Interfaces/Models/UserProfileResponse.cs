using System;

namespace Common.Models
{
    public class UserProfileResponse : IEquatable<UserProfileResponse>
    {
        public ApiFeedback ApiFeedback { get; }

        public UserProfile? UserProfile { get; }

        public UserProfileResponse(int httpCode)
        {
            ApiFeedback = new ApiFeedback(httpCode);
        }

        public UserProfileResponse(int httpCode, string? explanation)
        {
            ApiFeedback = new ApiFeedback(httpCode, explanation);
        }

        public UserProfileResponse(int httpCode, UserProfile userProfile)
        {
            ApiFeedback = new ApiFeedback(httpCode);
            UserProfile = userProfile;
        }

        public bool Equals(UserProfileResponse? input)
        {
            return input != null
                   && ApiFeedback.Equals(input.ApiFeedback)
                   && (
                       (UserProfile is null && input.UserProfile is null)
                       || (UserProfile != null
                           && input.UserProfile != null
                           && UserProfile.Username == input.UserProfile.Username
                           && UserProfile.Email == input.UserProfile.Email
                           && UserProfile.AdditionalData == input.UserProfile.AdditionalData)
                   );
        }

        public override int GetHashCode()
        {
            var hash = 19;
            unchecked
            { // allow "wrap around" in the int
                hash = hash * 31 + ApiFeedback.GetHashCode();
                hash = hash * 31 + (UserProfile == null ? 0 : UserProfile.Email.GetHashCode());
                hash = hash * 31 + (UserProfile == null ? 0 : UserProfile.Username.GetHashCode());
                hash = hash * 31 + (UserProfile == null || UserProfile.AdditionalData == null ? 0 : UserProfile.AdditionalData.GetHashCode());
            }
            return hash;
        }
    }
}