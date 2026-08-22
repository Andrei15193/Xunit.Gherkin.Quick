using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Gherkin;
using Gherkin.Ast;
using Xunit.Abstractions;
using Xunit.Gherkin.Quick.TestScenarios;
using Xunit.Sdk;

namespace Xunit.Gherkin.Quick
{
    internal class ScenarioDiscoverer : IXunitTestCaseDiscoverer
    {
        private readonly TestScenarioMapper _testScenarioMapper = new TestScenarioMapper(new GherkinDialectProvider());
        private readonly IReadOnlyCollection<string> _IgnoreTags = new List<string> { "ignore" };
        private readonly IMessageSink _messageSink;

        public ScenarioDiscoverer(IMessageSink messageSink)
            => _messageSink = messageSink;

        public IEnumerable<IXunitTestCase> Discover(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, IAttributeInfo factAttribute)
        {
            var featureClassType = testMethod.Method.Type.ToRuntimeType();
            var assembly = featureClassType.GetTypeInfo().Assembly;
            var featureFileRepository = new FeatureFileRepository(TestAssemblyInfo.FromAssembly(assembly).FeatureFileSearchPattern);

            var featureClassInfo = FeatureClassInfo.FromFeatureClassType(featureClassType);

            try
            {
                var featurePathsAndFiles = featureClassInfo.PathInfo.GetMatchingFeatures(featureFileRepository);

                return featurePathsAndFiles
                    .SelectMany(featurePath =>
                    {
                        return _GetTestCases(discoveryOptions, featurePath.Feature, testMethod)
                            .DefaultIfEmpty(
                                new ScenarioXunitUnavailableTestCase(
                                    _messageSink,
                                    discoveryOptions.MethodDisplayOrDefault(),
                                    testMethod,
                                    $"'{featurePath.Feature.Name}' :: No Scenarios Defined",
                                    $"Feature file '{featurePath.Feature.Name}' does not contain any scenarios."
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
                        $"'{featureClassType.Name}' :: Invalid Feature File",
                        $"The '{featureClassType.Name}' feature file is invalid, {parserException.Message}."
                    ),
                    1
                );
            }
        }

        private IEnumerable<IXunitTestCase> _GetTestCases(ITestFrameworkDiscoveryOptions discoveryOptions, global::Gherkin.Ast.Feature feature, ITestMethod testMethod)
        {
            Background scenarioBackground = null;
            foreach (var scenarioDefinition in feature.Children)
                if (scenarioDefinition is global::Gherkin.Ast.Background background)
                    scenarioBackground = background;
                else if (scenarioDefinition is global::Gherkin.Ast.Scenario scenario)
                    yield return _GetScenarioTestCase(discoveryOptions, testMethod, feature, scenarioBackground, scenario);
                else if (scenarioDefinition is global::Gherkin.Ast.ScenarioOutline scenarioOutline)
                    foreach (var testCase in _GetScenarioOutlineTestCases(discoveryOptions, testMethod, feature, scenarioBackground, scenarioOutline))
                        yield return testCase;
        }

        private IXunitTestCase _GetScenarioTestCase(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, global::Gherkin.Ast.Feature feature, Background scenarioBackground, global::Gherkin.Ast.Scenario scenario)
        {
            var displayName = _GetDisplayName(feature, scenario);
            var testScenario = _testScenarioMapper.Map(feature, _ApplyBackground(scenario, scenarioBackground));

            if (_IsIgnored(testScenario))
                return new ScenarioXunitUnavailableTestCase(
                    _messageSink,
                    discoveryOptions.MethodDisplayOrDefault(),
                    testMethod,
                    displayName,
                    "This scenario is skipped"
                );
            else
                return new ScenarioXunitTestCase(
                    _messageSink,
                    discoveryOptions.MethodDisplayOrDefault(),
                    testMethod,
                    displayName,
                    testScenario
                );
        }

        private IEnumerable<IXunitTestCase> _GetScenarioOutlineTestCases(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, global::Gherkin.Ast.Feature feature, global::Gherkin.Ast.Background scenarioBackground, global::Gherkin.Ast.ScenarioOutline scenarioOutline)
        {
            if (scenarioOutline.Examples is null || !scenarioOutline.Examples.Any())
                yield return new ScenarioXunitUnavailableTestCase(
                    _messageSink,
                    discoveryOptions.MethodDisplayOrDefault(),
                    testMethod,
                    $"{_GetDisplayName(feature, scenarioOutline)} :: No Examples Defined",
                    $"Scenario outline '{scenarioOutline.Name}' does not contain any examples."
                );
            else
                foreach (var example in scenarioOutline.Examples)
                {
                    var displayName = _GetDisplayName(feature, scenarioOutline, example);

                    if (example.TableHeader is null || example.TableBody is null || !example.TableBody.Any())
                        yield return new ScenarioXunitUnavailableTestCase(
                            _messageSink,
                            discoveryOptions.MethodDisplayOrDefault(),
                            testMethod,
                            $"{displayName} :: No Cases Defined",
                            $"Example '{example.Name}' for scenario outline '{scenarioOutline.Name}' does not contain any cases."
                        );
                    else if (
                            example
                                .TableHeader
                                .Cells
                                .GroupBy(headerCell => headerCell.Value, StringComparer.OrdinalIgnoreCase)
                                .Any(group => group.Count() > 1)
                        )
                        yield return new ScenarioXunitUnavailableTestCase(
                            _messageSink,
                            discoveryOptions.MethodDisplayOrDefault(),
                            testMethod,
                            $"{displayName} :: Duplicate Parameters",
                            $"Example '{example.Name}' for scenario outline '{scenarioOutline.Name}' contains multiple parameters with the same name (case-insensitive check)."
                        );
                    else
                        foreach (var testCase in _GetScenarioOutlineExampleTestCases(discoveryOptions, testMethod, feature, scenarioBackground, scenarioOutline, example))
                            yield return testCase;
                }
        }

        private IEnumerable<IXunitTestCase> _GetScenarioOutlineExampleTestCases(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, global::Gherkin.Ast.Feature feature, global::Gherkin.Ast.Background scenarioBackground, global::Gherkin.Ast.ScenarioOutline scenarioOutline, global::Gherkin.Ast.Examples example)
        {
            var generatedScenario = _ApplyBackground(
                new global::Gherkin.Ast.Scenario(
                    (scenarioOutline.Tags ?? Enumerable.Empty<Tag>())
                        .Concat(example.Tags ?? Enumerable.Empty<Tag>())
                        .ToArray(),
                    scenarioOutline.Location,
                    scenarioOutline.Keyword,
                    scenarioOutline.Name,
                    scenarioOutline.Description,
                    scenarioOutline.Steps as Step[] ?? scenarioOutline.Steps?.ToArray()
                ),
                scenarioBackground
            );

            foreach (var @case in example.TableBody)
            {
                var arguments = example
                    .TableHeader
                    .Cells
                    .Zip(@case.Cells, (headerCell, caseCell) => new { Name = headerCell.Value, Value = caseCell.Value })
                    .ToDictionary(argument => argument.Name, argument => argument.Value, StringComparer.OrdinalIgnoreCase);

                var argumentsDisplay = string.Join(
                    ", ",
                    example
                        .TableHeader
                        .Cells
                        .Zip(@case.Cells, (headerCell, caseCell) => $"{headerCell.Value} = {caseCell.Value}")
                );

                var displayName = $"{_GetDisplayName(feature, scenarioOutline, example)} ({argumentsDisplay})";
                var testScenario = _testScenarioMapper.Map(feature, generatedScenario, arguments);
                if (_IsIgnored(testScenario))
                    yield return new ScenarioXunitUnavailableTestCase(
                        _messageSink,
                        discoveryOptions.MethodDisplayOrDefault(),
                        testMethod,
                        displayName,
                        "This scenario is skipped"
                    );
                else
                    yield return new ScenarioXunitTestCase(
                        _messageSink,
                        discoveryOptions.MethodDisplayOrDefault(),
                        testMethod,
                        displayName,
                        testScenario
                    );
            }
        }

        private bool _IsIgnored(TestScenario testScenario)
            => _IgnoreTags.Any(ignoreTag => testScenario.Tags.Contains(ignoreTag, StringComparer.OrdinalIgnoreCase));

        private static string _GetDisplayName(params IHasDescription[] hasDescriptions)
            => string.Join(" :: ", hasDescriptions.Select(hasDescription => hasDescription.Name).Where(name => !string.IsNullOrWhiteSpace(name)));

        private static global::Gherkin.Ast.Scenario _ApplyBackground(global::Gherkin.Ast.Scenario scenario, global::Gherkin.Ast.Background background)
            => background is null || !background.Steps.Any()
            ? scenario
            : new global::Gherkin.Ast.Scenario(
                scenario.Tags as Tag[] ?? scenario.Tags.ToArray(),
                scenario.Location,
                scenario.Keyword,
                scenario.Name,
                scenario.Description,
                background.Steps.Concat(scenario.Steps).ToArray()
            );
    }
}