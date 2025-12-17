using Aikido.AdditionalData.Enums;
using Aikido.Data;
using Aikido.Dto.Seminars.Members.TrainerEditRequest;
using Aikido.Entities;
using Aikido.Entities.Seminar;
using Aikido.Entities.Seminar.SeminarMember;
using Aikido.Entities.Seminar.SeminarMemberRequest;
using Aikido.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Aikido.Services.DatabaseServices.Seminar
{
    public class SeminarTrainerEditRequestDbService : ISeminarTrainerEditRequestDbService
    {
        private readonly AppDbContext _context;

        public SeminarTrainerEditRequestDbService(AppDbContext context)
        {
            _context = context;
        }

        public async Task CreateInitialRequestsAsync(long seminarId)
        {
            // Группируем участников по тренерам и клубам
            var membersByTrainerClub = _context.SeminarMembersManagerRequests
                .Where(m => m.CoachId.HasValue
                && m.SeminarId == seminarId)
                .GroupBy(m => new { TrainerId = m.CoachId.Value, m.ClubId })
                .ToList();

            foreach (var group in membersByTrainerClub)
            {
                foreach (var member in group)
                {
                    var request = new SeminarMemberTrainerEditRequestEntity
                    {
                        SeminarId = seminarId,
                        TrainerId = group.Key.TrainerId,
                        ClubId = group.Key.ClubId.Value,
                        UserId = member.UserId,
                        GroupId = member.GroupId,
                        CoachId = member.CoachId,
                        SeminarGroupId = member.SeminarGroupId,
                        CertificationGrade = member.CertificationGrade,
                        Note = member.Note,
                        RequestType = TrainerEditRequestType.Add,
                        Status = TrainerEditRequestStatus.Pending,
                        ManagerId = member.ManagerId,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.SeminarMemberTrainerEditRequests.Add(request);
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task<List<SeminarMemberTrainerEditRequestEntity>> GetTrainerRequestsByClubAsync(
            long seminarId, long trainerId, long clubId)
        {
            return await _context.SeminarMemberTrainerEditRequests
                .Include(r => r.Trainer)
                .Include(r => r.User)
                .Include(r => r.Coach)
                .Include(r => r.Group)
                .Include(r => r.Manager)
                .Where(r => r.SeminarId == seminarId
                    && r.TrainerId == trainerId
                    && r.ClubId == clubId)
                .ToListAsync();
        }

        public async Task<List<SeminarMemberTrainerEditRequestEntity>> GetTrainerAllRequestsAsync(
            long seminarId, long trainerId)
        {
            return await _context.SeminarMemberTrainerEditRequests
                .Include(r => r.Trainer)
                .Include(r => r.User)
                .Include(r => r.Coach)
                .Include(r => r.Group)
                .Include(r => r.Club)
                .Include(r => r.Manager)
                .Where(r => r.SeminarId == seminarId && r.TrainerId == trainerId)
                .ToListAsync();
        }

        public async Task<List<SeminarMemberTrainerEditRequestEntity>> GetManagerRequestsByClubAsync(
            long seminarId, long managerId, long clubId)
        {
            return await _context.SeminarMemberTrainerEditRequests
                .Include(r => r.Trainer)
                .Include(r => r.User)
                .Include(r => r.Manager)
                .Include(r => r.Club)
                .Where(r => r.SeminarId == seminarId
                    && r.ManagerId == managerId
                    && r.ClubId == clubId
                    && r.Status == TrainerEditRequestStatus.Pending)
                .ToListAsync();
        }

        public async Task<List<SeminarMemberTrainerEditRequestEntity>> GetPendingRequestsAsync(long seminarId)
        {
            return await _context.SeminarMemberTrainerEditRequests
                .Include(r => r.Trainer)
                .Include(r => r.User)
                .Include(r => r.Manager)
                .Include(r => r.Club)
                .Where(r => r.SeminarId == seminarId && r.Status == TrainerEditRequestStatus.Pending)
                .ToListAsync();
        }

        public async Task UpdateRequestStatusAsync(long requestId, string status, string? comment = null)
        {
            var request = await GetByIdOrThrowException(requestId);

            request.Status = status.ToLower() switch
            {
                "approved" => TrainerEditRequestStatus.Approved,
                "rejected" => TrainerEditRequestStatus.Rejected,
                "pending" => TrainerEditRequestStatus.Pending,
                _ => throw new InvalidOperationException($"Неизвестный статус: {status}")
            };

            request.ManagerComment = comment;
            request.ReviewedAt = DateTime.UtcNow;

            _context.SeminarMemberTrainerEditRequests.Update(request);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Сохраняет заявки от тренера с автоматическим определением типа операции
        /// 
        /// Алгоритм:
        /// 1. Получаем текущих участников тренера в клубе из SeminarMemberManagerRequest
        /// 2. Сравниваем с новым списком:
        ///    - Члены в новом списке, но не в текущем → Add
        ///    - Члены в новом и в текущем → Update (если данные изменились)
        ///    - Члены в текущем, но не в новом → Delete
        /// </summary>
        public async Task SaveTrainerRequestsAsync(long seminarId, long trainerId, long clubId,
            List<SeminarMemberTrainerEditRequestCreationDto> newRequests)
        {
            // Получаем текущие члены семинара этого тренера в этом клубе из SeminarMemberManagerRequest
            var currentMembers = await _context.SeminarMembersManagerRequests
                .Where(m => m.SeminarId == seminarId
                    && m.CoachId == trainerId
                    && m.ClubId == clubId)
                .ToListAsync();

            // Удаляем старые необработанные заявки этого тренера по этому клубу
            var oldPendingRequests = await _context.SeminarMemberTrainerEditRequests
                .Where(r => r.SeminarId == seminarId
                    && r.TrainerId == trainerId
                    && r.ClubId == clubId
                    && r.Status == TrainerEditRequestStatus.Pending
                    && r.IsApplied == false)
                .ToListAsync();

            _context.SeminarMemberTrainerEditRequests.RemoveRange(oldPendingRequests);

            // Создаем карту новых запросов по UserId
            var newRequestsMap = newRequests.ToDictionary(r => r.UserId);

            // Создаем карту текущих членов по UserId
            var currentMembersMap = currentMembers.ToDictionary(m => m.UserId);

            // ОПЕРАЦИЯ DELETE: члены в текущем списке, но не в новом
            foreach (var currentMember in currentMembers)
            {
                if (!newRequestsMap.ContainsKey(currentMember.UserId))
                {
                    var deleteRequest = new SeminarMemberTrainerEditRequestEntity
                    {
                        SeminarId = seminarId,
                        TrainerId = trainerId,
                        ClubId = clubId,
                        UserId = currentMember.UserId,
                        GroupId = currentMember.GroupId,
                        CoachId = currentMember.CoachId,
                        SeminarGroupId = currentMember.SeminarGroupId,
                        CertificationGrade = currentMember.CertificationGrade,
                        Note = currentMember.Note,
                        RequestType = TrainerEditRequestType.Delete,
                        Status = TrainerEditRequestStatus.Pending,
                        ManagerId = currentMember.ManagerId,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.SeminarMemberTrainerEditRequests.Add(deleteRequest);
                }
            }

            // ОПЕРАЦИЯ ADD и UPDATE
            foreach (var newRequestDto in newRequests)
            {
                if (currentMembersMap.TryGetValue(newRequestDto.UserId, out var currentMember))
                {
                    // UPDATE: существует в обоих списках
                    // Проверяем, изменились ли данные
                    bool hasChanges = HasChanges(currentMember, newRequestDto);

                    if (hasChanges)
                    {
                        var updateRequest = new SeminarMemberTrainerEditRequestEntity
                        {
                            SeminarId = seminarId,
                            TrainerId = trainerId,
                            ClubId = clubId,
                            UserId = newRequestDto.UserId,
                            GroupId = newRequestDto.GroupId,
                            CoachId = newRequestDto.CoachId,
                            SeminarGroupId = newRequestDto.SeminarGroupId,
                            CertificationGrade = newRequestDto.CertificationGrade == null
                                ? Grade.None
                                : EnumParser.ConvertStringToEnum<Grade>(newRequestDto.CertificationGrade),
                            Note = newRequestDto.Note,
                            RequestType = TrainerEditRequestType.Update,
                            Status = TrainerEditRequestStatus.Pending,
                            ManagerId = currentMember.ManagerId,
                            CreatedAt = DateTime.UtcNow
                        };

                        _context.SeminarMemberTrainerEditRequests.Add(updateRequest);
                    }
                }
                else
                {
                    // ADD: существует только в новом списке
                    var addRequest = new SeminarMemberTrainerEditRequestEntity
                    {
                        SeminarId = seminarId,
                        TrainerId = trainerId,
                        ClubId = clubId,
                        UserId = newRequestDto.UserId,
                        GroupId = newRequestDto.GroupId,
                        CoachId = newRequestDto.CoachId,
                        SeminarGroupId = newRequestDto.SeminarGroupId,
                        CertificationGrade = newRequestDto.CertificationGrade == null
                            ? Grade.None
                            : EnumParser.ConvertStringToEnum<Grade>(newRequestDto.CertificationGrade),
                        Note = newRequestDto.Note,
                        RequestType = TrainerEditRequestType.Add,
                        Status = TrainerEditRequestStatus.Pending,
                        ManagerId = null, // Будет получено при применении
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.SeminarMemberTrainerEditRequests.Add(addRequest);
                }
            }

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Проверяет, изменились ли значимые данные члена семинара
        /// </summary>
        private bool HasChanges(SeminarMemberManagerRequestEntity current,
            SeminarMemberTrainerEditRequestCreationDto newData)
        {
            // Сравниваем ключевые поля
            if (current.GroupId != newData.GroupId)
                return true;

            if (current.CoachId != newData.CoachId)
                return true;

            if (current.SeminarGroupId != newData.SeminarGroupId)
                return true;

            var newCertGrade = newData.CertificationGrade == null
                ? Grade.None
                : EnumParser.ConvertStringToEnum<Grade>(newData.CertificationGrade);

            if (current.CertificationGrade != newCertGrade)
                return true;

            if (current.Note != newData.Note)
                return true;

            return false;
        }

        public async Task ApplyRequestAsync(long requestId)
        {
            var request = await GetByIdOrThrowException(requestId);

            if (request.Status != TrainerEditRequestStatus.Approved)
                throw new InvalidOperationException("Можно применить только одобренные заявки");

            // Получаем менеджера для новых Add-запросов
            if (request.RequestType == TrainerEditRequestType.Add && request.ManagerId == null)
            {
                var club = await _context.Clubs
                    .FirstOrDefaultAsync(c => c.Id == request.ClubId);

                if (club != null)
                {
                    request.ManagerId = club.ManagerId;
                }
            }

            // Получаем текущего члена по UserId
            var managerRequest = await _context.SeminarMembersManagerRequests
                .FirstOrDefaultAsync(r => r.SeminarId == request.SeminarId
                    && r.UserId == request.UserId
                    && r.ClubId == request.ClubId);

            if (request.RequestType == TrainerEditRequestType.Add)
            {
                // ADD: создаем нового члена если его еще нет
                if (managerRequest == null)
                {
                    // Получаем OldGrade из UserEntity
                    var user = await _context.Users
                        .FirstOrDefaultAsync(u => u.Id == request.UserId);

                    managerRequest = new SeminarMemberManagerRequestEntity
                    {
                        SeminarId = request.SeminarId,
                        UserId = request.UserId,
                        GroupId = request.GroupId,
                        ClubId = request.ClubId,
                        CoachId = request.CoachId,
                        SeminarGroupId = request.SeminarGroupId,
                        OldGrade = user?.Grade ?? Grade.None,
                        CertificationGrade = request.CertificationGrade,
                        Note = request.Note,
                        ManagerId = request.ManagerId
                    };
                    _context.SeminarMembersManagerRequests.Add(managerRequest);
                }
            }
            else if (request.RequestType == TrainerEditRequestType.Update)
            {
                // UPDATE: обновляем существующего члена
                if (managerRequest != null)
                {
                    managerRequest.GroupId = request.GroupId;
                    managerRequest.CoachId = request.CoachId;
                    managerRequest.SeminarGroupId = request.SeminarGroupId;
                    managerRequest.CertificationGrade = request.CertificationGrade;
                    managerRequest.Note = request.Note;
                    _context.SeminarMembersManagerRequests.Update(managerRequest);
                }
                else
                {
                    // Если при UPDATE члена не нашли, создаем его
                    var user = await _context.Users
                        .FirstOrDefaultAsync(u => u.Id == request.UserId);

                    managerRequest = new SeminarMemberManagerRequestEntity
                    {
                        SeminarId = request.SeminarId,
                        UserId = request.UserId,
                        GroupId = request.GroupId,
                        ClubId = request.ClubId,
                        CoachId = request.CoachId,
                        SeminarGroupId = request.SeminarGroupId,
                        OldGrade = user?.Grade ?? Grade.None,
                        CertificationGrade = request.CertificationGrade,
                        Note = request.Note,
                        ManagerId = request.ManagerId
                    };
                    _context.SeminarMembersManagerRequests.Add(managerRequest);
                }
            }
            else if (request.RequestType == TrainerEditRequestType.Delete)
            {
                // DELETE: удаляем члена
                if (managerRequest != null)
                {
                    _context.SeminarMembersManagerRequests.Remove(managerRequest);
                }
            }

            request.Status = TrainerEditRequestStatus.Applied;
            request.IsApplied = true;
            request.ReviewedAt = DateTime.UtcNow;

            _context.SeminarMemberTrainerEditRequests.Update(request);
            await _context.SaveChangesAsync();
        }

        public async Task<SeminarMemberTrainerEditRequestEntity> GetByIdOrThrowException(long id)
        {
            var request = await _context.SeminarMemberTrainerEditRequests
                .Include(r => r.Trainer)
                .Include(r => r.User)
                .Include(r => r.Coach)
                .Include(r => r.Manager)
                .FirstOrDefaultAsync(r => r.Id == id);

            return request ?? throw new EntityNotFoundException(
                $"Заявка с Id = {id} не найдена");
        }
    }
}