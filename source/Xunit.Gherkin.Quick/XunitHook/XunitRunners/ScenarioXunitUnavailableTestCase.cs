using System;
using System.ComponentModel;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit.Gherkin.Quick
{
    internal sealed class ScenarioXunitUnavailableTestCase : XunitTestCase
    {
        private string _skipReason;
        private string _displayName;

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
        public ScenarioXunitUnavailableTestCase()
        {
        }

        public ScenarioXunitUnavailableTestCase(
            IMessageSink diagnosticMessageSink,
            TestMethodDisplay testMethodDisplay,
            ITestMethod testMethod,
            string displayName,
            string skipReason,
            object[] testMethodArguments = null
        )
            : base(diagnosticMessageSink, testMethodDisplay, testMethod, testMethodArguments)
        {
            if (displayName is null)
                throw new ArgumentNullException(nameof(displayName));

            _displayName = displayName;
            _skipReason = skipReason;
        }

        public override void Serialize(IXunitSerializationInfo data)
        {
            data.AddValue(nameof(_displayName), _displayName, typeof(string));
            data.AddValue(nameof(_skipReason), _skipReason, typeof(string));
            base.Serialize(data);
        }

        public override void Deserialize(IXunitSerializationInfo data)
        {
            base.Deserialize(data);
            _displayName = data.GetValue<string>(nameof(_displayName));
            _skipReason = data.GetValue<string>(nameof(_skipReason));
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

        protected override string GetSkipReason(IAttributeInfo factAttribute)
            => _skipReason ?? base.GetSkipReason(factAttribute);
    }
}