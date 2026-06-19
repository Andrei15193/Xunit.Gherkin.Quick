using System;
using System.Threading.Tasks;
using Xunit.Gherkin.Quick.TestScenarios;

namespace Xunit.Gherkin.Quick
{
    internal sealed class ScenarioExecutor
    {
        public async Task ExecuteScenarioAsync(Feature featureInstance, TestScenario testScenario)
        {
            if (featureInstance is null)
                throw new ArgumentNullException(nameof(featureInstance));

            if (testScenario is null)
                throw new ArgumentNullException(nameof(testScenario));

            var featureClass = FeatureClass.FromFeatureInstance(featureInstance);

            var scenario = featureClass.ExtractScenario(testScenario);
            await scenario.ExecuteAsync(new ScenarioOutput(featureInstance.InternalOutput));
        }
    }
}
