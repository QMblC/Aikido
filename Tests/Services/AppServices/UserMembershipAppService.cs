using Aikido.AdditionalData.Enums;
using Aikido.Dto.Users.Creation;
using Aikido.Entities;
using Aikido.Entities.Users;
using Aikido.Exceptions;
using Aikido.Services.ApplicationServices.UserMembership;
using Aikido.Services.DatabaseServices.Group;
using Aikido.Services.DatabaseServices.User;
using Aikido.Services.NotificationService;
using Aikido.Services.UnitOfWork;
using DocumentFormat.OpenXml.Spreadsheet;
using FluentAssertions;
using Moq;
using Xunit;

namespace Tests.Services.AppServices
{
    public class UserMembershipApplicationServiceTests
    {
        private readonly Mock<IUserDbService> _userDbServiceMock;
        private readonly Mock<IUserMembershipDbService> _userMembershipDbServiceMock;
        private readonly Mock<IGroupDbService> _groupDbServiceMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;

        private readonly UserMembershipApplicationService _service;

        public UserMembershipApplicationServiceTests()
        {
            _userDbServiceMock = new Mock<IUserDbService>();
            _userMembershipDbServiceMock = new Mock<IUserMembershipDbService>();
            _groupDbServiceMock = new Mock<IGroupDbService>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();

            _service = new UserMembershipApplicationService(
                _userDbServiceMock.Object,
                _userMembershipDbServiceMock.Object,
                _groupDbServiceMock.Object,
                _unitOfWorkMock.Object,
                Mock.Of<INotificationService>()
            );
        }

        private static UserMembershipCreationDto CreateDto(long groupId = 10)
            => new()
            {
                GroupId = groupId,
                RoleInGroup = "User",
                IsMain = false
            };

        #region AddUserMembership

        [Fact]
        public async Task Add_ShouldCreate_WhenMembershipNotExists()
        {
            _userDbServiceMock.Setup(x => x.ExistsActive(1)).ReturnsAsync(true);
            _groupDbServiceMock.Setup(x => x.ExistsActive(10)).ReturnsAsync(true);

            _userMembershipDbServiceMock
                .Setup(x => x.UserMembershipExists(1, 10))
                .ReturnsAsync(false);

            _userMembershipDbServiceMock
                .Setup(x => x.GetActiveUserMembershipsAsUserAsync(1))
                .ReturnsAsync(new List<UserMembershipEntity>());

            var dto = CreateDto();

            await _service.AddUserMembershipAsync(1, dto);

            _userMembershipDbServiceMock.Verify(
                x => x.CreateUserMembershipAsync(1, dto),
                Times.Once);
        }

        [Fact]
        public async Task Add_ShouldThrow_WhenUserNotExists()
        {
            _userDbServiceMock.Setup(x => x.ExistsActive(1))
                .ReturnsAsync(false);

            var act = () => _service.AddUserMembershipAsync(1, CreateDto());

            await act.Should().ThrowAsync<EntityNotFoundException>();
        }

        [Fact]
        public async Task Add_ShouldThrow_WhenGroupNotExists()
        {
            _userDbServiceMock.Setup(x => x.ExistsActive(1)).ReturnsAsync(true);
            _groupDbServiceMock.Setup(x => x.ExistsActive(10)).ReturnsAsync(false);

            var act = () => _service.AddUserMembershipAsync(1, CreateDto());

            await act.Should().ThrowAsync<EntityNotFoundException>();
        }

        [Fact]
        public async Task Add_ShouldCallRecreate_WhenRoleChanged()
        {
            _userDbServiceMock.Setup(x => x.ExistsActive(1)).ReturnsAsync(true);
            _groupDbServiceMock.Setup(x => x.ExistsActive(10)).ReturnsAsync(true);

            var existing = new UserMembershipEntity(1, 10, 20, Role.User);

            _userMembershipDbServiceMock
                .Setup(x => x.UserMembershipExists(1, 10))
                .ReturnsAsync(true);

            _userMembershipDbServiceMock
                .Setup(x => x.GetActiveUserMembership(1, 10))
                .Returns(existing);

            _userMembershipDbServiceMock
                .Setup(x => x.GetActiveUserMembershipsAsUserAsync(1))
                .ReturnsAsync(new List<UserMembershipEntity>
                {
                    new UserMembershipEntity(1, 10, 20) { Id = 1, IsMain = true }
                });

            var dto = new UserMembershipCreationDto
            {
                GroupId = 10,
                RoleInGroup = "Coach",
                IsMain = false
            };

            await _service.AddUserMembershipAsync(1, dto);

            _userMembershipDbServiceMock.Verify(
                x => x.CloseUserMembershipAsync(1, 10),
                Times.AtLeastOnce);
        }

        #endregion

        #region CloseUserMembership

        [Fact]
        public async Task Close_ShouldCallClose_WhenNotMain()
        {
            var entity = new UserMembershipEntity(1, 10, 20)
            {
                IsMain = false
            };

            _userMembershipDbServiceMock
                .Setup(x => x.GetActiveUserMembership(1, 10))
                .Returns(entity);

            await _service.CloseUserMembershipAsync(1, 10);

            _userMembershipDbServiceMock.Verify(
                x => x.CloseUserMembershipAsync(1, 10),
                Times.Once);
        }

        [Fact]
        public async Task CloseUserMembershipAsync_ShouldUnsetMain_WhenMembershipIsMain()
        {
            var membership = new UserMembershipEntity(1, 10, 20)
            {
                IsMain = true,
                User = new UserEntity()
            };

            _userMembershipDbServiceMock
                .Setup(x => x.GetActiveUserMembership(1, 20))
                .Returns(membership);

            _userMembershipDbServiceMock
                .Setup(x => x.GetMainUserMembership(1))
                .Returns(membership);

            _userMembershipDbServiceMock
                .Setup(x => x.CloseUserMembershipAsync(1, 20))
                .Returns(Task.CompletedTask)
                .Callback(() => membership.IsMain = false);

            _userMembershipDbServiceMock
                .Setup(x => x.GetActiveUserMembershipsAsUserAsync(1))
                .ReturnsAsync(new List<UserMembershipEntity>());

            await _service.CloseUserMembershipAsync(1, 20);

            membership.IsMain.Should().BeFalse();

            _userMembershipDbServiceMock.Verify(
                x => x.CloseUserMembershipAsync(1, 20),
                Times.Once);

            _userMembershipDbServiceMock.Verify(
                x => x.UpdateUserMembershipAsync(It.IsAny<UserMembershipEntity>()),
                Times.Once);

            _userDbServiceMock.Verify(
                x => x.UpdateUser(It.IsAny<UserEntity>()),
                Times.AtLeastOnce);
        }

        [Fact]
        public async Task CloseUserMembershipAsync_ShouldNotUnsetMain_WhenMembershipIsNotMain()
        {
            var membership = new UserMembershipEntity(1, 10, 20)
            {
                IsMain = false,
                User = new UserEntity()
            };

            _userMembershipDbServiceMock
                .Setup(x => x.GetActiveUserMembership(1, 20))
                .Returns(membership);

            _userMembershipDbServiceMock
                .Setup(x => x.CloseUserMembershipAsync(1, 20))
                .Returns(Task.CompletedTask);

            await _service.CloseUserMembershipAsync(1, 20);

            membership.IsMain.Should().BeFalse();

            _userMembershipDbServiceMock.Verify(
                x => x.CloseUserMembershipAsync(1, 20),
                Times.Once);

            _userMembershipDbServiceMock.Verify(
                x => x.UpdateUserMembershipAsync(It.IsAny<UserMembershipEntity>()),
                Times.Never);
        }

        #endregion

        #region CloseExcessUserMemberships

        [Fact]
        public async Task CloseExcess_ShouldCloseMissingOnes()
        {
            _userMembershipDbServiceMock
                .Setup(x => x.GetActiveUserMembershipsAsync(1))
                .ReturnsAsync(new List<UserMembershipEntity>
                {
                    new UserMembershipEntity(1, 10, 20) { Id = 1 },
                    new UserMembershipEntity(1, 10, 21) { Id = 2 }
                });

            var newList = new List<UserMembershipCreationDto>
            {
                new() { GroupId = 20 }
            };

            await _service.CloseExcessUserMemberships(1, newList);

            _userMembershipDbServiceMock.Verify(
                x => x.CloseUserMembershipsAsync(It.IsAny<List<long>>()),
                Times.Once);
        }

        [Fact]
        public async Task CloseExcess_ShouldSetMain_WhenMainRemoved()
        {
            _userMembershipDbServiceMock
                .Setup(x => x.GetActiveUserMembershipsAsUserAsync(1))
                .ReturnsAsync(new List<UserMembershipEntity>
                {
                    new UserMembershipEntity(1, 10, 20) { Id = 1, IsMain = true }
                });

            _userMembershipDbServiceMock
                .Setup(x => x.GetActiveUserMembershipsAsync(1))
                .ReturnsAsync(new List<UserMembershipEntity>
                {
                    new UserMembershipEntity(1, 10, 20) { Id = 1, IsMain = true }
                });

            var newList = new List<UserMembershipCreationDto>();

            await _service.CloseExcessUserMemberships(1, newList);

            _userDbServiceMock.Verify(
                x => x.UpdateUser(It.IsAny<UserEntity>()),
                Times.AtLeastOnce);
        }

        #endregion

        #region MainMembershipLogic

        [Fact]
        public async Task ShouldCreateUserMembership_WhenUserHasNoExistingMembership()
        {
            var userId = 1;
            var groupId = 10;

            _userDbServiceMock
                .Setup(x => x.ExistsActive(userId))
                .ReturnsAsync(true);

            _groupDbServiceMock
                .Setup(x => x.ExistsActive(groupId))
                .ReturnsAsync(true);

            _userMembershipDbServiceMock
                .Setup(x => x.UserMembershipExists(userId, groupId))
                .ReturnsAsync(false);

            _userMembershipDbServiceMock
                .Setup(x => x.GetActiveUserMembershipsAsUserAsync(userId))
                .ReturnsAsync(new List<UserMembershipEntity>());

            var dto = new UserMembershipCreationDto
            {
                GroupId = groupId,
                RoleInGroup = "User",
                IsMain = false
            };

            await _service.AddUserMembershipAsync(userId, dto);

            _userMembershipDbServiceMock.Verify(
                x => x.CreateUserMembershipAsync(userId, It.IsAny<UserMembershipCreationDto>()),
                Times.Once);
        }

        [Fact]
        public async Task ShouldRecreateUserMembership_WhenUserRoleHasChanged()
        {
            var userId = 1;
            var groupId = 10;

            _userDbServiceMock
                .Setup(x => x.ExistsActive(userId))
                .ReturnsAsync(true);

            _groupDbServiceMock
                .Setup(x => x.ExistsActive(groupId))
                .ReturnsAsync(true);

            var existing = new UserMembershipEntity(userId, 10, groupId)
            {
                RoleInGroup = Role.User,
                IsMain = false
            };

            _userMembershipDbServiceMock
                .Setup(x => x.UserMembershipExists(userId, groupId))
                .ReturnsAsync(true);

            _userMembershipDbServiceMock
                .Setup(x => x.GetActiveUserMembership(userId, groupId))
                .Returns(existing);

            _userMembershipDbServiceMock
                .Setup(x => x.GetActiveUserMembershipsAsUserAsync(userId))
                .ReturnsAsync(new List<UserMembershipEntity>
                {
                    new UserMembershipEntity(1, 10, 20) { Id = 1, IsMain = true }
                });


            var dto = new UserMembershipCreationDto
            {
                GroupId = groupId,
                RoleInGroup = "Coach",
                IsMain = false
            };

            await _service.AddUserMembershipAsync(userId, dto);

            _userMembershipDbServiceMock.Verify(
                x => x.CloseUserMembershipAsync(userId, groupId),
                Times.Once);

            _userMembershipDbServiceMock.Verify(
                x => x.CreateUserMembershipAsync(userId, It.IsAny<UserMembershipCreationDto>()),
                Times.Once);
        }

        #endregion

        #region Helpers

        [Fact]
        public async Task Recover_ShouldCallDb()
        {
            await _service.RecoverUserMembershipAsync(1, 10);

            _userMembershipDbServiceMock.Verify(
                x => x.RecoverUserMembershipAsync(1, 10),
                Times.Once);
        }

        [Fact]
        public async Task Remove_ShouldCallDb()
        {
            await _service.RemoveUserMembership(1, 10);

            _userMembershipDbServiceMock.Verify(
                x => x.RemoveUserMembershipAsync(1, 10),
                Times.Once);
        }

        #endregion
    }
}