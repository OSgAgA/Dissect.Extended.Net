using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace mqtt2otel.Tests._10_UnitTests
{
    public class DissectTestCase
    {
        public string Name { get; set; } = string.Empty;

        public string Tok { get; set; } = string.Empty;

        public string Msg { get; set; } = string.Empty;

        public Dictionary<string, string> Expected { get; set; } = new();

        public bool Skip { get; set; } = false;

        public bool Fail { get; set; } = false;

        public string Append { get; set; } = string.Empty;

        public static IEnumerable<object[]> Load()
        {
            var path = Path.Combine(AppContext.BaseDirectory, "DissectTestCases.json");

            var json = File.ReadAllText(path);

            var cases = System.Text.Json.JsonSerializer.Deserialize<List<DissectTestCase>>(json,
                new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    ReadCommentHandling = System.Text.Json.JsonCommentHandling.Skip,
                    AllowTrailingCommas = true
                });

            foreach (var c in cases!)
            {
                yield return new object[] { c };
            }
        }
    }
}
