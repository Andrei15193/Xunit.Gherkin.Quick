using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Gherkin;
using Gherkin.Ast;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit.Gherkin.Quick
{
    internal sealed class MissingScenarioDiscoverer : IXunitTestCaseDiscoverer
    {
        private readonly IMessageSink _messageSink;

        public MissingScenarioDiscoverer(IMessageSink messageSink)
        {
            _messageSink = messageSink;
        }

        public IEnumerable<IXunitTestCase> Discover(
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
                        return _GetTestCases(discoveryOptions, feature, testMethod)
                            .DefaultIfEmpty(
                                new ScenarioXunitUnavailableTestCase(
                                    _messageSink,
                                    discoveryOptions.MethodDisplayOrDefault(),
                                    testMethod,
                                    $"'{feature.Name}' :: No Scenarios Defined",
                                    $"Feature file '{feature.Name}' does not contain any scenarios."
                                )
                            );
                    });
            }
            catch (ParserException parserException)
            {
                return Enumerable.Repeat(
                    new ScenarioXunitUnavailableTestCase(
                        _messageSink,
                        discoveryOptions.MethodDisplayOrDefault(),
                        testMethod,
                        $"'{missingFeatureClass.Name}' :: Invalid Feature File",
                        $"The '{missingFeatureClass.Name}' feature file is invalid, {parserException.Message}."
                    ),
                    1
                );
            }
        }

        private IEnumerable<IXunitTestCase> _GetTestCases(ITestFrameworkDiscoveryOptions discoveryOptions, global::Gherkin.Ast.Feature feature, ITestMethod testMethod)
        {
            foreach (var scenarioDefinition in feature.Children)
                if (scenarioDefinition is global::Gherkin.Ast.Scenario scenario)
                    yield return new ScenarioXunitUnavailableTestCase(
                        _messageSink,
                        discoveryOptions.MethodDisplayOrDefault(),
                        testMethod,
                        _GetDisplayName(feature, scenario),
                        $"Scenario `{scenario.Name}` is not implemented."
                    );
                else if (scenarioDefinition is global::Gherkin.Ast.ScenarioOutline scenarioOutline)
                {
                    var exampleCount = 1;
                    foreach (var example in scenarioOutline.Examples)
                    {
                        yield return new ScenarioXunitUnavailableTestCase(
                            _messageSink,
                            discoveryOptions.MethodDisplayOrDefault(),
                            testMethod,
                            _GetDisplayName(feature, scenarioOutline),
                            $"Scenario outline `{scenarioOutline.Name}`, example `{example.Name}` `#{exampleCount}` is not implemented."
                        );
                        exampleCount++;
                    }
                }
        }

        private static string _GetDisplayName(params IHasDescription[] hasDescriptions)
            => string.Join(" :: ", hasDescriptions.Select(hasDescription => hasDescription.Name).Where(name => !string.IsNullOrWhiteSpace(name)));
    }
}
