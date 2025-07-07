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

        public async Task ImportUsersFromExcelAsync(Stream excelStream)
        {
            using var workbook = new XLWorkbook(excelStream);
            var worksheet = workbook.Worksheets.First();

            var existingLogins = await GetExistingLoginsAsync();

            foreach (var row in worksheet.RowsUsed().Skip(1))
            {
                ValidateRequiredFields(row);

                var login = row.Cell(3).GetString()?.Trim();

                if (IsLoginDuplicate(login, existingLogins, context.Users.Local.ToList()))
                {
                    throw new Exception($"Логин '{login}' уже существует (строка {row.RowNumber()})");
                }

                var user = CreateUserFromRow(row);
                context.Users.Add(user);
            }

            try
            {
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                var innerException = ex.InnerException?.Message ?? "нет внутренней ошибки";
                throw new Exception($"Ошибка при сохранении в БД: {ex.Message}. Внутренняя ошибка: {innerException}");
            }
        }

        public async Task UpdateUsersFromExcelAsync(Stream excelStream)
        {
            using var workbook = new XLWorkbook(excelStream);
            var worksheet = workbook.Worksheets.First();

            foreach (var row in worksheet.RowsUsed().Skip(1))
            {
                ValidateRequiredFields(row);

                var id = TryParseLong(row.Cell(1).GetString());
                if (id == null)
                {
                    throw new Exception($"Некорректный ID пользователя (строка {row.RowNumber()})");
                }

                var user = await context.Users.FindAsync(id.Value);
                if (user == null)
                {
                    throw new Exception($"Пользователь с ID {id} не найден (строка {row.RowNumber()})");
                }

                UpdateUserFromRow(user, row);

                context.Users.Update(user);
            }

            await context.SaveChangesAsync();
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

        private async Task<List<string>> GetExistingLoginsAsync()
        {
            return await context.Users
                .Where(u => u.Login != null)
                .Select(u => u.Login!)
                .ToListAsync();
        }

        private void ValidateRequiredFields(IXLRow row)
        {
            if (string.IsNullOrWhiteSpace(row.Cell(3).GetString()?.Trim()) ||
                string.IsNullOrWhiteSpace(row.Cell(4).GetString()?.Trim()) ||
                string.IsNullOrWhiteSpace(row.Cell(2).GetString()?.Trim()) ||
                string.IsNullOrWhiteSpace(row.Cell(5).GetString()?.Trim()))
            {
                throw new Exception($"Не указаны обязательные поля в строке {row.RowNumber()}");
            }
        }

        private bool IsLoginDuplicate(string? login, List<string> existingLogins, List<UserEntity> usersToAdd)
        {
            if (string.IsNullOrEmpty(login))
                return false;

            return existingLogins.Contains(login) || usersToAdd.Any(u => u.Login == login);
        }

        private UserEntity CreateUserFromRow(IXLRow row)
        {
            return new UserEntity
            {
                Role = row.Cell(2).GetString(),
                Login = row.Cell(3).GetString()?.Trim(),
                // Пароль в Excel не используется, генерировать отдельно при создании
                FullName = row.Cell(5).GetString(),
                PhoneNumber = row.Cell(6).GetString(),
                Birthday = ConvertToUtc(TryParseDate(row.Cell(7).GetString())),
                City = row.Cell(8).GetString(),
                Grade = row.Cell(9).GetString(),
                CertificationDate = ConvertToUtc(TryParseDate(row.Cell(10).GetString())),
                AnnualFee = TryParseInt(row.Cell(11).GetString()),
                Sex = row.Cell(12).GetString(),
                ClubId = TryParseLong(row.Cell(13).GetString()),
                GroupId = TryParseLong(row.Cell(15).GetString()),
                SchoolClass = TryParseInt(row.Cell(17).GetString()),
                ParentFullName = row.Cell(18).GetString(),
                ParentFullNumber = row.Cell(19).GetString(),
                RegistrationDate = ConvertToUtc(TryParseDate(row.Cell(20).GetString()))
            };
        }

        private void UpdateUserFromRow(UserEntity user, IXLRow row)
        {
            user.Role = row.Cell(2).GetString();
            user.Login = row.Cell(3).GetString()?.Trim();
            // Пароль не обновляем из Excel
            user.FullName = row.Cell(5).GetString();
            user.PhoneNumber = row.Cell(6).GetString();
            user.Birthday = ConvertToUtc(TryParseDate(row.Cell(7).GetString()));
            user.City = row.Cell(8).GetString();
            user.Grade = row.Cell(9).GetString();
            user.CertificationDate = ConvertToUtc(TryParseDate(row.Cell(10).GetString()));
            user.AnnualFee = TryParseInt(row.Cell(11).GetString());
            user.Sex = row.Cell(12).GetString();
            user.ClubId = TryParseLong(row.Cell(13).GetString());
            user.GroupId = TryParseLong(row.Cell(15).GetString());
            user.SchoolClass = TryParseInt(row.Cell(17).GetString());
            user.ParentFullName = row.Cell(18).GetString();
            user.ParentFullNumber = row.Cell(19).GetString();
            user.RegistrationDate = ConvertToUtc(TryParseDate(row.Cell(20).GetString()));
        }

        private DateTime? TryParseDate(string? input)
        {
            return DateTime.TryParseExact(input, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result)
                ? result
                : null;
        }

        private DateTime? ConvertToUtc(DateTime? date)
        {
            if (!date.HasValue) return null;

            var dt = date.Value;
            if (dt.Kind == DateTimeKind.Unspecified)
                return DateTime.SpecifyKind(dt, DateTimeKind.Utc);

            if (dt.Kind == DateTimeKind.Local)
                return dt.ToUniversalTime();

            return dt; // уже UTC
        }

        private int? TryParseInt(string? input)
        {
            return int.TryParse(input, out var result) ? result : null;
        }

        private long? TryParseLong(string? input)
        {
            return long.TryParse(input, out var result) ? result : null;
        }
    }
}
