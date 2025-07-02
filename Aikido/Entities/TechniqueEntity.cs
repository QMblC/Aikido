using System.ComponentModel.DataAnnotations;

namespace Aikido.Entities
{
    public class TechniqueEntity : IDbEntity
    {
        [Key]
        public long Id { get; set; }
        public string Name { get; set; }
        public string DescriptionTechnique { get; set; }
        public byte[] Video { get; set; } = [];
    }
}
