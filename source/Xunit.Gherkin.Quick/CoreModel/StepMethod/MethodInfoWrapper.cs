using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Gherkin.Ast;
using Xunit.Gherkin.Quick.TestScenarios;

namespace Xunit.Gherkin.Quick
{
    internal sealed class MethodInfoWrapper
    {
        private readonly MethodInfo _methodInfo;
        private readonly object _target;

        private MethodInfoWrapper(MethodInfo methodInfo, object target)
        {
            _methodInfo = methodInfo ?? throw new ArgumentNullException(nameof(methodInfo));
            _target = target ?? throw new ArgumentNullException(nameof(target));
        }

        public static MethodInfoWrapper FromMethodInfo(MethodInfo methodInfo, object target)
        {
            if (IsAsyncMethod(methodInfo) && methodInfo.ReturnType == typeof(void))
            {
                throw new InvalidOperationException($"Method `{methodInfo.Name}` of `{methodInfo.DeclaringType.Name}` class is async and void, which looks like a mistake. Use either async with Task or void without async.");
            }

            return new MethodInfoWrapper(methodInfo, target);
        }

        private static bool IsAsyncMethod(MethodInfo method)
        {
            Type attType = typeof(AsyncStateMachineAttribute);
            var attrib = (AsyncStateMachineAttribute)method.GetCustomAttribute(attType);

            return (attrib != null);
        }

        public async Task InvokeMethodAsync(object[] parameters)
        {
            var result = _methodInfo.Invoke(_target, _MapParameters(parameters));
            if (result is Task resultAsTask)
                await resultAsTask;
        }

        public bool IsSameAs(MethodInfoWrapper other)
        {
            if (this == other)
                return true;

            return other != null
                && other._methodInfo.Equals(_methodInfo)
                && other._target == _target;
        }

        public string GetMethodName()
        {
            return _methodInfo.Name;
        }

        private object[] _MapParameters(object[] parameters)
        {
            var parameterInfos = _methodInfo.GetParameters();

            return parameters
                ?.Select((parameter, parameterIndex) =>
                {
                    if (parameterInfos[parameterIndex].ParameterType == typeof(DocString))
                        return _MapToDocString((TestStepDocStringArgument)parameter);
                    else if (parameterInfos[parameterIndex].ParameterType == typeof(DataTable))
                        return _MapToDataTable((TestStepTableArgument)parameter);
                    else
                        return parameter;
                })
                .ToArray();
        }

        private static DocString _MapToDocString(TestStepDocStringArgument docString)
            => new DocString(_MapToLocation(docString.Location), docString.ContentType, docString.Content);

        private static DataTable _MapToDataTable(TestStepTableArgument dataTable)
            => new DataTable(
                dataTable
                    .Rows
                    .Select(row => new TableRow(
                        _MapToLocation(row.Location),
                        row
                            .Cells
                            .Select(cell => new TableCell(_MapToLocation(cell.Location), cell.Value))
                            .ToArray()
                    ))
                    .ToArray()
            );

        private static Location _MapToLocation(TestStepArgumentLocation location)
            => location is null ? null : new Location(location.Line, location.Column);
    }
}
