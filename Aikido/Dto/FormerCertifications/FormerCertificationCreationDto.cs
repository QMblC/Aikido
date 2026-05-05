
namespace Aikido.Dto.FormerCertifications
{
    public class FormerCertificationCreationDto : IFormerCertificationCreationDto
    {
        public DateTime Date { get; set; }
        public string CertificationGrade { get; set; }
    }
}
