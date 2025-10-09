using Aikido.Dto.Seminars;
using Aikido.Entities.Seminar;
using Aikido.Exceptions;
using Aikido.Services.DatabaseServices.Seminar;
using Aikido.Services.DatabaseServices.User;

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
                throw new EntityNotFoundException($"Семинар с Id = {id} не найден");

            await _seminarDbService.UpdateAsync(id, seminarData);
        }

        public async Task DeleteSeminarAsync(long id)
        {
            if (!await _seminarDbService.Exists(id))
                throw new EntityNotFoundException($"Семинар с Id = {id} не найден");

            await _seminarDbService.DeleteAsync(id);
        }

        public async Task<List<SeminarMemberDto>> GetSeminarMembersAsync(long seminarId)
        {
            var members = await _seminarDbService.GetSeminarMembersAsync(seminarId);
            return members.Select(sm => new SeminarMemberDto(sm)).ToList();
        }

        public async Task RemoveMemberFromSeminarAsync(long seminarId, long userId)
        {
            await _seminarDbService.RemoveMemberAsync(seminarId, userId);
        }

        public async Task<bool> SeminarExistsAsync(long id)
        {
            return await _seminarDbService.Exists(id);
        }

        public async Task<byte[]> GetSeminarRegulationAsync(long seminarId)
        {
            var regulation = await _seminarDbService.GetSeminarRegulation(seminarId);

            return regulation.File;
        }

        public async Task AddSeminarRegulationAsync(long seminarId, byte[] fileInBytes)
        {
            await _seminarDbService.CreateSeminarRegulationAsync(seminarId, fileInBytes);
        }

        public async Task DeleteSeminarRegulationAsync(long seminarId)
        {
            await _seminarDbService.DeleteSeminarRegulationAsync(seminarId);
        }
    }
}
