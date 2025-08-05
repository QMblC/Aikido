using Aikido.AdditionalData;
using Aikido.Entities.Users;

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

        public IQueryable<UserEntity> ApplyTo(IQueryable<UserEntity> query)
        {
            if (Roles?.Any() == true)
            {
                var enumRoles = Roles.Select(EnumParser.ConvertStringToEnum<Role>).ToList();
                query = query.Where(u => enumRoles.Contains(u.Role));
            }

            if (Cities?.Any() == true)
                query = query.Where(u => Cities.Contains(u.City));

            if (Grades?.Any() == true)
            {
                var enumGrades = Grades.Select(EnumParser.ConvertStringToEnum<Grade>).ToList();
                query = query.Where(u => enumGrades.Contains(u.Grade));
            }

            if (ClubIds?.Any() == true)
                query = query.Where(u => u.UserGroupData.Any(ugd => ClubIds.Contains(ugd.ClubId)));

            if (GroupIds?.Any() == true)
                query = query.Where(u => u.UserGroupData.Any(ugd => GroupIds.Contains(ugd.GroupId)));

            if (Sex?.Any() == true)
            {
                var enumSexes = Sex.Select(EnumParser.ConvertStringToEnum<Sex>).ToList();
                query = query.Where(u => enumSexes.Contains(u.Sex));
            }

            if (!string.IsNullOrWhiteSpace(Name))
            {
                var lowered = Name.ToLower();
                query = query.Where(u =>
                    (u.LastName != null && u.LastName.ToLower().StartsWith(lowered)) ||
                    (u.FirstName != null && u.FirstName.ToLower().StartsWith(lowered)) ||
                    (u.SecondName != null && u.SecondName.ToLower().StartsWith(lowered))
                );
            }

            return query;
        }
    }
}
