using Aikido.Data;
using Aikido.Entities.Seminar;
using Aikido.Services.DatabaseServices.Base;

namespace Aikido.Services.DatabaseServices.Seminar
{
    public class SeminarCoachStatementDbService 
        : DbService<SeminarStatementEntity, SeminarCoachStatementDbService>, ISeminarCoachStatementDbService
    {
        public SeminarCoachStatementDbService(AppDbContext context, ILogger<SeminarCoachStatementDbService> logger) 
            : base(context, logger)
        {

        }

    }
}
