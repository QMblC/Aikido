using Aikido.Dto;
using System.Text.Json;

namespace Aikido.Requests
{
    public class ClubRequest
    {
        public IFormFile ClubDataJson { get; set; }

        public async Task<ClubDto> Parse()
        {

            return await JsonHelper.DeserializeJsonFormFileAsync<ClubDto>(ClubDataJson);
            
        }
    }
}
