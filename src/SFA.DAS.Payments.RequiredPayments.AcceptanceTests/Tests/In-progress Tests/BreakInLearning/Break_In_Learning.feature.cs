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
namespace SFA.DAS.Payments.RequiredPayments.AcceptanceTests.Tests.In_ProgressTests.BreakInLearning
{
    using TechTalk.SpecFlow;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "2.4.0.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [NUnit.Framework.TestFixtureAttribute()]
    [NUnit.Framework.DescriptionAttribute("Break in Learning, learner takes 2 months break")]
    public partial class BreakInLearningLearnerTakes2MonthsBreakFeature
    {
        
        private TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "Break_In_Learning.feature"
#line hidden
        
        [NUnit.Framework.OneTimeSetUpAttribute()]
        public virtual void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Break in Learning, learner takes 2 months break", null, ProgrammingLanguage.CSharp, ((string[])(null)));
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
#line 4
#line 6
 testRunner.Given("the current processing period is 8", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 8
 testRunner.And("a learner with LearnRefNumber learnref1 and Uln 10000 undertaking training with t" +
                    "raining provider 10000", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table58 = new TechTalk.SpecFlow.Table(new string[] {
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
            table58.AddRow(new string[] {
                        "1",
                        "2",
                        "403",
                        "1",
                        "",
                        "16-18 Apprenticeship (From May 2017) Non-Levy Contract (non-procured)",
                        "ZPROG001",
                        "01/09/2017",
                        "08/09/2018",
                        "31/10/2017",
                        "planned break"});
            table58.AddRow(new string[] {
                        "2",
                        "2",
                        "403",
                        "1",
                        "",
                        "16-18 Apprenticeship (From May 2017) Non-Levy Contract (non-procured)",
                        "ZPROG001",
                        "03/01/2018",
                        "08/11/2018",
                        "",
                        "Continuing"});
#line 10
 testRunner.And("the following course information:", ((string)(null)), table58, "And ");
#line hidden
            TechTalk.SpecFlow.Table table59 = new TechTalk.SpecFlow.Table(new string[] {
                        "PriceEpisodeIdentifier",
                        "EpisodeStartDate",
                        "EpisodeEffectiveTNPStartDate",
                        "TotalNegotiatedPrice",
                        "Learning_1"});
            table59.AddRow(new string[] {
                        "p1",
                        "01/09/2017",
                        "01/09/2017",
                        "15000",
                        "1000"});
#line 15
 testRunner.And("the following contract type 2 on programme earnings for periods 1-3 are provided " +
                    "in the latest ILR for the academic year 1718:", ((string)(null)), table59, "And ");
#line hidden
            TechTalk.SpecFlow.Table table60 = new TechTalk.SpecFlow.Table(new string[] {
                        "PriceEpisodeIdentifier",
                        "EpisodeStartDate",
                        "EpisodeEffectiveTNPStartDate",
                        "TotalNegotiatedPrice",
                        "Learning_1"});
            table60.AddRow(new string[] {
                        "p2",
                        "03/01/2018",
                        "03/01/2018",
                        "15000",
                        "1000"});
#line 20
 testRunner.And("the following contract type 2 on programme earnings for periods 6-14 are provided" +
                    " in the latest ILR for the academic year 1718:", ((string)(null)), table60, "And ");
#line hidden
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Contract Type 2 On programme payments")]
        [NUnit.Framework.CategoryAttribute("Non-DAS")]
        [NUnit.Framework.CategoryAttribute("minimum_additional")]
        [NUnit.Framework.CategoryAttribute("BreakInLearning")]
        [NUnit.Framework.CategoryAttribute("review")]
        [NUnit.Framework.TestCaseAttribute("Learning_1", "1000", null)]
        public virtual void ContractType2OnProgrammePayments(string transaction_Type, string amount, string[] exampleTags)
        {
            string[] @__tags = new string[] {
                    "Non-DAS",
                    "minimum_additional",
                    "BreakInLearning",
                    "review"};
            if ((exampleTags != null))
            {
                @__tags = System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Concat(@__tags, exampleTags));
            }
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Contract Type 2 On programme payments", null, @__tags);
#line 29
this.ScenarioInitialize(scenarioInfo);
            this.ScenarioStart();
#line 4
this.FeatureBackground();
#line hidden
            TechTalk.SpecFlow.Table table61 = new TechTalk.SpecFlow.Table(new string[] {
                        "LearnRefNumber",
                        "Ukprn",
                        "PriceEpisodeIdentifier",
                        "Period",
                        "ULN",
                        "TransactionType",
                        "Amount"});
            table61.AddRow(new string[] {
                        "learnref1",
                        "10000",
                        "p1",
                        "1",
                        "10000",
                        string.Format("{0}", transaction_Type),
                        string.Format("{0}", amount)});
            table61.AddRow(new string[] {
                        "learnref1",
                        "10000",
                        "p1",
                        "2",
                        "10000",
                        string.Format("{0}", transaction_Type),
                        string.Format("{0}", amount)});
#line 31
 testRunner.And("the following historical contract type 2 on programme payments exist:", ((string)(null)), table61, "And ");
#line 36
 testRunner.When("a TOBY is received", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
            TechTalk.SpecFlow.Table table62 = new TechTalk.SpecFlow.Table(new string[] {
                        "LearnRefNumber",
                        "Ukprn",
                        "PriceEpisodeIdentifier",
                        "Period",
                        "ULN",
                        "TransactionType",
                        "Amount"});
            table62.AddRow(new string[] {
                        "learnref1",
                        "10000",
                        "p1",
                        "1",
                        "10000",
                        string.Format("{0}", transaction_Type),
                        string.Format("{0}", amount)});
            table62.AddRow(new string[] {
                        "learnref1",
                        "10000",
                        "p1",
                        "2",
                        "10000",
                        string.Format("{0}", transaction_Type),
                        string.Format("{0}", amount)});
#line 38
 testRunner.Then("the payments due component will generate the following contract type 2 payable ea" +
                    "rnings:", ((string)(null)), table62, "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Contract Type 2 On programme payments changed price")]
        [NUnit.Framework.TestCaseAttribute("Learning_1", "1000", null)]
        public virtual void ContractType2OnProgrammePaymentsChangedPrice(string transaction_Type, string amount, string[] exampleTags)
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Contract Type 2 On programme payments changed price", null, exampleTags);
#line 48
this.ScenarioInitialize(scenarioInfo);
            this.ScenarioStart();
#line 4
this.FeatureBackground();
#line hidden
            TechTalk.SpecFlow.Table table63 = new TechTalk.SpecFlow.Table(new string[] {
                        "LearnRefNumber",
                        "Ukprn",
                        "PriceEpisodeIdentifier",
                        "Period",
                        "ULN",
                        "TransactionType",
                        "Amount"});
            table63.AddRow(new string[] {
                        "learnref1",
                        "10000",
                        "p2",
                        "5",
                        "10000",
                        string.Format("{0}", transaction_Type),
                        string.Format("{0}", amount)});
            table63.AddRow(new string[] {
                        "learnref1",
                        "10000",
                        "p2",
                        "6",
                        "10000",
                        string.Format("{0}", transaction_Type),
                        string.Format("{0}", amount)});
#line 50
 testRunner.And("the following historical contract type 2 on programme payments exist:", ((string)(null)), table63, "And ");
#line 55
 testRunner.When("a TOBY is received", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
            TechTalk.SpecFlow.Table table64 = new TechTalk.SpecFlow.Table(new string[] {
                        "LearnRefNumber",
                        "Ukprn",
                        "PriceEpisodeIdentifier",
                        "Period",
                        "ULN",
                        "TransactionType",
                        "Amount"});
            table64.AddRow(new string[] {
                        "learnref1",
                        "10000",
                        "p2",
                        "5",
                        "10000",
                        string.Format("{0}", transaction_Type),
                        string.Format("{0}", amount)});
            table64.AddRow(new string[] {
                        "learnref1",
                        "10000",
                        "p2",
                        "6",
                        "10000",
                        string.Format("{0}", transaction_Type),
                        string.Format("{0}", amount)});
            table64.AddRow(new string[] {
                        "learnref1",
                        "10000",
                        "p2",
                        "7",
                        "10000",
                        string.Format("{0}", transaction_Type),
                        string.Format("{0}", amount)});
#line 57
 testRunner.Then("the payments due component will generate the following contract type 2 payable ea" +
                    "rnings:", ((string)(null)), table64, "Then ");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion