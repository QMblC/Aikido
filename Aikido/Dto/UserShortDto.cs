using Aikido.Entities.Users;

namespace Aikido.Dto
{
    public class UserShortDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string? Photo { get; set; }

        public UserShortDto() { }

        public UserShortDto(UserEntity userEntity, bool addPhoto = true)
        {
            Id = userEntity.Id;
            Name = userEntity.FullName;
            if (addPhoto)
            {
                Photo = Convert.ToBase64String(userEntity.AvatarPath);
            }
            
        }
    }
}
