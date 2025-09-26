using LibraryManagement.WebAPI.Models;
using LibraryManagement.WebAPI.Models.Dtos;
using System.Net;
using System.Numerics;

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
            AvatarUrl = user.AvatarUrl
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

    public static User MapUserUpdateDtoToUser(this UserUpdateDto  userUpdateDto, User user)
    {
        if (userUpdateDto == null) return null;

        if (userUpdateDto.FirstName != null) 
            user.FirstName = userUpdateDto.FirstName;
        if (userUpdateDto.LastName != null)
            user.LastName = userUpdateDto.LastName;
        if (userUpdateDto.Email != null)
            user.Email = userUpdateDto.Email;
        if (userUpdateDto.Phone != null)
            user.Phone = userUpdateDto.Phone;
        if (userUpdateDto.Address != null)
            user.Address = userUpdateDto.Address;
        if (userUpdateDto.AvatarUrl != null)
            user.AvatarUrl = userUpdateDto.AvatarUrl;
        return user;
    }
}

