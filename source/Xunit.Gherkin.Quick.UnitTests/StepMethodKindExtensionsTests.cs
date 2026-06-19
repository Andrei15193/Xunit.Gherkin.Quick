using Xunit;
using Xunit.Gherkin.Quick;
using Xunit.Gherkin.Quick.TestScenarios;

namespace UnitTests
{
    public sealed class StepMethodKindExtensionsTests
    {
        public static object[][] AllStepDefinitionAttributes
        {
            get
            {
                return [
                    [
                        new GivenAttribute("123"),
                        TestStepType.Given
                    ],
                    [
                        new WhenAttribute("123"),
                        TestStepType.When
                    ],
                    [
                        new ThenAttribute("123"),
                        TestStepType.Then
                    ],
                    [
                        new AndAttribute("123"),
                        TestStepType.And
                    ],
                    [
                        new ButAttribute("123"),
                        TestStepType.But
                    ]
                ];
            }
        }

        [Theory]
        [MemberData(nameof(AllStepDefinitionAttributes))]
        internal void ToStepMethodKind_Converts_based_on_Attribute_type(
            BaseStepDefinitionAttribute attribute,
            TestStepType type
        )
        {
            //act.
            var actualType = PatternKindExtensions.ToTestStepType(attribute);

            //assert.
            Assert.Equal(type, actualType);
        }
    }
}
