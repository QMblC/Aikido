using Aikido.AdditionalData.Enums;
using Aikido.Application.Services;
using Aikido.Dto.Users.Creation;
using Aikido.Entities;
using Aikido.Entities.Users;
using Aikido.Services.ApplicationServices.UserMembership;
using Aikido.Services.DatabaseServices;
using Aikido.Services.DatabaseServices.Club;
using Aikido.Services.DatabaseServices.Group;
using Aikido.Services.DatabaseServices.Payment;
using Aikido.Services.DatabaseServices.Seminar;
using Aikido.Services.DatabaseServices.User;
using Aikido.Services.FileStorageServices;
using Aikido.Services.NotificationService;
using Aikido.Services.UnitOfWork;
using Microsoft.AspNetCore.Http;
using Moq;

namespace Tests.Services.AppServices
{
    public class UserAppServiceTests
    {
        private readonly Mock<IUserDbService> _userDbServiceMock;
        private readonly Mock<IUserMembershipDbService> _userMembershipDbServiceMock;
        private readonly Mock<IClubDbService> _clubDbServiceMock;
        private readonly Mock<IGroupDbService> _groupDbServiceMock;
        private readonly Mock<IUserMembershipApplicationService> _userMembershipApplicationServiceMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ISeminarDbService> _seminarDbServiceMock;
        private readonly Mock<IClubStaffDbService> _clubStaffDbServiceMock;
        private readonly Mock<INotificationService> _notificationServiceMock;
        private readonly Mock<IPaymentDbService> _paymentDbServiceMock;
        private readonly Mock<IFileStorageService> _fileStorageServiceMock;

        private readonly UserApplicationService _service;

        public UserAppServiceTests()
        {
            _userDbServiceMock = new Mock<IUserDbService>();
            _userMembershipDbServiceMock = new Mock<IUserMembershipDbService>();
            _clubDbServiceMock = new Mock<IClubDbService>();
            _groupDbServiceMock = new Mock<IGroupDbService>();
            _userMembershipApplicationServiceMock = new Mock<IUserMembershipApplicationService>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _seminarDbServiceMock = new Mock<ISeminarDbService>();
            _clubStaffDbServiceMock = new Mock<IClubStaffDbService>();
            _notificationServiceMock = new Mock<INotificationService>();
            _paymentDbServiceMock = new Mock<IPaymentDbService>();
            _fileStorageServiceMock = new Mock<IFileStorageService>();

            _service = new UserApplicationService(
                _userDbServiceMock.Object,
                _userMembershipDbServiceMock.Object,
                _clubDbServiceMock.Object,
                _groupDbServiceMock.Object,
                _userMembershipApplicationServiceMock.Object,
                _unitOfWorkMock.Object,
                _seminarDbServiceMock.Object,
                _clubStaffDbServiceMock.Object,
                _notificationServiceMock.Object,
                _paymentDbServiceMock.Object,
                _fileStorageServiceMock.Object
            );
        }

        private UserApplicationService CreateService()
        {
            return new UserApplicationService(
                _userDbServiceMock.Object,
                _userMembershipDbServiceMock.Object,
                _clubDbServiceMock.Object,
                _groupDbServiceMock.Object,
                _userMembershipApplicationServiceMock.Object,
                _unitOfWorkMock.Object,
                _seminarDbServiceMock.Object,
                _clubStaffDbServiceMock.Object,
                _notificationServiceMock.Object,
                _paymentDbServiceMock.Object,
                _fileStorageServiceMock.Object
            );
        }


        #region GetMethods

        [Fact]
        public async Task GetUserById_ShouldReturnUser_WhenUserExists()
        {
            var userId = 1;

            var user = new UserEntity { Id = userId };
            var memberships = new List<UserMembershipEntity>();

            _userDbServiceMock
                .Setup(x => x.GetByIdOrThrowException(userId))
                .ReturnsAsync(user);

            _userMembershipDbServiceMock
                .Setup(x => x.GetActiveUserMembershipsAsync(userId))
                .ReturnsAsync(memberships);

            var service = CreateService();

            var result = await service.GetUserByIdAsync(userId);

            result.Should().NotBeNull();
            result.Id.Should().Be(userId);
        }

        [Fact]
        public async Task GetUsers_ShouldReturnMappedUsers()
        {
            var ids = new List<long> { 1, 2 };

            var users = new List<UserEntity>
            {
                new UserEntity { Id = 1 },
                new UserEntity { Id = 2 }
            };

            _userDbServiceMock
                .Setup(x => x.GetUsersAsync(ids))
                .ReturnsAsync(users);

            var service = CreateService();

            var result = await service.GetUsers(ids);

            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetActiveUserShortList_ShouldReturnUsers()
        {
            var users = new List<UserEntity>
            {
                new UserEntity { Id = 1 }
            };

            _userDbServiceMock
                .Setup(x => x.GetActiveUsersAsync())
                .ReturnsAsync(users);

            var service = CreateService();

            var result = await service.GetActiveUserShortListAsync();

            result.Should().HaveCount(1);
        }

        #endregion

        #region CreateUpdateMethods

        [Fact]
        public async Task CreateUser_ShouldReturnId_WhenValid()
        {
            var dto = new UserCreationDto
            {
                Login = "test",
                Password = "123",
                FirstName = "A",
                LastName = "B",
                UserMembershipDtos = new List<UserMembershipCreationDto>()
            };

            var user = new UserEntity { Id = 10 };

            _userDbServiceMock.Setup(x => x.LoginExists(dto.Login))
                .ReturnsAsync(false);

            _userDbServiceMock.Setup(x => x.CreateUser(dto))
                .ReturnsAsync(user);

            _unitOfWorkMock
                .Setup(x => x.ExecuteInTransactionAsync(It.IsAny<Func<Task>>()))
                .Returns<Func<Task>>(f => f());

            var service = CreateService();

            var result = await service.CreateUserAsync(dto);

            result.Should().Be(10);

            _notificationServiceMock.Verify(
                x => x.UserDataChanged(NotificationAction.Create, 10),
                Times.Once);
        }

        [Fact]
        public async Task CreateUser_ShouldThrow_WhenLoginExists()
        {
            var dto = new UserCreationDto
            {
                Login = "test",
                Password = "123",
                FirstName = "A",
                LastName = "B"
            };

            _userDbServiceMock.Setup(x => x.LoginExists(dto.Login))
                .ReturnsAsync(true);

            var service = CreateService();

            Func<Task> act = async () => await service.CreateUserAsync(dto);

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task UpdateUser_ShouldCallDbService()
        {
            var userId = 1;
            var dto = new UserCreationDto();

            _unitOfWorkMock
                .Setup(x => x.ExecuteInTransactionAsync(It.IsAny<Func<Task>>()))
                .Returns<Func<Task>>(f => f());

            var service = CreateService();

            await service.UpdateUserAsync(userId, dto);

            _userDbServiceMock.Verify(x => x.UpdateUser(userId, dto), Times.Once);
        }

        #endregion

        #region Photo

        [Fact]
        public async Task DeleteUserPhoto_ShouldDeleteFile_WhenExists()
        {
            _fileStorageServiceMock
                .Setup(x => x.FileExists(It.IsAny<string>()))
                .Returns(true);

            _userDbServiceMock
                .Setup(x => x.GetByIdOrThrowException(1))
                .ReturnsAsync(new UserEntity());

            await _service.DeleteUserPhoto(1);

            _fileStorageServiceMock.Verify(x => x.DeleteFile(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task DeleteUserPhoto_ShouldNotThrow_WhenFileMissing()
        {
            _fileStorageServiceMock
                .Setup(x => x.FileExists(It.IsAny<string>()))
                .Returns(false);

            _userDbServiceMock
                .Setup(x => x.GetByIdOrThrowException(1))
                .ReturnsAsync(new UserEntity());

            Func<Task> act = async () => await _service.DeleteUserPhoto(1);

            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task UpdateUserPhoto_ShouldThrow_WhenInvalidExtension()
        {
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(x => x.FileName).Returns("file.txt");

            var service = CreateService();

            Func<Task> act = async () => await service.UpdateUserPhoto(1, fileMock.Object);

            await act.Should().ThrowAsync<InvalidDataException>();
        }

        [Fact]
        public async Task DeleteUserPhoto_ShouldCallStorage()
        {
            var userId = 1;

            _fileStorageServiceMock
                .Setup(x => x.FileExists(It.IsAny<string>()))
                .Returns(true);

            _userDbServiceMock
                .Setup(x => x.GetByIdOrThrowException(userId))
                .ReturnsAsync(new UserEntity { Id = userId });

            _userDbServiceMock
                .Setup(x => x.UpdateUser(It.IsAny<UserEntity>()))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            _notificationServiceMock
                .Setup(x => x.UserDataChanged(It.IsAny<NotificationAction>(), userId))
                .Returns(Task.CompletedTask);

            var service = CreateService();

            await service.DeleteUserPhoto(userId);

            _fileStorageServiceMock.Verify(
                x => x.DeleteFile(It.IsAny<string>()),
                Times.Once);
        }

        #endregion

        #region DeleteCloseRecoverMethods

        [Fact]
        public async Task CloseUser_ShouldThrow_WhenHasMemberships()
        {
            var userId = 1;

            var membership = new UserMembershipEntity(
                userId: userId,
                clubId: 10,
                groupId: 20,
                roleInGroup: Role.User
            );

            var user = new UserEntity
            {
                UserMemberships = new List<UserMembershipEntity>
        {
            membership
        }
            };

            _userDbServiceMock
                .Setup(x => x.GetByIdOrThrowException(userId))
                .ReturnsAsync(user);

            var service = CreateService();

            Func<Task> act = async () => await service.CloseUserAsync(userId);

            await act.Should()
                .ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task RecoverUser_ShouldCallDb()
        {
            var userId = 1;

            _unitOfWorkMock
                .Setup(x => x.ExecuteInTransactionAsync(It.IsAny<Func<Task>>()))
                .Returns<Func<Task>>(f => f());

            var service = CreateService();

            await service.RecoverUserAsync(userId);

            _userDbServiceMock.Verify(x => x.RecoverAsync(userId), Times.Once);
        }

        [Fact]
        public async Task DeleteUser_ShouldCallAllDependencies()
        {
            var userId = 1;

            _unitOfWorkMock
                .Setup(x => x.ExecuteInTransactionAsync(It.IsAny<Func<Task>>()))
                .Returns<Func<Task>>(f => f());

            var service = CreateService();

            await service.DeleteUserAsync(userId);

            _userMembershipDbServiceMock.Verify(x => x.RemoveUserMemberships(userId), Times.Once);
            _userDbServiceMock.Verify(x => x.Delete(userId), Times.Once);
        }

        #endregion

        #region History

        [Fact]
        public async Task GetUserAnnualFees_ShouldReturnDistinctYears()
        {
            var userId = 1;

            var payments = new List<PaymentEntity>
            {
                new PaymentEntity { Status = PaymentStatus.Completed, Type = PaymentType.AnnualFee, Date = new DateTime(2024,1,1) },
                new PaymentEntity { Status = PaymentStatus.Completed, Type = PaymentType.AnnualFee, Date = new DateTime(2025,1,1) }
            };

            _paymentDbServiceMock
                .Setup(x => x.GetPaymentsByUser(userId))
                .ReturnsAsync(payments);

            var service = CreateService();

            var result = await service.GetUserAnnualFees(userId);

            result.Should().Contain(new[] { 2024, 2025 });
        }

        #endregion
    }
}