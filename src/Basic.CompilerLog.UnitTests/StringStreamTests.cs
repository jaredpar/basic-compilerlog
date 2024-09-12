using System.Text;
using Basic.CompilerLog.Util;
using Xunit;
using Xunit.Abstractions;

namespace Basic.CompilerLog.UnitTests;

public class StringStreamTests
{
    private static readonly Encoding[] Encodings =
    [
        Encoding.UTF8,
        Encoding.UTF32
    ];

    private void RoundTripByteByByte(string input)
    {
        foreach (var encoding in Encodings)
        {
            using var inputStream = new StringStream(input, encoding);
            using var memoryStream = new MemoryStream();
            while (inputStream.ReadByte() is int b && b != -1)
            {
                memoryStream.WriteByte((byte)b);
            }

            memoryStream.Position = 0;
            var actual = encoding.GetString(memoryStream.ToArray());
            Assert.Equal(input, actual);
        }
    }

    private void RoundTripCopy(string input)
    {
        foreach (var encoding in Encodings)
        {
            using var inputStream = new StringStream(input, encoding);
            using var memoryStream = new MemoryStream();
            inputStream.CopyTo(memoryStream);

            memoryStream.Position = 0;
            var actual = encoding.GetString(memoryStream.ToArray());
            Assert.Equal(input, actual);
        }
    }

    private void RoundTripReset(string input)
    {
        foreach (var encoding in Encodings)
        {
            using var inputStream = new StringStream(input, encoding);
            using var memoryStream = new MemoryStream();
            inputStream.Position = 0;
            memoryStream.Position = 0;
            inputStream.CopyTo(memoryStream);

            memoryStream.Position = 0;
            var actual = encoding.GetString(memoryStream.ToArray());
            Assert.Equal(input, actual);
        }
    }

    private void RoundTripAll(string input)
    {
        RoundTripByteByByte(input);
        RoundTripCopy(input);
        RoundTripReset(input);
    }

    [Theory]
    [InlineData("Hello, world!")]
    [InlineData("")]
    [InlineData("lets try this value")]
    public void RoundTrip(string input) => RoundTripAll(input);

    [Fact]
    public void RoundTripGenerated()
    {
        RoundTripAll(new string('a', 1000));
        RoundTripAll(new string('a', 10_000));
    }
}