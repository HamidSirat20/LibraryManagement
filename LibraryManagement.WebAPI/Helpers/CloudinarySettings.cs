namespace LibraryManagement.WebAPI.Helpers;
public class CloudinarySettings
{
    //configure these properties in Program.cs from appsettings.json
    public required string CloudName { get; set; }
    public required string ApiKey { get; set; }
    public required string ApiSecret { get; set; }

}
