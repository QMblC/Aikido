﻿using Aikido.Dto;
using System.ComponentModel.DataAnnotations;

namespace Aikido.Entities
{
    public class ClubEntity : IDbEntity
    {
        [Key]
        public long Id { get; set; }
        public string? Name { get; set; }
        public string? City { get; set; }
        public string? Address { get; set; }
        public long[] Groups { get; set; } = [];

        public void UpdateFromJson(ClubDto clubNewData)
        {
            if (clubNewData.Name != null)
                Name = clubNewData.Name;

            if (clubNewData.City != null)
                City = clubNewData.City;

            if (clubNewData.Address != null)
                Address = clubNewData.Address;
        }
    }
}