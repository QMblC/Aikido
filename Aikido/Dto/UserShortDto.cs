﻿using Aikido.Entities;

namespace Aikido.Dto
{
    public class UserShortDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string? Photo { get; set; }//Mb delete

        public UserShortDto() { }

        public UserShortDto(UserEntity userEntity, bool addPhoto = true)
        {
            Id = userEntity.Id;
            Name = userEntity.FullName;
            if (addPhoto)
            {
                Photo = Convert.ToBase64String(userEntity.Photo);
            }
            
        }
    }
}
