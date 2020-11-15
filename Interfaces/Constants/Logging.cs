namespace Interfaces.Constants
{
    public static class Logging
    {
        public static readonly string FailedToGetList = "Failed to get @Entity list";
        public static readonly string FailedToGetId = "Failed to get @Entity for @NameInput : @Input";
        public static readonly string FailedToDeleteId = "Failed to delete @Entity for @Id";
        public static readonly string NullInput = "@Input should not be null";
        public static readonly string InvalidInput = "@InputName is invalid @Input";
        public static readonly string FailedToAddDuplicate = "Failed to add duplicate @EntityName : @Entity";
        public static readonly string FailedToAdd = "Failed to add @InputName : @Input";
        public static readonly string FailedToLogin = "Failed to login Username : @Username";
        public static readonly string BlankInput = "@InputName should not be blank";
    }
}