using Aikido.Entities;
using System.Text.Json;

namespace Aikido.Dto
{
    public class UserChangePendingRequestDto : DtoBase
    {
        public string RequestType { get; set; }

        public long RequestedById { get; set; }
        public string RequestedByName { get; set; }

        public long? TargetUserId { get; set; }
        public string? TargetUserName { get; set; }

        public string Status { get; set; } = RequestStatus.Pending.ToString();

        public DateTime? CreatedAt { get; set; }

        public UserChangePendingRequestDto(UserChangeRequestEntity userChangeRequest)
        {
            Id = userChangeRequest.Id;

            RequestType = EnumParser.ConvertEnumToString(userChangeRequest.RequestType);
            RequestedById = userChangeRequest.RequestedById;
            RequestedByName = userChangeRequest.RequestedBy.FullName;

            if (TargetUserId == null )
            {
                var data = userChangeRequest.UserDataJson;

                if (data != null)
                {
                    TargetUserName = JsonSerializer.Deserialize<UserDto>(data)?.FullName;
                }
                
            }
            else
            {
                TargetUserId = userChangeRequest.TargetUserId;
                TargetUserName = userChangeRequest.TargetUser?.FullName;
            }

                

            Status = EnumParser.ConvertEnumToString(userChangeRequest.Status);

            CreatedAt = userChangeRequest.CreatedAt;
        }
    }
}
