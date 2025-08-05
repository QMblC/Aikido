using Aikido.Data;
using Aikido.Dto;
using Aikido.Entities;
using Aikido.Services.DatabaseServices.Base;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aikido.Services.DatabaseServices
{
    public class ClubDbService : DbService<ClubEntity, ClubDbService>
    {
        public ClubDbService(AppDbContext context, ILogger<ClubDbService> logger) 
            : base(context, logger)
        {

        }
    }
}