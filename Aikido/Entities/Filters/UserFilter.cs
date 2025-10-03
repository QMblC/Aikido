using Aikido.AdditionalData;
using Aikido.Entities;

namespace Aikido.Entities.Filters
{
    public class UserFilter
    {
        public List<string>? Roles { get; set; }
        public List<string>? Cities { get; set; }
        public List<string>? Grades { get; set; }
        public List<long>? ClubIds { get; set; }
        public List<long>? GroupIds { get; set; }
        public List<string>? Sex { get; set; }
        public string? Name { get; set; }
        public DateTime? BirthdayFrom { get; set; }
        public DateTime? BirthdayTo { get; set; }
        public bool? HasBudoPassport { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Education { get; set; }
        public string? ProgramType { get; set; }

        public IQueryable<UserEntity> ApplyTo(IQueryable<UserEntity> query)
        {
            if (Roles?.Any() == true)
            {
                var enumRoles = Roles.Select(EnumParser.ConvertStringToEnum<Role>).ToList();
                query = query.Where(u => enumRoles.Contains(u.Role));
            }

            if (Cities?.Any() == true)
                query = query.Where(u => u.City != null && Cities.Contains(u.City));

            if (Grades?.Any() == true)
            {
                var enumGrades = Grades.Select(EnumParser.ConvertStringToEnum<Grade>).ToList();
                query = query.Where(u => enumGrades.Contains(u.Grade));
            }

            // Исправлено для новой 3НФ архитектуры - используем UserClubs
            if (ClubIds?.Any() == true)
                query = query.Where(u => u.UserMemberships.Any(um => ClubIds.Contains(um.ClubId)));

            // Исправлено для новой 3НФ архитектуры - используем UserGroups
            if (GroupIds?.Any() == true)
                query = query.Where(u => u.UserMemberships.Any(um => GroupIds.Contains(um.GroupId)));

            if (Sex?.Any() == true)
            {
                var enumSexes = Sex.Select(EnumParser.ConvertStringToEnum<Sex>).ToList();
                query = query.Where(u => enumSexes.Contains(u.Sex));
            }

            // Исправлено для работы с FullName вместо отдельных полей имени
            if (!string.IsNullOrWhiteSpace(Name))
            {
                var lowered = Name.ToLower();
                query = query.Where(u => u.FullName.ToLower().Contains(lowered));
            }

            // Дополнительные фильтры
            if (BirthdayFrom.HasValue)
                query = query.Where(u => u.Birthday >= BirthdayFrom.Value);

            if (BirthdayTo.HasValue)
                query = query.Where(u => u.Birthday <= BirthdayTo.Value);

            if (HasBudoPassport.HasValue)
                query = query.Where(u => u.HasBudoPassport == HasBudoPassport.Value);

            if (!string.IsNullOrWhiteSpace(PhoneNumber))
                query = query.Where(u => u.PhoneNumber != null && u.PhoneNumber.Contains(PhoneNumber));

            if (!string.IsNullOrWhiteSpace(Education))
            {
                var enumEducation = EnumParser.ConvertStringToEnum<Education>(Education);
                query = query.Where(u => u.Education == enumEducation);
            }

            if (!string.IsNullOrWhiteSpace(ProgramType))
            {
                var enumProgramType = EnumParser.ConvertStringToEnum<ProgramType>(ProgramType);
                query = query.Where(u => u.ProgramType == enumProgramType);
            }

            return query;
        }
    }
}
