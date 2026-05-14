using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Aikido.Services
{


    public class TimeSpanConverter : JsonConverter<TimeSpan>
    {
        private const string Format = @"hh\:mm";

        public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();

            if (TimeSpan.TryParseExact(value, Format, CultureInfo.InvariantCulture, out var ts))
                return ts;

            return TimeSpan.Parse(value, CultureInfo.InvariantCulture);
        }

        public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(Format));
        }
    }
}
