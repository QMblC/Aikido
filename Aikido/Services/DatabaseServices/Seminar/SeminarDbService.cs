using Aikido.AdditionalData.Enums;
using Aikido.Data;
using Aikido.Dto.Seminars;
using Aikido.Dto.Seminars.Creation;
using Aikido.Dto.Seminars.Members;
using Aikido.Dto.Seminars.Members.CoachEditRequest;
using Aikido.Entities;
using Aikido.Entities.Seminar;
using Aikido.Entities.Seminar.SeminarFilters;
using Aikido.Entities.Seminar.SeminarMember;
using Aikido.Entities.Seminar.SeminarMemberRequest;
using Aikido.Entities.Users;
using Aikido.Exceptions;
using DocumentFormat.OpenXml.ExtendedProperties;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.EntityFrameworkCore;
using System.Linq;

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
                .Include(s => s.Payments)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (seminar == null)
                throw new EntityNotFoundException($"Семинар с Id = {id} не найден");

            return seminar;
        }

        public async Task<bool> Exists(long id)
        {
            return await _context.Seminars.AnyAsync(s => s.Id == id);
        }

        public async Task<List<SeminarEntity>> GetAllAsync(SeminarFilter filter)
        {
            var seminars = await _context.Seminars
                .Include(s => s.Creator)
                .Include(s => s.Groups)
                .Include(s => s.ContactInfo)
                .Include(s => s.Schedule)
                .Include(s => s.Regulation)
                .OrderByDescending(s => s.Date)
                .ToListAsync();

            var result = new List<SeminarEntity>();

            if (filter.IsPast)
            {
                result.AddRange(seminars.Where(s => s.Date < DateTime.UtcNow));
            }
            if (filter.IsUpcoming)
            {
                result.AddRange(seminars.Where(s => s.Date >= DateTime.UtcNow));
            }

            return result;
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
                .Where(sm => sm.SeminarId == seminarId)
                .OrderBy(sm => sm.User!.LastName)
                .ThenBy(sm => sm.User!.FirstName)
                .ThenBy(sm => sm.User!.MiddleName)
                .ToListAsync();
        }

        public async Task<SeminarEntity> CreateAsync(SeminarCreationDto seminarData)
        {
            var seminar = new SeminarEntity(seminarData);
            _context.Seminars.Add(seminar);

            return seminar;
        }

        public async Task CreateSeminarSchedule(SeminarEntity seminar, List<SeminarScheduleCreationDto> schedule)
        {
            var scheduleEntity = schedule.Select(s => new SeminarScheduleEntity(
                seminar.Id, 
                seminar.Groups.First(g => g.Name == s.GroupName).Id, 
                s));

            await _context.SeminarSchedule.AddRangeAsync(scheduleEntity);
        }

        public async Task UpdateSeminarSchedule(SeminarEntity seminar, List<SeminarScheduleCreationDto> schedule)
        {
            var oldSchedules = _context.SeminarSchedule.Where(p => p.SeminarId == seminar.Id);
            _context.RemoveRange(oldSchedules);
            await CreateSeminarSchedule(seminar, schedule);
        }

        public async Task UpdateAsync(long id, SeminarCreationDto seminarData)
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

        #region ManagerMembers

        public async Task<List<SeminarMemberManagerRequestEntity>> GetManagerMembersAsync(long seminarId, long managerId)
        {
            var members = await _context.SeminarMembersManagerRequests.AsQueryable()
                .Where(sm => sm.SeminarId == seminarId
                && sm.ManagerId == managerId)
                .Include(sm => sm.User)
                .Include(sm => sm.Club)
                .Include(sm => sm.Group)
                .Include(sm => sm.Seminar)
                .Include(sm => sm.SeminarGroup)
                .Include(sm => sm.Coach)
                .Include(sm => sm.Manager)
                .ToListAsync();

            return members ?? new();

            throw new NotImplementedException();
        }

        public async Task<List<SeminarMemberManagerRequestEntity>> GetManagerMembersByClubAsync(
            long seminarId,
            long clubId)
        {
            var members = await _context.SeminarMembersManagerRequests.AsQueryable()
                .Where(sm => sm.SeminarId == seminarId
                && sm.ClubId == clubId)
                .Include(sm => sm.User)
                .Include(sm => sm.Club)
                .Include(sm => sm.Group)
                .Include(sm => sm.Seminar)
                .Include(sm => sm.SeminarGroup)
                .Include(sm => sm.Coach)
                .Include(sm => sm.Manager)
                .ToListAsync();

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

            await DeleteExcess(seminarId, managerRequest);

            var seminarMembersCreation = new List<SeminarMemberManagerRequestEntity>();
            var seminarMemberUpdate = new List<SeminarMemberManagerRequestEntity>();

            var clubSeminarMembers = await GetManagerMembersByClubAsync(seminarId, managerRequest.ClubId);

            foreach (var member in managerRequest.Members)
            {
                var userMembership = _context.UserMemberships.AsQueryable()
                    .Where(um => um.UserId == member.UserId
                    && um.GroupId == member.GroupId)
                    .Include(um => um.Club)
                    .FirstOrDefault() ?? throw new EntityNotFoundException(nameof(UserMembershipEntity));        
                
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

            await _context.SeminarMembersManagerRequests.AddRangeAsync(seminarMembersCreation);
            _context.SeminarMembersManagerRequests.UpdateRange(seminarMemberUpdate);
            await _context.SaveChangesAsync();
        }

        private async Task DeleteExcess(long seminarId, SeminarMemberManagerRequestListDto managerRequest)
        {
            var userIds = managerRequest.Members
                .Select(m => m.UserId)
                .ToList();

            var membersToDelete = await _context.SeminarMembersManagerRequests
                .Where(sm => sm.SeminarId == seminarId
                    && sm.ClubId == managerRequest.ClubId
                    && sm.ManagerId == managerRequest.ManagerId
                    && !userIds.Contains(sm.UserId))
                .ToListAsync();

            if (membersToDelete.Count == 0)
                return;

            var userIdsToDelete = membersToDelete
                .Select(m => m.UserId)
                .Distinct()
                .ToList();

            var paymentsToDelete = await _context.Payments
                .Where(p => p.EventType == EventType.Seminar
                    && p.EventId == seminarId
                    && userIdsToDelete.Contains(p.UserId))
                .ToListAsync();

            if (paymentsToDelete.Count > 0)
                _context.Payments.RemoveRange(paymentsToDelete);

            _context.SeminarMembersManagerRequests.RemoveRange(membersToDelete);

            await _context.SaveChangesAsync();
        }


        public async Task DeleteManagerMembersByClubAsync(long seminarId,
            long managerId,
            long clubId)
        {
            var members = await GetManagerMembersByClubAsync(seminarId, clubId);

            _context.SeminarMembersManagerRequests.RemoveRange(members);
            await _context.SaveChangesAsync();
        }

        public async Task ConfirmManagerMembersByClubAsync(long seminarId,
            long managerId,
            long clubId)
        {
            var members = await GetManagerMembersByClubAsync(seminarId, clubId);

            foreach (var member in members)
            {
                member.IsConfirmed = true;
            }

            _context.SeminarMembersManagerRequests.UpdateRange(members);

            await _context.SaveChangesAsync();
        }

        public async Task CancelManagerMemberByClubAsync(long seminarId,
            long managerId,
            long clubId)
        {
            var members = await GetManagerMembersByClubAsync(seminarId, clubId);

            foreach (var member in members)
            {
                member.IsConfirmed = false;
            }

            _context.SeminarMembersManagerRequests.UpdateRange(members);

            await _context.SaveChangesAsync();
        }

        #endregion

        #region SeminarMembers

        public async Task<List<SeminarMemberManagerRequestEntity>> GetRequestedMembers(long seminarId)
        {
            var members = await _context.SeminarMembersManagerRequests.AsQueryable()
                .Where(sm => sm.SeminarId == seminarId
                && sm.IsConfirmed)
                .Include(sm => sm.User)
                .Include(sm => sm.Club)
                .Include(sm => sm.Group)
                .Include(sm => sm.Seminar)
                .Include(sm => sm.SeminarGroup)
                .Include(sm => sm.Coach)
                .Include(sm => sm.Manager)
                .ToListAsync();

            return members;
        }

        public async Task CreateSeminarMembersFromRequest(long seminarId)
        {
            var membersToDelete = _context.SeminarMembers
                .Where(sm => sm.SeminarId == seminarId);

            _context.RemoveRange(membersToDelete);

            var members = await GetRequestedMembers(seminarId);
            var seminarMembersToCreate = new List<SeminarMemberEntity>();

            foreach(var member in members.Where(m => m.IsConfirmed))
            {
                seminarMembersToCreate.Add(new(member));
            }

            await _context.SeminarMembers.AddRangeAsync(seminarMembersToCreate);
            await _context.SaveChangesAsync();
        }

        public async Task CreateSeminarMembers(long seminarId, SeminarMemberListDto memberList)
        {
            var seminar = await _context.Seminars.FindAsync(seminarId);

            if (seminar == null)
            {
                throw new EntityNotFoundException(nameof(seminar));
            }

            await DeleteExcessMembers(seminarId, memberList);

            var seminarMembersCreation = new List<SeminarMemberEntity>();
            var seminarMembersUpdate = new List<SeminarMemberEntity>();

            var seminarMembers = await GetSeminarMembersAsync(seminarId);

            foreach (var member in memberList.Members)
            {
                var userMembership = _context.UserMemberships.AsQueryable()
                    .Where(um => um.UserId == member.UserId
                    && um.GroupId == member.GroupId)
                    .Include(um => um.Club)
                    .Include(um => um.User)
                    .Include(um => um.Group)
                    .FirstOrDefault() ?? throw new EntityNotFoundException(nameof(UserMembershipEntity));

                if (!seminarMembers.Any(m => m.UserId == member.UserId))
                {
                    seminarMembersCreation.Add(new(memberList.CreatorId, seminar, userMembership, member));
                }
                else
                {
                    var currentMember = seminarMembers.First(m => m.UserId == member.UserId);

                    currentMember.UpdateData(seminar, userMembership, member);
                    seminarMembersUpdate.Add(currentMember);
                }
            }

            await _context.SeminarMembers.AddRangeAsync(seminarMembersCreation);
            _context.SeminarMembers.UpdateRange(seminarMembersUpdate);
            await _context.SaveChangesAsync();
        }

        private async Task DeleteExcessMembers(long seminarId, SeminarMemberListDto memberList)
        {
            var userIds = memberList.Members
                .Select(m => m.UserId)
                .ToList();

            var membersToDelete = await _context.SeminarMembers
                .Where(sm => sm.SeminarId == seminarId
                    && !userIds.Contains(sm.UserId))
                .ToListAsync();

            if (membersToDelete.Count == 0)
                return;

            var userIdsToDelete = membersToDelete
                .Select(m => m.UserId)
                .Distinct()
                .ToList();

            var paymentsToDelete = await _context.Payments
                .Where(p => p.EventType == EventType.Seminar
                    && p.EventId == seminarId
                    && userIdsToDelete.Contains(p.UserId))
                .ToListAsync();

            if (paymentsToDelete.Count > 0)
                _context.Payments.RemoveRange(paymentsToDelete);

            _context.SeminarMembers.RemoveRange(membersToDelete);

            await _context.SaveChangesAsync();
        }

        #endregion

        public async Task<List<SeminarMemberManagerRequestEntity>> GetCoachMembersByClub(long seminarId, long clubId, long coachId)
        {
            var members = await _context.SeminarMembersManagerRequests.AsQueryable()
                .Where(sm => sm.SeminarId == seminarId
                && sm.ClubId == clubId && sm.CoachId == coachId)
                .Include(sm => sm.User)
                .Include(sm => sm.Club)
                .Include(sm => sm.Group)
                .Include(sm => sm.Seminar)
                .Include(sm => sm.SeminarGroup)
                .Include(sm => sm.Coach)
                .Include(sm => sm.Manager)
                .ToListAsync();

            return members;
        }

        public async Task CreateSeminarCoachMembers(long seminarId, SeminarMemberCoachRequestListCreationDto memberList)
        {
            var seminar = await _context.Seminars.FindAsync(seminarId);

            if (seminar == null)
            {
                throw new EntityNotFoundException(nameof(seminar));
            }

            await DeleteExcessMembers(seminarId, memberList);

            var seminarMembersCreation = new List<SeminarMemberManagerRequestEntity>();
            var seminarMembersUpdate = new List<SeminarMemberManagerRequestEntity>();

            var seminarMembers = await GetCoachMembersByClub(seminarId, memberList.ClubId.Value, memberList.CoachId.Value);

            foreach (var member in memberList.Members)
            {
                var userMembership = _context.UserMemberships.AsQueryable()
                    .Where(um => um.UserId == member.UserId
                    && um.GroupId == member.GroupId)
                    .Include(um => um.Club)
                    .Include(um => um.User)
                    .Include(um => um.Group)
                    .FirstOrDefault() ?? throw new EntityNotFoundException(nameof(UserMembershipEntity));

                if (!seminarMembers.Any(m => m.UserId == member.UserId))
                {
                    seminarMembersCreation.Add(new(memberList.CoachId.Value, seminar, userMembership, member));
                }
                else
                {
                    var currentMember = seminarMembers.First(m => m.UserId == member.UserId);

                    currentMember.UpdateData(seminar, userMembership, member);
                    seminarMembersUpdate.Add(currentMember);
                }
            }

            await _context.SeminarMembersManagerRequests.AddRangeAsync(seminarMembersCreation);
            _context.SeminarMembersManagerRequests.UpdateRange(seminarMembersUpdate);
            await _context.SaveChangesAsync();
        }

        private async Task DeleteExcessMembers(long seminarId, SeminarMemberCoachRequestListCreationDto memberList)
        {
            var userIds = memberList.Members
                .Select(m => m.UserId)
                .ToList();

            var membersToDelete = await _context.SeminarMembersManagerRequests
                .Where(sm => sm.SeminarId == seminarId
                    && sm.CoachId == memberList.CoachId
                    && sm.ClubId == memberList.ClubId
                    && !userIds.Contains(sm.UserId))
                .ToListAsync();

            if (membersToDelete.Count == 0)
                return;

            var userIdsToDelete = membersToDelete
                .Select(m => m.UserId)
                .Distinct()
                .ToList();

            var paymentsToDelete = await _context.Payments
                .Where(p => p.EventType == EventType.Seminar
                    && p.EventId == seminarId
                    && userIdsToDelete.Contains(p.UserId))
                .ToListAsync();

            if (paymentsToDelete.Count > 0)
                _context.Payments.RemoveRange(paymentsToDelete);

            _context.SeminarMembersManagerRequests.RemoveRange(membersToDelete);

            await _context.SaveChangesAsync();
        }

        public async Task<List<SeminarEntity>> GetUserSeminarHistory(long userId)
        {
            var seminars = await _context.SeminarMembers
                .Where(sm => sm.UserId == userId)
                .Select(sm => sm.Seminar)
                .Where(s => s.IsFinalStatementApplied)
                .ToListAsync();

            return seminars ?? new();
        }

        public async Task<List<SeminarMemberEntity>> GetUserCertificationHistory(long userId)
        {
            var members = await _context.SeminarMembers
                .Include(sm => sm.Seminar)
                .Where(sm => sm.UserId == userId
                    && sm.CertificationGrade != Grade.None
                    && sm.Seminar.IsFinalStatementApplied)
                .ToListAsync();

            return members ?? new();
        }
    }
}
