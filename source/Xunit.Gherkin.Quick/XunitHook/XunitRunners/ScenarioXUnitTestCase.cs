using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Gherkin.Quick.TestScenarios;
using Xunit.Sdk;

namespace Xunit.Gherkin.Quick
{
    internal sealed class ScenarioXunitTestCase : XunitTestCase
    {
        private string _displayName;

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
        public ScenarioXunitTestCase()
        {
        }

        public ScenarioXunitTestCase(
            IMessageSink diagnosticMessageSink,
            TestMethodDisplay testMethodDisplay,
            ITestMethod testMethod,
            string displayName,
            TestScenario testScenario
        )
            : base(diagnosticMessageSink, testMethodDisplay, testMethod, new[] { testScenario })
        {
            _displayName = displayName;
        }

        public override Task<RunSummary> RunAsync(IMessageSink diagnosticMessageSink, IMessageBus messageBus, object[] constructorArguments, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource)
            => new ScenarioXunitTestRunner(
                this,
                DisplayName,
                SkipReason,
                constructorArguments,
                TestMethodArguments,
                messageBus,
                aggregator,
                cancellationTokenSource
            ).RunAsync();

        public override void Serialize(IXunitSerializationInfo data)
        {
            data.AddValue(nameof(_displayName), _displayName, typeof(string));
            base.Serialize(data);
        }

        public override void Deserialize(IXunitSerializationInfo data)
        {
            base.Deserialize(data);
            _displayName = data.GetValue<string>(nameof(_displayName));
        }

        protected override string GetDisplayName(IAttributeInfo factAttribute, string displayName)
        {
            switch (DefaultMethodDisplay)
            {
                case TestMethodDisplay.Method:
                    return _displayName;

                default:
                case TestMethodDisplay.ClassAndMethod:
                    return $"{TestMethod.TestClass.Class.Name}.{_displayName}";
            }
        }

        protected override void Initialize()
        {
            base.Initialize();

            var testScenario = TestMethodArguments.OfType<TestScenario>().Single();

            // These traits allow support for the picklesdoc results visualizer (http://www.picklesdoc.com/)
            Traits["FeatureTitle"] = new List<string> { testScenario.FeatureName };
            Traits["Description"] = new List<string> { testScenario.ScenarioName };

            Traits["Category"] = new List<string>(testScenario.Tags);
        }
    }
}