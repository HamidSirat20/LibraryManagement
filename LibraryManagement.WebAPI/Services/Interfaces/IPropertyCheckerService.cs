namespace LibraryManagement.WebAPI.Services.Interfaces;
public interface IPropertyCheckerService
{
    bool TypeHasProperties<T>(string? fields);
}
