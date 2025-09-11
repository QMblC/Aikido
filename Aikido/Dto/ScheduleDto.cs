namespace Aikido.Dto
{
    public class ScheduleDto : DtoBase
    {
        //ToDo

        public Dictionary<string, string> Schedule { get; set; }
        public ExclusionDateDto ExclusionDate { get; set; }
    }
}
