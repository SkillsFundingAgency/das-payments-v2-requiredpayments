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
namespace SFA.DAS.Payments.RequiredPayments.AcceptanceTests.Tests
{
    using TechTalk.SpecFlow;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "2.4.0.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [NUnit.Framework.TestFixtureAttribute()]
    [NUnit.Framework.DescriptionAttribute("No history - ALL TT1 TT2 and TT3")]
    public partial class NoHistory_ALLTT1TT2AndTT3Feature
    {
        
        private TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "No history - ALL TT1 TT2 and TT3.feature"
#line hidden
        
        [NUnit.Framework.OneTimeSetUpAttribute()]
        public virtual void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "No history - ALL TT1 TT2 and TT3", null, ProgrammingLanguage.CSharp, ((string[])(null)));
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
 testRunner.Given("the current processing period is 10", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 7
 testRunner.And("the payments are for the current collection year", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 8
 testRunner.And("a learner with LearnRefNumber learnref1 and Uln 10000 undertaking training with t" +
                    "raining provider 10000", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 9
 testRunner.And("the SFA contribution percentage is 90%", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table1 = new TechTalk.SpecFlow.Table(new string[] {
                        "PriceEpisodeIdentifier",
                        "Period",
                        "TransactionType",
                        "Amount"});
            table1.AddRow(new string[] {
                        "p1",
                        "1",
                        "Learning (TT1)",
                        "1000"});
            table1.AddRow(new string[] {
                        "p1",
                        "2",
                        "Learning (TT1)",
                        "1000"});
            table1.AddRow(new string[] {
                        "p1",
                        "3",
                        "Learning (TT1)",
                        "1000"});
            table1.AddRow(new string[] {
                        "p1",
                        "4",
                        "Learning (TT1)",
                        "1000"});
            table1.AddRow(new string[] {
                        "p1",
                        "5",
                        "Learning (TT1)",
                        "1000"});
            table1.AddRow(new string[] {
                        "p1",
                        "6",
                        "Learning (TT1)",
                        "1000"});
            table1.AddRow(new string[] {
                        "p1",
                        "7",
                        "Learning (TT1)",
                        "1000"});
            table1.AddRow(new string[] {
                        "p1",
                        "8",
                        "Learning (TT1)",
                        "1000"});
            table1.AddRow(new string[] {
                        "p1",
                        "9",
                        "Learning (TT1)",
                        "1000"});
            table1.AddRow(new string[] {
                        "p1",
                        "10",
                        "Completion (TT2)",
                        "3000"});
            table1.AddRow(new string[] {
                        "p1",
                        "10",
                        "Balancing (TT3)",
                        "3000"});
#line 10
 testRunner.And("the payments due component generates the following contract type 2 payments due:", ((string)(null)), table1, "And ");
#line hidden
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Contract Type 2 On Programme Learning payments")]
        [NUnit.Framework.CategoryAttribute("Non-DAS")]
        [NUnit.Framework.CategoryAttribute("Learning")]
        [NUnit.Framework.CategoryAttribute("(TT1)")]
        [NUnit.Framework.CategoryAttribute("Completion")]
        [NUnit.Framework.CategoryAttribute("(TT2)")]
        [NUnit.Framework.CategoryAttribute("Balancing")]
        [NUnit.Framework.CategoryAttribute("(TT3)")]
        [NUnit.Framework.CategoryAttribute("NoHistory")]
        [NUnit.Framework.TestCaseAttribute("Learning (TT1)", "1000", null)]
        public virtual void ContractType2OnProgrammeLearningPayments(string transaction_Type, string amount, string[] exampleTags)
        {
            string[] @__tags = new string[] {
                    "Non-DAS",
                    "Learning",
                    "(TT1)",
                    "Completion",
                    "(TT2)",
                    "Balancing",
                    "(TT3)",
                    "NoHistory"};
            if ((exampleTags != null))
            {
                @__tags = System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Concat(@__tags, exampleTags));
            }
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Contract Type 2 On Programme Learning payments", null, @__tags);
#line 30
this.ScenarioInitialize(scenarioInfo);
            this.ScenarioStart();
#line 4
this.FeatureBackground();
#line 32
 testRunner.When("a payments due event is received", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
            TechTalk.SpecFlow.Table table2 = new TechTalk.SpecFlow.Table(new string[] {
                        "PriceEpisodeIdentifier",
                        "Period",
                        "TransactionType",
                        "Amount"});
            table2.AddRow(new string[] {
                        "p1",
                        "1",
                        string.Format("{0}", transaction_Type),
                        string.Format("{0}", amount)});
            table2.AddRow(new string[] {
                        "p1",
                        "2",
                        string.Format("{0}", transaction_Type),
                        string.Format("{0}", amount)});
            table2.AddRow(new string[] {
                        "p1",
                        "3",
                        string.Format("{0}", transaction_Type),
                        string.Format("{0}", amount)});
            table2.AddRow(new string[] {
                        "p1",
                        "4",
                        string.Format("{0}", transaction_Type),
                        string.Format("{0}", amount)});
            table2.AddRow(new string[] {
                        "p1",
                        "5",
                        string.Format("{0}", transaction_Type),
                        string.Format("{0}", amount)});
            table2.AddRow(new string[] {
                        "p1",
                        "6",
                        string.Format("{0}", transaction_Type),
                        string.Format("{0}", amount)});
            table2.AddRow(new string[] {
                        "p1",
                        "7",
                        string.Format("{0}", transaction_Type),
                        string.Format("{0}", amount)});
            table2.AddRow(new string[] {
                        "p1",
                        "8",
                        string.Format("{0}", transaction_Type),
                        string.Format("{0}", amount)});
            table2.AddRow(new string[] {
                        "p1",
                        "9",
                        string.Format("{0}", transaction_Type),
                        string.Format("{0}", amount)});
#line 34
 testRunner.Then("the required payments component will generate the following contract type 2 payab" +
                    "le earnings:", ((string)(null)), table2, "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Contract Type 2 On Programme Completion payment")]
        [NUnit.Framework.TestCaseAttribute("Completion (TT2)", "3000", null)]
        public virtual void ContractType2OnProgrammeCompletionPayment(string transaction_Type, string amount, string[] exampleTags)
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Contract Type 2 On Programme Completion payment", null, exampleTags);
#line 50
this.ScenarioInitialize(scenarioInfo);
            this.ScenarioStart();
#line 4
this.FeatureBackground();
#line 52
 testRunner.When("a payments due event is received", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
            TechTalk.SpecFlow.Table table3 = new TechTalk.SpecFlow.Table(new string[] {
                        "PriceEpisodeIdentifier",
                        "Period",
                        "TransactionType",
                        "Amount"});
            table3.AddRow(new string[] {
                        "p1",
                        "10",
                        string.Format("{0}", transaction_Type),
                        string.Format("{0}", amount)});
#line 54
 testRunner.Then("the required payments component will generate the following contract type 2 Compl" +
                    "etion (TT2) payable earnings:", ((string)(null)), table3, "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Contract Type 2 On Programme Balancing payment")]
        [NUnit.Framework.TestCaseAttribute("Balancing (TT3)", "3000", null)]
        public virtual void ContractType2OnProgrammeBalancingPayment(string transaction_Type, string amount, string[] exampleTags)
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Contract Type 2 On Programme Balancing payment", null, exampleTags);
#line 63
this.ScenarioInitialize(scenarioInfo);
            this.ScenarioStart();
#line 4
this.FeatureBackground();
#line 65
 testRunner.When("a payments due event is received", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
            TechTalk.SpecFlow.Table table4 = new TechTalk.SpecFlow.Table(new string[] {
                        "PriceEpisodeIdentifier",
                        "Period",
                        "TransactionType",
                        "Amount"});
            table4.AddRow(new string[] {
                        "p1",
                        "10",
                        string.Format("{0}", transaction_Type),
                        string.Format("{0}", amount)});
#line 67
 testRunner.Then("the required payments component will generate the following contract type 2 Balan" +
                    "cing (TT3) payable earnings:", ((string)(null)), table4, "Then ");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
