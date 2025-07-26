using Aikido.AdditionalData;
using Aikido.Dto;
using Aikido.Entities;

public class UserDto : DtoBase
{
    public string Role { get; set; }
    public string Login { get; set; }
    public string? Password { get; set; }
    public string Name { get; set; }

    public string? Photo { get; set; }
    public string? PhoneNumber { get; set; }
    public DateTime? Birthday { get; set; }
    public string? Sex { get; set; }
    public string? Education { get; set; }

    public string? Grade { get; set; }
    public string? ProgramType { get; set; }
    public List<DateTime>? CertificationDates { get; set; }
    public List<DateTime>? PaymentDates { get; set; }
    
    public long? ClubId { get; set; }
    public string? ClubName { get; set; }
    public string? City { get; set; }
    public long? GroupId { get; set; }
    public string? GroupName { get; set; }

    public string? ParentFullName { get; set; }
    public string? ParentPhoneNumber { get; set; }

    public DateTime? RegistrationDate { get; set; }

    public UserDto() { }

    public UserDto(UserEntity user)
    {
        UpdateFromEntity(user);
    }
    public UserDto(UserEntity user, ClubEntity club)
    {
        UpdateFromEntity(user);
        AddClubName(club);
    }
    public UserDto(UserEntity user, ClubEntity club, GroupEntity group)
    {
        UpdateFromEntity(user);
        AddClubName(club);
        AddGroupName(group);
    }

    public void UpdateFromEntity(UserEntity user)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));

        Id = user.Id;
        Name = user.FullName;
        Role = EnumParser.ConvertEnumToString(user.Role);
        Login = user.Login;
        Password = user.Password;
        Photo = Convert.ToBase64String(user.Photo);
        PhoneNumber = user.PhoneNumber;
        Birthday = user.Birthday;
        Sex = EnumParser.ConvertEnumToString(user.Sex);
        Education = EnumParser.ConvertEnumToString(user.Education);
        Grade = EnumParser.ConvertEnumToString(user.Grade);
        ProgramType = EnumParser.ConvertEnumToString(user.ProgramType);
        CertificationDates = user.CertificationDates;
        PaymentDates = user.PaymentDates;
        ClubId = user.ClubId;
        City = user.City;
        GroupId = user.GroupId;
        ParentFullName = user.ParentFullName;
        ParentPhoneNumber = user.ParentPhoneNumber;
        RegistrationDate = user.RegistrationDate;
    }

    

    public void AddClubName(ClubEntity club)
    {
        ClubName = club.Name;
    }

    public void AddGroupName(GroupEntity group)
    {
        GroupName = group.Name;
    }
}