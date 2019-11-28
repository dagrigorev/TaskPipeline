using NUnit.Framework;

namespace TestSignatuerKey
{
    using System;
    using Pipeline;

    public class TestSignatureKey
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestGetSignature()
        {
            Assert.IsNotNull(SignatureKey.GetSignature(FirstMethod));
        }

        [Test]
        public void TestSignatureEquality()
        {
            Assert.AreEqual(SignatureKey.GetSignature(FirstMethod), SignatureKey.GetSignature(FirstMethod));
        }

        [Test]
        public void TestSignatureNonEquality()
        {
            Assert.AreNotEqual(SignatureKey.GetSignature(FirstMethod), SignatureKey.GetSignature(ThirdMethod));
        }

        private object ThirdMethod()
        {
            throw new NotImplementedException();
        }

        public void FirstMethod() { }
        public void SecondMethod(int a) { }
        public void ThirdMethod(int a, string b) { }
        public bool FourthMethod() => false;
        public bool FifthMethod(object t) => false;
    }
}