﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (http://www.specflow.org/).
//      SpecFlow Version:2.4.0.0
//      SpecFlow Generator Version:2.4.0.0
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
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "2.4.0.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [NUnit.Framework.TestFixtureAttribute()]
    [NUnit.Framework.DescriptionAttribute("Contract Type Changes From ACT2 To ACT1")]
    public partial class ContractTypeChangesFromACT2ToACT1Feature
    {
        
        private TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "ContractTypeChangesFromAct2ToAct1.feature"
#line hidden
        
        [NUnit.Framework.OneTimeSetUpAttribute()]
        public virtual void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Contract Type Changes From ACT2 To ACT1", "\tDPP_965_01 - Levy apprentice, provider edits contract type (ACT) in the ILR, pre" +
                    "vious on-programme and English/math payments are refunded and repaid according t" +
                    "o latest contract type", ProgrammingLanguage.CSharp, ((string[])(null)));
            testRunner.OnFeatureStart(featureInfo);
        }
        
        [NUnit.Framework.OneTimeTearDownAttribute()]
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
        
        public virtual void ScenarioInitialize(TechTalk.SpecFlow.ScenarioInfo scenarioInfo)
        {
            testRunner.OnScenarioInitialize(scenarioInfo);
            testRunner.ScenarioContext.ScenarioContainer.RegisterInstanceAs<NUnit.Framework.TestContext>(NUnit.Framework.TestContext.CurrentContext);
        }
        
        public virtual void ScenarioStart()
        {
            testRunner.OnScenarioStart();
        }
        
        public virtual void ScenarioCleanup()
        {
            testRunner.CollectScenarioErrors();
        }
        
        public virtual void FeatureBackground()
        {
#line 7
#line 8
 testRunner.Given("the current processing period is 3", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 10
 testRunner.And("a learner with LearnRefNumber learnref1 and Uln 10000 undertaking training with t" +
                    "raining provider 10000", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table1 = new TechTalk.SpecFlow.Table(new string[] {
                        "AimSeqNumber",
                        "ProgrammeType",
                        "FrameworkCode",
                        "PathwayCode",
                        "StandardCode",
                        "FundingLineType",
                        "LearnAimRef",
                        "LearningStartDate",
                        "LearningPlannedEndDate",
                        "LearningActualEndDate",
                        "CompletionStatus"});
            table1.AddRow(new string[] {
                        "1",
                        "2",
                        "403",
                        "1",
                        "",
                        "16-18 Apprenticeship (From May 2017) Non-Levy Contract (non-procured)",
                        "ZPROG001",
                        "06/08/2017",
                        "20/08/2018",
                        "",
                        "continuing"});
#line 12
 testRunner.And("the following course information:", ((string)(null)), table1, "And ");
#line hidden
            TechTalk.SpecFlow.Table table2 = new TechTalk.SpecFlow.Table(new string[] {
                        "PriceEpisodeIdentifier",
                        "EpisodeStartDate",
                        "EpisodeEffectiveTNPStartDate",
                        "TotalNegotiatedPrice",
                        "Learning_1"});
            table2.AddRow(new string[] {
                        "p2",
                        "06/08/2017",
                        "06/08/2017",
                        "9000",
                        "600"});
#line 16
 testRunner.And("the following contract type 1 on programme earnings for periods 1-12 are provided" +
                    " in the latest ILR for the academic year 1718:", ((string)(null)), table2, "And ");
#line hidden
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Contract Type 2 On programme payments")]
        [NUnit.Framework.CategoryAttribute("Non-DAS")]
        [NUnit.Framework.CategoryAttribute("minimum_tests")]
        [NUnit.Framework.CategoryAttribute("learner_changes_contract_type")]
        [NUnit.Framework.CategoryAttribute("apprenticeship_contract_type_changes")]
        [NUnit.Framework.CategoryAttribute("partial")]
        [NUnit.Framework.TestCaseAttribute("Learning_1", "600", null)]
        public virtual void ContractType2OnProgrammePayments(string transaction_Type, string amount, string[] exampleTags)
        {
            string[] @__tags = new string[] {
                    "Non-DAS",
                    "minimum_tests",
                    "learner_changes_contract_type",
                    "apprenticeship_contract_type_changes",
                    "partial"};
            if ((exampleTags != null))
            {
                @__tags = System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Concat(@__tags, exampleTags));
            }
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Contract Type 2 On programme payments", null, @__tags);
#line 27
this.ScenarioInitialize(scenarioInfo);
            this.ScenarioStart();
#line 7
this.FeatureBackground();
#line hidden
            TechTalk.SpecFlow.Table table3 = new TechTalk.SpecFlow.Table(new string[] {
                        "LearnRefNumber",
                        "Ukprn",
                        "PriceEpisodeIdentifier",
                        "Period",
                        "ULN",
                        "TransactionType",
                        "Amount"});
            table3.AddRow(new string[] {
                        "learnref1",
                        "10000",
                        "p1",
                        "1",
                        "10000",
                        string.Format("{0}", transaction_Type),
                        string.Format("{0}", amount)});
            table3.AddRow(new string[] {
                        "learnref1",
                        "10000",
                        "p1",
                        "2",
                        "10000",
                        string.Format("{0}", transaction_Type),
                        string.Format("{0}", amount)});
#line 29
 testRunner.And("the following historical contract type 2 on programme payments exist:", ((string)(null)), table3, "And ");
#line 34
 testRunner.When("a TOBY is received", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
            TechTalk.SpecFlow.Table table4 = new TechTalk.SpecFlow.Table(new string[] {
                        "LearnRefNumber",
                        "Ukprn",
                        "PriceEpisodeIdentifier",
                        "Period",
                        "ULN",
                        "TransactionType",
                        "Amount"});
            table4.AddRow(new string[] {
                        "learnref1",
                        "10000",
                        "p1",
                        "1",
                        "10000",
                        string.Format("{0}", transaction_Type),
                        string.Format("{0}", amount)});
            table4.AddRow(new string[] {
                        "learnref1",
                        "10000",
                        "p1",
                        "2",
                        "10000",
                        string.Format("{0}", transaction_Type),
                        string.Format("{0}", amount)});
            table4.AddRow(new string[] {
                        "learnref1",
                        "10000",
                        "p1",
                        "1",
                        "10000",
                        string.Format("{0}", transaction_Type),
                        string.Format("-{0}", amount)});
            table4.AddRow(new string[] {
                        "learnref1",
                        "10000",
                        "p1",
                        "2",
                        "10000",
                        string.Format("{0}", transaction_Type),
                        string.Format("-{0}", amount)});
#line 36
 testRunner.Then("the payments due component will generate the following contract type 2 payable ea" +
                    "rnings:", ((string)(null)), table4, "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Contract Type 1 On programme payments")]
        [NUnit.Framework.CategoryAttribute("DAS")]
        [NUnit.Framework.CategoryAttribute("minimum_tests")]
        [NUnit.Framework.CategoryAttribute("learner_changes_contract_type")]
        [NUnit.Framework.CategoryAttribute("apprenticeship_contract_type_changes")]
        [NUnit.Framework.CategoryAttribute("partial")]
        [NUnit.Framework.TestCaseAttribute("Learning_1", "600", null)]
        public virtual void ContractType1OnProgrammePayments(string transaction_Type, string amount, string[] exampleTags)
        {
            string[] @__tags = new string[] {
                    "DAS",
                    "minimum_tests",
                    "learner_changes_contract_type",
                    "apprenticeship_contract_type_changes",
                    "partial"};
            if ((exampleTags != null))
            {
                @__tags = System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Concat(@__tags, exampleTags));
            }
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Contract Type 1 On programme payments", null, @__tags);
#line 54
this.ScenarioInitialize(scenarioInfo);
            this.ScenarioStart();
#line 7
this.FeatureBackground();
#line 56
 testRunner.When("a TOBY is received", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
            TechTalk.SpecFlow.Table table5 = new TechTalk.SpecFlow.Table(new string[] {
                        "LearnRefNumber",
                        "Ukprn",
                        "PriceEpisodeIdentifier",
                        "Period",
                        "ULN",
                        "TransactionType",
                        "Amount"});
            table5.AddRow(new string[] {
                        "learnref1",
                        "10000",
                        "p2",
                        "1",
                        "10000",
                        string.Format("{0}", transaction_Type),
                        string.Format("{0}", amount)});
            table5.AddRow(new string[] {
                        "learnref1",
                        "10000",
                        "p2",
                        "2",
                        "10000",
                        string.Format("{0}", transaction_Type),
                        string.Format("{0}", amount)});
            table5.AddRow(new string[] {
                        "learnref1",
                        "10000",
                        "p2",
                        "3",
                        "10000",
                        string.Format("{0}", transaction_Type),
                        string.Format("{0}", amount)});
#line 58
 testRunner.Then("the payments due component will generate the following contract type 1 payable ea" +
                    "rnings:", ((string)(null)), table5, "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Contract Type 2 Payable Earnings")]
        [NUnit.Framework.CategoryAttribute("Non-DAS")]
        [NUnit.Framework.CategoryAttribute("minimum_tests")]
        [NUnit.Framework.CategoryAttribute("learner_changes_contract_type")]
        [NUnit.Framework.CategoryAttribute("apprenticeship_contract_type_changes")]
        public virtual void ContractType2PayableEarnings()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Contract Type 2 Payable Earnings", null, new string[] {
                        "Non-DAS",
                        "minimum_tests",
                        "learner_changes_contract_type",
                        "apprenticeship_contract_type_changes"});
#line 82
this.ScenarioInitialize(scenarioInfo);
            this.ScenarioStart();
#line 7
this.FeatureBackground();
#line 84
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
            table6.AddRow(new string[] {
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
            table6.AddRow(new string[] {
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
            table6.AddRow(new string[] {
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
#line 86
 testRunner.Then("the payments due component will generate the following contract type 2 payable ea" +
                    "rnings:", ((string)(null)), table6, "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Contract Type 1 Payable Earnings")]
        [NUnit.Framework.CategoryAttribute("DAS")]
        [NUnit.Framework.CategoryAttribute("minimum_tests")]
        [NUnit.Framework.CategoryAttribute("learner_changes_contract_type")]
        [NUnit.Framework.CategoryAttribute("apprenticeship_contract_type_changes")]
        public virtual void ContractType1PayableEarnings()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Contract Type 1 Payable Earnings", null, new string[] {
                        "DAS",
                        "minimum_tests",
                        "learner_changes_contract_type",
                        "apprenticeship_contract_type_changes"});
#line 99
this.ScenarioInitialize(scenarioInfo);
            this.ScenarioStart();
#line 7
this.FeatureBackground();
#line 101
 testRunner.When("a TOBY is received", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
            TechTalk.SpecFlow.Table table7 = new TechTalk.SpecFlow.Table(new string[] {
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
            table7.AddRow(new string[] {
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
            table7.AddRow(new string[] {
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
            table7.AddRow(new string[] {
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
#line 103
 testRunner.Then("the payments due component will generate the following contract type 1 payable ea" +
                    "rnings:", ((string)(null)), table7, "Then ");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
