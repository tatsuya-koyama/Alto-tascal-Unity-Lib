using System;
using NUnit.Framework;

namespace AltoLib.Tests
{
    public class DigestUtilTest
    {
        [Test]
        public void TestGetMD5()
        {
            Assert.That(DigestUtil.GetMD5(""), Is.EqualTo("d41d8cd98f00b204e9800998ecf8427e"));
            Assert.That(DigestUtil.GetMD5("hoge"), Is.EqualTo("ea703e7aa1efda0064eaa507d9e8ab7e"));
        }

        [Test]
        public void TestGetMD5AsBase64()
        {
            Assert.That(DigestUtil.GetMD5AsBase64(""), Is.EqualTo("1B2M2Y8AsgTpgAmY7PhCfg=="));
            Assert.That(DigestUtil.GetMD5AsBase64("hoge"), Is.EqualTo("6nA+eqHv2gBk6qUH2eirfg=="));
        }
    }
}
