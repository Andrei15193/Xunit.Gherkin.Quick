using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Gherkin.Ast;
using Xunit.Gherkin.Quick.TestScenarios;

namespace Xunit.Gherkin.Quick
{
    internal sealed class StepMethod
    {
        private readonly StepMethodInfo _stepMethodInfo;

        public string StepText { get; }

        public TestStepType Type { get; }

        public string Pattern { get; }

        private StepMethod(StepMethodInfo stepMethodInfo, TestStepType type, string pattern, string stepText)
        {
            _stepMethodInfo = stepMethodInfo ?? throw new ArgumentNullException(nameof(stepMethodInfo));
            Type = type;
            Pattern = !string.IsNullOrWhiteSpace(pattern) ? pattern : throw new ArgumentNullException(nameof(pattern));

            StepText = !string.IsNullOrWhiteSpace(stepText)
                ? stepText
                : throw new ArgumentNullException(nameof(stepText));

        }

        public static StepMethod FromStepMethodInfo(StepMethodInfo stepMethodInfo, TestStep testStep)
        {
            var matchingPattern = stepMethodInfo.GetMatchingPattern(testStep);

            if (matchingPattern == null)
                throw new InvalidOperationException($"This step method info (`{stepMethodInfo.GetMethodName()}`) cannot handle given scenario step: `{testStep.Type} {testStep.Text.Trim()}`.");

            var stepMethodInfoClone = stepMethodInfo.Clone();
            stepMethodInfoClone.DigestScenarioStepValues(testStep);
            return new StepMethod(stepMethodInfoClone, matchingPattern.Type, matchingPattern.OriginalPattern, testStep.Text);
        }

        public async Task ExecuteAsync()
        {
            await _stepMethodInfo.ExecuteAsync();
        }
    }
}
