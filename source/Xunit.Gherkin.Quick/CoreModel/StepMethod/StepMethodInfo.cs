using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xunit.Gherkin.Quick.TestScenarios;

namespace Xunit.Gherkin.Quick
{
    internal sealed class StepMethodInfo
    {
        public ReadOnlyCollection<ScenarioStepPattern> ScenarioStepPatterns { get; }

        private readonly ReadOnlyCollection<StepMethodArgument> _arguments;

        private readonly MethodInfoWrapper _methodInfoWrapper;
        public string GetMethodName()
        {
            return _methodInfoWrapper.GetMethodName();
        }

        private string _lastDigestedStepText;

        private StepMethodInfo(
            IEnumerable<ScenarioStepPattern> scenarioStepPatterns,
            IEnumerable<StepMethodArgument> arguments,
            MethodInfoWrapper methodInfoWrapper)
        {
            ScenarioStepPatterns = scenarioStepPatterns != null
                ? scenarioStepPatterns.ToList().AsReadOnly()
                : throw new ArgumentNullException(nameof(scenarioStepPatterns));

            _arguments = arguments != null
                ? arguments.ToList().AsReadOnly()
                : throw new ArgumentNullException(nameof(arguments));

            _methodInfoWrapper = methodInfoWrapper;
        }

        public string GetDigestedStepText()
        {
            if (_lastDigestedStepText == null)
                throw new InvalidOperationException($"Not yet digested. Call `{nameof(DigestScenarioStepValues)}` first.");

            return _lastDigestedStepText;
        }

        public static StepMethodInfo FromMethodInfo(MethodInfo methodInfo, Feature featureInstance)
        {
            if (methodInfo == null)
                throw new ArgumentNullException(nameof(methodInfo));

            var stepDefinitionAttribute = methodInfo.GetCustomAttributes<BaseStepDefinitionAttribute>();

            return new StepMethodInfo(
                ScenarioStepPattern.ListFromStepAttributes(stepDefinitionAttribute),
                StepMethodArgument.ListFromMethodInfo(methodInfo),
                MethodInfoWrapper.FromMethodInfo(methodInfo, featureInstance));
        }

        public bool IsSameAs(StepMethodInfo other)
        {
            if (other == this)
                return true;

            return other != null
                && other._methodInfoWrapper.IsSameAs(_methodInfoWrapper);
        }

        public async Task ExecuteAsync()
        {
            await _methodInfoWrapper.InvokeMethodAsync(_arguments.Select(arg => arg.Value).ToArray());
        }

        public StepMethodInfo Clone()
        {
            var argumentsClone = _arguments.Select(arg => arg.Clone());

            return new StepMethodInfo(ScenarioStepPatterns, argumentsClone, _methodInfoWrapper);
        }

        public void DigestScenarioStepValues(TestStep testStep)
        {
            if (_arguments.Count == 0)
                return;

            var matchingPattern = GetMatchingPattern(testStep);
            var testStepText = testStep.Text.Trim();

            if (matchingPattern == null)
                throw new InvalidOperationException($"This step (`{_methodInfoWrapper.GetMethodName()}`) cannot handle scenario step `{testStep.Type} {testStepText}`.");

            var argumentValuesFromStep = Regex.Match(testStepText, matchingPattern.RegexPattern).Groups.Cast<Group>()
                .Skip(1)
                .Select(g => g.Value)
                .ToArray();

            foreach (var arg in _arguments)
            {
                if (testStep.DocStringArgument is object)
                    arg.DigestScenarioStepValues(argumentValuesFromStep, testStep.DocStringArgument);
                else if (testStep.TableArgument is object)
                    arg.DigestScenarioStepValues(argumentValuesFromStep, testStep.TableArgument);
                else
                    arg.DigestScenarioStepValues(argumentValuesFromStep);
            }

            _lastDigestedStepText = testStepText;
        }

        public ScenarioStepPattern GetMatchingPattern(TestStep testStep)
        {
            var testStepText = testStep.Text.Trim();

            foreach (var pattern in ScenarioStepPatterns)
            {
                if ((pattern.Type & testStep.Type) != pattern.Type)
                    continue;

                var match = Regex.Match(testStepText, pattern.RegexPattern);
                if (!match.Success || !match.Value.Equals(testStepText))
                    continue;

                return pattern;
            }

            return null;
        }

        public bool Matches(TestStep testStep)
        {
            var matchingPattern = GetMatchingPattern(testStep);
            var isMatch = matchingPattern != null;
            return isMatch;
        }
    }

    internal static class PatternKindExtensions
    {
        public static TestStepType ToTestStepType(BaseStepDefinitionAttribute @this)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));

            switch (@this)
            {
                case GivenAttribute _:
                    return TestStepType.Given;

                case WhenAttribute _:
                    return TestStepType.When;

                case ThenAttribute _:
                    return TestStepType.Then;

                case AndAttribute _:
                    return TestStepType.And;

                case ButAttribute _:
                    return TestStepType.But;

                default:
                    throw new NotSupportedException($"Cannot convert into step method kind: Attribute type {@this.GetType()} is not supported.");
            }
        }
    }
}
