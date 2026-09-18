using System.Globalization;
using DBDStudio.Converters;
using DBDStudio.Interfaces.Rules;
using DBDStudio.Models;
using DBDStudio.Models.Component.Condition;
using DBDStudio.ViewModels;

namespace DBDStudio.Tests;

public sealed class InfrastructureAndConverterTests
{
    [Fact]
    public void RelayCommand_ExecutesAndRespectsCanExecute()
    {
        var executed = false;
        var command = new RelayCommand(() => executed = true, () => true);

        Assert.True(command.CanExecute(null));
        command.Execute(null);

        Assert.True(executed);
    }

    [Fact]
    public void RelayCommandGeneric_UsesDefaultWhenParameterTypeMismatch()
    {
        int? captured = null;
        var command = new RelayCommand<int>(v => captured = v);

        command.Execute("not-an-int");

        Assert.Equal(0, captured);
    }

    [Fact]
    public void RelayCommand_RaiseCanExecuteChanged_FiresEvent()
    {
        var fired = false;
        var command = new RelayCommand(() => { });
        command.CanExecuteChanged += (_, _) => fired = true;

        command.RaiseCanExecuteChanged();

        Assert.True(fired);
    }

    [Fact]
    public void ConstructStateClassConverter_ConvertsKnownModes()
    {
        var converter = new ConstructStateClassConverter();

        Assert.Equal("Modified", converter.Convert(ConstructState.Modified, typeof(string), "label", CultureInfo.InvariantCulture));
        Assert.Equal(true, converter.Convert(ConstructState.Ephemeral, typeof(bool), "is-ephemeral", CultureInfo.InvariantCulture));
        Assert.Equal(false, converter.Convert(ConstructState.Primordial, typeof(bool), "is-primordial-edited", CultureInfo.InvariantCulture));
        Assert.Equal(false, converter.Convert(ConstructState.Primordial, typeof(bool), "!is-primordial", CultureInfo.InvariantCulture));
    }

    [Fact]
    public void StringNotNullOrEmptyConverter_ConvertsExpectedValues()
    {
        var converter = new StringNotNullOrEmptyConverter();

        Assert.Equal(true, converter.Convert("x", typeof(bool), null, CultureInfo.InvariantCulture));
        Assert.Equal(false, converter.Convert("", typeof(bool), null, CultureInfo.InvariantCulture));
        Assert.Equal(false, converter.Convert(null, typeof(bool), null, CultureInfo.InvariantCulture));
        Assert.Throws<NotSupportedException>(() => converter.ConvertBack(true, typeof(string), null, CultureInfo.InvariantCulture));
    }

    [Fact]
    public void ConditionPublicLists_ExposeOperatorAndConjunctionLabels()
    {
        Assert.Contains("==", Condition.OperatorSymbols);
        Assert.Contains("AND", Condition.ConjunctionLabels);
    }
}
