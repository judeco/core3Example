using Common.Models;
using DataLayer;
using Interfaces.Models;
using Moq;
using Serilog;
using Services;
using Xunit;
using static Common.Constants.HttpConstants;

namespace Tests
{
    public class UserProfileServiceTests
    {
        private readonly Mock<ILogger> _loggerMock = new Mock<ILogger>();
        private readonly Mock<IProfileDataService> _dataServiceMock = new Mock<IProfileDataService>();
        private readonly Mock<IPasswordService> _passwordServiceMock = new Mock<IPasswordService>();

        private readonly string OkEmail = "anyone@anywhere.com";

        private readonly AdditionalData _okAdditionalData = new AdditionalData
        {
            FirstName = "first name",
            LastName = "last name",
        };

        private readonly string OkUserName = "username";

        [Fact]
        public void Add_Returns_BadRequest_for_null_userProfile()
        {
            var uut = GetUut();
            UserProfileResponse expected = new UserProfileResponse(HttpBadRequest);

            var actual = uut.Add(null).GetAwaiter().GetResult();

            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("   ")]
        [InlineData((string)null)]
        public void Add_Returns_BadRequest_for_bad_userName(string badUserName)
        {
            var uut = GetUut();
            UserProfileResponse expected = new UserProfileResponse(HttpBadRequest);
            var badUserProfile = new UserProfile
            {
                Username = badUserName,
                Email = OkEmail,
                AdditionalData = _okAdditionalData
            };

            var actual = uut.Add(badUserProfile).GetAwaiter().GetResult();

            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("   ")]
        [InlineData((string)null)]
        [InlineData("missingAt.Symbol")]
        [InlineData("missingFullStop")]
        public void Add_Returns_BadRequest_for_bad_email(string badEmail)
        {
            var uut = GetUut();
            UserProfileResponse expected = new UserProfileResponse(HttpBadRequest);
            var badUserProfile = new UserProfile
            {
                Username = OkUserName,
                Email = badEmail,
                AdditionalData = _okAdditionalData
            };

            var actual = uut.Add(badUserProfile).GetAwaiter().GetResult();

            Assert.Equal(expected, actual);
        }

        private UserProfileService GetUut()
        {
            return new UserProfileService(_loggerMock.Object, _dataServiceMock.Object, _passwordServiceMock.Object);
        }
    }
}