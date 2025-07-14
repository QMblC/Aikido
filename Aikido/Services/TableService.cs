using Aikido.Data;
using Aikido.Entities;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;
using Aikido.AdditionalData;

namespace Aikido.Services
{
    public class TableService : DbService
    {
        public TableService(AppDbContext context) : base(context) { }

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
                worksheet.Cell(row, 2).Value = EnumParser.ConvertEnumToString(user.Role);
                worksheet.Cell(row, 3).Value = user.Login;
                // Пароль не выгружается
                worksheet.Cell(row, 5).Value = user.FullName;
                worksheet.Cell(row, 6).Value = user.PhoneNumber;
                worksheet.Cell(row, 7).Value = user.Birthday?.ToString("yyyy-MM-dd");
                worksheet.Cell(row, 8).Value = user.City;
                worksheet.Cell(row, 9).Value = EnumParser.ConvertEnumToString(user.Grade);
                worksheet.Cell(row, 10).Value = user.CertificationDates != null && user.CertificationDates.Any()
                    ? user.CertificationDates.Last().ToString("yyyy-MM-dd")
                    : null;
                worksheet.Cell(row, 11).Value = user.PaymentDates != null && user.PaymentDates.Any()
                    ? user.PaymentDates.Last().ToString("yyyy-MM-dd")
                    : null;
                worksheet.Cell(row, 12).Value = EnumParser.ConvertEnumToString(user.Sex);
                worksheet.Cell(row, 13).Value = user.ClubId;
                worksheet.Cell(row, 14).Value = user.ClubId.HasValue && clubs.TryGetValue(user.ClubId.Value, out var clubName)
                    ? clubName
                    : null;
                worksheet.Cell(row, 15).Value = user.GroupId;
                worksheet.Cell(row, 16).Value = user.GroupId.HasValue && groups.TryGetValue(user.GroupId.Value, out var groupName)
                    ? groupName
                    : null;
                worksheet.Cell(row, 17).Value = EnumParser.ConvertEnumToString(user.Education);
                worksheet.Cell(row, 18).Value = user.ParentFullName;
                worksheet.Cell(row, 19).Value = user.ParentPhoneNumber;
                worksheet.Cell(row, 20).Value = user.RegistrationDate?.ToString("yyyy-MM-dd");
            }

            worksheet.Columns().AdjustToContents();

            var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            return stream;
        }      

        public async Task<MemoryStream> GenerateUserUpdateTemplateExcelAsync()
        {
            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Шаблон пользователей");

            WriteExcelHeader(worksheet);
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
                "Кю/Дан", "Дата аттестации", "Дата последнего взноса", "Пол", "Клуб ID", "Клуб", "Группа ID",
                "Группа", "Образование", "ФИО родителя", "Телефон родителя", "Дата регистрации"
            };

            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cell(1, i + 1).Value = headers[i];
            }
        }

        public async Task<MemoryStream> GenerateUserCreateTemplateExcelAsync()
        {
            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Шаблон создания пользователей");

            WriteExcelCreateHeader(worksheet);
            worksheet.Columns().AdjustToContents();

            var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            return stream;
        }

        private void WriteExcelCreateHeader(IXLWorksheet worksheet)
        {
            var headers = new[]
            {
                "Роль", "Логин", "Пароль", "ФИО", "Телефон", "Дата рождения", "Город",
                "Кю/Дан", "Дата аттестации", "Взнос", "Пол", "Клуб ID", "Клуб", "Группа ID",
                "Группа", "Класс", "ФИО родителя", "Телефон родителя", "Дата регистрации"
            };

            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cell(1, i + 1).Value = headers[i];
            }
        }

        private bool IsLoginDuplicate(string? login, List<string> existingLogins, List<UserEntity> usersToAdd)
        {
            if (string.IsNullOrEmpty(login))
                return false;

            return existingLogins.Contains(login) || usersToAdd.Any(u => u.Login == login);
        }
    }
}
