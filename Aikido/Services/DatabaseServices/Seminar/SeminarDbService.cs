using Aikido.AdditionalData;
using Aikido.Data;
using Aikido.Dto.Seminars;
using Aikido.Dto.Seminars.Members;
using Aikido.Entities;
using Aikido.Entities.Seminar;
using Aikido.Entities.Seminar.SeminarMember;
using Aikido.Entities.Seminar.SeminarMemberRequest;
using Aikido.Entities.Users;
using Aikido.Exceptions;
using DocumentFormat.OpenXml.ExtendedProperties;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.EntityFrameworkCore;

namespace Aikido.Services.DatabaseServices.Seminar
{
    public class SeminarDbService : ISeminarDbService
    {
        private readonly AppDbContext _context;

        public SeminarDbService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<SeminarEntity> GetByIdOrThrowException(long id)
        {
            var seminar = await _context.Seminars
                .Include(s => s.Creator)
                .Include(s => s.Groups)
                .Include(s => s.ContactInfo)
                .Include(s => s.Schedule)
                .Include(s => s.Regulation)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (seminar == null)
                throw new EntityNotFoundException($"Семинар с Id = {id} не найден");

            return seminar;
        }

        public async Task<bool> Exists(long id)
        {
            return await _context.Seminars.AnyAsync(s => s.Id == id);
        }

        public async Task<List<SeminarEntity>> GetAllAsync()
        {
            return await _context.Seminars
                .Include(s => s.Creator)
                .Include(s => s.Groups)
                .Include(s => s.ContactInfo)
                .Include(s => s.Schedule)
                .Include(s => s.Regulation)
                .OrderByDescending(s => s.Date)
                .ToListAsync();
        }

        public async Task<List<SeminarMemberEntity>> GetSeminarMembersAsync(long seminarId)
        {
            return await _context.SeminarMembers
                .Include(sm => sm.User)
                .Include(sm => sm.Seminar)
                .Include(sm => sm.Club)
                .Include(sm => sm.Group)
                .Include(sm => sm.SeminarGroup)
                .Include(sm => sm.Coach)
                .Include(sm => sm.SeminarPayment)
                .Include(sm => sm.BudoPassportPayment)
                .Include(sm => sm.AnnualFeePayment)
                .Include(sm => sm.CertificationPayment)
                .Where(sm => sm.SeminarId == seminarId)
                .OrderBy(sm => sm.User!.LastName)
                .ThenBy(sm => sm.User!.FirstName)
                .ThenBy(sm => sm.User!.MiddleName)
                .ToListAsync();
        }

        public async Task<long> CreateAsync(SeminarDto seminarData)
        {
            var seminar = new SeminarEntity(seminarData);
            _context.Seminars.Add(seminar);
            await _context.SaveChangesAsync(); 

            return seminar.Id;
        }

        public async Task UpdateAsync(long id, SeminarDto seminarData)
        {
            var seminar = await GetByIdOrThrowException(id);
            seminar.UpdateFromJson(seminarData);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(long id)
        {
            var seminar = await GetByIdOrThrowException(id);
            _context.Remove(seminar);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateEditorList(long seminarId, List<long> editorIds)
        {
            var seminar = await GetByIdOrThrowException(seminarId);

            seminar.Editors = editorIds.Select(id => _context.Users.Find(id)
                ?? throw new EntityNotFoundException(nameof(UserEntity)))
                .ToList();

            _context.Update(seminar);
            await _context.SaveChangesAsync();
        }

        public async Task AddSeminarMembersAsync(long seminarId, SeminarMemberGroupDto memberGroup)//ToDo UnitOfWork
        {
            await RemoveExcess(seminarId, memberGroup.Members, memberGroup.CoachId);

            var coachId = memberGroup.CoachId;

            var seminar = await _context.Seminars.FindAsync(seminarId);
            if (seminar == null)
            {
                throw new EntityNotFoundException(nameof(seminar));
            }

            var userIds = memberGroup.Members.Select(m => m.UserId).ToList();
            var usersDict = await _context.Users
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id);

            var existingMembers = await _context.SeminarMembers
                .Where(sm => sm.SeminarId == seminarId && userIds.Contains(sm.UserId))
                .ToDictionaryAsync(sm => sm.UserId);

            foreach (var memberDto in memberGroup.Members)
            {
                if (!usersDict.TryGetValue(memberDto.UserId, out var user))
                {
                    throw new EntityNotFoundException(nameof(UserEntity));
                }

                var userMembership = _context.UserMemberships.AsQueryable()
                    .Where(um => um.UserId == memberDto.UserId
                    && um.RoleInGroup == Role.User)
                    .FirstOrDefault(um => um.Group.UserMemberships.Any(um => um.UserId == coachId));

                if (userMembership == null)
                {
                    throw new EntityNotFoundException(nameof(UserMembershipEntity));
                }

                if (!existingMembers.TryGetValue(memberDto.UserId, out var member))
                {
                    member = new SeminarMemberEntity(coachId, seminar, userMembership, memberDto);
                    _context.SeminarMembers.Add(member);
                }
                else
                {
                    member.UpdateData(coachId, seminar, userMembership, memberDto);
                    _context.SeminarMembers.Update(member);
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task SetFinalSeminarMembersAsync(long seminarId, List<FinalSeminarMemberDto> members)
        {
            await RemoveExcess(seminarId, members.Select(sm => sm as SeminarMemberCreationDto).ToList());

            var seminar = await _context.Seminars.FindAsync(seminarId);
            if (seminar == null)
            {
                throw new EntityNotFoundException(nameof(seminar));
            }

            var creatorId = seminar.CreatorId;

            var userIds = members.Select(m => m.UserId).ToList();
            var usersDict = await _context.Users
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id);

            var existingMembers = await _context.SeminarMembers
                .Where(sm => sm.SeminarId == seminarId && userIds.Contains(sm.UserId))
                .ToDictionaryAsync(sm => sm.UserId);

            foreach (var memberDto in members)
            {
                if (!usersDict.TryGetValue(memberDto.UserId, out var user))
                {
                    throw new EntityNotFoundException(nameof(UserEntity));
                }

                var userMembership = _context.UserMemberships.AsQueryable()
                    .Where(um => um.UserId == memberDto.UserId
                    && um.RoleInGroup == Role.User)
                    .FirstOrDefault();

                if (userMembership == null)
                {
                    throw new EntityNotFoundException(nameof(UserMembershipEntity));
                }

                if (!existingMembers.TryGetValue(memberDto.UserId, out var member))
                {
                    member = new SeminarMemberEntity(
                        creatorId,
                        seminar,
                        userMembership,
                        memberDto,
                        EnumParser.ConvertStringToEnum<SeminarMemberStatus>(memberDto.Status));
                    _context.SeminarMembers.Add(member);
                }
                else
                {
                    member.UpdateData(
                        member.CoachId.Value,
                        seminar,
                        userMembership,
                        memberDto,
                        EnumParser.ConvertStringToEnum<SeminarMemberStatus>(memberDto.Status));
                    _context.SeminarMembers.Update(member);
                }
            }
            await _context.SaveChangesAsync();
        }

        private async Task RemoveExcess(long seminarId, List<SeminarMemberCreationDto> membersDto, long? creatorId = null)
        {
            var userIds = membersDto.Select(m => m.UserId).ToList();

            var membersToDelete = _context.SeminarMembers
                .Where(sm => sm.SeminarId == seminarId)
                .Where(sm => !userIds.Contains(sm.UserId));

            if (creatorId != null)
            {
                membersToDelete = membersToDelete.Where(sm => sm.CoachId == creatorId);
            }

            if (await membersToDelete.AnyAsync())
            {
                _context.SeminarMembers.RemoveRange(membersToDelete);
                await _context.SaveChangesAsync();
            }
        }

        public async Task RemoveMemberAsync(long seminarId, long userId)
        {
            var member = await _context.SeminarMembers
                .FirstOrDefaultAsync(sm => sm.SeminarId == seminarId && sm.UserId == userId);

            if (member != null)
            {
                _context.SeminarMembers.Remove(member);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> IsMemberAsync(long seminarId, long userId)
        {
            return await _context.SeminarMembers
                .AnyAsync(sm => sm.SeminarId == seminarId && sm.UserId == userId);
        }

        public async Task<int> GetMemberCountAsync(long seminarId)
        {
            return await _context.SeminarMembers.CountAsync(sm => sm.SeminarId == seminarId);
        }

        public async Task<SeminarMemberEntity> GetSeminarMemberAsync(long seminarId, long userId)
        {
            return await _context.SeminarMembers.AsQueryable()
                .Include(sm => sm.User)
                .Include(sm => sm.Club)
                .Include(sm => sm.Group)
                .Include(sm => sm.Seminar)
                .Include(sm => sm.SeminarGroup)
                .Include(sm => sm.Coach)
                .Include(sm => sm.SeminarPayment)
                .Include(sm => sm.BudoPassportPayment)
                .Include(sm => sm.AnnualFeePayment)
                .Include(sm => sm.CertificationPayment)
                .Where(sm => sm.UserId == userId
                && sm.SeminarId == seminarId)
                .FirstOrDefaultAsync() ?? throw new EntityNotFoundException(nameof(SeminarMemberEntity));
        }

        public async Task<List<SeminarEntity>> GetSeminarsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Seminars
                .Include(s => s.Creator)
                .Where(s => s.Date >= startDate && s.Date <= endDate)
                .OrderBy(s => s.Date)
                .ToListAsync();
        }

        public async Task<SeminarRegulationEntity> GetSeminarRegulation(long seminarId)
        {
            var regulation = await _context.SeminarRegulation.FirstOrDefaultAsync(r => r.SeminarId == seminarId);

            return regulation 
                ?? throw new EntityNotFoundException(nameof(SeminarRegulationEntity));
        }

        public async Task CreateSeminarRegulationAsync(long seminarId, byte[] fileInBytes)
        {
            var seminar = await _context.Seminars.FindAsync(seminarId);
            var oldRegulation = await _context.SeminarRegulation.FirstOrDefaultAsync(r => r.SeminarId == seminarId);
            if (seminar == null)
            {
                throw new EntityNotFoundException(nameof(seminar));
            }

            if (oldRegulation != null)
            {
                _context.Remove(oldRegulation);
            }

            var regulation = new SeminarRegulationEntity(seminarId, fileInBytes);
            _context.SeminarRegulation.Add(regulation);
            seminar.RegulationId = regulation.Id;

            await _context.SaveChangesAsync();
        }


        public async Task DeleteSeminarRegulationAsync(long seminarId)
        {
            var seminar = _context.Seminars.Find(seminarId);
            var regulation = _context.SeminarRegulation.FirstOrDefaultAsync(r => r.SeminarId == seminarId);

            if (seminar == null)
            {
                throw new EntityNotFoundException(nameof(SeminarEntity));
            }
            if (regulation == null)
            {
                throw new EntityNotFoundException(nameof(SeminarRegulationEntity));
            }
            
            _context.Remove(regulation);
            seminar.RegulationId = null;
            await _context.SaveChangesAsync();
        }

        public async Task<List<SeminarGroupEntity>> GetSeminarGroups(long seminarId)
        {
            var seminar = await _context.Seminars.FindAsync(seminarId);

            var groups = seminar?.Groups;

            return groups != null ? groups.ToList() : new List<SeminarGroupEntity>();
        }

        public async Task ApplySeminarResult(long seminarid)
        {
            var seminar = await _context.Seminars.FindAsync(seminarid);

            seminar.IsFinalStatementApplied = true;
            _context.Update(seminar);
            await _context.SaveChangesAsync();
        }

        public async Task CancelSeminarResult(long seminarid)
        {
            var seminar = await _context.Seminars.FindAsync(seminarid);

            seminar.IsFinalStatementApplied = false;
            _context.Update(seminar);
            await _context.SaveChangesAsync();
        }

        public async Task<List<SeminarMemberEntity>> GetCoachMembersAsync(long seminarId, long coachId)
        {
            return await _context.SeminarMembers
                .AsQueryable()
                .Where(sm => sm.SeminarId == seminarId
                && sm.CoachId == coachId)
                .Include(sm => sm.User)
                .Include(sm => sm.Club)
                .Include(sm => sm.Group)
                .Include(sm => sm.Seminar)
                .Include(sm => sm.SeminarGroup)
                .Include(sm => sm.Coach)
                .Include(sm => sm.SeminarPayment)
                .Include(sm => sm.BudoPassportPayment)
                .Include(sm => sm.AnnualFeePayment)
                .Include(sm => sm.CertificationPayment)
                .ToListAsync();
        }

        #region ManagerMembers

        public async Task<List<SeminarMemberManagerRequestEntity>> GetManagerMembersAsync(long seminarId, long managerId)
        {
            var members = await _context.SeminarMembersManagerRequest.AsQueryable()
                .Where(sm => sm.SeminarId == seminarId
                && sm.ManagerId == managerId)
                .Include(sm => sm.User)
                .Include(sm => sm.Club)
                .Include(sm => sm.Group)
                .Include(sm => sm.Seminar)
                .Include(sm => sm.SeminarGroup)
                .Include(sm => sm.Coach)
                .Include(sm => sm.SeminarPayment)
                .Include(sm => sm.BudoPassportPayment)
                .Include(sm => sm.AnnualFeePayment)
                .Include(sm => sm.CertificationPayment)
                .ToListAsync();

            return members ?? new();

            throw new NotImplementedException();
        }

        public async Task<List<SeminarMemberManagerRequestEntity>> GetManagerMembersByClubAsync(
            long seminarId,
            long managerId,
            long clubId)
        {
            var members = await GetManagerMembersAsync(seminarId, managerId);

            return members
                .Where(sm => sm.ClubId == clubId)
                .ToList();
        }

        public async Task CreateManagerMembersByClubAsync(long seminarId, SeminarMemberManagerRequestListDto managerRequest)
        {
            var seminar = await _context.Seminars.FindAsync(seminarId);

            if (seminar == null)
            {
                throw new EntityNotFoundException(nameof(seminar));
            }

            var seminarMembersCreation = new List<SeminarMemberManagerRequestEntity>();
            var seminarMemberUpdate = new List<SeminarMemberManagerRequestEntity>();

            foreach (var member in managerRequest.Members)
            {
                var userMembership = _context.UserMemberships.AsQueryable()
                    .Where(um => um.UserId == member.UserId
                    && um.GroupId == member.GroupId)
                    .FirstOrDefault() ?? throw new EntityNotFoundException(nameof(UserMembershipEntity));

                var clubSeminarMembers = await GetManagerMembersByClubAsync(seminarId, managerRequest.ManagerId, managerRequest.ClubId);
                
                if (!clubSeminarMembers.Any(m => m.UserId == member.UserId))
                {
                    seminarMembersCreation.Add(new(seminar, userMembership, member));
                }
                else
                {
                    var currentMember = clubSeminarMembers.First(m => m.UserId == member.UserId);
                    
                    currentMember.UpdateData(seminar, userMembership, member);
                    seminarMemberUpdate.Add(currentMember);
                }     
            }

            await _context.SeminarMembersManagerRequest.AddRangeAsync(seminarMembersCreation);
            _context.SeminarMembersManagerRequest.UpdateRange(seminarMemberUpdate);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteManagerMembersByClubAsync(long seminarId,
            long managerId,
            long clubId)
        {
            var members = await GetManagerMembersByClubAsync(seminarId, managerId, clubId);

            _context.SeminarMembersManagerRequest.RemoveRange(members);
            await _context.SaveChangesAsync();
        }

        public async Task ConfirmManagerMembersByClubAsync(long seminarId,
            long managerId,
            long clubId)
        {
            var members = await GetManagerMembersByClubAsync(seminarId, managerId, clubId);

            members.Select(m => m.IsConfirmed = true);

            _context.SeminarMembersManagerRequest.UpdateRange(members);

            await _context.SaveChangesAsync();
        }

        public async Task CancelManagerMemberByClubAsync(long seminarId,
            long managerId,
            long clubId)
        {
            var members = await GetManagerMembersByClubAsync(seminarId, managerId, clubId);

            members.Select(m => m.IsConfirmed = false);

            _context.SeminarMembersManagerRequest.UpdateRange(members);

            await _context.SaveChangesAsync();
        }

        #endregion

        public async Task InitializeSeminar(long seminarId)
        {
            var seminar = await _context.Seminars.FindAsync(seminarId);

            if (seminar == null)
            {
                throw new EntityNotFoundException(nameof(seminar));
            }

            var mainUserMemberships = _context.UserMemberships.AsQueryable()
                .Where(um => um.IsMain)
                .ToList();

            var request = new List<SeminarMemberManagerRequestEntity>();

            foreach(var mainUserMembership in mainUserMemberships)
            {
                var clubManager = mainUserMembership.Club?.Manager 
                    ?? throw new EntityNotFoundException(nameof(mainUserMembership.Club));

                var coach = mainUserMembership.Group?.UserMemberships.First(um => um.RoleInGroup == Role.Coach)
                    ?? throw new EntityNotFoundException(nameof(mainUserMembership.Club));//Здесь возможно стоит переделать и начать выделять главного тренера в группе

                request.Add(new(seminar, mainUserMembership));
            }
        }
    }
}
