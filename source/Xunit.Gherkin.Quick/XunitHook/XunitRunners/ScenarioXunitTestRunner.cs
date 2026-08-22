using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit.Gherkin.Quick
{
    internal sealed class ScenarioXunitTestRunner : XunitTestCaseRunner
    {
        public ScenarioXunitTestRunner(IXunitTestCase testCase, string displayName, string skipReason, object[] constructorArguments, object[] testMethodArguments, IMessageBus messageBus, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource)
            : base(testCase, displayName, skipReason, constructorArguments, testMethodArguments, messageBus, aggregator, cancellationTokenSource)
        {
        }

        protected override XunitTestRunner CreateTestRunner(ITest test, IMessageBus messageBus, Type testClass, object[] constructorArguments, MethodInfo testMethod, object[] testMethodArguments, string skipReason, IReadOnlyList<BeforeAfterTestAttribute> beforeAfterAttributes, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource)
        {
            var testOutputHelper = new TestOutputHelper();
            testOutputHelper.Initialize(messageBus, test);

            var updatedTestMethodArguments = new object[(testMethodArguments?.Length ?? 0) + 1];
            Array.Copy(
                sourceArray: testMethodArguments,
                sourceIndex: 0,
                destinationArray: updatedTestMethodArguments,
                destinationIndex: 1,
                length: testMethodArguments.Length
            );
            updatedTestMethodArguments[0] = testOutputHelper;

            return base.CreateTestRunner(
                test,
                messageBus,
                testClass,
                constructorArguments,
                testMethod,
                updatedTestMethodArguments,
                skipReason,
                beforeAfterAttributes,
                aggregator,
                cancellationTokenSource
            );
        }
    }
}