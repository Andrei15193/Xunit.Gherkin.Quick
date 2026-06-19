using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Gherkin.Quick;
using Xunit.Gherkin.Quick.TestScenarios;

namespace UnitTests
{
    public sealed class StepMethodInfoTests
    {
        public StepMethodInfoTests()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
        }

        [Fact]
        public void Ctor_Initializes_Properties()
        {
            //arrange.
            var featureInstance = new FeatureForCtorTest();

            //act.
            var sut = StepMethodInfo.FromMethodInfo(featureInstance.GetType().GetMethod(nameof(FeatureForCtorTest.When_Something)), featureInstance);

            //assert.
            Assert.NotNull(sut);
            Assert.NotNull(sut.ScenarioStepPatterns);
            Assert.Single(sut.ScenarioStepPatterns);

            Assert.Equal(TestStepType.When, sut.ScenarioStepPatterns[0].Type);
            Assert.Equal(FeatureForCtorTest.WhenStepText, sut.ScenarioStepPatterns[0].OriginalPattern);
        }

        private sealed class FeatureForCtorTest : Feature
        {
            public const string WhenStepText = "some text 123";

            [When(WhenStepText)]
            public void When_Something()
            {

            }
        }

        [Fact]
        public void Clone_Creates_Similar_Instance()
        {
            //arrange.
            var featureInstance = new FeatureForCtorTest();
            var sut = StepMethodInfo.FromMethodInfo(featureInstance.GetType().GetMethod(nameof(FeatureForCtorTest.When_Something)), featureInstance);

            //act.
            var clone = sut.Clone();

            //assert.
            Assert.NotNull(clone);
            Assert.True(clone.IsSameAs(sut));
        }

        [Fact]
        public void IsSameAs_Identifies_Similar_Instances()
        {
            //arrange.
            var featureInstance = new FeatureForCtorTest();
            var sut = StepMethodInfo.FromMethodInfo(featureInstance.GetType().GetMethod(nameof(FeatureForCtorTest.When_Something)), featureInstance);
            var clone = StepMethodInfo.FromMethodInfo(featureInstance.GetType().GetMethod(nameof(FeatureForCtorTest.When_Something)), featureInstance);

            //act.
            var same = sut.IsSameAs(clone) && clone.IsSameAs(sut);

            //assert.
            Assert.True(same);
        }

        [Fact]
        public void DigestScenarioStepValues_Expects_Exact_Number_Of_Groups_Not_Less()
        {
            //arrange.
            var featureInstance = new FeatureForApplyArgumentValues_LessThanNeededGroups();
            var sut = StepMethodInfo.FromMethodInfo(
                featureInstance.GetType().GetMethod(nameof(FeatureForApplyArgumentValues_LessThanNeededGroups.Method_With_Arguments)),
                featureInstance);

            var number = 123;
            var text = "Ana";
            var date = new DateTime(2018, 5, 23);
            var stepText = FeatureForApplyArgumentValues_LessThanNeededGroups.StepMethodText
                .Replace(@"(\d+)", $"{number}")
                .Replace(@"(\w+)", $"{text}");

            var step = new TestStep(TestStepType.Then, stepText);

            //act / assert.
            Assert.Throws<InvalidOperationException>(() => sut.DigestScenarioStepValues(step));
        }

        private sealed class FeatureForApplyArgumentValues_LessThanNeededGroups : Feature
        {
            public const string StepMethodText = @"I should have (\d+) apples from (\w+) by 5/23/2018";

            [Then(StepMethodText)]
            public void Method_With_Arguments(int number, string text, DateTime date)
            { }
        }

        [Theory]
        [InlineData(TestStepType.Then)]
        [InlineData(TestStepType.All)]
        public void DigestScenarioStepValues_Sets_Primitive_Values(TestStepType stepType)
        {
            //arrange.
            var featureInstance = new FeatureForApplyArgumentValues();
            var sut = StepMethodInfo.FromMethodInfo(
                featureInstance.GetType().GetMethod(nameof(FeatureForApplyArgumentValues.Method_With_Arguments)),
                featureInstance);

            var number = 123;
            var text = "Ana";
            var date = new DateTime(2018, 5, 23);
            var stepText = FeatureForApplyArgumentValues.StepMethodText
                .Replace(@"(\d+)", $"{number}")
                .Replace(@"(\w+)", $"{text}")
                .Replace(@"([\d/]+)", $"{date.Month}/{date.Day}/{date.Year}");

            var step = new TestStep(stepType, stepText);

            //act.
            sut.DigestScenarioStepValues(step);

            //assert.
            var digestedText = sut.GetDigestedStepText();
            Assert.Equal(stepText, digestedText);
        }
        
        private sealed class FeatureForApplyArgumentValues : Feature
        {
            public const string StepMethodText = @"I should have (\d+) apples from (\w+) by ([\d/]+)";

            [Then(StepMethodText)]
            public void Method_With_Arguments(int number, string text, DateTime date)
            { }
        }

        [Fact]
        public async Task Execute_Invokes_StepMethod()
        {
            //arrange.
            var featureInstance = new FeatureForExecuteTest();
            var sut = StepMethodInfo.FromMethodInfo(featureInstance.GetType().GetMethod(nameof(FeatureForExecuteTest.Call_This_Method)), featureInstance);

            //act.
            await sut.ExecuteAsync();

            //assert.
            Assert.True(featureInstance.Called);
        }

        private sealed class FeatureForExecuteTest : Feature
        {
            public bool Called { get; private set; } = false;

            [And("Call this")]
            public void Call_This_Method()
            {
                Called = true;
            }
        }

        [Fact]
        public void GetDigestedStepText_Throws_Error_If_Not_Yet_Digested()
        {
            //arrange.
            var featureInstance = new Feature_For_GetDigestedStepTextTest();
            var sut = StepMethodInfo.FromMethodInfo(featureInstance.GetType().GetMethod(nameof(Feature_For_GetDigestedStepTextTest.When_Something_Method)), featureInstance);

            //act / assert.
            Assert.Throws<InvalidOperationException>(() => sut.GetDigestedStepText());
        }

        private sealed class Feature_For_GetDigestedStepTextTest : Feature
        {
            [When("something")]
            public void When_Something_Method() { }
        }

        [Fact]
        public void FromMethodInfo_Creates_StepMethodInfo_With_DataTable()
        {
            //arrange.
            var featureInstance = new FeatureWithDataTableScenarioStep();

            //act.
            var sut = StepMethodInfo.FromMethodInfo(
                featureInstance.GetType().GetMethod(nameof(FeatureWithDataTableScenarioStep.When_DataTable_Is_Expected)),
                featureInstance
                );

            //assert.
            Assert.NotNull(sut);
        }

        [Fact]
        public void DigestScenarioStepValues_Sets_DataTable_Value()
        {
            //arrange.
            var featureInstance = new FeatureWithDataTableScenarioStep();
            var sut = StepMethodInfo.FromMethodInfo(
                featureInstance.GetType().GetMethod(nameof(FeatureWithDataTableScenarioStep.When_DataTable_Is_Expected)),
                featureInstance
                );

            var step = new TestStep(
                TestStepType.When,
                FeatureWithDataTableScenarioStep.Steptext,
                new TestStepTableArgument([
                    new(
                        [
                            new("First argument", null),
                            new("Second argument", null),
                            new("Result", null)
                        ],
                        null
                    ),
                    new(
                        [
                            new("1", null),
                            new("2", null),
                            new("3", null)
                        ],
                        null
                    ),
                    new(
                        [
                            new("a", null),
                            new("b", null),
                            new("c", null)
                        ],
                        null
                    )
                ])
            );

            //act.
            sut.DigestScenarioStepValues(step);

            //assert.
            var digestedText = sut.GetDigestedStepText();
            Assert.Equal(FeatureWithDataTableScenarioStep.Steptext, digestedText);
        }

        private sealed class FeatureWithDataTableScenarioStep : Feature
        {
            public Gherkin.Ast.DataTable ReceivedDataTable { get; private set; }

            public const string Steptext = "Some step text";

            [When(Steptext)]
            public void When_DataTable_Is_Expected(Gherkin.Ast.DataTable dataTable)
            {
                ReceivedDataTable = dataTable;
            }
        }

        [Fact]
        public void FromMethodInfo_Creates_StepMethodInfo_With_DocString()
        {
            //arrange.
            var featureInstance = new FeatureWithDocStringScenarioStep();

            //act.
            var sut = StepMethodInfo.FromMethodInfo(
                featureInstance.GetType().GetMethod(nameof(FeatureWithDocStringScenarioStep.Step_With_DocString_Argument)),
                featureInstance
                );

            //assert.
            Assert.NotNull(sut);
        }

        private sealed class FeatureWithDocStringScenarioStep : Feature
        {
            public Gherkin.Ast.DocString ReceivedDocString { get; private set; }

            public const string StepWithDocStringText = "Step with docstirng";

            [Given(StepWithDocStringText)]
            public void Step_With_DocString_Argument(Gherkin.Ast.DocString docString)
            {
                ReceivedDocString = docString;
            }
        }

        [Fact]
        public void DigestScenarioStepValues_Sets_DocString_Value()
        {
            //arrange.
            var featureInstance = new FeatureWithDocStringScenarioStep();
            var sut = StepMethodInfo.FromMethodInfo(
                featureInstance.GetType().GetMethod(nameof(FeatureWithDocStringScenarioStep.Step_With_DocString_Argument)),
                featureInstance
                );

            var docStringContent = @"some content
+++
with multi lines
---
in it";

            var step = new TestStep(
                TestStepType.Given,
                FeatureWithDocStringScenarioStep.StepWithDocStringText,
                new TestStepDocStringArgument(docStringContent, null, null));

            //act.
            sut.DigestScenarioStepValues(step);

            //assert.
            var digestedText = sut.GetDigestedStepText();
            Assert.Equal(FeatureWithDocStringScenarioStep.StepWithDocStringText, digestedText);
        }

        [Fact]
        public void FromMethodInfo_Creates_StepMethodInfo_With_Multiple_Patterns()
        {
            //arrange.
            var featureInstance = new FeatureWithMultipleStepPatterns();

            //act.
            var sut = StepMethodInfo.FromMethodInfo(
                featureInstance.GetType().GetMethod(nameof(FeatureWithMultipleStepPatterns.Step_With_Multiple_Patterns)),
                featureInstance);

            //assert.
            Assert.NotNull(sut);
            Assert.Equal(10, sut.ScenarioStepPatterns.Count);

            AssertPattern(0, TestStepType.Given, "something");
            AssertPattern(1, TestStepType.Given, "something else");
            AssertPattern(2, TestStepType.And, "something");
            AssertPattern(3, TestStepType.And, "something else");
            AssertPattern(4, TestStepType.When, "something");
            AssertPattern(5, TestStepType.When, "something else");
            AssertPattern(6, TestStepType.And, "something");
            AssertPattern(7, TestStepType.And, "something else");
            AssertPattern(8, TestStepType.But, "something");
            AssertPattern(9, TestStepType.But, "something else");

            void AssertPattern(int index, TestStepType stepType, string pattern)
            {
                var thePattern = sut.ScenarioStepPatterns[index];

                Assert.NotNull(thePattern);
                Assert.Equal(stepType, thePattern.Type);
                Assert.Equal(pattern, thePattern.OriginalPattern);
            }
        }

        private sealed class FeatureWithMultipleStepPatterns : Feature
        {
            [Given("something")]
            [Given("something else")]
            [And("something")]
            [And("something else")]
            [When("something")]
            [When("something else")]
            [And("something")]
            [And("something else")]
            [But("something")]
            [But("something else")]
            public void Step_With_Multiple_Patterns()
            { }
        }

        [Fact]
        public void FromMethodInfo_DoesNotAllow_AsyncAvoid_Steps()
        {
            //arrange.
            var featureInstance = new FeatureWithAsyncVoidStep();
            var methodInfo = typeof(FeatureWithAsyncVoidStep).GetMethod(nameof(FeatureWithAsyncVoidStep.StepWithAsyncVoid));

            //act / assert.
            Assert.Throws<InvalidOperationException>(() => StepMethodInfo.FromMethodInfo(methodInfo, featureInstance));
        }

        private sealed class FeatureWithAsyncVoidStep : Feature
        {
            [Given("Any step text")]
            public async void StepWithAsyncVoid()
            {
                await Task.CompletedTask;
            }
        }

        [Fact]
        public void GetMethodName_Returns_Wrapped_Method_Name()
        {
            //arrange.
            var featureInstance = new FeatureForMethodName();
            var sut = StepMethodInfo.FromMethodInfo(
                featureInstance.GetType().GetMethod(nameof(FeatureForMethodName.Step_Name_Must_Be_This)),
                featureInstance
                );

            //act.
            var methodName = sut.GetMethodName();

            //assert.
            Assert.Equal(nameof(FeatureForMethodName.Step_Name_Must_Be_This), methodName);
        }

        private sealed class FeatureForMethodName : Feature
        {
            [Given("something")]
            public void Step_Name_Must_Be_This()
            {

            }
        }

        [Theory]
        [InlineData(TestStepType.When)]
        [InlineData(TestStepType.All)]
        public void GetMatchingPattern_Finds_Match_For_Step(TestStepType stepType)
        {
            //arrange.
            var featureInstance = new FeatureForMatch();
            var sut = StepMethodInfo.FromMethodInfo(
                typeof(FeatureForMatch).GetMethod(nameof(FeatureForMatch.Method1)),
                featureInstance);

            //act.
            var match = sut.GetMatchingPattern(new(stepType, "this matches"));

            //assert.
            Assert.NotNull(match);
            Assert.Equal(TestStepType.When, match.Type);
            Assert.Equal("this matches", match.OriginalPattern);
        }

        [Fact]
        public void GetMatchingPattern_Finds_Nothing_When_No_Match()
        {
            //arrange.
            var featureInstance = new FeatureForMatch();
            var sut = StepMethodInfo.FromMethodInfo(
                typeof(FeatureForMatch).GetMethod(nameof(FeatureForMatch.Method1)),
                featureInstance);

            //act.
            var match = sut.GetMatchingPattern(new(TestStepType.When, "this does not matches"));

            //assert.
            Assert.Null(match);
        }

        [Theory]
        [InlineData(TestStepType.When)]
        [InlineData(TestStepType.All)]
        public void Match_IsPositive_For_Corresponding_Step(TestStepType stepType)
        {
            //arrange.
            var featureInstance = new FeatureForMatch();
            var sut = StepMethodInfo.FromMethodInfo(
                typeof(FeatureForMatch).GetMethod(nameof(FeatureForMatch.Method1)),
                featureInstance);

            //act.
            var match = sut.Matches(new(stepType, "this matches"));

            //assert.
            Assert.True(match);
        }

        [Fact]
        public void Match_IsNegative_For_Different_Step()
        {
            //arrange.
            var featureInstance = new FeatureForMatch();
            var sut = StepMethodInfo.FromMethodInfo(
                typeof(FeatureForMatch).GetMethod(nameof(FeatureForMatch.Method1)),
                featureInstance);

            //act.
            var match = sut.Matches(new(TestStepType.When, "this does not matches"));

            //assert.
            Assert.False(match);
        }

        private sealed class FeatureForMatch : Feature
        {
            [When("this matches")]
            public void Method1()
            {
            }
        }
    }
}
