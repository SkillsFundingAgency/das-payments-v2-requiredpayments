﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (http://www.specflow.org/).
//      SpecFlow Version:1.9.0.77
//      SpecFlow Generator Version:1.9.0.0
//      Runtime Version:4.0.30319.42000
// 
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------
#region Designer generated code
#pragma warning disable
namespace SFA.DAS.Payments.RequiredPayments.AcceptanceTests.Tests.Minimum
{
    using TechTalk.SpecFlow;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "1.9.0.77")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [NUnit.Framework.TestFixtureAttribute()]
    [NUnit.Framework.DescriptionAttribute("Contract Type Changes From ACT2 To ACT1")]
    public partial class ContractTypeChangesFromACT2ToACT1Feature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "ContractTypeChangesFromAct2ToAct1.feature"
#line hidden
        
        [NUnit.Framework.TestFixtureSetUpAttribute()]
        public virtual void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Contract Type Changes From ACT2 To ACT1", "DPP_965_01 - Levy apprentice, provider edits contract type (ACT) in the ILR, prev" +
                    "ious on-programme and English/math payments are refunded and repaid according to" +
                    " latest contract type", ProgrammingLanguage.CSharp, ((string[])(null)));
            testRunner.OnFeatureStart(featureInfo);
        }
        
        [NUnit.Framework.TestFixtureTearDownAttribute()]
        public virtual void FeatureTearDown()
        {
            testRunner.OnFeatureEnd();
            testRunner = null;
        }
        
        [NUnit.Framework.SetUpAttribute()]
        public virtual void TestInitialize()
        {
        }
        
        [NUnit.Framework.TearDownAttribute()]
        public virtual void ScenarioTearDown()
        {
            testRunner.OnScenarioEnd();
        }
        
        public virtual void ScenarioSetup(TechTalk.SpecFlow.ScenarioInfo scenarioInfo)
        {
            testRunner.OnScenarioStart(scenarioInfo);
        }
        
        public virtual void ScenarioCleanup()
        {
            testRunner.CollectScenarioErrors();
        }
        
        public virtual void FeatureBackground()
        {
#line 3
#line 4
 testRunner.Given("the current processing period is 3", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
            TechTalk.SpecFlow.Table table1 = new TechTalk.SpecFlow.Table(new string[] {
                        "LearnRefNumber",
                        "Ukprn",
                        "ULN"});
            table1.AddRow(new string[] {
                        "learnref1",
                        "10000",
                        "10000"});
#line 6
 testRunner.And("the following learners:", ((string)(null)), table1, "And ");
#line hidden
            TechTalk.SpecFlow.Table table2 = new TechTalk.SpecFlow.Table(new string[] {
                        "LearnRefNumber",
                        "Ukprn",
                        "ULN",
                        "AimSeqNumber",
                        "ProgrammeType",
                        "FrameworkCode",
                        "PathwayCode",
                        "StandardCode",
                        "SfaContributionPercentage",
                        "FundingLineType",
                        "LearnAimRef",
                        "LearningStartDate"});
            table2.AddRow(new string[] {
                        "learnref1",
                        "10000",
                        "10000",
                        "1",
                        "2",
                        "403",
                        "1",
                        "",
                        "0.90000",
                        "16-18 Apprenticeship (From May 2017) Non-Levy Contract (non-procured)",
                        "ZPROG001",
                        "06/08/2017"});
#line 10
 testRunner.And("the following course information:", ((string)(null)), table2, "And ");
#line hidden
            TechTalk.SpecFlow.Table table3 = new TechTalk.SpecFlow.Table(new string[] {
                        "LearnRefNumber",
                        "Ukprn",
                        "PriceEpisodeIdentifier",
                        "Period",
                        "ULN",
                        "SfaContributionPercentage",
                        "Learning_1",
                        "Completion_2",
                        "Balancing_3",
                        "First16To18EmployerIncentive_4",
                        "First16To18ProviderIncentive_5",
                        "Second16To18EmployerIncentive_6",
                        "Second16To18ProviderIncentive_7",
                        "OnProgramme16To18FrameworkUplift_8",
                        "Completion16To18FrameworkUplift_9",
                        "Balancing16To18FrameworkUplift_10",
                        "FirstDisadvantagePayment_11",
                        "SecondDisadvantagePayment_12",
                        "OnProgrammeMathsAndEnglish_13",
                        "BalancingMathsAndEnglish_14",
                        "LearningSupport_15"});
            table3.AddRow(new string[] {
                        "learnref1",
                        "10000",
                        "p1",
                        "1",
                        "10000",
                        "0.90000",
                        "600",
                        "0",
                        "",
                        "0",
                        "0",
                        "0",
                        "0",
                        "0",
                        "0",
                        "0",
                        "0",
                        "0",
                        "0",
                        "0",
                        "0"});
            table3.AddRow(new string[] {
                        "learnref1",
                        "10000",
                        "p1",
                        "2",
                        "10000",
                        "0.90000",
                        "600",
                        "0",
                        "",
                        "0",
                        "0",
                        "0",
                        "0",
                        "0",
                        "0",
                        "0",
                        "0",
                        "0",
                        "0",
                        "0",
                        "0"});
#line 14
 testRunner.And("the following historical contract type 2 payments exist:", ((string)(null)), table3, "And ");
#line hidden
            TechTalk.SpecFlow.Table table4 = new TechTalk.SpecFlow.Table(new string[] {
                        "LearnRefNumber",
                        "Ukprn",
                        "PriceEpisodeIdentifier",
                        "EpisodeStartDate",
                        "EpisodeEffectiveTNPStartDate",
                        "TotalNegotiatedPrice",
                        "Learning_1"});
            table4.AddRow(new string[] {
                        "learnref1",
                        "10000",
                        "p2",
                        "06/08/2017",
                        "06/08/2017",
                        "9000",
                        "600"});
#line 19
 testRunner.And("the following contract type 1 on programme earnings for periods 1-12 are provided" +
                    " in the latest ILR for the academic year 1718:", ((string)(null)), table4, "And ");
#line hidden
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Contract Type 2 Payable Earnings")]
        [NUnit.Framework.CategoryAttribute("learner_changes_contract_type")]
        [NUnit.Framework.CategoryAttribute("Non-DAS")]
        [NUnit.Framework.CategoryAttribute("apprenticeship_contract_type_changes")]
        [NUnit.Framework.CategoryAttribute("minimum_tests")]
        public virtual void ContractType2PayableEarnings()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Contract Type 2 Payable Earnings", new string[] {
                        "learner_changes_contract_type",
                        "Non-DAS",
                        "apprenticeship_contract_type_changes",
                        "minimum_tests"});
#line 27
this.ScenarioSetup(scenarioInfo);
#line 3
this.FeatureBackground();
#line 29
 testRunner.When("a TOBY is received", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
            TechTalk.SpecFlow.Table table5 = new TechTalk.SpecFlow.Table(new string[] {
                        "LearnRefNumber",
                        "Ukprn",
                        "PriceEpisodeIdentifier",
                        "Period",
                        "ULN",
                        "SfaContributionPercentage",
                        "Learning_1",
                        "Completion_2",
                        "Balancing_3",
                        "First16To18EmployerIncentive_4",
                        "First16To18ProviderIncentive_5",
                        "Second16To18EmployerIncentive_6",
                        "Second16To18ProviderIncentive_7",
                        "OnProgramme16To18FrameworkUplift_8",
                        "Completion16To18FrameworkUplift_9",
                        "Balancing16To18FrameworkUplift_10",
                        "FirstDisadvantagePayment_11",
                        "SecondDisadvantagePayment_12",
                        "OnProgrammeMathsAndEnglish_13",
                        "BalancingMathsAndEnglish_14",
                        "LearningSupport_15"});
            table5.AddRow(new string[] {
                        "learnref1",
                        "10000",
                        "p1",
                        "1",
                        "10000",
                        "0.90000",
                        "600",
                        "0",
                        "",
                        "0",
                        "0",
                        "0",
                        "0",
                        "0",
                        "0",
                        "0",
                        "0",
                        "0",
                        "0",
                        "0",
                        "0"});
            table5.AddRow(new string[] {
                        "learnref1",
                        "10000",
                        "p1",
                        "2",
                        "10000",
                        "0.90000",
                        "600",
                        "0",
                        "",
                        "0",
                        "0",
                        "0",
                        "0",
                        "0",
                        "0",
                        "0",
                        "0",
                        "0",
                        "0",
                        "0",
                        "0"});
            table5.AddRow(new string[] {
                        "learnref1",
                        "10000",
                        "p1",
                        "1",
                        "10000",
                        "0.90000",
                        "-600",
                        "0",
                        "",
                        "0",
                        "0",
                        "0",
                        "0",
                        "0",
                        "0",
                        "0",
                        "0",
                        "0",
                        "0",
                        "0",
                        "0"});
            table5.AddRow(new string[] {
                        "learnref1",
                        "10000",
                        "p1",
                        "2",
                        "10000",
                        "0.90000",
                        "-600",
                        "0",
                        "",
                        "0",
                        "0",
                        "0",
                        "0",
                        "0",
                        "0",
                        "0",
                        "0",
                        "0",
                        "0",
                        "0",
                        "0"});
#line 31
 testRunner.Then("the payments due component will generate the following contract type 2 payable ea" +
                    "rnings:", ((string)(null)), table5, "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Contract Type 1 Payable Earnings")]
        [NUnit.Framework.CategoryAttribute("learner_changes_contract_type")]
        [NUnit.Framework.CategoryAttribute("Non-DAS")]
        [NUnit.Framework.CategoryAttribute("apprenticeship_contract_type_changes")]
        [NUnit.Framework.CategoryAttribute("minimum_tests")]
        public virtual void ContractType1PayableEarnings()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Contract Type 1 Payable Earnings", new string[] {
                        "learner_changes_contract_type",
                        "Non-DAS",
                        "apprenticeship_contract_type_changes",
                        "minimum_tests"});
#line 42
this.ScenarioSetup(scenarioInfo);
#line 3
this.FeatureBackground();
#line 44
 testRunner.When("a TOBY is received", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
            TechTalk.SpecFlow.Table table6 = new TechTalk.SpecFlow.Table(new string[] {
                        "LearnRefNumber",
                        "Ukprn",
                        "PriceEpisodeIdentifier",
                        "Period",
                        "ULN",
                        "SfaContributionPercentage",
                        "Learning_1",
                        "Completion_2",
                        "Balancing_3",
                        "First16To18EmployerIncentive_4",
                        "First16To18ProviderIncentive_5",
                        "Second16To18EmployerIncentive_6",
                        "Second16To18ProviderIncentive_7",
                        "OnProgramme16To18FrameworkUplift_8",
                        "Completion16To18FrameworkUplift_9",
                        "Balancing16To18FrameworkUplift_10",
                        "FirstDisadvantagePayment_11",
                        "SecondDisadvantagePayment_12",
                        "OnProgrammeMathsAndEnglish_13",
                        "BalancingMathsAndEnglish_14",
                        "LearningSupport_15"});
            table6.AddRow(new string[] {
                        "learnref1",
                        "10000",
                        "p2",
                        "1",
                        "10000",
                        "0.90000",
                        "600",
                        "0",
                        "",
                        "0",
                        "0",
                        "0",
                        "0",
                        "0",
                        "0",
                        "0",
                        "0",
                        "0",
                        "0",
                        "0",
                        "0"});
            table6.AddRow(new string[] {
                        "learnref1",
                        "10000",
                        "p2",
                        "2",
                        "10000",
                        "0.90000",
                        "600",
                        "0",
                        "",
                        "0",
                        "0",
                        "0",
                        "0",
                        "0",
                        "0",
                        "0",
                        "0",
                        "0",
                        "0",
                        "0",
                        "0"});
            table6.AddRow(new string[] {
                        "learnref1",
                        "10000",
                        "p2",
                        "3",
                        "10000",
                        "0.90000",
                        "600",
                        "0",
                        "",
                        "0",
                        "0",
                        "0",
                        "0",
                        "0",
                        "0",
                        "0",
                        "0",
                        "0",
                        "0",
                        "0",
                        "0"});
#line 46
 testRunner.Then("the payments due component will generate the following contract type 1 payable ea" +
                    "rnings:", ((string)(null)), table6, "Then ");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion