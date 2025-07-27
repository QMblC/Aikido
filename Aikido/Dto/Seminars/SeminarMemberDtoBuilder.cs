using Aikido.Entities.Users;

namespace Aikido.Dto.Seminars
{
    public class SeminarMemberDtoBuilder
    {
        private SeminarMemberDto _dto = new SeminarMemberDto();

        public SeminarMemberDtoBuilder() { }

        public SeminarMemberDtoBuilder SetUser(UserEntity userEntity)
        {
            _dto.Id = userEntity.Id;
            _dto.
        }
    }
}
