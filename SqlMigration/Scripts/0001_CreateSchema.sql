CREATE TABLE UserProfiles (
    id int IDENTITY(1,1) PRIMARY KEY,
    userName nvarchar(255) NOT NULL,
    CONSTRAINT UserProfile_UserName UNIQUE(UserName), 
    email nvarchar(255),
    --json formatted additional data to allow functionality to be extended without over complicating the schema
    AdditionalData nvarchar(4000)
);

CREATE TABLE UserAuthentication (
    id int IDENTITY(1,1) PRIMARY KEY,
    userId int NOT NULL,
    passwordHash nvarchar(255) NOT NULL,
    passwordSalt nvarchar(255) NOT NULL
) 