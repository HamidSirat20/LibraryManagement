using System.Runtime.Serialization;

namespace LibraryManagement.WebAPI.Models;

[DataContract]
public enum Genre
{
    [EnumMember]
    Fiction,
    [EnumMember]
    NonFiction,
    [EnumMember]
    Mystery,
    [EnumMember]
    ScienceFiction,
    [EnumMember]
    Fantasy,
    [EnumMember]
    Biography,
    [EnumMember]
    History,
    [EnumMember]
    Romance,
    [EnumMember]
    Thriller,
    [EnumMember]
    Horror,
    [EnumMember]
    Other
}
