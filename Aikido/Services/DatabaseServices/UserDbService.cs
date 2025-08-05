using Aikido.AdditionalData;
using Aikido.Data;
using Aikido.Dto;
using Aikido.Dto.Seminars;
using Aikido.Entities;
using Aikido.Entities.Filters;
using Aikido.Entities.Seminar;
using Aikido.Entities.Users;
using Aikido.Services.DatabaseServices.Base;
using Microsoft.EntityFrameworkCore;

namespace Aikido.Services.DatabaseServices
{
    public class UserDbService : DbService<UserEntity, UserDbService>, IUserDbService
    {
        public UserDbService(AppDbContext context, ILogger<UserDbService> logger)
            : base(context, logger)
        {

        }

        
        //private async Task EnsureLoginIsUnique(string? login, long? excludeUserId = null)
        //{//Вынести
        //    if (string.IsNullOrWhiteSpace(login))
        //        return;

        //    var loginExists = await context.Users
        //        .AnyAsync(u => u.Login == login && (!excludeUserId.HasValue || u.Id != excludeUserId.Value));

        //    if (loginExists)
        //        throw new Exception($"Пользователь с логином '{login}' уже существует.");
        //}

        //public async Task ApplySeminarResults(SeminarMemberDto seminarMember, SeminarEntity seminar)
        //{//Вынести
        //    var user = await GetByIdOrThrowException(seminarMember.Id.Value);

        //    var newGrade = EnumParser.ConvertStringToEnum<Grade>(seminarMember.CertificationGrade);

        //    if(seminarMember.BudoPassportPrice > 0)
        //    {
        //        user.HasBudoPassport = true;
        //    }

        //    if (newGrade != Grade.None)
        //    {
        //        user.Grade = newGrade;
        //        user.CertificationDates.Add(seminar.Date);
        //        if (!seminarMember.CertificationGrade.Contains("Child"))
        //        {
        //            user.ProgramType = ProgramType.Adult;
        //        }
        //    }

        //    await SaveChangesAsync();
        //}

        //public async Task DiscardSeminarResult(SeminarMemberDto seminarMember, SeminarEntity seminar)
        //{//Вынести
        //    var user = await GetByIdOrThrowException(seminarMember.Id.Value);

        //    if (seminarMember.BudoPassportPrice > 0)
        //    {
        //        user.HasBudoPassport = false;
        //    }

        //    user.Grade = EnumParser.ConvertStringToEnum<Grade>(seminarMember.Grade);
        //    user.CertificationDates.Remove(seminar.Date);
        //    if (seminarMember.Grade.Contains("Child"))
        //    {
        //        user.ProgramType = ProgramType.Child;
        //    }

        //    await SaveChangesAsync();
        //}

        public async Task<List<UserEntity>> GetFilteredUserListAlphabetAscending(UserFilter filter)
        {
            var query = filter.ApplyTo(Query());

            var totalCount = await query.CountAsync();

            var usersEntities = await query
                .OrderBy(user => user.LastName)
                .ThenBy(user => user.FirstName)
                .ThenBy(user => user.SecondName)
                .ToListAsync();

            return usersEntities;
        }

        public async Task<List<GroupEntity>> GetUserGroups(long userId)
        {
            return await context.UserGroupData
                .Where(data =>  data.UserId == userId)
                .Select(data => data.Group)
                .ToListAsync();
        }

        public async Task<List<UserEntity>> GetCoachStudents(long coachId)
        {
            return await context.UserGroupData
                .Where(data => data.UserId == coachId && data.RoleInGroup == Role.Coach)
                .SelectMany(data => data.Group.MemberData)
                .Where(data => data.RoleInGroup == Role.User)
                .Select(data => data.User)
                .ToListAsync();
        }
    }
}
