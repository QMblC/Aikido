namespace Aikido.Dto
{
    public class EnumItemDto
    {
        public string Value { get; set; }
        public string Name { get; set; }

        public EnumItemDto(string value, string name)
        {
            Value = value;
            Name = name;
        }
    }
}
