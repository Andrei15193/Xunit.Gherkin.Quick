using Gherkin.Ast;
using Moq;
using System;
using Xunit;
using Xunit.Gherkin.Quick;
using Xunit.Gherkin.Quick.TestScenarios;

namespace UnitTests
{
    public sealed class DataTableArgumentTests
    {
        [Fact]
        public void DigestScenarioStepValues_Throws_Error_If_No_Arguments_And_No_DataTable()
        {
            //arrange.
            var sut = new DataTableArgument();

            //act / assert.
            Assert.Throws<InvalidOperationException>(() => sut.DigestScenarioStepValues([], (TestStepTableArgument)null));
        }

        [Fact]
        public void DigestScenarioStepValues_Throws_Error_If_Arguments_Present_But_No_DataTable()
        {
            //arrange.
            var sut = new DataTableArgument();

            //act / assert.
            Assert.Throws<InvalidOperationException>(() => sut.DigestScenarioStepValues(["1", "2", "3"], (TestStepTableArgument)null));
        }

        [Fact]
        public void DigestScenarioStepValues_Sets_Value_As_DataTable_When_Only_DataTable()
        {
            //arrange.
            var sut = new DataTableArgument();
            var dataTable = new TestStepTableArgument([
                new(
                    [
                        new("First argument", null),
                        new("Second argument", null),
                        new("Result", null)
                    ],
                    null
                ),
                new(
                    [
                        new("1", null),
                        new("2", null),
                        new("3", null)
                    ],
                    null
                ),
                new(
                    [
                        new("a", null),
                        new("b", null),
                        new("c", null)
                    ],
                    null
                )
            ]);

            //act.
            sut.DigestScenarioStepValues([], dataTable);

            //assert.
            var gherkinDataTable = Assert.IsType<DataTable>(sut.Value);
            Assert.Collection(
                gherkinDataTable.Rows,
                firstRow => Assert.Multiple(
                    () => Assert.Null(firstRow.Location),
                    () => Assert.Collection(
                        firstRow.Cells,
                        firstCell => Assert.Multiple(
                            () => Assert.Null(firstCell.Location),
                            () => Assert.Equal("First argument", firstCell.Value)
                        ),
                        secondCell => Assert.Multiple(
                            () => Assert.Null(secondCell.Location),
                            () => Assert.Equal("Second argument", secondCell.Value)
                        ),
                        thirdCell => Assert.Multiple(
                            () => Assert.Null(thirdCell.Location),
                            () => Assert.Equal("Result", thirdCell.Value)
                        )
                    )
                ),
                secondRow => Assert.Multiple(
                    () => Assert.Null(secondRow.Location),
                    () => Assert.Collection(
                        secondRow.Cells,
                        firstCell => Assert.Multiple(
                            () => Assert.Null(firstCell.Location),
                            () => Assert.Equal("1", firstCell.Value)
                        ),
                        secondCell => Assert.Multiple(
                            () => Assert.Null(secondCell.Location),
                            () => Assert.Equal("2", secondCell.Value)
                        ),
                        thirdCell => Assert.Multiple(
                            () => Assert.Null(thirdCell.Location),
                            () => Assert.Equal("3", thirdCell.Value)
                        )
                    )
                ),
                thirdRow => Assert.Multiple(
                    () => Assert.Null(thirdRow.Location),
                    () => Assert.Collection(
                        thirdRow.Cells,
                        firstCell => Assert.Multiple(
                            () => Assert.Null(firstCell.Location),
                            () => Assert.Equal("a", firstCell.Value)
                        ),
                        secondCell => Assert.Multiple(
                            () => Assert.Null(secondCell.Location),
                            () => Assert.Equal("b", secondCell.Value)
                        ),
                        thirdCell => Assert.Multiple(
                            () => Assert.Null(thirdCell.Location),
                            () => Assert.Equal("c", thirdCell.Value)
                        )
                    )
                )
            );
        }

        [Fact]
        public void DigestScenarioStepValues_Sets_Value_As_DataTable_When_DataTable_And_Other_Args_Present()
        {
            //arrange.
            var sut = new DataTableArgument();
            var dataTable = new TestStepTableArgument([
                new(
                    [
                        new("First argument", null),
                        new("Second argument", null),
                        new("Result", null)
                    ],
                    null
                ),
                new(
                    [
                        new("1", null),
                        new("2", null),
                        new("3", null)
                    ],
                    null
                ),
                new(
                    [
                        new("a", null),
                        new("b", null),
                        new("c", null)
                    ],
                    null
                )
            ]);

            //act.
            sut.DigestScenarioStepValues(["1", "2", "3"], dataTable);

            //assert.
            var gherkinDataTable = Assert.IsType<DataTable>(sut.Value);
            Assert.Collection(
                gherkinDataTable.Rows,
                firstRow => Assert.Multiple(
                    () => Assert.Null(firstRow.Location),
                    () => Assert.Collection(
                        firstRow.Cells,
                        firstCell => Assert.Multiple(
                            () => Assert.Null(firstCell.Location),
                            () => Assert.Equal("First argument", firstCell.Value)
                        ),
                        secondCell => Assert.Multiple(
                            () => Assert.Null(secondCell.Location),
                            () => Assert.Equal("Second argument", secondCell.Value)
                        ),
                        thirdCell => Assert.Multiple(
                            () => Assert.Null(thirdCell.Location),
                            () => Assert.Equal("Result", thirdCell.Value)
                        )
                    )
                ),
                secondRow => Assert.Multiple(
                    () => Assert.Null(secondRow.Location),
                    () => Assert.Collection(
                        secondRow.Cells,
                        firstCell => Assert.Multiple(
                            () => Assert.Null(firstCell.Location),
                            () => Assert.Equal("1", firstCell.Value)
                        ),
                        secondCell => Assert.Multiple(
                            () => Assert.Null(secondCell.Location),
                            () => Assert.Equal("2", secondCell.Value)
                        ),
                        thirdCell => Assert.Multiple(
                            () => Assert.Null(thirdCell.Location),
                            () => Assert.Equal("3", thirdCell.Value)
                        )
                    )
                ),
                thirdRow => Assert.Multiple(
                    () => Assert.Null(thirdRow.Location),
                    () => Assert.Collection(
                        thirdRow.Cells,
                        firstCell => Assert.Multiple(
                            () => Assert.Null(firstCell.Location),
                            () => Assert.Equal("a", firstCell.Value)
                        ),
                        secondCell => Assert.Multiple(
                            () => Assert.Null(secondCell.Location),
                            () => Assert.Equal("b", secondCell.Value)
                        ),
                        thirdCell => Assert.Multiple(
                            () => Assert.Null(thirdCell.Location),
                            () => Assert.Equal("c", thirdCell.Value)
                        )
                    )
                )
            );
        }

        [Fact]
        public void IsSameAs_Identifies_Similar_Instances()
        {
            //arrange.
            var sut = new DataTableArgument();
            var other = new DataTableArgument();

            //act.
            var same = sut.IsSameAs(other);

            //assert.
            Assert.True(same);
        }

        [Fact]
        public void IsSameAs_Distinguishes_Different_Instances()
        {
            //arrange.
            var sut = new DataTableArgument();
            var other = new Mock<StepMethodArgument>().Object;

            //act.
            var same = sut.IsSameAs(other);

            //assert.
            Assert.False(same);
        }

        [Fact]
        public void Clone_Creates_Similar_Instance()
        {
            //arrange.
            var sut = new DataTableArgument();

            //act.
            var clone = sut.Clone();

            //assert.
            Assert.True(clone.IsSameAs(sut));
            Assert.NotSame(clone, sut);
        }
    }
}
