using Aikido.Dto;
using Aikido.Dto.Seminars;
using Aikido.Services.DatabaseServices.Seminar;
using Aikido.Services.DatabaseServices.User;
using Aikido.Exceptions;

namespace Aikido.Application.Services
{
    public class SeminarApplicationService
    {
        private readonly ISeminarDbService _seminarDbService;
        private readonly IUserDbService _userDbService;

        public SeminarApplicationService(
            ISeminarDbService seminarDbService,
            IUserDbService userDbService)
        {
            _seminarDbService = seminarDbService;
            _userDbService = userDbService;
        }

        public async Task<SeminarDto> GetSeminarByIdAsync(long id)
        {
            var seminar = await _seminarDbService.GetByIdOrThrowException(id);
            return new SeminarDto(seminar);
        }

        public async Task<List<SeminarDto>> GetAllSeminarsAsync()
        {
            var seminars = await _seminarDbService.GetAllAsync();
            return seminars.Select(s => new SeminarDto(s)).ToList();
        }

        public async Task<long> CreateSeminarAsync(SeminarDto seminarData)
        {
            return await _seminarDbService.CreateAsync(seminarData);
        }

        public async Task UpdateSeminarAsync(long id, SeminarDto seminarData)
        {
            if (!await _seminarDbService.Exists(id))
            {
                throw new EntityNotFoundException($"Семинар с Id = {id} не найден");
            }
            await _seminarDbService.UpdateAsync(id, seminarData);
        }

        public async Task DeleteSeminarAsync(long id)
        {
            if (!await _seminarDbService.Exists(id))
            {
                throw new EntityNotFoundException($"Семинар с Id = {id} не найден");
            }
            await _seminarDbService.DeleteAsync(id);
        }

        public async Task<List<SeminarMemberDto>> GetSeminarMembersAsync(long seminarId)
        {
            var members = await _seminarDbService.GetSeminarMembersAsync(seminarId);
            var result = new List<SeminarMemberDto>();

            foreach (var member in members)
            {
                var user = await _userDbService.GetByIdOrThrowException(member.UserId);
                result.Add(new SeminarMemberDto(member, user));
            }

            return result;
        }

        public async Task AddMemberToSeminarAsync(long seminarId, long userId)
        {
            if (!await _seminarDbService.Exists(seminarId))
            {
                throw new EntityNotFoundException($"Семинара с Id = {seminarId} не существует");
            }

            if (!await _userDbService.Exists(userId))
            {
                throw new EntityNotFoundException($"Пользователя с Id = {userId} не существует");
            }

            await _seminarDbService.AddMemberAsync(seminarId, userId);
        }

        public async Task RemoveMemberFromSeminarAsync(long seminarId, long userId)
        {
            await _seminarDbService.RemoveMemberAsync(seminarId, userId);
        }

        public async Task<bool> SeminarExistsAsync(long id)
        {
            return await _seminarDbService.Exists(id);
        }
    }
}