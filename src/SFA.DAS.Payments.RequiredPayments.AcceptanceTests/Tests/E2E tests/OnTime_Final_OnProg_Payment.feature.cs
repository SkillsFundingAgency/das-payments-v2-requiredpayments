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
namespace SFA.DAS.Payments.RequiredPayments.AcceptanceTests.Tests.E2ETests
{
    using TechTalk.SpecFlow;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "2.4.0.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [NUnit.Framework.TestFixtureAttribute()]
    [NUnit.Framework.DescriptionAttribute("R12 - Final OnProgram payment")]
    public partial class R12_FinalOnProgramPaymentFeature
    {
        
        private TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "OnTime_Final_OnProg_Payment.feature"
#line hidden
        
        [NUnit.Framework.OneTimeSetUpAttribute()]
        public virtual void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "R12 - Final OnProgram payment", null, ProgrammingLanguage.CSharp, ((string[])(null)));
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
#line 3
#line 4
 testRunner.Given("the current processing period is 12", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 6
 testRunner.And("a learner with LearnRefNumber learnref1 and Uln 10000 undertaking training with t" +
                    "raining provider 10000", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 8
 testRunner.And("the SFA contribution percentage is \"90%\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table37 = new TechTalk.SpecFlow.Table(new string[] {
                        "LearnRefNumber",
                        "Ukprn",
                        "PriceEpisodeIdentifier",
                        "Period",
                        "ULN",
                        "TransactionType",
                        "Amount"});
            table37.AddRow(new string[] {
                        "learnref1",
                        "10000",
                        "p1",
                        "1",
                        "10000",
                        "Learning (TT1)",
                        "600"});
            table37.AddRow(new string[] {
                        "learnref1",
                        "10000",
                        "p1",
                        "2",
                        "10000",
                        "Learning (TT1)",
                        "600"});
            table37.AddRow(new string[] {
                        "learnref1",
                        "10000",
                        "p1",
                        "3",
                        "10000",
                        "Learning (TT1)",
                        "600"});
            table37.AddRow(new string[] {
                        "learnref1",
                        "10000",
                        "p1",
                        "4",
                        "10000",
                        "Learning (TT1)",
                        "600"});
            table37.AddRow(new string[] {
                        "learnref1",
                        "10000",
                        "p1",
                        "5",
                        "10000",
                        "Learning (TT1)",
                        "600"});
            table37.AddRow(new string[] {
                        "learnref1",
                        "10000",
                        "p1",
                        "6",
                        "10000",
                        "Learning (TT1)",
                        "600"});
            table37.AddRow(new string[] {
                        "learnref1",
                        "10000",
                        "p1",
                        "7",
                        "10000",
                        "Learning (TT1)",
                        "600"});
            table37.AddRow(new string[] {
                        "learnref1",
                        "10000",
                        "p1",
                        "8",
                        "10000",
                        "Learning (TT1)",
                        "600"});
            table37.AddRow(new string[] {
                        "learnref1",
                        "10000",
                        "p1",
                        "9",
                        "10000",
                        "Learning (TT1)",
                        "600"});
            table37.AddRow(new string[] {
                        "learnref1",
                        "10000",
                        "p1",
                        "10",
                        "10000",
                        "Learning (TT1)",
                        "600"});
            table37.AddRow(new string[] {
                        "learnref1",
                        "10000",
                        "p1",
                        "11",
                        "10000",
                        "Learning (TT1)",
                        "600"});
            table37.AddRow(new string[] {
                        "learnref1",
                        "10000",
                        "p1",
                        "12",
                        "10000",
                        "Learning (TT1)",
                        "600"});
#line 10
 testRunner.And("the payments due component generates the following contract type 2 payments due:", ((string)(null)), table37, "And ");
#line hidden
            TechTalk.SpecFlow.Table table38 = new TechTalk.SpecFlow.Table(new string[] {
                        "LearnRefNumber",
                        "Ukprn",
                        "PriceEpisodeIdentifier",
                        "Period",
                        "ULN",
                        "TransactionType",
                        "Amount"});
            table38.AddRow(new string[] {
                        "learnref1",
                        "10000",
                        "p1",
                        "1",
                        "10000",
                        "Learning (TT1)",
                        "600"});
            table38.AddRow(new string[] {
                        "learnref1",
                        "10000",
                        "p1",
                        "2",
                        "10000",
                        "Learning (TT1)",
                        "600"});
            table38.AddRow(new string[] {
                        "learnref1",
                        "10000",
                        "p1",
                        "3",
                        "10000",
                        "Learning (TT1)",
                        "600"});
            table38.AddRow(new string[] {
                        "learnref1",
                        "10000",
                        "p1",
                        "4",
                        "10000",
                        "Learning (TT1)",
                        "600"});
            table38.AddRow(new string[] {
                        "learnref1",
                        "10000",
                        "p1",
                        "5",
                        "10000",
                        "Learning (TT1)",
                        "600"});
            table38.AddRow(new string[] {
                        "learnref1",
                        "10000",
                        "p1",
                        "6",
                        "10000",
                        "Learning (TT1)",
                        "600"});
            table38.AddRow(new string[] {
                        "learnref1",
                        "10000",
                        "p1",
                        "7",
                        "10000",
                        "Learning (TT1)",
                        "600"});
            table38.AddRow(new string[] {
                        "learnref1",
                        "10000",
                        "p1",
                        "8",
                        "10000",
                        "Learning (TT1)",
                        "600"});
            table38.AddRow(new string[] {
                        "learnref1",
                        "10000",
                        "p1",
                        "9",
                        "10000",
                        "Learning (TT1)",
                        "600"});
            table38.AddRow(new string[] {
                        "learnref1",
                        "10000",
                        "p1",
                        "10",
                        "10000",
                        "Learning (TT1)",
                        "600"});
            table38.AddRow(new string[] {
                        "learnref1",
                        "10000",
                        "p1",
                        "11",
                        "10000",
                        "Learning (TT1)",
                        "600"});
#line 25
 testRunner.And("the following historical contract type 2 On Programme Learning payments exist:", ((string)(null)), table38, "And ");
#line hidden
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Contract Type 2 On Programme Learning payments")]
        [NUnit.Framework.CategoryAttribute("Non-DAS")]
        [NUnit.Framework.CategoryAttribute("Learning")]
        [NUnit.Framework.CategoryAttribute("(TT1)")]
        [NUnit.Framework.TestCaseAttribute("Learning (TT1)", "600", null)]
        public virtual void ContractType2OnProgrammeLearningPayments(string transaction_Type, string amount, string[] exampleTags)
        {
            string[] @__tags = new string[] {
                    "Non-DAS",
                    "Learning",
                    "(TT1)"};
            if ((exampleTags != null))
            {
                @__tags = System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Concat(@__tags, exampleTags));
            }
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Contract Type 2 On Programme Learning payments", null, @__tags);
#line 42
this.ScenarioInitialize(scenarioInfo);
            this.ScenarioStart();
#line 3
this.FeatureBackground();
#line 44
 testRunner.When("a payments due event is received", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
            TechTalk.SpecFlow.Table table39 = new TechTalk.SpecFlow.Table(new string[] {
                        "LearnRefNumber",
                        "Ukprn",
                        "PriceEpisodeIdentifier",
                        "Period",
                        "ULN",
                        "TransactionType",
                        "Amount"});
            table39.AddRow(new string[] {
                        "learnref1",
                        "10000",
                        "p1",
                        "12",
                        "10000",
                        string.Format("{0}", transaction_Type),
                        string.Format("{0}", amount)});
#line 46
 testRunner.Then("the required payments component will generate the following contract type 2 payab" +
                    "le earnings:", ((string)(null)), table39, "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Contract Type 2 no On Programme Completion payment")]
        public virtual void ContractType2NoOnProgrammeCompletionPayment()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Contract Type 2 no On Programme Completion payment", null, ((string[])(null)));
#line 56
this.ScenarioInitialize(scenarioInfo);
            this.ScenarioStart();
#line 3
this.FeatureBackground();
#line 58
 testRunner.When("a payments due event is received", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 60
 testRunner.Then("the required payments component will not generate any contract type 2 Completion " +
                    "(TT2) payable earnings", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Contract Type 2 no On Programme Balancing payment")]
        public virtual void ContractType2NoOnProgrammeBalancingPayment()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Contract Type 2 no On Programme Balancing payment", null, ((string[])(null)));
#line 63
this.ScenarioInitialize(scenarioInfo);
            this.ScenarioStart();
#line 3
this.FeatureBackground();
#line 65
 testRunner.When("a payments due event is received", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 67
 testRunner.Then("the required payments component will not generate any contract type 2 Balancing (" +
                    "TT3) payable earnings", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
