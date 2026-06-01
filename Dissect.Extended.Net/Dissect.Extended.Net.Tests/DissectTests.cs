using Dissect.Extended.Net.Library;
using Newtonsoft.Json.Linq;
using System.Globalization;
using Xunit.Abstractions;

namespace mqtt2otel.Tests._10_UnitTests
{
    public class DissectTests
    {
        private readonly ITestOutputHelper _output;

        public DissectTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Theory]
        [InlineData("this is a test", "%{+a} %{+a} %{+a} %{+a}", "a", "thisisatest")]
        [InlineData("this is a test", "%{+a} %{+a/1} %{+a/3} %{+a/2}", "a", "thisistesta")]
        public void ShouldParsePartialStringValues(string input, string pattern, string expectedKey, string expectedResult)
        {
            var parser = new Parser(pattern);
            var result = parser.Parse(input);

            Assert.True(result.Success);
            var resultDict = result.ToDictionary();
            Assert.Single(resultDict);
            Assert.True(resultDict.ContainsKey(expectedKey));
            Assert.Equal(expectedResult, resultDict[expectedKey]);
        }

        [Theory]
        [InlineData("this is a test", "%{+[,]a} %{+a}", "a", "this,is a test")]
        [InlineData("this is a test", "%{+a} %{+a}", "a", "this*is a test")]
        [InlineData("this is a test", "%{+[...]a} %{+a}", "a", "this...is a test")]
        [InlineData("this is a test", "%{+[]a} %{+a}", "a", "thisis a test")]
        [InlineData("this is a test", "%{+[+]a} %{+[-]a} %{a} %{a}", "a", "this+is-a-test")]
        public void ShouldUseCorrectSeparatorForAppendModifier(string input, string pattern, string expectedKey, string expectedResult)
        {
            var parser = new Parser(pattern, separator: "*");
            var result = parser.Parse(input);

            Assert.True(result.Success);
            var resultDict = result.ToDictionary();
            Assert.Single(resultDict);
            Assert.True(resultDict.ContainsKey(expectedKey));
            Assert.Equal(expectedResult, resultDict[expectedKey]);
        }

        [Theory]
        [InlineData("this is a test", "%{} %{} %{} %{a}", "a", "test")]
        [InlineData("this is a test", "%{?} %{?} %{?} %{a}", "a", "test")]
        public void ShouldParseSkipKeys(string input, string pattern, string expectedKey, string expectedResult)
        {
            var parser = new Parser(pattern);
            var result = parser.Parse(input);

            Assert.True(result.Success);
            var resultDict = result.ToDictionary();
            Assert.Single(resultDict);
            Assert.True(resultDict.ContainsKey(expectedKey));
            Assert.Equal(expectedResult, resultDict[expectedKey]);
        }

        [Fact]
        public void ShouldParseKeyAndValue()
        {
            string pattern = "%{*a} %{&a}";
            string input = "key value";
            var parser = new Parser(pattern);
            var result = parser.Parse(input);

            Assert.True(result.Success);
            var resultDict = result.ToDictionary();
            Assert.Single(resultDict);
            Assert.True(resultDict.ContainsKey("key"));
            Assert.Equal("value", resultDict["key"]);
        }

        [Fact]
        public void ShouldParseDateTimeFromReferenceModifier()
        {
            string pattern = "%{*a} %{&a:DateTime}";
            string input = "StartOfYear 2026-01-01";
            var parser = new Parser(pattern);
            var result = parser.Parse(input);

            Assert.True(result.Success);
            var resultDict = result.ToDictionary();
            Assert.Single(resultDict);
            Assert.True(resultDict.ContainsKey("StartOfYear"));
            Assert.IsType<DateTime>(resultDict["StartOfYear"]);
            Assert.Equal(new DateTime(2026,1,1), resultDict["StartOfYear"]);
        }

        [Theory]
        [InlineData("this is a test", "%{a} %{left->} %{right}", "is", "a test")]
        [InlineData("this is  a test", "%{a} %{left->} %{right}", "is", "a test")]
        [InlineData("this is   a test", "%{a} %{left->} %{right}", "is", "a test")]
        [InlineData("this is,,a test", "%{a} %{left->},%{right}", "is", "a test")]
        [InlineData("this is.:.:.:a test", "%{a} %{left->}.:%{right}", "is", "a test")]
        public void ShouldParseRightPadding(string input, string pattern, string expectedLeft, string expectedRight)
        {
            var parser = new Parser(pattern);
            var result = parser.Parse(input);

            Assert.True(result.Success);
            var resultDict = result.ToDictionary();
            Assert.Equal(3, resultDict.Count);
            Assert.True(resultDict.ContainsKey("left"));
            Assert.True(resultDict.ContainsKey("right"));
            Assert.Equal(expectedLeft, resultDict["left"]);
            Assert.Equal(expectedRight, resultDict["right"]);
        }

        [Fact]
        public void ShouldParseDateTime()
        {
            var expected = DateTime.SpecifyKind(new DateTime(2026, 5, 1, 19, 26, 12), DateTimeKind.Utc);
            var parser = new Parser("%{value:DateTime}");
            var result = parser.Parse("2026-05-01 19:26:12");

            Assert.True(result.Success);
            var resultDict = result.ToDictionary();
            Assert.Single(resultDict);
            Assert.True(resultDict.ContainsKey("value"));
            Assert.IsType<DateTime>(resultDict["value"]);
            Assert.Equal(expected, resultDict["value"]);
        }

        [Fact]
        public void ShouldParseDateTimeWithExplicitTimeZone()
        {
            var expected = DateTime.SpecifyKind(new DateTime(2026, 5, 1, 17, 26, 12), DateTimeKind.Utc);
            var parser = new Parser("%{value:DateTime[Europe/Berlin]}");
            var result = parser.Parse("2026-05-01 19:26:12");

            Assert.True(result.Success);
            var resultDict = result.ToDictionary();
            Assert.Single(resultDict);
            Assert.True(resultDict.ContainsKey("value"));
            Assert.IsType<DateTime>(resultDict["value"]);
            Assert.Equal(expected, resultDict["value"]);
        }

        [Fact]
        public void ShouldParseDateTimeWithLocalTimeZone()
        {
            var expected = new DateTime(2026, 5, 1, 19, 26, 12).ToUniversalTime();
            var parser = new Parser("%{value:DateTime[local]}");
            var result = parser.Parse("2026-05-01 19:26:12");

            Assert.True(result.Success);
            var resultDict = result.ToDictionary();
            Assert.Single(resultDict);
            Assert.True(resultDict.ContainsKey("value"));
            Assert.IsType<DateTime>(resultDict["value"]);
            Assert.Equal(expected, resultDict["value"]);
        }

        [Fact]
        public void ShouldParseDateTimeFromAppendModifier()
        {
            var expected = new DateTime(2026, 5, 1, 19, 26, 12);
            var parser = new Parser("%{value:DateTime} hello world %{+value:DateTime}", separator: " ");
            var result = parser.Parse("2026-05-01 hello world 19:26:12");

            Assert.True(result.Success);
            var resultDict = result.ToDictionary();
            Assert.Single(resultDict);
            Assert.True(resultDict.ContainsKey("value"));
            Assert.IsType<DateTime>(resultDict["value"]);
            Assert.Equal(expected, resultDict["value"]);
        }

        /// <summary>
        /// Tests the tests as specified in: https://github.com/elastic/dissect-specification
        /// </summary>
        /// <param name="test">The test case from the json file.</param>
        [Theory]
        [MemberData(nameof(DissectTestCase.Load), MemberType = typeof(DissectTestCase))]
        public void ShouldPassSpecificationTests(DissectTestCase test)
        {
            _output.WriteLine($"Running test '{test.Name}'");
            _output.WriteLine($"  Parameter tok: '{test.Tok}'");
            _output.WriteLine($"  Parameter msg: '{test.Msg}'");
            string expected = "<<empty>>";
            if (test.Expected != null && test.Expected.Count > 0)
            {
                expected = string.Join(", ", test.Expected.Select(kv => $"{kv.Key}={kv.Value}"));
            }
            _output.WriteLine($"  Parameter expected: '{expected}'");
            _output.WriteLine($"  Parameter append: '{test.Append}'");
            _output.WriteLine($"  Parameter fail: '{test.Fail}'");

            var parser = new Parser(test.Tok, separator: test.Append);
            var result = parser.Parse(test.Msg);

            if (test.Fail)
            {
                Assert.False(result.Success, $"{test.Name}: Is success.");
                return;
            }

            Assert.True(result.Success, $"{test.Name}: Is success.");

            var dict = result.ToDictionary();
            Assert.NotNull(test.Expected);
            Assert.Equal(test.Expected.Count, dict.Count);

            foreach (var kv in test.Expected)
            {
                Assert.True(dict.ContainsKey(kv.Key), $"{test.Name}: Missing key '{kv.Key}'");
                Assert.Equal(kv.Value, dict[kv.Key]);
            }
        }

        [Theory]
        [InlineData("%{a", "missing closing }.")]
        [InlineData("%{a?}", "Usage of ? in key name.")]
        [InlineData("%{a/2-->:hallo}", "Modifier --> must be at end of key.")]
        [InlineData("%{a:hallo}", "Usage of illegal data type hello.")]
        [InlineData("%{a:DateTime[non exisiting timezone]}", "Usage of non exsiting timezone.")]
        [InlineData("%{a:DateTime[utc}", "Missing ] for datatype parameter.")]
        [InlineData("%{a:float[non existing culture}", "Float data type with non existing culture identifier.")]
        [InlineData("%{a:decimal[non existing culture}", "Decimal data type with non existing culture identifier.")]
        [InlineData("%{a:double[non existing culture}", "Double data type with non existing culture identifier.")]
        public void ShouldFailParsingPattern(string pattern, string description)
        {
            var parser = new Parser(pattern);

            Assert.False(parser.IsValid, description);
        }

        [Theory]
        [InlineData("%{a:string}", "hello world", typeof(string), "hello world")]
        [InlineData("%{a:int}", "123", typeof(int), 123)]
        [InlineData("%{a:double}", "123.45", typeof(double), 123.45)]
        [InlineData("%{a:double[DE-de]}", "123,45", typeof(double), 123.45)]
        [InlineData("%{a:float}", "123.45", typeof(float), 123.45f)]
        [InlineData("%{a:float[De-de]}", "123,45", typeof(float), 123.45f)]
        [InlineData("%{a:String}", "hello world", typeof(string), "hello world")]
        [InlineData("%{a:INT}", "123", typeof(int), 123)]
        [InlineData("%{a:dOuBlE}", "123.45", typeof(double), 123.45)]
        public void ShouldReturnCorrectDataType(string pattern, string input, Type type, object expectedResult)
        {
            var parser = new Parser(pattern);
            Assert.True(parser.IsValid, $"Parsing pattern {pattern} is not valid.");
            var result = parser.Parse(input);

            Assert.True(result.Success, $"Could not successfully parse input: '{input}'");
            var resultDict = result.ToDictionary();
            Assert.Single(resultDict);
            Assert.True(resultDict.ContainsKey("a"));
            Assert.IsType(type, resultDict["a"]);
            Assert.Equal(expectedResult, resultDict["a"]);
        }

        [Theory]
        [InlineData("%{+a/2:int} %{+a/1:int}")]
        [InlineData("%{+a:int/2} %{+a:int/1}")]
        [InlineData("%{+a:int/2} %{+a/1:int}")]
        public void ShouldParseModifierInArbitraryOrder(string pattern)
        {
            var parser = new Parser(pattern);
            var result = parser.Parse("2 1");

            Assert.True(result.Success);
            var resultDict = result.ToDictionary();
            Assert.Single(resultDict);
            Assert.True(resultDict.ContainsKey("a"));
            Assert.Equal(12, resultDict["a"]);
        }

        [Theory]
        [InlineData("%{a:double[DE-de]", "123.45", "Using culture DE-de with input 123.45.")]
        [InlineData("%{a:double", "123,45", "Using default culture with input 123,45.")]
        [InlineData("%{a:dateTime", "Hello world", "Using DateTime with input Hello world.")]
        [InlineData("%{a:int", "Hello world", "Using int with input Hello world.")]
        public void ShouldFailInputParsing(string pattern, string input, string description)
        {
            var parser = new Parser(pattern);
            var result = parser.Parse(input);

            Assert.False(result.Success, description);
        }

        [Fact]
        public void JustATest()
        {
            var parser = new Parser("%{timestamp:DateTime[Europe/Berlin]} [%{log_level}] %{message}");

            if (!parser.IsValid) throw new Exception("invalid parse expression");

            var result = parser.Parse("2026-05-31 19:20:41 [Info] Successfully connected to server.");

            if (result.Success)
            {
                foreach (var keyValue in result.ToDictionary())
                {
                    string text = $"key: {keyValue.Key}, value: {keyValue.Value}";
                }
            }
        }
    }
}
