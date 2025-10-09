using Aikido.Dto.Seminars;
using System.Text.Json;

namespace Aikido.Requests
{
    public class SeminarRequest
    {
        public SeminarDto SeminarData { get; set; }
        public List<SeminarScheduleDto>? Schedule { get; set; }
        public List<SeminarContactInfoDto>? ContactInfo { get; set; }
        public List<SeminarGroupDto>? Groups { get; set; }
        public IFormFile? PdfFile { get; set; }
    }
}

