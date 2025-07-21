using Aikido.AdditionalData;
using Aikido.Data;
using Aikido.Dto;
using Aikido.Entities;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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

        public async Task<MemoryStream> CreateCoachStatement(
            UserEntity coach,
            List<UserEntity> members,
            List<ClubEntity> clubs,
            List<GroupEntity> groups,
            SeminarEntity seminar)
        {
            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Ведомость");

            AddSeminarData(worksheet, seminar);


            var offset = 6; // Смещение таблицы вниз на 6 строк

            AddTableHeaders(worksheet, offset);

            var totalsRow = AddMembersData(worksheet, offset, members, coach, clubs, groups);
            var row = totalsRow - 1;

            // "Итого" в колонке "ФИО"
            worksheet.Cell(totalsRow, 3).Value = "Итого";
            worksheet.Cell(totalsRow, 3).Style.Font.Bold = true;
            worksheet.Cell(totalsRow, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

            // Суммы по платёжной ведомости (столбцы K, L, M, N = 11–14)
            for (int col = 11; col <= 14; col++)
            {
                var colLetter = XLHelper.GetColumnLetterFromNumber(col);
                worksheet.Cell(totalsRow, col).FormulaA1 = $"SUM({colLetter}{offset + 3}:{colLetter}{row - 1})";
                worksheet.Cell(totalsRow, col).Style.Font.Bold = true;
            }

            // Итоговая сумма в "Примечаниях" (столбец O = 15)
            worksheet.Cell(totalsRow, 15).FormulaA1 = $"SUM(K{totalsRow}:N{totalsRow})";
            worksheet.Cell(totalsRow, 15).Style.Font.Bold = true;


            // Скрытие ID
            worksheet.Column(2).Hide();

            worksheet.Columns().AdjustToContents();

            var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;
            return stream;
        }

        private void AddSeminarData(IXLWorksheet worksheet, SeminarEntity seminar)
        {
            // Верхняя часть: Информация о семинаре
            worksheet.Range("A1:O1").Merge();
            worksheet.Cell("A1").Value = seminar.Name;
            worksheet.Cell("A1").Style.Font.Bold = true;
            worksheet.Cell("A1").Style.Font.FontSize = 24;
            worksheet.Cell("A1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;


            worksheet.Range("A2:O2").Merge();
            worksheet.Cell("A2").Value = seminar.Date.ToString("dd-MM-yyyy");
            worksheet.Cell("A2").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            worksheet.Range("A3:O3").Merge();
            worksheet.Cell("A3").Value = seminar.Location;
            worksheet.Cell("A3").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }

        private void AddTableHeaders(IXLWorksheet worksheet, int offset)
        {
            // Заголовки объединённых ячеек
            worksheet.Range(offset + 1, 1, offset + 1, 8).Merge().Value = "Данные участника";
            worksheet.Range(offset + 1, 9, offset + 1, 10).Merge().Value = "Распределение";
            worksheet.Range(offset + 1, 11, offset + 1, 14).Merge().Value = "Платёжная ведомость";
            worksheet.Cell(offset + 1, 15).Value = "Примечания";

            var darkBlue = XLColor.DarkBlue;
            worksheet.Range(offset + 1, 1, offset + 1, 15).Style.Fill.BackgroundColor = XLColor.BlueGray;
            worksheet.Range(offset + 1, 1, offset + 1, 15).Style.Font.FontColor = XLColor.White;
            worksheet.Range(offset + 1, 1, offset + 1, 15).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Range(offset + 1, 1, offset + 1, 15).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            worksheet.Range(offset + 1, 1, offset + 1, 15).Style.Font.Bold = true;

            // Подзаголовки
            var headers = new[]
            {
                "№", "ID", "ФИО", "Степень кю/дан", "Аттестуется", "Тренер", "Клуб", "Город",
                "Возрастная группа", "Программа",
                "Годовой взнос", "Семинар", "Аттестация", "Паспорт",
                ""
            };

            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cell(offset + 2, i + 1).Value = headers[i];
            }

            var lightBlue = XLColor.LightBlue;
            worksheet.Range(offset + 2, 1, offset + 2, 15).Style.Fill.BackgroundColor = lightBlue;
            worksheet.Range(offset + 2, 1, offset + 2, 15).Style.Font.Bold = true;
            worksheet.Range(offset + 2, 1, offset + 2, 15).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }

        private int AddMembersData(IXLWorksheet worksheet,
            int offset,
            List<UserEntity> members,
            UserEntity coach,
            List<ClubEntity> clubs,
            List<GroupEntity> groups)
        {
            // Данные участников
            var row = offset + 3;
            foreach (var member in members)
            {
                var club = clubs.FirstOrDefault(c => c.Id == member.ClubId);
                var group = groups.FirstOrDefault(g => g.Id == member.GroupId);

                worksheet.Cell(row, 1).Value = row - (offset + 2);            // №
                worksheet.Cell(row, 2).Value = member.Id;                     // ID (будет скрыт)
                worksheet.Cell(row, 3).Value = member.FullName;
                worksheet.Cell(row, 4).Value = EnumParser.GetEnumMemberValue(member.Grade);
                // Аттестуется — пропущено (5)
                worksheet.Cell(row, 6).Value = coach.FullName;
                worksheet.Cell(row, 7).Value = club?.Name ?? "";
                worksheet.Cell(row, 8).Value = member.City ?? "";
                worksheet.Cell(row, 9).Value = EnumParser.GetEnumMemberValue(group.AgeGroup);
                worksheet.Cell(row, 10).Value = EnumParser.GetEnumMemberValue(member.ProgramType);

                // Финансовые поля пока нули
                worksheet.Cell(row, 11).Value = 0;
                worksheet.Cell(row, 12).Value = 0;
                worksheet.Cell(row, 13).Value = 0;
                worksheet.Cell(row, 14).Value = 0;

                row++;
            }

            return row + 1;
        }

        public List<CoachStatementMemberDto> ParseCoachStatement(byte[] table)
        {
            var result = new List<CoachStatementMemberDto>();

            using var stream = new MemoryStream(table);
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheet("Ведомость");

            var offset = 6;
            var startRow = offset + 3;

            while (true)
            {
                var idCell = worksheet.Cell(startRow, 2);
                if (idCell.IsEmpty() || !long.TryParse(idCell.GetValue<string>(), out var id))
                    break;

                var dto = new CoachStatementMemberDto
                {
                    Id = id,
                    Name = worksheet.Cell(startRow, 3).GetValue<string>(),
                    Grade = EnumParser.GetEnumMemberValue<Grade>(worksheet.Cell(startRow, 4).GetValue<string>()).ToString(),
                    CertificationGrade = EnumParser.GetEnumMemberValue<Grade>(worksheet.Cell(startRow, 5).GetValue<string>()).ToString(),
                    CoachName = worksheet.Cell(startRow, 6).GetValue<string>(),
                    ClubName = worksheet.Cell(startRow, 7).GetValue<string>(),
                    City = worksheet.Cell(startRow, 8).GetValue<string>(),
                    AgeGroup = EnumParser.GetEnumMemberValue<AgeGroup>(worksheet.Cell(startRow, 9).GetValue<string>()).ToString(),
                    ProgramType = EnumParser.GetEnumMemberValue<ProgramType>(worksheet.Cell(startRow, 10).GetValue<string>()).ToString(),
                };

                var annual = worksheet.Cell(startRow, 11).GetValue<string>();
                var seminar = worksheet.Cell(startRow, 12).GetValue<string>();
                var certification = worksheet.Cell(startRow, 13).GetValue<string>();
                var passport = worksheet.Cell(startRow, 14).GetValue<string>();

                dto.AddPrices(
                    ParseDecimal(annual),
                    ParseDecimal(seminar),
                    ParseDecimal(certification),
                    ParseDecimal(passport)
                );

                result.Add(dto);
                startRow++;
            }

            return result;
        }

        public async Task<MemoryStream> CreateCoachStatement(
            List<CoachStatementMemberDto> members,
            SeminarEntity seminar)
        {
            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Ведомость");

            AddSeminarData(worksheet, seminar);

            var offset = 6;
            AddTableHeaders(worksheet, offset);

            var totalsRow = AddMembersData(worksheet, offset, members);
            var row = totalsRow - 1;

            // "Итого" в колонке "ФИО"
            worksheet.Cell(totalsRow, 3).Value = "Итого";
            worksheet.Cell(totalsRow, 3).Style.Font.Bold = true;
            worksheet.Cell(totalsRow, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

            // Суммы по платёжной ведомости (столбцы K, L, M, N = 11–14)
            for (int col = 11; col <= 14; col++)
            {
                var colLetter = XLHelper.GetColumnLetterFromNumber(col);
                worksheet.Cell(totalsRow, col).FormulaA1 = $"SUM({colLetter}{offset + 3}:{colLetter}{row - 1})";
                worksheet.Cell(totalsRow, col).Style.Font.Bold = true;
            }

            // Итоговая сумма в "Примечаниях" (столбец O = 15)
            worksheet.Cell(totalsRow, 15).FormulaA1 = $"SUM(K{totalsRow}:N{totalsRow})";
            worksheet.Cell(totalsRow, 15).Style.Font.Bold = true;

            worksheet.Column(2).Hide();
            worksheet.Columns().AdjustToContents();

            var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;
            return stream;
        }

        private int AddMembersData(IXLWorksheet worksheet, int offset, List<CoachStatementMemberDto> members)
        {
            var row = offset + 3;
            foreach (var member in members)
            {
                worksheet.Cell(row, 1).Value = row - (offset + 2);        // №
                worksheet.Cell(row, 2).Value = member.Id;                 // ID
                worksheet.Cell(row, 3).Value = member.Name;
                worksheet.Cell(row, 4).Value = EnumParser.GetEnumMemberValue(EnumParser.ConvertStringToEnum<Grade>(member.Grade));
                worksheet.Cell(row, 5).Value = EnumParser.GetEnumMemberValue(EnumParser.ConvertStringToEnum<Grade>(member.CertificationGrade));
                worksheet.Cell(row, 6).Value = member.CoachName;
                worksheet.Cell(row, 7).Value = member.ClubName;
                worksheet.Cell(row, 8).Value = member.City;
                worksheet.Cell(row, 9).Value = EnumParser.GetEnumMemberValue(EnumParser.ConvertStringToEnum<AgeGroup>(member.AgeGroup));
                worksheet.Cell(row, 10).Value = EnumParser.GetEnumMemberValue(EnumParser.ConvertStringToEnum<ProgramType>(member.ProgramType));

                worksheet.Cell(row, 11).Value = member.AnnualPrice ?? 0;
                worksheet.Cell(row, 12).Value = member.SeminarPrice ?? 0;
                worksheet.Cell(row, 13).Value = member.CertificationPrice ?? 0;
                worksheet.Cell(row, 14).Value = member.BudoPassportPrice ?? 0;

                row++;
            }

            return row + 1;
        }


        private decimal? ParseDecimal(string value)
        {
            if (decimal.TryParse(value, out var result))
                return result;
            return null;
        }


    }
}
