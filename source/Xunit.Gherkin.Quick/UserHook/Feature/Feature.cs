using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Gherkin.Quick.TestScenarios;

namespace Xunit.Gherkin.Quick
{
    /// <summary>
    /// Base class for feature classes.
    /// Derived classes should define scenario step methods by using
    /// <see cref="GivenAttribute"/>, <see cref="WhenAttribute"/>, 
    /// <see cref="ThenAttribute"/>, <see cref="AndAttribute"/>, 
    /// and <see cref="ButAttribute"/>.
    /// Derived classes can also specify the feature text file by using
    /// <see cref="FeatureFileAttribute"/>.
    /// </summary>
    public abstract class Feature : FeatureBase
    {
        [Scenario]
        internal async Task Scenarios(ITestOutputHelper testOutputHelper, TestScenario testScenario)
        {
            InternalOutput = testOutputHelper;
            var featureEvaluator = new ScenarioExecutor();

            await featureEvaluator.ExecuteScenarioAsync(this, testScenario);
        }
    }
}
