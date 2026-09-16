using System;
using NUnit.Framework;

namespace AltoLib.Tests
{
    public class MasterDataCsvParserTest
    {
        [Test]
        public void Parse_SimpleCsv_UnifiesLineBreaks()
        {
            string source = "id,value,text\r\n1001,100,hoge\r\n";

            string result = MasterDataCsvParser.Parse(source);

            Assert.AreEqual("id,value,text\n1001,100,hoge\n", result);
        }

        [Test]
        public void Parse_QuotedValues_EscapesCommaAndLineBreak()
        {
            string source = "id,text,note\n1,\"hello,world\",\"line1\nline2\"\n";

            string result = MasterDataCsvParser.Parse(source);

            Assert.AreEqual("id,text,note\n1,hello<comma>world,line1<br>line2\n", result);
        }

        [Test]
        public void Parse_ShortRow_PadsCellsToHeaderCount()
        {
            string source = "id,value,text\n1001,100\n";

            string result = MasterDataCsvParser.Parse(source);

            Assert.AreEqual("id,value,text\n1001,100,\n", result);
        }

        [Test]
        public void Parse_UnterminatedQuotedValue_ThrowsFormatException()
        {
            string source = "id,text\n1,\"unterminated\n";

            Assert.Throws<FormatException>(() => MasterDataCsvParser.Parse(source));
        }
    }
}
