using System;
using Xunit.Gherkin.Quick.TestScenarios;

namespace Xunit.Gherkin.Quick
{
    internal sealed class DocStringArgument : StepMethodArgument
    {
        public override StepMethodArgument Clone()
            => new DocStringArgument();

        public override void DigestScenarioStepValues(string[] argumentValues, TestStepDocStringArgument docStringArgument)
            => Value = docStringArgument ?? throw new InvalidOperationException("DocString cannot be extracted from Gherkin.");

        public override void DigestScenarioStepValues(string[] argumentValues, TestStepTableArgument tableArgument)
            => throw new InvalidOperationException("DocString cannot be extracted from Gherkin.");

        public override void DigestScenarioStepValues(string[] argumentValues)
            => throw new InvalidOperationException("DocString cannot be extracted from Gherkin.");

        public override bool IsSameAs(StepMethodArgument other)
            => other is DocStringArgument;
    }
}