using Aikido.AdditionalData.Enums;
using Aikido.Data;
using Aikido.Dto;
using Aikido.Dto.Groups;
using Aikido.Dto.Seminars;
using Aikido.Dto.Seminars.Members;
using Aikido.Dto.Seminars.Members.Creation;
using Aikido.Dto.Users;
using Aikido.Dto.Users.Creation;
using Aikido.Entities;
using Aikido.Entities.Seminar;
using ClosedXML.Excel;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;

namespace Aikido.Services
{
    public class TableService
    {
        private readonly AppDbContext _context;
        private static readonly (int from, int to)[] PaymentColumnPairs =
{
            (12, 13),
            (14, 15),
            (16, 17),
            (18, 19)
        };

        public TableService(AppDbContext context)
        {
            _context = context;
        }

        public MemoryStream ExportUsersToExcel(List<UserDto> users)
        {
            var workbook = new XLWorkbook();

            CreateUsersSheet(workbook, users);
            CreateDictionarySheet(workbook);

            var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            return stream;
        }

        private void CreateUsersSheet(XLWorkbook workbook, List<UserDto> users)
        {
            var worksheet = workbook.Worksheets.Add("Пользователи");

            var headers = new List<string>
            {
                "Id",
                "Фамилия",
                "Имя",
                "Отчество",
                "Логин",
                "Новый пароль",
                "Роль",
                "Пол",
                "Дата рождения (ДД.ММ.ГГГГ)",
                "Пояс",
                "Телефон"
            };

            for (var i = 0; i < headers.Count; i++)
            {
                var cell = worksheet.Cell(1, i + 1);
                cell.Value = headers[i];
                cell.Style.Fill.SetBackgroundColor(XLColor.LightBlue);
                cell.Style.Font.SetBold();
                cell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }

            var rowNum = 2;

            foreach (var user in users)
            {
                worksheet.Cell(rowNum, 1).Value = user.Id;
                worksheet.Cell(rowNum, 2).Value = user.LastName ?? "";
                worksheet.Cell(rowNum, 3).Value = user.FirstName ?? "";
                worksheet.Cell(rowNum, 4).Value = user.MiddleName ?? "";
                worksheet.Cell(rowNum, 5).Value = user.Login ?? "";

                worksheet.Cell(rowNum, 7).Value = user.Role != null
                    ? EnumParser.GetEnumMemberValue(EnumParser.ConvertStringToEnum<Role>(user.Role))
                    : "";

                worksheet.Cell(rowNum, 8).Value = user.Sex != null
                    ? EnumParser.GetEnumMemberValue(EnumParser.ConvertStringToEnum<Sex>(user.Sex))
                    : "";

                worksheet.Cell(rowNum, 9).Value = user.Birthday != null
                    ? user.Birthday.Value.ToString("dd.MM.yyyy", CultureInfo.GetCultureInfo("ru-RU"))
                    : "";

                worksheet.Cell(rowNum, 10).Value = user.Grade != null
                    ? EnumParser.GetEnumMemberValue(EnumParser.ConvertStringToEnum<Grade>(user.Grade))
                    : "";

                worksheet.Cell(rowNum, 11).Value = user.PhoneNumber ?? "";

                rowNum++;
            }

            worksheet.Columns().AdjustToContents();

            worksheet.Column(6).Hide();
        }

        private void CreateDictionarySheet(XLWorkbook workbook)
        {
            var worksheet = workbook.Worksheets.Add("Справочник");

            var headers = new[] { "Роль", "Пояс", "Пол" };

            for (int i = 0; i < headers.Length; i++)
            {
                var cell = worksheet.Cell(1, i + 1);
                cell.Value = headers[i];
                cell.Style.Fill.SetBackgroundColor(XLColor.LightBlue);
                cell.Style.Font.SetBold();
                cell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }

            var belts = Enum.GetValues(typeof(Grade)).Cast<Grade>()
                .Select(EnumParser.GetEnumMemberValue)
                .ToList();

            var sexes = Enum.GetValues(typeof(Sex)).Cast<Sex>()
                .Select(EnumParser.GetEnumMemberValue)
                .ToList();

            var roles = Enum.GetValues(typeof(Role)).Cast<Role>()
                .Select(EnumParser.GetEnumMemberValue)
                .ToList();

            var maxRows = Math.Max(Math.Max(belts.Count, sexes.Count), roles.Count);

            for (int i = 0; i < maxRows; i++)
            {
                worksheet.Cell(i + 2, 1).Value = i < roles.Count ? roles[i] : "";
                worksheet.Cell(i + 2, 2).Value = i < belts.Count ? belts[i] : "";
                worksheet.Cell(i + 2, 3).Value = i < sexes.Count ? sexes[i] : "";
            }

            worksheet.Columns().AdjustToContents();
        }

        public MemoryStream GenerateUserCreateTemplateExcelAsync()
        {
            var workbook = new XLWorkbook();

            GenerateUserTemplateAsync(workbook);
            CreateDictionarySheet(workbook);

            var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            return stream;
        }

        private void GenerateUserTemplateAsync(XLWorkbook workbook)
        {
            var worksheet = workbook.Worksheets.Add("Пользователи");

            var headers = new List<string>
            {
                "Фамилия",
                "Имя",
                "Отчество",
                "Логин",
                "Пароль",
                "Роль",
                "Пол",
                "Дата рождения (ДД.ММ.ГГГГ)",
                "Пояс",
                "Телефон"
            };

            for (var i = 0; i < headers.Count; i++)
            {
                var cell = worksheet.Cell(1, i + 1);
                cell.Value = headers[i];
                cell.Style.Fill.SetBackgroundColor(XLColor.LightBlue);
                cell.Style.Font.SetBold();
                cell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }

            worksheet.Columns().AdjustToContents();
        }

        public MemoryStream CreateSeminarMembersTable(SeminarDto seminar, List<ISeminarMemberDataDto> members)
        {
            var workbook = new XLWorkbook();

            CreateMembersWorksheet(workbook, seminar, members);
            CreateLookupWorksheet(seminar.Id.Value, workbook);

            var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            return stream;
        }

        private void CreateMembersWorksheet(XLWorkbook workbook, SeminarDto seminar, List<ISeminarMemberDataDto> members)
        {
            var worksheet = workbook.Worksheets.Add("Ведомость");

            var colCount = 20;

            worksheet.Cell(1, 1).Value = $"Ведомость на {seminar.Name}";
            worksheet.Range(1, 1, 1, colCount).Merge().Style
                .Font.SetBold().Font.SetFontSize(18).Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            worksheet.Cell(2, 1).Value = $"{seminar.Date:dd MMMM yyyy}";
            worksheet.Range(2, 1, 2, colCount).Merge().Style
                .Font.SetFontSize(12).Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            worksheet.Range(3, 1, 4, colCount).Style.Fill.SetBackgroundColor(XLColor.White);

            worksheet.Range(5, 3, 5, 11).Merge().Value = "Данные участника";
            worksheet.Range(5, 12, 5, 13).Merge().Value = "Распределение";
            worksheet.Range(5, 14, 5, 19).Merge().Value = "Платёжная ведомость";
            worksheet.Cell(5, 20).Value = "Примечания";

            worksheet.Range(5, 1, 5, colCount).Style
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                .Font.SetBold();

            worksheet.Range(5, 1, 5, colCount).Style.Fill.SetBackgroundColor(XLColor.LightBlue);
            worksheet.Range(5, 1, 5, colCount).Style.Border.OutsideBorder = XLBorderStyleValues.Double;

            var headers = new List<string>
            {
                "№", "userId", "ФИО", "Дата рождения", "Степень кю/дан", "Аттестуется",
                "Группа семинара", "Возрастная группа", "Тренер", "Клуб", "Город",
                $"Годовой взнос {seminar.Date.Year}", "Оплачено",
                "Семинар", "Оплачено",
                "Аттестация", "Оплачено",
                "Паспорт", "Оплачено",
                "Примечания"
            };

            for (var i = 0; i < colCount; i++)
            {
                var cell = worksheet.Cell(6, i + 1);
                cell.Value = headers[i];
                cell.Style.Fill.SetBackgroundColor(XLColor.LightBlue);
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                cell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                cell.Style.Font.SetBold();
            }

            var rowNum = 7;
            var index = 1;

            foreach (var m in members)
            {
                var oldGrade = EnumParser.ConvertStringToEnum<Grade>(m.OldGrade);
                var certGrade = EnumParser.ConvertStringToEnum<Grade>(m.CertificationGrade);

                worksheet.Cell(rowNum, 1).Value = index++;
                worksheet.Cell(rowNum, 2).Value = m.UserId;
                worksheet.Cell(rowNum, 3).Value = m.UserFullName ?? "";
                worksheet.Cell(rowNum, 4).Value = m.UserBirthday?.ToString("dd.MM.yyyy") ?? "";
                worksheet.Cell(rowNum, 5).Value = oldGrade == Grade.None ? "" : EnumParser.GetEnumMemberValue(oldGrade);
                worksheet.Cell(rowNum, 6).Value = certGrade == Grade.None ? "" : EnumParser.GetEnumMemberValue(certGrade);

                worksheet.Cell(rowNum, 7).Value = m.SeminarGroupName ?? "";
                worksheet.Cell(rowNum, 8).Value = m.AgeGroup == null ? ""
                    : EnumParser.GetEnumMemberValue(EnumParser.ConvertStringToEnum<AgeGroup>(m.AgeGroup));
                worksheet.Cell(rowNum, 9).Value = m.CoachName ?? "";
                worksheet.Cell(rowNum, 10).Value = m.ClubName;
                worksheet.Cell(rowNum, 11).Value = m.ClubCity;

                worksheet.Cell(rowNum, 12).Value = m.AnnualFeePriceInRubles;
                worksheet.Cell(rowNum, 13).Value = m.IsAnnualFeePayed ? "+" : "";

                worksheet.Cell(rowNum, 14).Value = m.SeminarPriceInRubles;
                worksheet.Cell(rowNum, 15).Value = m.IsSeminarPayed ? "+" : "";

                worksheet.Cell(rowNum, 16).Value = m.CertificationPriceInRubles;
                worksheet.Cell(rowNum, 17).Value = m.IsCertificationPayed ? "+" : "";

                worksheet.Cell(rowNum, 18).Value = m.BudoPassportPriceInRubles;
                worksheet.Cell(rowNum, 19).Value = m.IsBudoPassportPayed ? "+" : "";

                rowNum++;
            }

            var startDataRow = 7;
            var endDataRow = rowNum - 1;

            ApplyPaymentBorders(worksheet, 6, rowNum);

            worksheet.Cell(rowNum, 12).FormulaA1 = $"SUMIF(M{startDataRow}:M{endDataRow}, \"+\", L{startDataRow}:L{endDataRow})";
            worksheet.Cell(rowNum, 14).FormulaA1 = $"SUMIF(O{startDataRow}:O{endDataRow}, \"+\", N{startDataRow}:N{endDataRow})";
            worksheet.Cell(rowNum, 16).FormulaA1 = $"SUMIF(Q{startDataRow}:Q{endDataRow}, \"+\", P{startDataRow}:P{endDataRow})";
            worksheet.Cell(rowNum, 18).FormulaA1 = $"SUMIF(S{startDataRow}:S{endDataRow}, \"+\", R{startDataRow}:R{endDataRow})";

            worksheet.Cell(rowNum, 20).FormulaA1 = $"SUM(L{rowNum}:R{rowNum})";

            worksheet.Range(rowNum, 1, rowNum, colCount).Style.Font.SetBold();
            worksheet.Range(rowNum, 1, rowNum, colCount).Style.Fill.SetBackgroundColor(XLColor.LightGray);
            worksheet.Range(rowNum, 1, rowNum, colCount).Style.Border.OutsideBorder = XLBorderStyleValues.Thick;

            worksheet.Columns().AdjustToContents();
        }

        private void CreateLookupWorksheet(long seminarId, XLWorkbook workbook)
        {
            var worksheet = workbook.Worksheets.Add("Справочник");

            var headers = new[] { "Пояс", "Группа семинара" };

            for (int i = 0; i < headers.Length; i++)
            {
                var cell = worksheet.Cell(1, i + 1);
                cell.Value = headers[i];
                cell.Style.Fill.SetBackgroundColor(XLColor.LightBlue);
                cell.Style.Font.SetBold();
                cell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }

            var belts = Enum.GetValues(typeof(Grade)).Cast<Grade>()
                .Select(EnumParser.GetEnumMemberValue)
                .ToList();

            var seminarGroups = _context.SeminarGroups
                .Where(g => g.SeminarId == seminarId)
                .Select(g => g.Name)
                .ToList();

            var maxRows = Math.Max(belts.Count, seminarGroups.Count);

            for (int i = 0; i < maxRows; i++)
            {
                worksheet.Cell(i + 2, 1).Value = i < belts.Count ? belts[i] : "";
                worksheet.Cell(i + 2, 2).Value = i < seminarGroups.Count ? seminarGroups[i] : "";
            }

            worksheet.Columns().AdjustToContents();
        }

        public List<UserCreationDto> ParseUserCreationTable(Stream excelStream)
        {
            var users = new List<UserCreationDto>();
            var exceptions = new StringBuilder();

            using var workbook = new XLWorkbook(excelStream);
            var worksheet = workbook.Worksheets.First();

            var rowNum = 2;

            while (true)
            {
                var row = worksheet.Row(rowNum);

                if (row.Cell(1).IsEmpty())
                    break;

                var user = new UserCreationDto();

                if (string.IsNullOrWhiteSpace(row.Cell(1).GetString()))
                {
                    exceptions.AppendLine($"Пустая фамилия в строке: {rowNum}");
                }
                else
                {
                    user.LastName = row.Cell(1).GetString();
                }
                    

                if (string.IsNullOrWhiteSpace(row.Cell(2).GetString()))
                {
                    exceptions.AppendLine($"Пустое имя в строке: {rowNum}");
                }
                else
                {
                    user.FirstName = row.Cell(2).GetString();
                }
                    

                user.MiddleName = row.Cell(3).GetString();

                var login = row.Cell(4).GetString();
                if (string.IsNullOrWhiteSpace(login))
                {
                    exceptions.AppendLine($"Пустой логин в строке: {rowNum}");
                }
                else if (_context.Users.Any(u => u.Login == login))
                {
                    exceptions.AppendLine($"Логин в строке {rowNum} уже существует");
                }
                else
                {
                    user.Login = login;
                }

                var password = row.Cell(5).GetString();
                if (string.IsNullOrWhiteSpace(password))
                {
                    exceptions.AppendLine($"Пароль пустой в строке: {rowNum}");
                }
                else if (!Regex.IsMatch(password, "^[A-Za-z0-9]+$"))
                {
                    exceptions.AppendLine($"Пароль должен содержать только A-Z, a-z, 0-9 в строке: {rowNum}");
                }
                else
                {
                    user.Password = password;
                }

                var roleStr = row.Cell(6).GetString();
                if (!string.IsNullOrWhiteSpace(roleStr))
                {
                    try
                    {
                        user.Role = EnumParser.GetEnumByMemberValue<Role>(roleStr).ToString();
                    }
                    catch
                    {
                        exceptions.AppendLine($"Некорректная роль в строке: {rowNum}");
                    }
                }
                else
                {
                    exceptions.AppendLine($"Роль в строке: {rowNum} - не указана");
                }

                var sexStr = row.Cell(7).GetString();
                if (!string.IsNullOrWhiteSpace(sexStr))
                {
                    try
                    {
                        user.Sex = EnumParser.GetEnumByMemberValue<Sex>(sexStr).ToString();
                    }
                    catch
                    {
                        exceptions.AppendLine($"Некорректный пол в строке: {rowNum}");
                    }
                }
                else
                {
                    exceptions.AppendLine($"Пол в строке: {rowNum} - не указан");
                }

                user.Birthday = ParseDateCell(
                    row.Cell(9),
                    rowNum,
                    "Дата рождения",
                    exceptions
                );

                var gradeStr = row.Cell(9).GetString();
                if (!string.IsNullOrWhiteSpace(gradeStr))
                {
                    try
                    {
                        user.Grade = EnumParser.ConvertEnumToString(EnumParser.GetEnumByMemberValue<Grade>(gradeStr));
                    }
                    catch
                    {
                        exceptions.AppendLine($"Некорректный пояс в строке: {rowNum}");
                    }
                }

                user.PhoneNumber = row.Cell(10).GetString();

                users.Add(user);
                rowNum++;
            }

            if (exceptions.Length > 0)
                throw new InvalidDataException(exceptions.ToString());

            return users;
        }

        public List<(long Id, UserCreationDto Data)> ParseUserUpdateTable(Stream excelStream)
        {
            var result = new List<(long, UserCreationDto)>();
            var exceptions = new StringBuilder();

            using var workbook = new XLWorkbook(excelStream);
            var worksheet = workbook.Worksheets.First();

            var rowNum = 2;

            while (true)
            {
                var row = worksheet.Row(rowNum);

                if (row.Cell(1).IsEmpty())
                    break;

                int id;
                if (!int.TryParse(row.Cell(1).GetString(), out id))
                {
                    exceptions.AppendLine($"Некорректный Id в строке: {rowNum}");
                    rowNum++;
                    continue;
                }

                var user = new UserCreationDto();

                if (string.IsNullOrWhiteSpace(row.Cell(2).GetString()))
                    exceptions.AppendLine($"Пустая фамилия в строке: {rowNum}");
                else
                    user.LastName = row.Cell(2).GetString();

                if (string.IsNullOrWhiteSpace(row.Cell(3).GetString()))
                    exceptions.AppendLine($"Пустое имя в строке: {rowNum}");
                else
                    user.FirstName = row.Cell(3).GetString();

                user.MiddleName = row.Cell(4).GetString();

                var login = row.Cell(5).GetString();
                if (string.IsNullOrWhiteSpace(login))
                {
                    exceptions.AppendLine($"Пустой логин в строке: {rowNum}");
                }
                else if (_context.Users.Any(u => u.Login == login && u.Id != id))
                {
                    exceptions.AppendLine($"Логин в строке {rowNum} уже существует");
                }
                else
                {
                    user.Login = login;
                }

                var password = row.Cell(6).GetString();

                if (!string.IsNullOrWhiteSpace(password) && !Regex.IsMatch(password, "^[A-Za-z0-9]+$"))
                {
                    exceptions.AppendLine($"Пароль должен содержать только A-Z, a-z, 0-9 в строке: {rowNum}");
                }
                else
                {
                    user.Password = password;
                }

                var roleStr = row.Cell(7).GetString();
                if (!string.IsNullOrWhiteSpace(roleStr))
                {
                    try
                    {
                        user.Role = EnumParser.GetEnumByMemberValue<Role>(roleStr).ToString();
                    }
                    catch
                    {
                        exceptions.AppendLine($"Некорректная роль в строке: {rowNum}");
                    }
                }
                else
                {
                    exceptions.AppendLine($"Роль в строке: {rowNum} - не указана");
                }

                var sexStr = row.Cell(8).GetString();
                if (!string.IsNullOrWhiteSpace(sexStr))
                {
                    try
                    {
                        user.Sex = EnumParser.GetEnumByMemberValue<Sex>(sexStr).ToString();
                    }
                    catch
                    {
                        exceptions.AppendLine($"Некорректный пол в строке: {rowNum}");
                    }
                }
                else
                {
                    exceptions.AppendLine($"Пол в строке: {rowNum} - не указан");
                }

                user.Birthday = ParseDateCell(
                    row.Cell(9),
                    rowNum,
                    "Дата рождения",
                    exceptions
                );

                var gradeStr = row.Cell(10).GetString();
                if (!string.IsNullOrWhiteSpace(gradeStr))
                {
                    try
                    {
                        user.Grade = EnumParser.ConvertEnumToString(
                            EnumParser.GetEnumByMemberValue<Grade>(gradeStr));
                    }
                    catch
                    {
                        exceptions.AppendLine($"Некорректный пояс в строке: {rowNum}");
                    }
                }

                user.PhoneNumber = row.Cell(11).GetString();

                result.Add((id, user));
                rowNum++;
            }

            if (exceptions.Length > 0)
                throw new InvalidDataException(exceptions.ToString());

            return result;
        }

        public List<SeminarMemberCreationDto> ParseSeminarMembersTable(Stream excelStream)
        {
            var members = new List<SeminarMemberCreationDto>();
            var exceptions = new StringBuilder();

            using var workbook = new XLWorkbook(excelStream);
            var worksheet = workbook.Worksheets.First();

            var rowNum = 7;

            while (true)
            {
                var row = worksheet.Row(rowNum);

                if (row.Cell(2).IsEmpty())
                    break;

                var member = new SeminarMemberCreationDto();

                member.UserId = row.Cell(2).GetValue<long>();
                if (member.UserId <= 0 || _context.Users.Find(member.UserId) == null)
                {
                    exceptions.AppendLine($"Неправильный Id в строке: {rowNum}");
                }

                try
                {
                    member.CertificationGrade = ParseGrade(row.Cell(6).GetString());
                }
                catch
                {
                    exceptions.AppendLine($"Неправильный пояс в строке: {rowNum}");
                }

                var seminarGroup = row.Cell(7).GetString();

                if (seminarGroup != null)
                {
                    var groupId = _context.SeminarGroups.FirstOrDefault(g => g.Name == seminarGroup)?.Id;
                    
                    if (groupId == null)
                    {
                        exceptions.AppendLine($"Неправильное наименование группы в строке: {rowNum}");
                    }
                    else
                    {
                        member.SeminarGroupId = groupId;
                    }
                } 

                member.AnnualFeePriceInRubles = GetDecimalOrNull(row.Cell(12));
                member.IsAnnualFeePayed = IsPaid(row.Cell(13));

                member.SeminarPriceInRubles = GetDecimalOrNull(row.Cell(14));
                member.IsSeminarPayed = IsPaid(row.Cell(15));

                member.CertificationPriceInRubles = GetDecimalOrNull(row.Cell(16));
                member.IsCertificationPayed = IsPaid(row.Cell(17));

                member.BudoPassportPriceInRubles = GetDecimalOrNull(row.Cell(18));
                member.IsBudoPassportPayed = IsPaid(row.Cell(19));

                member.Note = row.Cell(20).GetString();

                members.Add(member);
                rowNum++;
            }

            if (exceptions.Length > 0)
            {
                throw new InvalidDataException(exceptions.ToString());
            }

            return members;
        }

        private DateTime? ParseDateCell(IXLCell cell, int rowNum, string fieldName, StringBuilder errors)
        {
            if (cell.IsEmpty())
                return null;

            try
            {
                if (cell.DataType == XLDataType.DateTime)
                {
                    return cell.GetDateTime();
                }

                if (cell.DataType == XLDataType.Number)
                {
                    return DateTime.FromOADate(cell.GetDouble());
                }

                var str = cell.GetString();

                if (DateTime.TryParseExact(
                        str,
                        "dd.MM.yyyy",
                        CultureInfo.GetCultureInfo("ru-RU"),
                        DateTimeStyles.None,
                        out var date))
                {
                    return date;
                }

                errors.AppendLine($"Некорректное поле \"{fieldName}\" в строке: {rowNum}");
                return null;
            }
            catch
            {
                errors.AppendLine($"Ошибка при чтении \"{fieldName}\" в строке: {rowNum}");
                return null;
            }
        }

        private void ApplyPaymentBorders(IXLWorksheet worksheet, int startRow, int endRow)
        {
            foreach (var (from, to) in PaymentColumnPairs)
            {
                var range = worksheet.Range(startRow, from, endRow, to);
                range.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
            }
            var range2 = worksheet.Range(startRow, 20, endRow, 20);
            range2.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
        }

        private DateTime? TryParseDate(IXLCell cell)
        {
            if (cell.IsEmpty())
                return null;

            if (cell.TryGetValue<DateTime>(out var date))
                return date;

            if (DateTime.TryParse(cell.GetString(), out date))
                return date;

            return null;
        }

        private string ParseGrade(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "None";

            return EnumParser.ConvertEnumToString(EnumParser.GetEnumByMemberValue<Grade>(value));
        }

        private bool IsPaid(IXLCell cell)
        {
            return cell.GetString().Trim() == "+";
        }

        public MemoryStream GetAttendanceTable(GroupDashboardDto dashboard, int year, int month)
        {
            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Таблица посещений");

            var firstDay = new DateTime(year, month, 1);
            var lastDay = firstDay.AddMonths(1).AddDays(-1);

            var scheduleDays = dashboard.Schedule?
                .Select(s => s.DayOfWeek)
                .Distinct()
                .ToList() ?? new List<DayOfWeek>();

            var trainingDates = Enumerable.Range(0, (lastDay - firstDay).Days + 1)
                .Select(offset => firstDay.AddDays(offset))
                .Where(d => scheduleDays.Contains(d.DayOfWeek))
                .ToList();

            var excludedDates = dashboard.ExclusionDates?
                .Where(e => e.Type == "Minor")
                .Select(e => e.Date.Date)
                .Where(d => d.Month == month && d.Year == year)
                .ToHashSet() ?? new HashSet<DateTime>();

            trainingDates = trainingDates
                .Where(d => !excludedDates.Contains(d))
                .ToList();

            var addedDates = dashboard.ExclusionDates?
                .Where(e => e.Type == "Extra")
                .Select(e => e.Date.Date)
                .Where(d => d.Month == month && d.Year == year)
                .ToList() ?? new List<DateTime>();

            foreach (var date in addedDates)
                if (!trainingDates.Contains(date))
                    trainingDates.Add(date);

            trainingDates = trainingDates.OrderBy(x => x).ToList();

            var headers = new[] {
                "ФИО", "Долг", "Аванс", "Оплатить", "Оплачено", "Посещено"
            }.Concat(trainingDates.Select(d => d.ToString("dd.MM.yyyy"))).ToList();

            for (int col = 0; col < headers.Count; col++)
            {
                worksheet.Cell(1, col + 1).Value = headers[col];
                worksheet.Cell(1, col + 1).Style.Fill.SetBackgroundColor(XLColor.Gray);
                worksheet.Cell(1, col + 1).Style.Font.SetBold();
                worksheet.Cell(1, col + 1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                worksheet.Column(col + 1).Width = col < 6 ? 14 : 12;
            }

            for (var rowIdx = 0; rowIdx < dashboard.Users.Count; rowIdx++)
            {
                var user = dashboard.Users[rowIdx];
                var fullName = $"{user.LastName} {user.FirstName} {user.MiddleName}".Trim();
                worksheet.Cell(rowIdx + 2, 1).Value = fullName;
                worksheet.Cell(rowIdx + 2, 2).Value = 0;
                worksheet.Cell(rowIdx + 2, 3).Value = 0;
                worksheet.Cell(rowIdx + 2, 4).Value = "";
                worksheet.Cell(rowIdx + 2, 5).Value = "";

                var attendanceDates = user.Attendances
                    .Select(a => a.Date.Date)
                    .Where(d => d.Month == month && d.Year == year)
                    .ToHashSet();

                worksheet.Cell(rowIdx + 2, 6).Value = trainingDates.Count(d => attendanceDates.Contains(d));

                for (var colIdx = 0; colIdx < trainingDates.Count; colIdx++)
                {
                    var cell = worksheet.Cell(rowIdx + 2, 7 + colIdx);
                    cell.Value = attendanceDates.Contains(trainingDates[colIdx]) ? "ИСТИНА" : "ЛОЖЬ";
                    cell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                }
            }

            worksheet.Columns().AdjustToContents();

            var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;
            return stream;
        }

        private decimal? GetDecimalOrNull(IXLCell cell)
        {
            return cell.IsEmpty() ? (decimal?)null : decimal.Parse(cell.GetString());
        }
    }
}
