using Dapper;

namespace DataLayer
{
    [Table(tableName: "UserAuthentication")]
    public class UserAuthenticationDto
    {
        [Column(columnName: "Id")]
        public int? Id { get; set; }

        [Column(columnName: "userId")]
        public int? UserId { get; set; }

        [Column(columnName: "passwordHash")]
        public string? PasswordHash { get; set; }

        [Column(columnName: "passwordSalt")]
        public string? PasswordSalt { get; set; }
    }
}