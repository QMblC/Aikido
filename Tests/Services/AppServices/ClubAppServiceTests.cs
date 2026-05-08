using Aikido.AdditionalData.Enums;
using Aikido.Application.Services;
using Aikido.Dto.Clubs;
using Aikido.Entities;
using Aikido.Entities.Clubs;
using Aikido.Entities.Users;
using Aikido.Exceptions;
using Aikido.Services.DatabaseServices.Club;
using Aikido.Services.DatabaseServices.Group;
using Aikido.Services.DatabaseServices.Schedule;
using Aikido.Services.DatabaseServices.User;
using Aikido.Services.NotificationService;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.Services.AppServices
{
    public class ClubAppServiceTests
    {
        private readonly Mock<IClubDbService> _clubDbServiceMock;
        private readonly Mock<IGroupDbService> _groupDbServiceMock;
        private readonly Mock<IUserDbService> _userDbServiceMock;
        private readonly Mock<IScheduleDbService> _scheduleDbServiceMock;
        private readonly Mock<IClubStaffDbService> _clubStaffDbServiceMock;
        private readonly Mock<INotificationService> _notificationServiceMock;

        private readonly ClubApplicationService _service;

        public ClubAppServiceTests()
        {
            _clubDbServiceMock = new Mock<IClubDbService>();
            _groupDbServiceMock = new Mock<IGroupDbService>();
            _userDbServiceMock = new Mock<IUserDbService>();
            _scheduleDbServiceMock = new Mock<IScheduleDbService>();
            _clubStaffDbServiceMock = new Mock<IClubStaffDbService>();
            _notificationServiceMock = new Mock<INotificationService>();

            _service = new ClubApplicationService(
                _clubDbServiceMock.Object,
                _groupDbServiceMock.Object,
                _userDbServiceMock.Object,
                _scheduleDbServiceMock.Object,
                _clubStaffDbServiceMock.Object,
                _notificationServiceMock.Object
            );
        }

        #region GetMethods

        [Fact]
        public async Task GetClubById_ShouldReturnClub()
        {
            var clubId = 1;
            var club = new ClubEntity { Id = clubId };

            _clubDbServiceMock
                .Setup(x => x.GetByIdOrThrowException(clubId))
                .ReturnsAsync(club);

            var result = await _service.GetClubByIdAsync(clubId);

            result.Should().NotBeNull();
            result.Id.Should().Be(clubId);
        }

        [Fact]
        public async Task GetAllClubs_ShouldReturnList()
        {
            _clubDbServiceMock
                .Setup(x => x.GetAllActiveAsync())
                .ReturnsAsync(new List<ClubEntity>
                {
                    new ClubEntity { Id = 1 }
                });

            var result = await _service.GetAllClubsAsync();

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetArchivedClubs_ShouldReturnList()
        {
            _clubDbServiceMock
                .Setup(x => x.GetAllArchivedAsync())
                .ReturnsAsync(new List<ClubEntity>
                {
                    new ClubEntity { Id = 1 }
                });

            var result = await _service.GetArchivedClubsAsync();

            result.Should().HaveCount(1);
        }

        #endregion

        #region Details

        [Fact]
        public async Task GetClubDetails_ShouldReturnData()
        {
            var clubId = 1;

            _clubDbServiceMock
                .Setup(x => x.GetByIdOrThrowException(clubId))
                .ReturnsAsync(new ClubEntity { Id = clubId });

            _groupDbServiceMock
                .Setup(x => x.GetGroupsByClub(clubId))
                .ReturnsAsync(new List<GroupEntity>
                {
                    new GroupEntity { Id = 1 }
                });

            _clubDbServiceMock
                .Setup(x => x.GetClubMembersAsync(clubId))
                .ReturnsAsync(new List<UserMembershipEntity>());

            var result = await _service.GetClubDetailsAsync(clubId);

            result.Should().NotBeNull();
        }

        #endregion

        #region CreateUpdate

        [Fact]
        public async Task CreateClub_ShouldReturnId_AndSendNotification()
        {
            var clubData = new Mock<IClubDto>();
            clubData.Setup(x => x.ManagerId).Returns(1);

            _clubDbServiceMock
                .Setup(x => x.CreateAsync(clubData.Object))
                .ReturnsAsync(10);

            _clubDbServiceMock
                .Setup(x => x.GetClubById(10))
                .ReturnsAsync(new ClubEntity
                {
                    Id = 10,
                    ManagerId = null
                });

            _clubDbServiceMock
                .Setup(x => x.UpdateAsync(It.IsAny<ClubEntity>()))
                .Returns(Task.CompletedTask);

            var result = await _service.CreateClubAsync(clubData.Object);

            result.Should().Be(10);

            _notificationServiceMock.Verify(
                x => x.ClubDataChanged(NotificationAction.Create, 10),
                Times.Once);
        }

        [Fact]
        public async Task UpdateClub_ShouldThrow_WhenNotExists()
        {
            _clubDbServiceMock
                .Setup(x => x.ExistsActive(It.IsAny<long>()))
                .ReturnsAsync(false);

            var dto = new Mock<IClubDto>();

            Func<Task> act = async () => await _service.UpdateClubAsync(1, dto.Object);

            await act.Should().ThrowAsync<EntityNotFoundException>();
        }

        #endregion

        #region Manager

        [Fact]
        public async Task UpdateClubManager_ShouldReplaceManager()
        {
            var club = new ClubEntity { Id = 1, ManagerId = 1 };

            _clubDbServiceMock
                .Setup(x => x.GetClubById(1))
                .ReturnsAsync(club);

            await _service.UpdateClubManager(1, 2);

            _clubStaffDbServiceMock.Verify(
                x => x.DeleteAsync(1, 1),
                Times.Once);

            _clubStaffDbServiceMock.Verify(
                x => x.CreateAsync(1, 2, Role.Manager),
                Times.Once);
        }

        [Fact]
        public async Task UpdateClubManager_ShouldUpdateStaff()
        {
            var clubId = 1;

            var club = new ClubEntity
            {
                Id = clubId,
                ManagerId = 2
            };

            _clubDbServiceMock
                .Setup(x => x.GetClubById(clubId))
                .ReturnsAsync(club);

            var dto = new Mock<IClubDto>();
            dto.Setup(x => x.ManagerId).Returns(3);

            await _service.UpdateClubManager(clubId, 3);

            _clubStaffDbServiceMock.Verify(x =>
                x.CreateAsync(clubId, 3, Role.Manager),
                Times.Once);
        }

        #endregion

        #region DeleteCloseRecover

        [Fact]
        public async Task CloseClub_ShouldThrow_WhenHasGroups()
        {
            var clubId = 1;

            _clubDbServiceMock
                .Setup(x => x.GetClubGroupsAsync(clubId))
                .ReturnsAsync(new List<GroupEntity>
                {
                    new GroupEntity()
                });

            Func<Task> act = async () => await _service.CloseClubAsync(clubId);

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task RecoverClub_ShouldCallDb()
        {
            var clubId = 1;

            await _service.RecoverClubAsync(clubId);

            _clubDbServiceMock.Verify(x => x.RecoverAsync(clubId), Times.Once);
        }

        [Fact]
        public async Task DeleteClub_ShouldCallDeleteFlow()
        {
            var clubId = 1;

            _clubDbServiceMock
                .Setup(x => x.GetClubById(clubId))
                .ReturnsAsync(new ClubEntity { Id = clubId });

            await _service.DeleteClubAsync(clubId);

            _clubDbServiceMock.Verify(x => x.DeleteAsync(clubId), Times.Once);
        }

        [Fact]
        public async Task CloseClub_ShouldClose_WhenNoGroups()
        {
            _clubDbServiceMock
                .Setup(x => x.GetClubGroupsAsync(1))
                .ReturnsAsync(new List<GroupEntity>());

            await _service.CloseClubAsync(1);

            _clubDbServiceMock.Verify(x => x.CloseAsync(1), Times.Once);
        }

        #endregion

        #region MembersStaff

        [Fact]
        public async Task GetClubMembers_ShouldFilterByRole()
        {
            var clubId = 1;

            var user = new UserEntity { Id = 1 };

            _clubDbServiceMock
                .Setup(x => x.GetClubMembersAsync(clubId))
                .ReturnsAsync(new List<UserMembershipEntity>
                {
                    new UserMembershipEntity(1, 10, 20, Role.User)
                    {
                        User = user,
                        RoleInGroup = Role.User
                    }
                });

            var result = await _service.GetClubMembersAsync(clubId, Role.User);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task UpdateClubStaff_ShouldCallCreateAndDelete()
        {
            var clubId = 1;

            _clubStaffDbServiceMock
                .Setup(x => x.GetClubStaffByClub(clubId))
                .ReturnsAsync(new List<ClubStaffEntity>
                {
                    new ClubStaffEntity(
                        clubId: clubId,
                        userId: 1,
                        roleInClub: Role.Coach
                    )
                });

            var newStaff = new List<long> { 2 };

            await _service.UpdateClubStaff(clubId, newStaff);

            _clubStaffDbServiceMock.Verify(x =>
                x.CreateRangeAsync(clubId, It.IsAny<List<long>>()),
                Times.Once);
        }

        #endregion
    }
}
