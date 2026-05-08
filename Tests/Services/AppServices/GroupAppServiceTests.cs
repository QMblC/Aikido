using Aikido.AdditionalData.Enums;
using Aikido.Application.Services;
using Aikido.Dto.Groups;
using Aikido.Dto.Users.Creation;
using Aikido.Entities;
using Aikido.Entities.Users;
using Aikido.Exceptions;
using Aikido.Services.ApplicationServices.Attendnace;
using Aikido.Services.ApplicationServices.UserMembership;
using Aikido.Services.DatabaseServices.Club;
using Aikido.Services.DatabaseServices.Group;
using Aikido.Services.DatabaseServices.Schedule;
using Aikido.Services.DatabaseServices.User;
using Aikido.Services.NotificationService;
using Aikido.Services.UnitOfWork;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.Services.AppServices
{
    public class GroupAppServiceTests
    {
        private readonly Mock<IGroupDbService> _groupDbServiceMock = new();
        private readonly Mock<IUserDbService> _userDbServiceMock = new();
        private readonly Mock<IUserMembershipDbService> _userMembershipDbServiceMock = new();
        private readonly Mock<IClubDbService> _clubDbServiceMock = new();
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
        private readonly Mock<IScheduleDbService> _scheduleDbServiceMock = new();
        private readonly Mock<IUserMembershipApplicationService> _userMembershipAppMock = new();
        private readonly Mock<IAttendanceApplicationService> _attendanceAppMock = new();
        private readonly Mock<INotificationService> _notificationMock = new();

        private readonly GroupApplicationService _service;

        public GroupAppServiceTests()
        {
            _service = new GroupApplicationService(
                _groupDbServiceMock.Object,
                _userDbServiceMock.Object,
                _userMembershipDbServiceMock.Object,
                _clubDbServiceMock.Object,
                _unitOfWorkMock.Object,
                _scheduleDbServiceMock.Object,
                _userMembershipAppMock.Object,
                _attendanceAppMock.Object,
                _notificationMock.Object
            );
        }

        #region Get

        [Fact]
        public async Task GetGroupById_ShouldReturnGroupDto()
        {
            var group = new GroupEntity { Id = 1 };

            _groupDbServiceMock
                .Setup(x => x.GetByIdOrThrowException(1, It.IsAny<bool>()))
                .ReturnsAsync(group);

            var result = await _service.GetGroupByIdAsync(1);

            result.Id.Should().Be(1);
        }

        [Fact]
        public async Task GetAllGroups_ShouldReturnList()
        {
            _groupDbServiceMock
                .Setup(x => x.GetAllActiveAsync())
                .ReturnsAsync(new List<GroupEntity>
                {
            new() { Id = 1 },
            new() { Id = 2 }
                });

            var result = await _service.GetAllGroupsAsync();

            result.Should().HaveCount(2);
        }

        #endregion

        #region CreateUpdate

        [Fact]
        public async Task CreateGroup_ShouldReturnId_AndCreateCoaches()
        {
            var dto = new GroupCreationDto
            {
                ClubId = 1,
                Coaches = new List<long> { 10 },
                MainCoachId = 5
            };

            _clubDbServiceMock.Setup(x => x.ExistsActive(1)).ReturnsAsync(true);

            _groupDbServiceMock
                .Setup(x => x.CreateAsync(dto))
                .ReturnsAsync(new GroupEntity { Id = 100 });

            _unitOfWorkMock
                .Setup(x => x.ExecuteInTransactionAsync(It.IsAny<Func<Task>>()))
                .Returns<Func<Task>>(f => f());

            var result = await _service.CreateGroupAsync(dto);

            result.Should().Be(100);

            _userMembershipAppMock.Verify(
                x => x.AddUserMembershipAsync(10, It.IsAny<UserMembershipCreationDto>()),
                Times.Once);
        }

        [Fact]
        public async Task CreateGroup_ShouldThrow_WhenMainCoachMissing()
        {
            var dto = new GroupCreationDto
            {
                ClubId = 1,
                Coaches = new List<long> { 10 },
                MainCoachId = null
            };

            _clubDbServiceMock.Setup(x => x.ExistsActive(1)).ReturnsAsync(true);

            Func<Task> act = async () => await _service.CreateGroupAsync(dto);

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task CreateGroup_ShouldReturnId_AndSendNotification()
        {
            var dto = new GroupCreationDto
            {
                ClubId = 1,
                Coaches = new List<long>()
            };

            _clubDbServiceMock
                .Setup(x => x.ExistsActive(1))
                .ReturnsAsync(true);

            _groupDbServiceMock
                .Setup(x => x.CreateAsync(dto))
                .ReturnsAsync(new GroupEntity { Id = 10 });

            _unitOfWorkMock
                .Setup(x => x.ExecuteInTransactionAsync(It.IsAny<Func<Task>>()))
                .Returns<Func<Task>>(f => f());

            var result = await _service.CreateGroupAsync(dto);

            result.Should().Be(10);

            _notificationMock.Verify(
                x => x.GroupDataChanged(NotificationAction.Create, 10),
                Times.Once);
        }

        [Fact]
        public async Task CreateGroup_ShouldThrow_WhenClubNotExists()
        {
            var dto = new GroupCreationDto
            {
                ClubId = 1,
                Coaches = new List<long>()
            };

            _clubDbServiceMock
                .Setup(x => x.ExistsActive(1))
                .ReturnsAsync(false);

            Func<Task> act = async () => await _service.CreateGroupAsync(dto);

            await act.Should().ThrowAsync<EntityNotFoundException>();
        }

        #endregion

        #region CoachManagement

        [Fact]
        public async Task UpdateGroup_ShouldSyncCoaches()
        {
            var groupId = 1;

            var dto = new GroupCreationDto
            {
                ClubId = 1,
                Coaches = new List<long> { 1, 2 },
                MainCoachId = 1
            };

            _groupDbServiceMock
                .Setup(x => x.ExistsActive(groupId))
                .ReturnsAsync(true);

            _clubDbServiceMock
                .Setup(x => x.ExistsActive(1))
                .ReturnsAsync(true);

            _groupDbServiceMock
                .Setup(x => x.GetGroupByIdAsync(groupId))
                .ReturnsAsync(new GroupEntity
                {
                    UserMemberships = new List<UserMembershipEntity>()
                });

            _unitOfWorkMock
                .Setup(x => x.ExecuteInTransactionAsync(It.IsAny<Func<Task>>()))
                .Returns<Func<Task>>(f => f());

            await _service.UpdateGroupAsync(groupId, dto);

            _userMembershipAppMock.Verify(
                x => x.AddUserMembershipAsync(It.IsAny<long>(), It.IsAny<UserMembershipCreationDto>()),
                Times.Exactly(2));
        }

        #endregion

        #region CloseRecoverDelete

        [Fact]
        public async Task CloseGroup_ShouldCallClose_WhenNoUsers()
        {
            var groupId = 1;

            _groupDbServiceMock
                .Setup(x => x.GetGroupByIdAsync(groupId))
                .ReturnsAsync(new GroupEntity
                {
                    UserMemberships = new List<UserMembershipEntity>()
                });

            _unitOfWorkMock
                .Setup(x => x.ExecuteInTransactionAsync(It.IsAny<Func<Task>>()))
                .Returns<Func<Task>>(f => f());

            await _service.CloseGroupAsync(groupId);

            _groupDbServiceMock.Verify(x => x.CloseAsync(groupId), Times.Once);
        }

        [Fact]
        public async Task CloseGroup_ShouldCallClose_WhenEmpty()
        {
            var group = new GroupEntity
            {
                UserMemberships = new List<UserMembershipEntity>()
            };

            _groupDbServiceMock
                .Setup(x => x.GetGroupByIdAsync(1))
                .ReturnsAsync(group);

            _unitOfWorkMock
                .Setup(x => x.ExecuteInTransactionAsync(It.IsAny<Func<Task>>()))
                .Returns<Func<Task>>(f => f());

            await _service.CloseGroupAsync(1);

            _groupDbServiceMock.Verify(x => x.CloseAsync(1), Times.Once);
        }

        [Fact]
        public async Task CloseGroup_ShouldThrow_WhenUsersExist()
        {
            var groupId = 1;

            _groupDbServiceMock
                .Setup(x => x.GetGroupByIdAsync(groupId))
                .ReturnsAsync(new GroupEntity
                {
                    UserMemberships = new List<UserMembershipEntity>
                    {
                new UserMembershipEntity(1, 1, 1)
                    }
                });

            Func<Task> act = async () => await _service.CloseGroupAsync(groupId);

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        #endregion

        #region UserManagement

        [Fact]
        public async Task AddUserToGroup_ShouldCallAddMembership()
        {
            var userId = 1;

            var dto = new UserMembershipCreationShortDto
            {
                GroupId = 10,
                IsMain = true,
                IsCoach = false
            };

            _groupDbServiceMock
                .Setup(x => x.ExistsActive(10))
                .ReturnsAsync(true);

            _userDbServiceMock
                .Setup(x => x.ExistsActive(userId))
                .ReturnsAsync(true);

            _groupDbServiceMock
                .Setup(x => x.GetGroupByIdAsync(10))
                .ReturnsAsync(new GroupEntity { ClubId = 1 });

            await _service.AddUserToGroupAsync(userId, dto);

            _userMembershipAppMock.Verify(
                x => x.AddUserMembershipAsync(userId, It.IsAny<UserMembershipCreationDto>()),
                Times.Once);
        }

        [Fact]
        public async Task RemoveUserFromGroup_ShouldCallClose()
        {
            _unitOfWorkMock
                .Setup(x => x.ExecuteInTransactionAsync(It.IsAny<Func<Task>>()))
                .Returns<Func<Task>>(f => f());

            await _service.RemoveUserFromGroupAsync(1, 2);

            _userMembershipAppMock.Verify(
                x => x.CloseUserMembershipAsync(2, 1),
                Times.Once);
        }

        #endregion
    }
}
