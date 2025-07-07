namespace Aikido.Entities.Filters
{
    public class UserFilter
    {
        public List<string>? Roles { get; set; }
        public List<string>? Cities { get; set; }
        public List<string>? Grades { get; set; }
        public List<long>? ClubIds { get; set; }
        public List<long>? GroupIds { get; set; }
        public List<string>? Sexes { get; set; }
        public string? Name { get; set; }
    }
}
