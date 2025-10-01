using Aikido.Dto;
using System.Text.Json;

public class GroupRequest
{
    public string? JsonData { get; set; }

    public async Task<GroupDto> Parse()
    {
        if (string.IsNullOrEmpty(JsonData))
            throw new ArgumentException("Данные запроса пусты");

        return await Task.Run(() => JsonSerializer.Deserialize<GroupDto>(JsonData)
            ?? throw new ArgumentException("Ошибка при десериализации данных"));
    }
}
