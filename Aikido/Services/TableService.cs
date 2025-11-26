using Aikido.AdditionalData;
using Aikido.Data;
using Aikido.Dto;
using Aikido.Dto.Groups;
using Aikido.Dto.Seminars.Members;
using Aikido.Dto.Users;
using Aikido.Entities;
using ClosedXML.Excel;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.EntityFrameworkCore;

namespace Aikido.Services
{
    public class TableService
    {
        private readonly AppDbContext _context;

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

            var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
            worksheetPart.Worksheet = new Worksheet(new SheetData());

            var sheets = document.WorkbookPart.Workbook.AppendChild(new Sheets());
            var sheet = new Sheet
            {
                Id = document.WorkbookPart.GetIdOfPart(worksheetPart),
                SheetId = 1,
                Name = "UserTemplate"
            };
            sheets.Append(sheet);

            var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();

            var headerRow = new Row();
            var headers = new List<string>();

            if (includeIds)
                headers.Add("Id");

            headers.AddRange(new[]
            {
                "Name", "Role", "Grade", "Phone", "City",
                "ClubIds (comma separated)", "GroupIds (comma separated)",
                "Sex", "Birthday (YYYY-MM-DD)", "Education", "ProgramType",
                "ParentFullName", "ParentPhoneNumber"
            });

            foreach (var header in headers)
            {
                headerRow.Append(new Cell { CellValue = new CellValue(header), DataType = CellValues.String });
            }

            sheetData.AppendChild(headerRow);

            workbookPart.Workbook.Save();
            document.Dispose();

            stream.Position = 0;
            return stream;
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

        public MemoryStream CreateSeminarMembersTable(List<SeminarMemberDto> members)
        {
            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Шаблон пользователей");

            var colCount = 15;

            worksheet.Cell(1, 1).Value = $"Ведомость на {members.First().SeminarName}";
            worksheet.Range(1, 1, 1, colCount).Merge().Style
                .Font.SetBold().Font.SetFontSize(18).Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            worksheet.Cell(2, 1).Value = $"{members.First().SeminarDate:dd MMMM yyyy}";
            worksheet.Range(2, 1, 2, colCount).Merge().Style
                .Font.SetBold(false).Font.SetFontSize(12).Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            worksheet.Range(3, 1, 4, colCount).Style.Fill.SetBackgroundColor(XLColor.White);

            worksheet.Cell(5, 1).Value = ""; 
            worksheet.Cell(5, 2).Value = ""; 
            worksheet.Range(5, 3, 5, 8).Merge().Value = "Данные участника";
            worksheet.Range(5, 3, 5, 8).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center).Font.SetBold();
            worksheet.Range(5, 3, 5, 8).Style.Fill.SetBackgroundColor(XLColor.LightBlue);

            worksheet.Range(5, 9, 5, 10).Merge().Value = "Распределение";
            worksheet.Range(5, 9, 5, 10).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center).Font.SetBold();
            worksheet.Range(5, 9, 5, 10).Style.Fill.SetBackgroundColor(XLColor.LightBlue);

            worksheet.Range(5, 11, 5, 14).Merge().Value = "Платёжная ведомость";
            worksheet.Range(5, 11, 5, 14).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center).Font.SetBold();
            worksheet.Range(5, 11, 5, 14).Style.Fill.SetBackgroundColor(XLColor.LightBlue);

            worksheet.Cell(5, 15).Value = "примечания";
            worksheet.Row(5).Style.Font.SetBold();
            worksheet.Row(5).Style.Fill.SetBackgroundColor(XLColor.LightBlue);

            var headers = new List<string>
            {
                "№", "userId", "ФИО", "Степень кю/дан", "Аттестуется", "Тренер", "Клуб", "Город",
                "Возрастная группа", "Программа",
                $"Годовой взнос {members.First().SeminarDate.Value.Year}", "Семинар", "Аттестация", "Паспорт", "примечания"
            };

            for (var i = 0; i < colCount; i++)
            {
                worksheet.Cell(6, i + 1).Value = headers[i];
                worksheet.Cell(6, i + 1).Style.Fill.SetBackgroundColor(XLColor.LightBlue);
                worksheet.Cell(6, i + 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(6, i + 1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                worksheet.Cell(6, i + 1).Style.Font.SetBold();
            }

            var rowNum = 7;
            var index = 1;
            decimal sumAnnual = 0, sumSeminar = 0, sumCertification = 0, sumPassport = 0;

            foreach (var m in members)
            {
                worksheet.Cell(rowNum, 1).Value = index++;
                worksheet.Cell(rowNum, 2).Value = m.UserId;
                worksheet.Cell(rowNum, 3).Value = m.UserFullName ?? "";
                worksheet.Cell(rowNum, 4).Value = m.OldGrade == "None" ? ""
                    : EnumParser.GetEnumMemberValue(EnumParser.ConvertStringToEnum<Grade>(m.OldGrade));
                worksheet.Cell(rowNum, 5).Value = m.CertificationGrade == "None" ? "" 
                    : EnumParser.GetEnumMemberValue(EnumParser.ConvertStringToEnum<Grade>(m.CertificationGrade));
                worksheet.Cell(rowNum, 6).Value = m.CreatorFullName ?? ""; 
                worksheet.Cell(rowNum, 7).Value = m.ClubName;
                worksheet.Cell(rowNum, 8).Value = m.ClubCity;
                worksheet.Cell(rowNum, 9).Value = m.AgeGroup == null ? "" 
                    : EnumParser.GetEnumMemberValue(EnumParser.ConvertStringToEnum<AgeGroup>(m.AgeGroup));

                if (EnumParser.ConvertStringToEnum<Grade>(m.CertificationGrade) == Grade.None)
                {
                    worksheet.Cell(rowNum, 10).Value = "";
                }
                else if (EnumParser.ConvertStringToEnum<Grade>(m.CertificationGrade) > Grade.Kyu1Child)
                {
                    worksheet.Cell(rowNum, 10).Value = "Взрослая";
                }
                else
                {
                    worksheet.Cell(rowNum, 10).Value = "Детская";
                }
                worksheet.Cell(rowNum, 11).Value = m.AnnualFeePriceInRubles ?? (decimal?)null;
                worksheet.Cell(rowNum, 12).Value = m.SeminarPriceInRubles ?? (decimal?)null;
                worksheet.Cell(rowNum, 13).Value = m.CertificationPriceInRubles ?? (decimal?)null;
                worksheet.Cell(rowNum, 14).Value = m.BudoPassportPriceInRubles ?? (decimal?)null;

                worksheet.Cell(rowNum, 15).Value = "";

                sumAnnual += m.AnnualFeePriceInRubles ?? 0;
                sumSeminar += m.SeminarPriceInRubles ?? 0;
                sumCertification += m.CertificationPriceInRubles ?? 0;
                sumPassport += m.BudoPassportPriceInRubles ?? 0;

                rowNum++;
            }

            var startDataRow = 7;
            var endDataRow = rowNum - 1;

            worksheet.Cell(rowNum, 11).FormulaA1 = $"SUM(K{startDataRow}:K{endDataRow})";
            worksheet.Cell(rowNum, 12).FormulaA1 = $"SUM(L{startDataRow}:L{endDataRow})";
            worksheet.Cell(rowNum, 13).FormulaA1 = $"SUM(M{startDataRow}:M{endDataRow})";
            worksheet.Cell(rowNum, 14).FormulaA1 = $"SUM(N{startDataRow}:N{endDataRow})";

            worksheet.Cell(rowNum, 15).FormulaA1 = $"SUM(K{rowNum}:N{rowNum})";


            worksheet.Range(rowNum, 1, rowNum, colCount).Style.Font.SetBold();
            worksheet.Range(rowNum, 1, rowNum, colCount).Style.Fill.SetBackgroundColor(XLColor.LightGray);
            worksheet.Range(rowNum, 1, rowNum, colCount).Style.Border.OutsideBorder = XLBorderStyleValues.Thick;

            worksheet.Column(2).Width = 4;

            worksheet.Columns().AdjustToContents();

            var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            return stream;
        }

        public List<SeminarMemberDto> ParseSeminarMembersTable(Stream excelStream)
        {
            var members = new List<SeminarMemberDto>();
            using (var workbook = new XLWorkbook(excelStream))
            {
                var worksheet = workbook.Worksheets.First();

                var rowNum = 7;
                while (true)
                {
                    var row = worksheet.Row(rowNum);

                    if (row.Cell(2).IsEmpty())
                        break;

                    var member = new SeminarMemberDto
                    {
                        UserId = row.Cell(2).GetValue<long>(),
                        UserFullName = row.Cell(3).GetString(),
                        OldGrade = EnumParser.ConvertEnumToString(EnumParser.GetEnumByMemberValue<Grade>(row.Cell(4).GetString())),
                        CertificationGrade = EnumParser.ConvertEnumToString(EnumParser.GetEnumByMemberValue<Grade>(row.Cell(5).GetString())),
                        CreatorFullName = row.Cell(6).GetString(),
                        ClubName = row.Cell(7).GetString(),
                        ClubCity = row.Cell(8).GetString(),
                        AgeGroup = row.Cell(9).GetString(),
                        AnnualFeePriceInRubles = GetDecimalOrNull(row.Cell(11)),
                        SeminarPriceInRubles = GetDecimalOrNull(row.Cell(12)),
                        CertificationPriceInRubles = GetDecimalOrNull(row.Cell(13)),
                        BudoPassportPriceInRubles = GetDecimalOrNull(row.Cell(14)),
                    };

                    members.Add(member);
                    rowNum++;
                }
            }
            return members;
        }

        public MemoryStream GetAttendanceTable(GroupDashboardDto dashboard)
        {
            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Таблица посещений");

            var allAttendanceDates = dashboard.Users
                .SelectMany(u => u.Attendances)
                .Select(a => a.Date.Date)
                .ToList();

            DateTime targetMonth = allAttendanceDates.Any()
                ? new DateTime(allAttendanceDates.Min().Year, allAttendanceDates.Min().Month, 1)
                : DateTime.Now;

            var firstDay = new DateTime(targetMonth.Year, targetMonth.Month, 1);
            var lastDay = firstDay.AddMonths(1).AddDays(-1);

            var sessionDays = dashboard.Schedule?
                .Select(s => s.DayOfWeek)
                .Distinct()
                .ToList() ?? new List<DayOfWeek>();

            var trainingDates = Enumerable.Range(0, (lastDay - firstDay).Days + 1)
                .Select(offset => firstDay.AddDays(offset))
                .Where(d => sessionDays.Contains(d.DayOfWeek))
                .ToList();

            var excludedDates = dashboard.ExclusionDates?
                .Where(e => e.Type == "Minor")
                .Select(e => e.Date.Date)
                .Where(d => d.Month == firstDay.Month && d.Year == firstDay.Year)
                .ToHashSet() ?? new HashSet<DateTime>();

            trainingDates = trainingDates
                .Where(d => !excludedDates.Contains(d))
                .ToList();

            var addedDates = dashboard.ExclusionDates?
                .Where(e => e.Type == "Extra")
                .Select(e => e.Date.Date)
                .Where(d => d.Month == firstDay.Month && d.Year == firstDay.Year)
                .ToList() ?? new List<DateTime>();

            foreach (var date in addedDates)
            {
                if (!trainingDates.Contains(date))
                    trainingDates.Add(date);
            }
            trainingDates = trainingDates.OrderBy(x => x).ToList();

            var headers = new[] {
        "ФИО", "Долг", "Аванс", "Оплатить", "Оплачено", "Посещено"
    }.Concat(trainingDates.Select(d => d.ToString("dd.MM.yyyy"))).ToList();

            for (var col = 0; col < headers.Count; col++)
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
                    .Where(d => d.Month == firstDay.Month && d.Year == firstDay.Year)
                    .ToHashSet();

                var attendanceCount = trainingDates.Count(d => attendanceDates.Contains(d));
                worksheet.Cell(rowIdx + 2, 6).Value = attendanceCount;

                for (var colIdx = 0; colIdx < trainingDates.Count; colIdx++)
                {
                    var cell = worksheet.Cell(rowIdx + 2, 7 + colIdx);
                    cell.Value = attendanceDates.Contains(trainingDates[colIdx]) ? "+" : "";
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
