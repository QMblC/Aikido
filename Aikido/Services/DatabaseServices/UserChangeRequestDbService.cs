using Aikido.Data;
using Aikido.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Aikido.Dto.Users;

namespace Aikido.Services.DatabaseServices
{
    public class UserChangeRequestDbService
    {
        private readonly AppDbContext _context;

        public UserChangeRequestDbService(AppDbContext context)
        {
            _context = context;
        }

        // Создать заявку на создание пользователя
        public async Task<long> CreateUserRequest(long coachId, UserDto userData)
        {
            var request = new UserChangeRequestEntity
            {
                RequestType = ChangeRequestType.Create,
                RequestedById = coachId,
                UserDataJson = JsonSerializer.Serialize(userData),
                Status = RequestStatus.Pending
            };

            _context.UserChangeRequests.Add(request);
            await _context.SaveChangesAsync();
            return request.Id;
        }

        // Создать заявку на редактирование
        public async Task<long> UpdateUserRequest(long coachId, long userId, UserDto userData)
        {
            var targetUser = await _context.Users.FindAsync(userId);
            if (targetUser == null)
                throw new KeyNotFoundException($"Пользователь {userId} не найден");

            var request = new UserChangeRequestEntity
            {
                RequestType = ChangeRequestType.Update,
                RequestedById = coachId,
                TargetUserId = userId,
                UserDataJson = JsonSerializer.Serialize(userData),
                Status = RequestStatus.Pending
            };

            _context.UserChangeRequests.Add(request);
            await _context.SaveChangesAsync();
            return request.Id;
        }

        // Создать заявку на удаление
        public async Task<long> DeleteUserRequest(long coachId, long userId)
        {
            var targetUser = await _context.Users.FindAsync(userId);
            if (targetUser == null)
                throw new KeyNotFoundException($"Пользователь {userId} не найден");

            var request = new UserChangeRequestEntity
            {
                RequestType = ChangeRequestType.Delete,
                RequestedById = coachId,
                TargetUserId = userId,
                Status = RequestStatus.Pending
            };

            _context.UserChangeRequests.Add(request);
            await _context.SaveChangesAsync();
            return request.Id;
        }

        // Получить все ожидающие заявки
        public async Task<List<UserChangePendingRequestDto>> GetPendingRequests()
        {
            return await _context.UserChangeRequests
                .Include(r => r.RequestedBy)
                .Include(r => r.TargetUser)
                .Where(r => r.Status == RequestStatus.Pending)
                .OrderBy(r => r.CreatedAt)
                .Select(r => new UserChangePendingRequestDto(r))
                .ToListAsync();
        }

        // Одобрить заявку
        public async Task ApproveRequest(long requestId)
        {
            var request = await _context.UserChangeRequests
                .Include(r => r.TargetUser)
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request == null)
                throw new KeyNotFoundException("Заявка не найдена");

            if (request.Status != RequestStatus.Pending)
                throw new InvalidOperationException("Заявка уже обработана");

            request.Status = RequestStatus.Approved;
            request.ReviewedAt = DateTime.UtcNow;

            await ApplyChanges(request);

            request.Status = RequestStatus.Applied;
            await _context.SaveChangesAsync();
        }

        public async Task RejectRequest(long requestId)
        {
            var request = await _context.UserChangeRequests.FindAsync(requestId);
            if (request == null)
                throw new KeyNotFoundException("Заявка не найдена");

            if (request.Status != RequestStatus.Pending)
                throw new InvalidOperationException("Заявка уже обработана");

            request.Status = RequestStatus.Rejected;
            request.ReviewedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        private async Task ApplyChanges(UserChangeRequestEntity request)
        {
            switch (request.RequestType)
            {
                case ChangeRequestType.Create:
                    var newUserData = JsonSerializer.Deserialize<UserDto>(request.UserDataJson!);
                    var newUser = new UserEntity(newUserData!);
                    _context.Users.Add(newUser);
                    break;

                case ChangeRequestType.Update:
                    var updateData = JsonSerializer.Deserialize<UserDto>(request.UserDataJson!);
                    var existingUser = await _context.Users.FindAsync(request.TargetUserId);
                    if (existingUser != null)
                    {
                        existingUser.UpdateFromJson(updateData!);
                    }
                    break;

                case ChangeRequestType.Delete:
                    var userToDelete = await _context.Users.FindAsync(request.TargetUserId);
                    if (userToDelete != null)
                    {
                        _context.Users.Remove(userToDelete);
                    }
                    break;
            }
        }
    }
}
