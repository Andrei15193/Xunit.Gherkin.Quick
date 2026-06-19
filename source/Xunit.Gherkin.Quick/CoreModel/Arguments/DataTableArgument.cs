using System;
using Xunit.Gherkin.Quick.TestScenarios;

namespace Xunit.Gherkin.Quick
{
    internal sealed class DataTableArgument : StepMethodArgument
    {
        public override StepMethodArgument Clone()
            => new DataTableArgument();

        public override void DigestScenarioStepValues(string[] argumentValues, TestStepDocStringArgument docStringArgument)
            => throw new InvalidOperationException("DataTable cannot be extracted from Gherkin.");

        public override void DigestScenarioStepValues(string[] argumentValues, TestStepTableArgument tableArgument)
            => Value = tableArgument ?? throw new InvalidOperationException("DataTable cannot be extracted from Gherkin.");

        public override void DigestScenarioStepValues(string[] argumentValues)
            => throw new InvalidOperationException("DataTable cannot be extracted from Gherkin.");

        public override bool IsSameAs(StepMethodArgument other)
            => other is DataTableArgument;
    }
}