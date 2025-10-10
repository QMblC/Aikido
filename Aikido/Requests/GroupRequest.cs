using Aikido.Dto;
using System.Text.Json;

public class GroupRequest
{
    public IFormFile GroupDataJson { get; set; }

    public async Task<GroupDto> Parse()
    {
        if (GroupDataJson == null || GroupDataJson.Length == 0)
            throw new ArgumentException("Файл не загружен или пуст");

        using var stream = GroupDataJson.OpenReadStream();
        using var reader = new StreamReader(stream);
        var json = await reader.ReadToEndAsync();

        return JsonSerializer.Deserialize<GroupDto>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true 
        })
        ?? throw new ArgumentException("Ошибка при десериализации данных");
    }
}


