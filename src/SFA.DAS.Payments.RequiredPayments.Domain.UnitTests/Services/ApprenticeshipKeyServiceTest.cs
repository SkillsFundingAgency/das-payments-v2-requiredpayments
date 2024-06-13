using System;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using SFA.DAS.Payments.Model.Core.Entities;
using SFA.DAS.Payments.RequiredPayments.Domain.Services;

namespace SFA.DAS.Payments.RequiredPayments.Domain.UnitTests.Services
{
    [TestFixture]
    public class ApprenticeshipKeyServiceTest
    {
        [Test]
        public void ApprenticeshipKeyContainsAllElements()
        {
            // arrange
            var learnerReferenceNumber = "1";
            var ukprn = 2;
            var frameworkCode = 3;
            var pathwayCode = 4;
            var programmeType = 25;
            var standardCode = 5;
            var learnAimRef = "6";
            short academicYear = 1819;
            // act
            var key = new ApprenticeshipKeyService().GenerateApprenticeshipKey(ukprn, learnerReferenceNumber,
                frameworkCode, pathwayCode, programmeType, standardCode, learnAimRef, academicYear, ContractType.Act1);

            // assert
            ClassicAssert.AreEqual(0, key.IndexOf("2", StringComparison.Ordinal), "UKPRN should go first");
            ClassicAssert.Less(key.IndexOf("2", StringComparison.Ordinal), key.IndexOf("1", StringComparison.Ordinal),
                "LearnRefNumber should be after UKPRN");
            ClassicAssert.Less(key.IndexOf("1", StringComparison.Ordinal), key.IndexOf("3", StringComparison.Ordinal),
                "FrameworkCode should be after LearnRefNumber");
            ClassicAssert.Less(key.IndexOf("3", StringComparison.Ordinal), key.IndexOf("4", StringComparison.Ordinal),
                "PathwayCode should be after FrameworkCode");
            ClassicAssert.Less(key.IndexOf("4", StringComparison.Ordinal), key.IndexOf("25", StringComparison.Ordinal),
                "ProgrammeType should be after PathwayCode");
            ClassicAssert.Less(key.IndexOf("25", StringComparison.Ordinal), key.IndexOf("5", StringComparison.Ordinal),
                "StandardCode should be after ProgrammeType");
            ClassicAssert.Less(key.IndexOf("5", StringComparison.Ordinal), key.IndexOf("6", StringComparison.Ordinal),
                "LearnAimRef should be after StandardCode");
            ClassicAssert.Less(key.IndexOf("6", StringComparison.Ordinal), key.IndexOf("1819", StringComparison.Ordinal),
                "AcademicYear should be after LearnAimRef");
        }


        [Test]
        public void TestApprenticeshipKeyChangesCase()
        {
            // arrange
            var learnerReferenceNumber = "A";
            var ukprn = 2;
            var frameworkCode = 3;
            var pathwayCode = 4;
            var programmeType = 25;
            var standardCode = 5;
            var learnAimRef = "B";
            short academicYear = 1819;

            // act
            var key = new ApprenticeshipKeyService().GenerateApprenticeshipKey(ukprn, learnerReferenceNumber,
                frameworkCode, pathwayCode, programmeType, standardCode, learnAimRef, academicYear, ContractType.Act1);

            // assert
            ClassicAssert.IsFalse(key.Contains("A"));
            ClassicAssert.IsFalse(key.Contains("B"));
            ClassicAssert.IsTrue(key.Contains("a"));
            ClassicAssert.IsTrue(key.Contains("b"));
        }


        [Test]
        public void CanParseValidApprenticeshipKey()
        {
            // arrange
            var learnerReferenceNumber = "1";
            var ukprn = 2;
            var frameworkCode = 3;
            var pathwayCode = 4;
            var programmeType = 25;
            var standardCode = 5;
            var learnAimRef = "6";
            short academicYear = 1819;
            var service = new ApprenticeshipKeyService();
            var key = service.GenerateApprenticeshipKey(ukprn, learnerReferenceNumber, frameworkCode, pathwayCode,
                programmeType, standardCode, learnAimRef, academicYear, ContractType.Act1);

            // act
            var apprenticeshipKey = service.ParseApprenticeshipKey(key);

            // assert
            ClassicAssert.AreEqual(apprenticeshipKey.Ukprn, ukprn);
            ClassicAssert.AreEqual(apprenticeshipKey.LearnerReferenceNumber, learnerReferenceNumber);
            ClassicAssert.AreEqual(apprenticeshipKey.FrameworkCode, frameworkCode);
            ClassicAssert.AreEqual(apprenticeshipKey.PathwayCode, pathwayCode);
            ClassicAssert.AreEqual(apprenticeshipKey.ProgrammeType, programmeType);
            ClassicAssert.AreEqual(apprenticeshipKey.StandardCode, standardCode);
            ClassicAssert.AreEqual(apprenticeshipKey.LearnAimRef, learnAimRef);
            ClassicAssert.AreEqual(apprenticeshipKey.AcademicYear, academicYear);
        }

        [Test]
        public void CanParseApprenticeshipKey_With_Null_Values()
        {
            // arrange
            var learnerReferenceNumber = "1";
            var ukprn = 2;
            var frameworkCode = 3;
            var pathwayCode = 4;
            var programmeType = 25;
            var standardCode = 5;
            string learnAimRef = null;
            short academicYear = 1819;
            var service = new ApprenticeshipKeyService();
            var contractType = ContractType.Act1;
            var key = service.GenerateApprenticeshipKey(ukprn, learnerReferenceNumber, frameworkCode, pathwayCode,
                programmeType, standardCode, learnAimRef, academicYear, contractType);

            // act
            var apprenticeshipKey = service.ParseApprenticeshipKey(key);

            // assert
            ClassicAssert.AreEqual(apprenticeshipKey.Ukprn, ukprn);
            ClassicAssert.AreEqual(apprenticeshipKey.LearnerReferenceNumber, learnerReferenceNumber);
            ClassicAssert.AreEqual(apprenticeshipKey.FrameworkCode, frameworkCode);
            ClassicAssert.AreEqual(apprenticeshipKey.PathwayCode, pathwayCode);
            ClassicAssert.AreEqual(apprenticeshipKey.ProgrammeType, programmeType);
            ClassicAssert.AreEqual(apprenticeshipKey.StandardCode, standardCode);
            ClassicAssert.AreEqual(apprenticeshipKey.LearnAimRef, string.Empty);
            ClassicAssert.AreEqual(apprenticeshipKey.AcademicYear, academicYear);
            ClassicAssert.AreEqual(apprenticeshipKey.ContractType, contractType);
        }
    }
}