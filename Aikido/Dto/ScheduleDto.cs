namespace Aikido.Dto
{
    public class ScheduleDto : DtoBase
    {
        //ToDo

        public long? Id { get; set; }
        public Dictionary<string, string> Schedule { get; set; }
        public ExclusionDateDto ExclusionDate { get; set; }
    }
}
