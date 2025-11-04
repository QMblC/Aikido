using Aikido.AdditionalData;
using Aikido.Data;
using Aikido.Dto;
using Aikido.Dto.Seminars.Members;
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

            // Количество столбцов — 15 (N)
            var colCount = 15;

            // 1. Первая строка: Название по центру, жирным, крупно
            worksheet.Cell(1, 1).Value = $"Ведомость на {members.First().SeminarName}";
            worksheet.Range(1, 1, 1, colCount).Merge().Style
                .Font.SetBold().Font.SetFontSize(18).Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            // 2. Вторая строка: Дата по центру, мелко
            worksheet.Cell(2, 1).Value = $"{members.First().SeminarDate:dd MMMM yyyy}";
            worksheet.Range(2, 1, 2, colCount).Merge().Style
                .Font.SetBold(false).Font.SetFontSize(12).Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            // 3. Пропуск двух строк (3, 4)
            worksheet.Range(3, 1, 4, colCount).Style.Fill.SetBackgroundColor(XLColor.White);

            // 4. Первая строка шапки (объединённые области, заливка)
            worksheet.Cell(5, 1).Value = ""; // №
            worksheet.Cell(5, 2).Value = ""; // userId
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

            // 5. Вторая строка шапки — обычные заголовки
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

            // 6. Данные участников — с userId во втором столбце
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
                worksheet.Cell(rowNum, 6).Value = m.CreatorFullName ?? ""; // Тренер
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

            // 7. Итоговая строка
            int startDataRow = 7;
            int endDataRow = rowNum - 1; // последняя строка с данными перед итогом

            // Задаём формулы для сумм
            worksheet.Cell(rowNum, 11).FormulaA1 = $"SUM(K{startDataRow}:K{endDataRow})";
            worksheet.Cell(rowNum, 12).FormulaA1 = $"SUM(L{startDataRow}:L{endDataRow})";
            worksheet.Cell(rowNum, 13).FormulaA1 = $"SUM(M{startDataRow}:M{endDataRow})";
            worksheet.Cell(rowNum, 14).FormulaA1 = $"SUM(N{startDataRow}:N{endDataRow})";

            // Общая сумма
            worksheet.Cell(rowNum, 15).FormulaA1 = $"SUM(K{rowNum}:N{rowNum})";


            worksheet.Range(rowNum, 1, rowNum, colCount).Style.Font.SetBold();
            worksheet.Range(rowNum, 1, rowNum, colCount).Style.Fill.SetBackgroundColor(XLColor.LightGray);
            worksheet.Range(rowNum, 1, rowNum, colCount).Style.Border.OutsideBorder = XLBorderStyleValues.Thick;

            // 8.  Минимальная ширина userId столбца
            worksheet.Column(2).Width = 4;

            // 9. Автоподбор
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

                // Считываем таблицу начиная с 7 строки (первая строки с данными)
                var rowNum = 7;
                while (true)
                {
                    var row = worksheet.Row(rowNum);

                    // Если строка пустая — выход
                    if (row.Cell(2).IsEmpty())
                        break;

                    // Парсим поля (с учётом порядка, как в выводе)
                    var member = new SeminarMemberDto
                    {
                        UserId = row.Cell(2).GetValue<long>(),
                        UserFullName = row.Cell(3).GetString(),
                        OldGrade = EnumParser.ConvertEnumToString(EnumParser.GetEnumByMemberValue<Grade>(row.Cell(4).GetString())),
                        CertificationGrade = EnumParser.ConvertEnumToString(EnumParser.GetEnumByMemberValue<Grade>(row.Cell(5).GetString())),
                        CreatorFullName = row.Cell(6).GetString(),
                        // Если структура SeminarMemberDto изменилась — замени поля ниже:
                        ClubName = row.Cell(7).GetString(),
                        ClubCity = row.Cell(8).GetString(),
                        AgeGroup = row.Cell(9).GetString(),
                        // Программа можно вывести, если нужна: row.Cell(10).GetString(),
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

        private decimal? GetDecimalOrNull(IXLCell cell)
        {
            // Возвращает null если ячейка пустая, иначе парсит число
            return cell.IsEmpty() ? (decimal?)null : decimal.Parse(cell.GetString());
        }
    }
}
