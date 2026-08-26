using System;
using System.Threading.Tasks;

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
        internal Task Scenario()
        {
            throw new NotImplementedException($"Scenario is not implemented.");
        }
    }
}
