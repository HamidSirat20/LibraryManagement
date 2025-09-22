using LibraryManagement.WebAPI.Models;
using LibraryManagement.WebAPI.Models.Dtos;

namespace LibraryManagement.WebAPI.Services.ORM;
    public static class UserMapper
    {
    public static UserReadDto MapUserToUserReadDto(this User user)
    {
        if (user == null) return null;

        return new UserReadDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Phone = user.Phone,
            Address = user.Address,
            AvatarUrl = user.AvatarUrl,
            Role = user.Role,
            MembershipStartDate = user.MembershipStartDate,
            MembershipEndDate = user.MembershipEndDate
        };
    }

    public static User MapUserCreateDtoToUser(this UserCreateDto userCreateDto)
    {
        if (userCreateDto == null) return null;

        return new User(
            fName: userCreateDto.FirstName,
            lName: userCreateDto.LastName,
            email: userCreateDto.Email,
            phone: userCreateDto.Phone,
            address: userCreateDto.Address,
            membershipStartDate: DateTime.UtcNow, 
            membershipEndDate: DateTime.UtcNow.AddYears(1) 
        )
        {
            Password = userCreateDto.Password, 
            AvatarUrl = userCreateDto.AvatarUrl
        };
    }
    public static UserUpdateDto MapToUserUpdateDto(this User user)
    {
        if (user == null) return null;

        return new UserUpdateDto
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Phone = user.Phone,
            Address = user.Address,
            AvatarUrl = user.AvatarUrl,
            Password = string.Empty 
        };
    }
    public static UserCreateDto MapToUserCreateDto(this User user)
    {
        if (user == null) return null;

        return new UserCreateDto
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Phone = user.Phone,
            Address = user.Address,
            AvatarUrl = user.AvatarUrl,
            Password = string.Empty 
        };
    }
}

