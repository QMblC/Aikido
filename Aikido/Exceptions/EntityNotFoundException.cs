namespace Aikido.Exceptions
{
    public class EntityNotFoundException : Exception
    {
        public EntityNotFoundException(string entityName, long id)
        : base($"Сущность {entityName} с Id = {id} не найдена.") { }

        public EntityNotFoundException(string entityName)
        : base($"Одна или несколько сущностей {entityName} не найдена.") { }
    }
}
