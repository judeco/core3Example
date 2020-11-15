namespace Interfaces.Constants
{
    public static class UserFeedback
    {
        public static readonly string DuplicateEmail = "Email duplicate";
        public static readonly string DuplicateUserName = "Username duplicate";

        public static readonly string InvalidEmail = "Email invalid";

        public const string InternalServerErrorMsg = "Please try again. If problem persist then reoprt to webmaster";
        public const string LoginFailedMsg = "Sorry we cannot find you. Try again or Register";
        public const string BadRequestMsg = "Please try again";
    }
}