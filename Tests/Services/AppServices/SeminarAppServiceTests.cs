using Aikido.AdditionalData.Enums;
using Aikido.Application.Services;
using Aikido.Dto.Seminars.Creation;
using Aikido.Dto.Seminars.Members;
using Aikido.Dto.Seminars.Members.Creation;
using Aikido.Entities;
using Aikido.Entities.Seminar;
using Aikido.Entities.Seminar.SeminarFilters;
using Aikido.Entities.Seminar.SeminarMember;
using Aikido.Entities.Seminar.SeminarMemberRequest;
using Aikido.Entities.Users;
using Aikido.Exceptions;
using Aikido.Services.DatabaseServices.Group;
using Aikido.Services.DatabaseServices.Payment;
using Aikido.Services.DatabaseServices.Seminar;
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
    public class SeminarAppServiceTests
    {
        private readonly Mock<ISeminarDbService> _seminarDbServiceMock = new();
        private readonly Mock<IUserDbService> _userDbServiceMock = new();
        private readonly Mock<IUserMembershipDbService> _userMembershipDbServiceMock = new();
        private readonly Mock<IGroupDbService> _groupDbServiceMock = new();
        private readonly Mock<IPaymentDbService> _paymentDbServiceMock = new();
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
        private readonly Mock<INotificationService> _notificationServiceMock = new();

        private readonly SeminarApplicationService _service;

        public SeminarAppServiceTests()
        {
            _service = new SeminarApplicationService(
                _seminarDbServiceMock.Object,
                _userDbServiceMock.Object,
                _userMembershipDbServiceMock.Object,
                _groupDbServiceMock.Object,
                _paymentDbServiceMock.Object,
                _unitOfWorkMock.Object,
                _notificationServiceMock.Object
            );
        }

        #region GetMethods

        [Fact]
        public async Task GetSeminarByIdAsync_ShouldReturnSeminar_WhenExists()
        {
            var seminar = new SeminarEntity
            {
                Id = 1,
                Name = "Test"
            };

            _seminarDbServiceMock
                .Setup(x => x.GetByIdOrThrowException(1))
                .ReturnsAsync(seminar);

            var result = await _service.GetSeminarByIdAsync(1);

            result.Should().NotBeNull();
            result.Id.Should().Be(1);
        }

        [Fact]
        public async Task GetAllSeminarsAsync_ShouldReturnMappedList()
        {
            _seminarDbServiceMock
                .Setup(x => x.GetAllAsync(It.IsAny<TimeFilter>()))
                .ReturnsAsync(new List<SeminarEntity>
                {
                    new SeminarEntity { Id = 1, Name = "A" },
                    new SeminarEntity { Id = 2, Name = "B" }
                });

            var result = await _service.GetAllSeminarsAsync(new TimeFilter());

            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetSeminarMembersAsync_ShouldReturnMembers()
        {
            _seminarDbServiceMock
                .Setup(x => x.GetSeminarMembersAsync(1))
                .ReturnsAsync(new List<SeminarMemberEntity>
                {
                    new SeminarMemberEntity
                    {
                        UserId = 1
                    }
                });

            _paymentDbServiceMock
                .Setup(x => x.GetSeminarMemberPayments(1, 1))
                .ReturnsAsync(new List<PaymentEntity>());

            var result = await _service.GetSeminarMembersAsync(1);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetSeminarRegulationAsync_ShouldReturnFile()
        {
            var bytes = new byte[] { 1, 2, 3 };

            _seminarDbServiceMock
                .Setup(x => x.GetSeminarRegulation(1))
                .ReturnsAsync(new SeminarRegulationEntity(1, bytes));

            var result = await _service.GetSeminarRegulationAsync(1);

            result.Should().BeEquivalentTo(bytes);
        }

        #endregion

        #region CreateUpdateDelete

        [Fact]
        public async Task CreateSeminarAsync_ShouldReturnId_AndSendNotification()
        {
            var dto = new SeminarCreationDto
            {
                CreatorId = 1,
                Editors = new(),
                Schedule = new()
            };

            _seminarDbServiceMock
                .Setup(x => x.CreateAsync(dto))
                .ReturnsAsync(new SeminarEntity
                {
                    Id = 10
                });

            _unitOfWorkMock
                .Setup(x => x.ExecuteInTransactionAsync(It.IsAny<Func<Task>>()))
                .Returns<Func<Task>>(f => f());

            var result = await _service.CreateSeminarAsync(dto);

            result.Should().Be(10);

            _notificationServiceMock.Verify(
                x => x.SeminarDataChanged(NotificationAction.Create, 10),
                Times.Once);
        }

        [Fact]
        public async Task CreateSeminarAsync_ShouldThrow_WhenCreatorIsNull()
        {
            var dto = new SeminarCreationDto
            {
                CreatorId = null
            };

            Func<Task> act = async () => await _service.CreateSeminarAsync(dto);

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task UpdateSeminarAsync_ShouldCallUpdate_WhenExists()
        {
            var dto = new SeminarCreationDto
            {
                Editors = new(),
                Schedule = new()
            };

            _seminarDbServiceMock
                .Setup(x => x.Exists(1))
                .ReturnsAsync(true);

            _seminarDbServiceMock
                .Setup(x => x.GetByIdOrThrowException(1))
                .ReturnsAsync(new SeminarEntity());

            _unitOfWorkMock
                .Setup(x => x.ExecuteInTransactionAsync(It.IsAny<Func<Task>>()))
                .Returns<Func<Task>>(f => f());

            await _service.UpdateSeminarAsync(1, dto);

            _seminarDbServiceMock.Verify(
                x => x.UpdateAsync(1, dto),
                Times.Once);
        }

        [Fact]
        public async Task UpdateSeminarAsync_ShouldThrow_WhenSeminarNotExists()
        {
            _seminarDbServiceMock
                .Setup(x => x.Exists(1))
                .ReturnsAsync(false);

            Func<Task> act = async () =>
                await _service.UpdateSeminarAsync(1, new SeminarCreationDto());

            await act.Should().ThrowAsync<EntityNotFoundException>();
        }

        [Fact]
        public async Task DeleteSeminarAsync_ShouldDelete_WhenExists()
        {
            _seminarDbServiceMock
                .Setup(x => x.Exists(1))
                .ReturnsAsync(true);

            await _service.DeleteSeminarAsync(1);

            _seminarDbServiceMock.Verify(
                x => x.DeleteAsync(1),
                Times.Once);
        }

        [Fact]
        public async Task DeleteSeminarAsync_ShouldThrow_WhenNotExists()
        {
            _seminarDbServiceMock
                .Setup(x => x.Exists(1))
                .ReturnsAsync(false);

            Func<Task> act = async () =>
                await _service.DeleteSeminarAsync(1);

            await act.Should().ThrowAsync<EntityNotFoundException>();
        }

        #endregion

        #region Regulation

        [Fact]
        public async Task AddSeminarRegulationAsync_ShouldCallCreate()
        {
            var file = new byte[] { 1, 2 };

            await _service.AddSeminarRegulationAsync(1, file);

            _seminarDbServiceMock.Verify(
                x => x.CreateSeminarRegulationAsync(1, file),
                Times.Once);
        }

        [Fact]
        public async Task DeleteSeminarRegulationAsync_ShouldCallDelete()
        {
            await _service.DeleteSeminarRegulationAsync(1);

            _seminarDbServiceMock.Verify(
                x => x.DeleteSeminarRegulationAsync(1),
                Times.Once);
        }

        #endregion

        #region SeminarResults

        [Fact]
        public async Task ApplySeminarResult_ShouldUpdateGrades_AndPassport()
        {
            var seminar = new SeminarEntity
            {
                Payments = new List<PaymentEntity>
                {
                    new PaymentEntity
                    {
                        UserId = 1,
                        Type = PaymentType.BudoPassport
                    }
                }
            };

            _seminarDbServiceMock
                .Setup(x => x.GetByIdOrThrowException(1))
                .ReturnsAsync(seminar);

            _seminarDbServiceMock
                .Setup(x => x.GetSeminarMembersAsync(1))
                .ReturnsAsync(new List<SeminarMemberEntity>
                {
                    new SeminarMemberEntity
                    {
                        UserId = 1,
                        Status = SeminarMemberStatus.Certified,
                        CertificationGrade = Grade.Dan1
                    }
                });

            await _service.ApplySeminarResult(1);

            _userDbServiceMock.Verify(
                x => x.UpdateUserGrade(1, Grade.Dan1),
                Times.Once);

            _userDbServiceMock.Verify(
                x => x.UpdateUserBudoPassport(1, true),
                Times.Once);
        }

        [Fact]
        public async Task CancelSeminarResult_ShouldRollbackGrades_AndPassport()
        {
            var seminar = new SeminarEntity
            {
                Payments = new List<PaymentEntity>
                {
                    new PaymentEntity
                    {
                        UserId = 1,
                        Type = PaymentType.BudoPassport
                    }
                }
            };

            _seminarDbServiceMock
                .Setup(x => x.GetByIdOrThrowException(1))
                .ReturnsAsync(seminar);

            _seminarDbServiceMock
                .Setup(x => x.GetSeminarMembersAsync(1))
                .ReturnsAsync(new List<SeminarMemberEntity>
                {
                    new SeminarMemberEntity
                    {
                        UserId = 1,
                        Status = SeminarMemberStatus.Certified,
                        OldGrade = Grade.Kyu5
                    }
                });

            await _service.CancelSeminarResult(1);

            _userDbServiceMock.Verify(
                x => x.UpdateUserGrade(1, Grade.Kyu5),
                Times.Once);

            _userDbServiceMock.Verify(
                x => x.UpdateUserBudoPassport(1, false),
                Times.Once);
        }

        #endregion

        #region ManagerRequests

        [Fact]
        public async Task GetNewSeminarMemberManagerRequest_ShouldThrow_WhenAlreadyRequested()
        {
            _seminarDbServiceMock
                .Setup(x => x.GetRequestedMembers(1, true))
                .ReturnsAsync(new List<SeminarMemberManagerRequestEntity>
                {
                    new SeminarMemberManagerRequestEntity
                    {
                        UserId = 1
                    }
                });

            _seminarDbServiceMock
                .Setup(x => x.GetRequestedMembers(1, false))
                .ReturnsAsync(new List<SeminarMemberManagerRequestEntity> 
                {
                    new SeminarMemberManagerRequestEntity
                    {
                        UserId = 1
                    }
                });

            Func<Task> act = async () =>
                await _service.GetNewSeminarMemberManagerRequest(1, 1);

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task ConfirmManagerMembersByClubAsync_ShouldThrow_WhenNoMembers()
        {
            _seminarDbServiceMock
                .Setup(x => x.GetByIdOrThrowException(1))
                .ReturnsAsync(new SeminarEntity());

            _seminarDbServiceMock
                .Setup(x => x.GetManagerMembersByClubAsync(1, 1))
                .ReturnsAsync(new List<SeminarMemberManagerRequestEntity>());

            Func<Task> act = async () =>
                await _service.ConfirmManagerMembersByClubAsync(1, 1, 1);

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task ConfirmManagerMembersByClubAsync_ShouldConfirm_WhenMembersExists()
        {
            _seminarDbServiceMock
                .Setup(x => x.GetByIdOrThrowException(1))
                .ReturnsAsync(new SeminarEntity());

            _seminarDbServiceMock
                .Setup(x => x.GetManagerMembersByClubAsync(1, 1))
                .ReturnsAsync(new List<SeminarMemberManagerRequestEntity>
                {
                    new SeminarMemberManagerRequestEntity()
                });

            await _service.ConfirmManagerMembersByClubAsync(1, 1, 1);

            _seminarDbServiceMock.Verify(
                x => x.ConfirmManagerMembersByClubAsync(1, 1, 1),
                Times.Once);
        }

        #endregion

        #region Members

        [Fact]
        public async Task SaveSeminarMembers_ShouldThrow_WhenMembersDuplicated()
        {
            var dto = new SeminarMemberListDto
            {
                Members = new List<SeminarMemberCreationDto>
                {
                    new() { UserId = 1 },
                    new() { UserId = 1 }
                }
            };

            Func<Task> act = async () =>
                await _service.SaveSeminarMembers(1, dto);

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task SaveSeminarMembers_ShouldCreateMembers()
        {
            var dto = new SeminarMemberListDto
            {
                Members = new List<SeminarMemberCreationDto>
                {
                    new()
                    {
                        UserId = 1,
                        GroupId = 1,
                        CoachId = 1
                    }
                }
            };

            _seminarDbServiceMock
                .Setup(x => x.GetByIdOrThrowException(1))
                .ReturnsAsync(new SeminarEntity());

            _userMembershipDbServiceMock
                .Setup(x => x.GetMainUserMembership(1))
                .Returns(new UserMembershipEntity(1, 1, 1)
                {
                    Group = new GroupEntity
                    {
                        MainCoachId = 1
                    }
                });

            _paymentDbServiceMock
                .Setup(x => x.GetFakeSeminarMemberPayment(1, 1))
                .ReturnsAsync(new List<PaymentEntity>());

            await _service.SaveSeminarMembers(1, dto);

            _seminarDbServiceMock.Verify(
                x => x.CreateSeminarMembers(1, dto),
                Times.Once);
        }

        [Fact]
        public async Task IsClubSeminarMembersManagerRequestConfirmed_ShouldReturnFalse_WhenNoMembers()
        {
            _seminarDbServiceMock
                .Setup(x => x.GetManagerMembersByClubAsync(1, 1))
                .ReturnsAsync(new List<SeminarMemberManagerRequestEntity>());

            var result = await _service.IsClubSeminarMembersManagerRequestConfirmed(1, 1);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsClubSeminarMembersManagerRequestConfirmed_ShouldReturnTrue_WhenAllConfirmed()
        {
            _seminarDbServiceMock
                .Setup(x => x.GetManagerMembersByClubAsync(1, 1))
                .ReturnsAsync(new List<SeminarMemberManagerRequestEntity>
                {
                    new SeminarMemberManagerRequestEntity()
                    {
                        IsConfirmed = true
                    }
                });

            var result = await _service.IsClubSeminarMembersManagerRequestConfirmed(1, 1);

            result.Should().BeTrue();
        }

        #endregion

        #region Statements

        [Fact]
        public async Task BlockSeminarStatements_ShouldBlockStatements()
        {
            var seminar = new SeminarEntity();

            _seminarDbServiceMock
                .Setup(x => x.GetByIdOrThrowException(1))
                .ReturnsAsync(seminar);

            await _service.BlockSeminarStatements(1);

            seminar.AreStatementsBlocked.Should().BeTrue();

            _seminarDbServiceMock.Verify(
                x => x.UpdateAsync(seminar),
                Times.Once);
        }

        [Fact]
        public async Task UnlockSeminarStatements_ShouldUnlockStatements()
        {
            var seminar = new SeminarEntity
            {
                AreStatementsBlocked = true
            };

            _seminarDbServiceMock
                .Setup(x => x.GetByIdOrThrowException(1))
                .ReturnsAsync(seminar);

            await _service.UnlockSeminarStatements(1);

            seminar.AreStatementsBlocked.Should().BeFalse();
        }

        #endregion
    }
}
