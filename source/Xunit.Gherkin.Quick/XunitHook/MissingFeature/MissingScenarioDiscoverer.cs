using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Gherkin;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit.Gherkin.Quick
{
    internal sealed class MissingScenarioDiscoverer : ScenarioDiscoverer, IXunitTestCaseDiscoverer
    {
        public MissingScenarioDiscoverer(IMessageSink messageSink)
            : base(messageSink)
        {
        }

        public override IEnumerable<IXunitTestCase> Discover(
            ITestFrameworkDiscoveryOptions discoveryOptions,
            ITestMethod testMethod,
            IAttributeInfo factAttribute)
        {
            var missingFeatureClass = testMethod.TestClass.Class.ToRuntimeType();
            var missingFeatureClassInfo = MissingFeatureClassInfo.FromMissingFeatureClassType(missingFeatureClass);
            var testAssembly = missingFeatureClass.GetTypeInfo().Assembly;
            var features = new MissingFeatureDiscoveryModel(new FeatureFileRepository(missingFeatureClassInfo.FileNameSearchPattern), new FeatureClassInfoRepository(testAssembly)).Discover();
            try
            {
                return features
                    .SelectMany(feature =>
                    {
                        return GetTestCases(discoveryOptions, feature, testMethod)
                            .DefaultIfEmpty(
                                new ScenarioXunitUnavailableTestCase(
                                    MessageSink,
                                    discoveryOptions.MethodDisplayOrDefault(),
                                    testMethod,
                                    $"'{feature.Name}' :: No Scenarios Defined",
                                    $"Feature file '{feature.Name}' does not contain any scenarios.",
                                    new[] { feature.Name }
                                )
                            );
                    });
            }
            catch (ParserException parserException)
            {
                return Enumerable.Repeat(
                    new ScenarioXunitUnavailableTestCase(
                        MessageSink,
                        discoveryOptions.MethodDisplayOrDefault(),
                        testMethod,
                        $"'{missingFeatureClass.Name}' :: Invalid Feature File",
                        $"The '{missingFeatureClass.Name}' feature file is invalid, {parserException.Message}.",
                        new[] { missingFeatureClassInfo.FileNameSearchPattern }
                    ),
                    1
                );
            }
        }
    }
}
