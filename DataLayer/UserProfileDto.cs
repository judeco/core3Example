using Dapper;

namespace DataLayer
{
    [Table(tableName: "UserProfiles")]
    public class UserProfileDto
    {
        //Warnings not needed for dto
#nullable disable

        [Column(columnName: "Id")]
        public int? Id { get; set; }

        [Column(columnName: "Username")]
        public string Username { get; set; }

        [Column(columnName: "Email")]
        public string Email { get; set; }

        [Column(columnName: "AdditionalData")]
        public string AdditionalData { get; set; }

#nullable enable
    }
}