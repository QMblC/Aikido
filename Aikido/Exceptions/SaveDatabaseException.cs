namespace Aikido.Exceptions
{
    public class SaveDatabaseException : Exception
    {
        public SaveDatabaseException(string className, string exceptionMessage)
            : base($"Ошибка при обработке {className}: {exceptionMessage}") { }
    }
}
