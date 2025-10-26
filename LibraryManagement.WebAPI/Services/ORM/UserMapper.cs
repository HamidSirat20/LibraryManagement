using LibraryManagement.WebAPI.Models;
using LibraryManagement.WebAPI.Models.Dtos;
using LibraryManagement.WebAPI.Services.ORM.Interfaces;

namespace LibraryManagement.WebAPI.Services.ORM;

public  class UserMapper : IUserMapper
{
    public UserReadDto ToReadDto(User user)
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
            PublicId = user.PublicId,
            MembershipStartDate = user.MembershipStartDate,
            MembershipEndDate = user.MembershipEndDate
        };
    }

    public User ToEntity(UserCreateDto userCreateDto)
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
            PublicId = userCreateDto.PublicId
        };
    }

    public UserUpdateDto ToUpdateDto(User user)
    {
        if (user == null) return null;

        return new UserUpdateDto
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Phone = user.Phone,
            Address = user.Address,
            PublicId = user.PublicId
        };
    }

    public UserCreateDto ToCreateDto(User user)
    {
        if (user == null) return null;

        return new UserCreateDto
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Phone = user.Phone,
            Address = user.Address,
            Password = string.Empty,
            PublicId = user.PublicId
        };
    }

    public User UpdateFromDto(User user, UserUpdateDto userUpdateDto)
    {
        if (userUpdateDto == null) return user;

        user.FirstName = userUpdateDto.FirstName ?? user.FirstName;
        user.LastName = userUpdateDto.LastName ?? user.LastName;
        user.Email = userUpdateDto.Email ?? user.Email;
        user.Phone = userUpdateDto.Phone ?? user.Phone;
        user.Address = userUpdateDto.Address ?? user.Address;
        user.PublicId = userUpdateDto.PublicId ?? user.PublicId;

        return user;
    }
}
