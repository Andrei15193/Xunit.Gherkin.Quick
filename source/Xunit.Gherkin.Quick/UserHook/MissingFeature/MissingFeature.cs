using System;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Gherkin.Quick.TestScenarios;

namespace Xunit.Gherkin.Quick
{
    /// <summary>
    /// Base class which you need to inherit if you want to handle not implemented features 
    /// (i.e., feature files that don't have corresponding feature classes).
    /// If you want to ignore such feature files, don't inherit this class.
    /// Derived classes can also specify the feature text file name search pattern via
    /// <see cref="FeatureFileSearchPatternAttribute"/>.
    /// </summary>
    public abstract class MissingFeature : FeatureBase
    {
        [MissingScenario]
        internal Task Scenario(ITestOutputHelper testOutputHelper, TestScenario testScenario)
        {
            throw new NotImplementedException($"Scenario `{testScenario.ScenarioName}` is not implemented.");
        }
    }
}
