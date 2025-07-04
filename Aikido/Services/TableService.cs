using Aikido.Data;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;

namespace Aikido.Services
{
    public class TableService
    {
        private readonly AppDbContext context;

        public TableService(AppDbContext context)
        {
            this.context = context;
        }

        public async Task<MemoryStream> ExportUsersToExcelAsync()
        {
            var users = await context.Users.ToListAsync();
            var clubs = await context.Clubs.ToDictionaryAsync(c => c.Id, c => c.Name);
            var groups = await context.Groups.ToDictionaryAsync(g => g.Id, g => g.Name);

            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Пользователи");

            WriteExcelHeader(worksheet);

            for (int i = 0; i < users.Count; i++)
            {
                var user = users[i];
                var row = i + 2;

                worksheet.Cell(row, 1).Value = user.Id;
                worksheet.Cell(row, 2).Value = user.Role;
                worksheet.Cell(row, 3).Value = user.Login;
                // Пароль не выгружается
                worksheet.Cell(row, 5).Value = user.FullName;
                worksheet.Cell(row, 6).Value = user.PhoneNumber;
                worksheet.Cell(row, 7).Value = user.Birthday?.ToString("yyyy-MM-dd");
                worksheet.Cell(row, 8).Value = user.City;
                worksheet.Cell(row, 9).Value = user.Grade;
                worksheet.Cell(row, 10).Value = user.CertificationDate?.ToString("yyyy-MM-dd");
                worksheet.Cell(row, 11).Value = user.AnnualFee;
                worksheet.Cell(row, 12).Value = user.Sex;
                worksheet.Cell(row, 13).Value = user.ClubId;
                worksheet.Cell(row, 14).Value = user.ClubId.HasValue && clubs.TryGetValue(user.ClubId.Value, out var clubName)
                    ? clubName
                    : null;
                worksheet.Cell(row, 15).Value = user.GroupId;
                worksheet.Cell(row, 16).Value = user.GroupId.HasValue && groups.TryGetValue(user.GroupId.Value, out var groupName)
                    ? groupName
                    : null;
                worksheet.Cell(row, 17).Value = user.SchoolClass;
                worksheet.Cell(row, 18).Value = user.ParentFullName;
                worksheet.Cell(row, 19).Value = user.ParentFullNumber;
                worksheet.Cell(row, 20).Value = user.RegistrationDate?.ToString("yyyy-MM-dd");
            }

            worksheet.Columns().AdjustToContents();

            var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            return stream;
        }

        private void WriteExcelHeader(IXLWorksheet worksheet)
        {
            var headers = new[]
            {
                "ID", "Роль", "Логин", "Пароль", "ФИО", "Телефон", "Дата рождения", "Город",
                "Кю/Дан", "Дата аттестации", "Взнос", "Пол", "Клуб ID", "Клуб", "Группа ID",
                "Группа", "Класс", "ФИО родителя", "Телефон родителя", "Дата регистрации"
            };

            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cell(1, i + 1).Value = headers[i];
            }
        }



    }
}
