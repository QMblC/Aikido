namespace Aikido.Dto.FormerCertifications
{
    public interface IFormerCertificationCreationDto
    {
        public DateTime Date { get; set; }

        public string CertificationGrade { get; set; }
    }
}
