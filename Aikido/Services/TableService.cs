using Aikido.Data;
using Aikido.Dto;
using Aikido.Entities;
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
                .Include(u => u.UserClubs)
                    .ThenInclude(uc => uc.Club)
                .Include(u => u.UserGroups)
                    .ThenInclude(ug => ug.Group)
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

            // Заголовки
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

            // Данные
            foreach (var user in users)
            {
                var clubNames = string.Join(", ",
                    user.UserClubs.Where(uc => uc.IsActive && uc.Club != null)
                                  .Select(uc => uc.Club!.Name));

                var groupNames = string.Join(", ",
                    user.UserGroups.Where(ug => ug.IsActive && ug.Group != null)
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
            document.Close();

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

            // Заголовки шаблона
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
            document.Close();

            stream.Position = 0;
            return stream;
        }

        public async Task<MemoryStream> ExportSeminarsToExcelAsync()
        {
            var seminars = await _context.Seminars
                .Include(s => s.Instructor)
                .Include(s => s.SeminarMembers)
                    .ThenInclude(sm => sm.User)
                .Where(s => s.IsActive)
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

            // Заголовки
            var headerRow = new Row();
            headerRow.Append(
                new Cell { CellValue = new CellValue("Id"), DataType = CellValues.String },
                new Cell { CellValue = new CellValue("Name"), DataType = CellValues.String },
                new Cell { CellValue = new CellValue("StartDate"), DataType = CellValues.String },
                new Cell { CellValue = new CellValue("EndDate"), DataType = CellValues.String },
                new Cell { CellValue = new CellValue("Location"), DataType = CellValues.String },
                new Cell { CellValue = new CellValue("Instructor"), DataType = CellValues.String },
                new Cell { CellValue = new CellValue("Cost"), DataType = CellValues.String },
                new Cell { CellValue = new CellValue("Participants"), DataType = CellValues.String }
            );
            sheetData.AppendChild(headerRow);

            // Данные
            foreach (var seminar in seminars)
            {
                var row = new Row();
                row.Append(
                    new Cell { CellValue = new CellValue(seminar.Id.ToString()), DataType = CellValues.String },
                    new Cell { CellValue = new CellValue(seminar.Name), DataType = CellValues.String },
                    new Cell { CellValue = new CellValue(seminar.StartDate.ToString("yyyy-MM-dd")), DataType = CellValues.String },
                    new Cell { CellValue = new CellValue(seminar.EndDate.ToString("yyyy-MM-dd")), DataType = CellValues.String },
                    new Cell { CellValue = new CellValue(seminar.Location ?? ""), DataType = CellValues.String },
                    new Cell { CellValue = new CellValue(seminar.Instructor?.FullName ?? ""), DataType = CellValues.String },
                    new Cell { CellValue = new CellValue(seminar.Cost?.ToString() ?? ""), DataType = CellValues.String },
                    new Cell { CellValue = new CellValue(seminar.CurrentParticipants.ToString()), DataType = CellValues.String }
                );
                sheetData.AppendChild(row);
            }

            workbookPart.Workbook.Save();
            document.Close();

            stream.Position = 0;
            return stream;
        }
    }
}
