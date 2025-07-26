namespace Aikido.Requests
{
    public class TableRequest
    {
        public IFormFile File { get; set; }

        public async Task<byte[]> Parse()
        {
            if (File == null || File.Length == 0)
                throw new Exception("Файл не передан или пуст.");

            byte[] table;
            using (var memoryStream = new MemoryStream())
            {
                await File.CopyToAsync(memoryStream);
                table = memoryStream.ToArray();
            }
            return table;
        }
    }
}
