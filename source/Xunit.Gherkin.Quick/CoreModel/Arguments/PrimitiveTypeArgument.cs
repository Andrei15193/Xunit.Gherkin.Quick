using System;
using System.Reflection;

namespace Xunit.Gherkin.Quick
{
    internal sealed class PrimitiveTypeArgument : StepMethodArgument
    {
        private readonly ParameterInfo _parameterInfo;
        private readonly int _index;

        public PrimitiveTypeArgument(ParameterInfo parameterInfo, int index)
        {
            _parameterInfo = parameterInfo ?? throw new ArgumentNullException(nameof(parameterInfo));
            _index = index;
        }

        public override StepMethodArgument Clone()
            => new PrimitiveTypeArgument(_parameterInfo, _index);

        public override void DigestScenarioStepValues(string[] argumentValues, object argument)
        {
            if (argumentValues.Length <= _index)
                throw new InvalidOperationException($"Cannot extract value for parameter `{_parameterInfo.Name}` at index {_index}; only {argumentValues.Length} parameters were provided. Method `{_parameterInfo.Member.Name}`.");

            Value = Convert.ChangeType(argumentValues[_index], _parameterInfo.ParameterType);
        }

        public override bool IsSameAs(StepMethodArgument other)
            => other is PrimitiveTypeArgument otherPrimitive && otherPrimitive._index == _index;
    }
}
