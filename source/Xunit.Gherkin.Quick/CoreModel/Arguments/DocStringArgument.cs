using System;
using Gherkin.Ast;
using Xunit.Gherkin.Quick.TestScenarios;

namespace Xunit.Gherkin.Quick
{
    internal sealed class DocStringArgument : StepMethodArgument
    {
        public override StepMethodArgument Clone()
            => new DocStringArgument();

        public override void DigestScenarioStepValues(string[] argumentValues, object argument)
        {
            if (argument is DocString gherkinDocString)
                Value = gherkinDocString;
            else if (argument is TestStepDocStringArgument testStepDocString)
                Value = new DocString(
                    _MapLocation(testStepDocString.Location),
                    testStepDocString.ContentType,
                    testStepDocString.Content
                );
            else
                throw new InvalidOperationException("DocString cannot be extracted from Gherkin.");
        }

        public override bool IsSameAs(StepMethodArgument other)
            => other is DocStringArgument;
    }
}