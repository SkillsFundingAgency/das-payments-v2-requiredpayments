using System;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using SFA.DAS.Payments.RequiredPayments.Domain.Services;

namespace SFA.DAS.Payments.RequiredPayments.Domain.UnitTests.Services
{
    [TestFixture]
    public class PaymentKeyServiceTests
    {
        [Test]
        public void PaymentKeyContainsAllElements()
        {
            // arrange
            var learnAimRef = "6";
            var transactionType = 3;
            byte deliveryPeriod = 5;
            short academicYear = 1516;

            // act
            var key = new PaymentKeyService().GeneratePaymentKey(learnAimRef, transactionType, academicYear, deliveryPeriod);

            // assert
            ClassicAssert.AreEqual(0, key.IndexOf("6", StringComparison.Ordinal), "LearnAimRef should go first");
            ClassicAssert.Less(key.IndexOf("6", StringComparison.Ordinal), key.IndexOf("3", StringComparison.Ordinal), "TransactionType should be after LearnAimRef");
            ClassicAssert.Less(key.IndexOf("3", StringComparison.Ordinal), key.IndexOf("5", StringComparison.Ordinal), "DeliveryPeriod should be after TransactionType");
        }

        [Test]
        public void TestPaymentKeyChangesCase()
        {
            // arrange
            var learnAimRef = "B";
            var transactionType = 3;
            byte deliveryPeriod = 5;
            short academicYear = 1718;

            // act
            var key = new PaymentKeyService().GeneratePaymentKey(learnAimRef, transactionType, academicYear, deliveryPeriod);

            // assert
            ClassicAssert.IsFalse(key.Contains("B"));
            ClassicAssert.IsTrue(key.Contains("b"));
        }
    }
}