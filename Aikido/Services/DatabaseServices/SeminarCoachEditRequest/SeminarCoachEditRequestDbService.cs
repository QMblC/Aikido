using Aikido.AdditionalData.Enums;
using Aikido.Data;
using Aikido.Dto.Seminars.Members;
using Aikido.Dto.Seminars.Members.CoachEditRequest;
using Aikido.Dto.Seminars.Members.Creation;
using Aikido.Entities;
using Aikido.Entities.Seminar;
using Aikido.Entities.Seminar.SeminarMember;
using Aikido.Entities.Seminar.SeminarMemberRequest;
using Aikido.Entities.Users;
using Aikido.Exceptions;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Aikido.Services.DatabaseServices.Seminar
{
    public class SeminarCoachEditRequestDbService 
    {
        private readonly AppDbContext _context;

        public SeminarCoachEditRequestDbService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<SeminarMemberCoachRequestEntity> GetCoachRequest(long id)
        {
            return await _context.SeminarMemberCoachRequests.FindAsync(id)
                ?? throw new EntityNotFoundException(nameof(SeminarMemberCoachRequestEntity));
        }

        public async Task<List<SeminarMemberCoachRequestEntity>> GetCoachRequestsByClub(long seminarId,
            long clubId,
            long coachId)
        {
            var requests = await _context.SeminarMemberCoachRequests
                .Include(r => r.Club)
                .Include(r => r.RequestedBy)
                .Include(r => r.Seminar)
                .Include(r => r.ReviewedBy)
                .Where(r => r.SeminarId == seminarId
                && r.ClubId == clubId
                && r.RequestedById == coachId)
                .OrderByDescending(r => r.RequestedById)
                .ToListAsync();

            return requests;
        }

        public async Task<List<SeminarMemberRequestDto>> GetCoachRequestData(long id)
        {
            var request = await GetCoachRequest(id);
            var data = JsonSerializer.Deserialize<List<SeminarMemberRequestCreationDto>>(request.RequestJson);
            var dataOutput = new List<SeminarMemberRequestDto>();

            foreach (var member in data)
            {
                var user = await _context.Users.FindAsync(member.UserId);
                var group = await _context.Groups
                    .Include(g => g.UserMemberships)
                        .ThenInclude(um => um.User)
                    .Include(g => g.Club)
                        .ThenInclude(c => c.Manager)
                    .FirstOrDefaultAsync(g => g.Id == member.GroupId);
                var seminar = await _context.Seminars.FindAsync(request.SeminarId);
                var seminarGroup = await _context.SeminarGroups.FindAsync(member.SeminarGroupId);

                dataOutput.Add(new(member, user, seminar, seminarGroup, group));
            }

            return dataOutput;
        }

        public async Task<List<SeminarMemberCoachRequestEntity>> GetCoachRequests(long seminarId, long clubId)
        {
            return await _context.SeminarMemberCoachRequests
                .Where(r => r.SeminarId == seminarId
                && r.ClubId == clubId)
                .ToListAsync();
        }

        public async Task<SeminarMemberCoachRequestEntity> GetCoachRequest(long seminarId, long clubId, long coachId)
        {
            return await _context.SeminarMemberCoachRequests
                .FirstOrDefaultAsync(r => r.SeminarId == seminarId
                && r.ClubId == clubId
                && r.RequestedById == coachId)
                ?? throw new EntityNotFoundException(nameof(SeminarMemberCoachRequestEntity));
        }

        public async Task CreateCoachRequest(long seminarId, SeminarMemberCoachRequestListCreationDto requestData)
        {
            var request = new SeminarMemberCoachRequestEntity(seminarId, requestData);

            await _context.SeminarMemberCoachRequests.AddAsync(request);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateRequestByCoach(long id, SeminarMemberCoachRequestListCreationDto requestData)
        {
            var request = await GetCoachRequest(id);

            request.UpdateByCoach(requestData);

            _context.SeminarMemberCoachRequests.Update(request);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteRequest(long id)
        {
            var request = await GetCoachRequest(id);

            _context.SeminarMemberCoachRequests.Remove(request);
            await _context.SaveChangesAsync();
        }

        public async Task ApplyRequest(long id, long reviewerId)
        {
            var request = await GetCoachRequest(id);

            request.Status = RequestStatus.Applied;
            request.ReviewedAt = DateTime.UtcNow;
            request.ReviewerComment = null;
            request.ReviewedById = reviewerId;

            _context.SeminarMemberCoachRequests.Update(request);
            await _context.SaveChangesAsync();
        }

        public async Task RejectRequest(long id, long reviewerId, string comment)
        {
            var request = await GetCoachRequest(id);

            request.ReviewedAt = DateTime.UtcNow;
            request.Status = RequestStatus.Rejected;
            request.ReviewedById = reviewerId;
            request.ReviewerComment = comment;

            _context.SeminarMemberCoachRequests.Update(request);
            await _context.SaveChangesAsync();
        }
    }
}