using Moq;
using System;
using Xunit;
using Xunit.Gherkin.Quick;
using Xunit.Gherkin.Quick.TestScenarios;

namespace UnitTests
{
    public sealed class DocStringArgumentTests
    {
        [Fact]
        public void DigestScenarioStepValues_Throws_Error_If_No_Arguments_And_No_DocString()
        {
            //arrange.
            var sut = new DocStringArgument();

            //act / assert.
            Assert.Throws<InvalidOperationException>(() => sut.DigestScenarioStepValues([], (TestStepDocStringArgument)null));
        }

        [Fact]
        public void DigestScenarioStepValues_Throws_Error_If_Arguments_Present_But_No_DocString()
        {
            //arrange.
            var sut = new DocStringArgument();

            //act / assert.
            Assert.Throws<InvalidOperationException>(() => sut.DigestScenarioStepValues(["1", "2", "3"], (TestStepDocStringArgument)null));
        }

        [Fact]
        public void DigestScenarioStepValues_Sets_Value_As_DocString_When_Only_DocString()
        {
            //arrange.
            var sut = new DocStringArgument();
            var docStringContent = @"some content
+++
with multi lines
---
in it";
            var docString = new TestStepDocStringArgument(docStringContent, null, null);

            //act.
            sut.DigestScenarioStepValues([], docString);

            //assert.
            Assert.Same(docString, sut.Value);
        }

        [Fact]
        public void DigestScenarioStepValues_Sets_Value_As_DataTable_When_DataTable_And_Other_Args_Present()
        {
            //arrange.
            var sut = new DocStringArgument();
            var docStringContent = @"some content
+++
with multi lines
---
in it";
            var docString = new TestStepDocStringArgument(docStringContent, null, null);

            //act.
            sut.DigestScenarioStepValues(["1", "2", "3"], docString);

            //assert.
            Assert.Same(docString, sut.Value);
        }
        
        [Fact]
        public void IsSameAs_Identifies_Similar_Instances()
        {
            //arrange.
            var sut = new DocStringArgument();
            var other = new DocStringArgument();

            //act.
            var same = sut.IsSameAs(other);

            //assert.
            Assert.True(same);
        }

        [Fact]
        public void IsSameAs_Distinguishes_Different_Instances()
        {
            //arrange.
            var sut = new DocStringArgument();
            var other = new Mock<StepMethodArgument>().Object;

            //act.
            var same = sut.IsSameAs(other);

            //assert.
            Assert.False(same);
        }

        [Fact]
        public void Clone_Creates_Similar_Instance()
        {
            //arrange.
            var sut = new DocStringArgument();

            //act.
            var clone = sut.Clone();

            //assert.
            Assert.True(clone.IsSameAs(sut));
            Assert.NotSame(clone, sut);
        }
    }
}
