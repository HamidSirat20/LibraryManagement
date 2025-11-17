using LibraryManagement.WebAPI.Models;
using LibraryManagement.WebAPI.Models.Dtos;

namespace LibraryManagement.WebAPI.Services.ORM.Interfaces;

public interface IUsersMapper
{
    UserReadDto ToReadDto(User user);
    User ToEntity(UserCreateDto userCreateDto);
    UserUpdateDto ToUpdateDto(User user);
    UserCreateDto ToCreateDto(User user);
    User UpdateFromDto(User user, UserUpdateDto userUpdateDto);
}
