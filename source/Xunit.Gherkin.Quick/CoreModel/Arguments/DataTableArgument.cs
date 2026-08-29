using System;
using System.Linq;
using Gherkin.Ast;
using Xunit.Gherkin.Quick.TestScenarios;

namespace Xunit.Gherkin.Quick
{
    internal sealed class DataTableArgument : StepMethodArgument
    {
        public override StepMethodArgument Clone()
            => new DataTableArgument();

        public override void DigestScenarioStepValues(string[] argumentValues, object argument)
        {
            if (argument is DataTable gherkinDataTable)
                Value = gherkinDataTable;
            else if (argument is TestStepTableArgument testStepDataTable)
                Value = new DataTable(
                    testStepDataTable
                        .Rows
                        .Select(row => new TableRow(
                            _MapLocation(row.Location),
                            row
                                .Cells
                                .Select(cell => new TableCell(_MapLocation(cell.Location), cell.Value))
                                .ToArray()
                        ))
                        .ToArray()
                );
            else
                throw new InvalidOperationException("DataTable cannot be extracted from Gherkin.");
        }

        public override bool IsSameAs(StepMethodArgument other)
            => other is DataTableArgument;
    }
}