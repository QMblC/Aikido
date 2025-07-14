using Aikido.Data;
using Aikido.Dto;
using Aikido.Entities;
using DocumentFormat.OpenXml.Office2010.Excel;

namespace Aikido.Services
{
    public class SeminarService : DbService
    {
        public SeminarService(AppDbContext context) : base(context)
        {
        }

        public async Task<SeminarEntity> Get(long id)
        {
            var seminarEntity = context.Seminars.FindAsync(id).Result;
            if (seminarEntity == null)
            {
                throw new KeyNotFoundException($"Семинар с Id = {id} не найдет");
            }

            return seminarEntity;
        }

        public async Task<long> Create(SeminarDto seminarData)
        {
            var seminarEntity = new SeminarEntity(seminarData);

            await context.Seminars.AddAsync(seminarEntity);

            await SaveDb();

            return seminarEntity.Id;
        }

        public async Task Delete(long id)
        {
            var seminarEntity = await Get(id);

            context.Seminars.Remove(seminarEntity);

            await SaveDb();
        }

        public async Task Update(long id, SeminarDto seminarData)
        {
            var seminarEntity = await Get(id);

            seminarEntity.UpdateFromJson(seminarData);

            await SaveDb();
        }
    }
}
