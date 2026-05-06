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

        public async Task<MemoryStream> ExportUsersToExcelAsync()
        {
            var users = await _context.Users
                .Include(u => u.UserMemberships)
                    .ThenInclude(um => um.Club)
                .Include(u => u.UserMemberships)
                    .ThenInclude(um => um.Group)
                .ToListAsync();

            var stream = new MemoryStream();
            using var document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook);

            var workbookPart = document.AddWorkbookPart();
            workbookPart.Workbook = new Workbook();

            var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
            worksheetPart.Worksheet = new Worksheet(new SheetData());

            var sheets = document.WorkbookPart.Workbook.AppendChild(new Sheets());
            var sheet = new Sheet
            {
                Id = document.WorkbookPart.GetIdOfPart(worksheetPart),
                SheetId = 1,
                Name = "Users"
            };
            sheets.Append(sheet);

            var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();

            var headerRow = new Row();
            headerRow.Append(
                new Cell { CellValue = new CellValue("Id"), DataType = CellValues.String },
                new Cell { CellValue = new CellValue("Name"), DataType = CellValues.String },
                new Cell { CellValue = new CellValue("Role"), DataType = CellValues.String },
                new Cell { CellValue = new CellValue("Grade"), DataType = CellValues.String },
                new Cell { CellValue = new CellValue("Phone"), DataType = CellValues.String },
                new Cell { CellValue = new CellValue("City"), DataType = CellValues.String },
                new Cell { CellValue = new CellValue("Clubs"), DataType = CellValues.String },
                new Cell { CellValue = new CellValue("Groups"), DataType = CellValues.String }
            );
            sheetData.AppendChild(headerRow);

            foreach (var user in users)
            {
                var clubNames = string.Join(", ",
                    user.UserMemberships.Where(um => um.Club != null)
                                  .Select(uc => uc.Club!.Name));

                var groupNames = string.Join(", ",
                    user.UserMemberships.Where(um => um.Club != null)
                                   .Select(ug => ug.Group!.Name));

                var row = new Row();
                row.Append(
                    new Cell { CellValue = new CellValue(user.Id.ToString()), DataType = CellValues.String },
                    new Cell { CellValue = new CellValue(user.FullName), DataType = CellValues.String },
                    new Cell { CellValue = new CellValue(user.Role.ToString()), DataType = CellValues.String },
                    new Cell { CellValue = new CellValue(user.Grade.ToString()), DataType = CellValues.String },
                    new Cell { CellValue = new CellValue(user.PhoneNumber ?? ""), DataType = CellValues.String },
                    new Cell { CellValue = new CellValue(user.City ?? ""), DataType = CellValues.String },
                    new Cell { CellValue = new CellValue(clubNames), DataType = CellValues.String },
                    new Cell { CellValue = new CellValue(groupNames), DataType = CellValues.String }
                );
                sheetData.AppendChild(row);
            }

            workbookPart.Workbook.Save();
            document.Dispose();

            stream.Position = 0;
            return stream;
        }

        public async Task<MemoryStream> GenerateUserUpdateTemplateExcelAsync()
        {
            return await GenerateUserTemplateAsync(includeIds: true);
        }

        public async Task<MemoryStream> GenerateUserCreateTemplateExcelAsync()
        {
            return await GenerateUserTemplateAsync(includeIds: false);
        }

        private async Task<MemoryStream> GenerateUserTemplateAsync(bool includeIds)
        {
            var stream = new MemoryStream();
            using var document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook);

            var workbookPart = document.AddWorkbookPart();
            workbookPart.Workbook = new Workbook();

            var sheets = document.WorkbookPart.Workbook.AppendChild(new Sheets());

            var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
            var styles = AddStyles(workbookPart);
            worksheetPart.Worksheet = new Worksheet(new SheetData());
            var columns = new Columns(
                new Column
                {
                    Min = 8,
                    Max = 8,
                    Style = 1, 
                    Width = 20,
                    CustomWidth = true
                },
                new Column
                {
                    Min = 11,
                    Max = 11,
                    Style = 1, 
                    Width = 20,
                    CustomWidth = true
                }
            );

            worksheetPart.Worksheet.InsertAt(columns, 0);

            var sheet = new Sheet
            {
                Id = workbookPart.GetIdOfPart(worksheetPart),
                SheetId = 1,
                Name = "Создание пользователей"
            };
            sheets.Append(sheet);

            var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();

            var headerRow = new Row();
            var headers = new List<string>();
            if (includeIds) headers.Add("Id");

            headers.AddRange(new[]
            {
                "Фамилия", "Имя", "Отчество",
                "Логин", "Пароль", "Роль",
                "Пояс", "Телефон", "Город",
                "Пол", "Дата рождения (ДД.ММ.ГГГГ)"
            });

            foreach (var header in headers)
            {
                headerRow.Append(new Cell
                {
                    CellValue = new CellValue(header),
                    DataType = CellValues.String
                });
            }

            sheetData.AppendChild(headerRow);

            var lookupSheetPart = workbookPart.AddNewPart<WorksheetPart>();
            lookupSheetPart.Worksheet = new Worksheet(new SheetData());

            var lookupSheet = new Sheet
            {
                Id = workbookPart.GetIdOfPart(lookupSheetPart),
                SheetId = 2,
                Name = "Справочник"
            };
            sheets.Append(lookupSheet);

            var lookupSheetData = lookupSheetPart.Worksheet.GetFirstChild<SheetData>();

            var lookupHeaderRow = new Row();
            var lookupHeaders = new[] { "Роль", "Пояс", "Пол" };
            foreach (var h in lookupHeaders)
            {
                lookupHeaderRow.Append(new Cell
                {
                    CellValue = new CellValue(h),
                    DataType = CellValues.String
                });
            }
            lookupSheetData.AppendChild(lookupHeaderRow);

            var roles = Enum.GetValues(typeof(Role))
                            .Cast<Role>()
                            .Select(EnumParser.GetEnumMemberValue)
                            .ToArray();

            var belts = Enum.GetValues(typeof(Grade))
                            .Cast<Grade>()
                            .Select(EnumParser.GetEnumMemberValue)
                            .ToArray();

            var sexes = Enum.GetValues(typeof(Sex))
                            .Cast<Sex>()
                            .Select(EnumParser.GetEnumMemberValue)
                            .ToArray();

            var maxRows = Math.Max(roles.Length, Math.Max(belts.Length, sexes.Length));

            for (int i = 0; i < maxRows; i++)
            {
                var row = new Row();

                row.Append(new Cell
                {
                    CellValue = new CellValue(i < roles.Length ? roles[i] : string.Empty),
                    DataType = CellValues.String
                });

                row.Append(new Cell
                {
                    CellValue = new CellValue(i < belts.Length ? belts[i] : string.Empty),
                    DataType = CellValues.String
                });

                row.Append(new Cell
                {
                    CellValue = new CellValue(i < sexes.Length ? sexes[i] : string.Empty),
                    DataType = CellValues.String
                });

                lookupSheetData.AppendChild(row);
            }

            workbookPart.Workbook.Save();
            document.Dispose();

            stream.Position = 0;
            return stream;
        }

        private WorkbookStylesPart AddStyles(WorkbookPart workbookPart)
        {
            var stylesPart = workbookPart.AddNewPart<WorkbookStylesPart>();

            stylesPart.Stylesheet = new Stylesheet(
                new Fonts(new Font()),
                new Fills(new Fill()),
                new Borders(new Border()),
                new CellFormats(
                    new CellFormat(), // default
                    new CellFormat { NumberFormatId = 49, ApplyNumberFormat = true } // текст
                )
            );

            stylesPart.Stylesheet.Save();
            return stylesPart;
        }

        public async Task<MemoryStream> ExportSeminarsToExcelAsync()
        {
            var seminars = await _context.Seminars
                .Include(s => s.Creator)
                .Include(s => s.Members)
                    .ThenInclude(sm => sm.User)
                .ToListAsync();

            var stream = new MemoryStream();
            using var document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook);

            var workbookPart = document.AddWorkbookPart();
            workbookPart.Workbook = new Workbook();

            var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
            worksheetPart.Worksheet = new Worksheet(new SheetData());

            var sheets = document.WorkbookPart.Workbook.AppendChild(new Sheets());
            var sheet = new Sheet
            {
                Id = document.WorkbookPart.GetIdOfPart(worksheetPart),
                SheetId = 1,
                Name = "Seminars"
            };
            sheets.Append(sheet);

            var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();

            var headerRow = new Row();
            headerRow.Append(
                new Cell { CellValue = new CellValue("Id"), DataType = CellValues.String },
                new Cell { CellValue = new CellValue("Name"), DataType = CellValues.String },
                new Cell { CellValue = new CellValue("StartDate"), DataType = CellValues.String },
                new Cell { CellValue = new CellValue("Location"), DataType = CellValues.String }

            );
            sheetData.AppendChild(headerRow);

            foreach (var seminar in seminars)
            {
                var row = new Row();
                row.Append(
                    new Cell { CellValue = new CellValue(seminar.Id.ToString()), DataType = CellValues.String },
                    new Cell { CellValue = new CellValue(seminar.Name), DataType = CellValues.String },
                    new Cell { CellValue = new CellValue(seminar.Date.ToString("yyyy-MM-dd")), DataType = CellValues.String },
                    new Cell { CellValue = new CellValue(seminar.Location ?? ""), DataType = CellValues.String }
                );
                sheetData.AppendChild(row);
            }

            workbookPart.Workbook.Save();
            document.Dispose();

            stream.Position = 0;
            return stream;
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

        public List<UserCreationDto> ParseUserCreationTable(Stream excelStream)
        {
            var users = new List<UserCreationDto>();

            using var document = SpreadsheetDocument.Open(excelStream, false);
            var workbookPart = document.WorkbookPart;

            var sheet = workbookPart.Workbook.Sheets.Cast<Sheet>()
                .FirstOrDefault(s => s.Name == "Создание пользователей");

            if (sheet == null)
                return users;

            var worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id!);

            var rows = worksheetPart.Worksheet
                .Elements<SheetData>()
                .First()
                .Elements<Row>()
                .Skip(1); // пропускаем заголовок

            foreach (var row in rows)
            {
                var cellDictionary = row.Elements<Cell>()
                    .ToDictionary(GetColumnIndex);

                if (!cellDictionary.Any())
                    continue;

                int index = 0;
                var dto = new UserCreationDto();

                string? GetCellValue(int i)
                {
                    if (!cellDictionary.TryGetValue(i, out var cell))
                        return null;

                    return GetCellText(cell, workbookPart);
                }

                dto.LastName = GetCellValue(index++);
                dto.FirstName = GetCellValue(index++);
                dto.MiddleName = GetCellValue(index++);
                dto.Login = GetCellValue(index++);
                dto.Password = GetCellValue(index++);

                dto.Role = EnumParser.ConvertEnumToString(
                    EnumParser.GetEnumByMemberValue<Role>(GetCellValue(index++)));

                dto.Grade = EnumParser.ConvertEnumToString(
                    EnumParser.GetEnumByMemberValue<Grade>(GetCellValue(index++)));

                dto.PhoneNumber = GetCellValue(index++);
                dto.City = GetCellValue(index++);

                dto.Sex = EnumParser.ConvertEnumToString(
                    EnumParser.GetEnumByMemberValue<Sex>(GetCellValue(index++)));

                var birthdayStr = GetCellValue(index++);

                if (!string.IsNullOrWhiteSpace(birthdayStr))
                {
                    DateTime birthday;

                    if (double.TryParse(birthdayStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var oaDate))
                    {
                        birthday = DateTime.FromOADate(oaDate);
                    }
                    else if (DateTime.TryParseExact(
                        birthdayStr,
                        "dd.MM.yyyy",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out var parsed))
                    {
                        birthday = parsed;
                    }
                    else
                    {
                        throw new InvalidDataException("Неправильный формат даты");
                    }

                    dto.Birthday = DateTime.SpecifyKind(birthday.Date, DateTimeKind.Utc);
                }

                users.Add(dto);
            }

            return users;
        }

        private int GetColumnIndex(Cell cell)
        {
            var reference = cell.CellReference!.Value;

            int columnIndex = 0;

            foreach (var ch in reference)
            {
                if (char.IsLetter(ch))
                {
                    columnIndex *= 26;
                    columnIndex += (ch - 'A' + 1);
                }
                else
                {
                    break;
                }
            }

            return columnIndex - 1;
        }

        private string? GetCellText(Cell cell, WorkbookPart workbookPart)
        {
            if (cell == null)
                return null;

            var value = cell.InnerText;

            if (cell.DataType == null)
                return value;

            if (cell.DataType.Value == CellValues.SharedString)
            {
                var stringTable = workbookPart.SharedStringTablePart;
                return stringTable?.SharedStringTable.ElementAt(int.Parse(value)).InnerText;
            }

            return value;
        }

        private decimal? GetDecimalOrNull(IXLCell cell)
        {
            return cell.IsEmpty() ? (decimal?)null : decimal.Parse(cell.GetString());
        }
    }
}
