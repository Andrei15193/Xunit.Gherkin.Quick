using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Gherkin.Ast;
using Xunit.Gherkin.Quick.TestScenarios;

namespace Xunit.Gherkin.Quick
{
    internal abstract class StepMethodArgument
    {
        public static List<StepMethodArgument> ListFromMethodInfo(MethodInfo methodInfo)
        {
            return methodInfo.GetParameters().Select((p, i) => FromParameter(p, i))
                .ToList();
        }

        private static StepMethodArgument FromParameter(ParameterInfo parameter, int parameterIndex)
        {
            if (parameter.ParameterType.Equals(typeof(DataTable)))
                return new DataTableArgument();

            if (parameter.ParameterType.Equals(typeof(DocString)))
                return new DocStringArgument();

            return new PrimitiveTypeArgument(parameter, parameterIndex);
        }

        public object Value { get; protected set; }

        public abstract bool IsSameAs(StepMethodArgument other);

        public abstract StepMethodArgument Clone();

        public abstract void DigestScenarioStepValues(string[] argumentValues, object argument);

        protected static Location _MapLocation(TestStepArgumentLocation location)
        {
            if (location is null)
                return null;

            return new Location(location.Line, location.Column);
        }
    }
}
