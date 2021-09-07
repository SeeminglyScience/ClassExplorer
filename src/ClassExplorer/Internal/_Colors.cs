using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Management.Automation;
using System.Threading;

namespace ClassExplorer.Internal;

#pragma warning disable IDE1006

[EditorBrowsable(EditorBrowsableState.Never)]
public class _Colors
{
    private static _Colors? s_instance;

    private static bool s_isInited;

    private readonly PSObject _psrlOptions;

    [Hidden, EditorBrowsable(EditorBrowsableState.Never)]
    public _Colors(PSObject psrlOptions)
    {
        _psrlOptions = psrlOptions;
    }

    [Hidden, EditorBrowsable(EditorBrowsableState.Never)]
    public static _Colors Instance
    {
        get
        {
            object? @null = null;
            return LazyInitializer.EnsureInitialized(
                ref s_instance,
                ref s_isInited,
                ref @null,
                static () =>
                {
                    using var pwsh = PowerShell.Create(RunspaceMode.CurrentRunspace);
                    PSObject? options = pwsh.AddScript(
                        @"
                        if (Get-Module PSReadLine -ErrorAction Ignore) {
                            return Get-PSReadLineOption
                        }")
                        .Invoke()
                        .FirstOrDefault();

                    return new _Colors(options ?? new PSObject());
                })!;
        }
    }

    [Hidden, EditorBrowsable(EditorBrowsableState.Never)]
    public string Reset { get; set; } = "\x1b[0m";

    [Hidden, EditorBrowsable(EditorBrowsableState.Never)]
    public string StringEscape { get; set; } = "\x1b[38;2;215;186;125m";

    [Hidden, EditorBrowsable(EditorBrowsableState.Never)]
    public string Success { get; set; } = "\x1b[92m";

    [Hidden, EditorBrowsable(EditorBrowsableState.Never)]
    public string Failure { get; set; } = "\x1b[91m";

    private string? _continuationPrompt;

    [Hidden, EditorBrowsable(EditorBrowsableState.Never)]
    public string ContinuationPrompt
    {
        get => MaybeInitFromReadLine(ref _continuationPrompt, "ContinuationPromptColor", "\x1b[37m");
        set => _continuationPrompt = value;
    }

    private string? _defaultToken;

    [Hidden, EditorBrowsable(EditorBrowsableState.Never)]
    public string DefaultToken
    {
        get => MaybeInitFromReadLine(ref _defaultToken, "DefaultTokenColor", "\x1b[37m");
        set => _defaultToken = value;
    }

    private string? _comment;

    [Hidden, EditorBrowsable(EditorBrowsableState.Never)]
    public string Comment
    {
        get => MaybeInitFromReadLine(ref _comment, "CommentColor", "\x1b[32m");
        set => _comment = value;
    }

    private string? _keyword;

    [Hidden, EditorBrowsable(EditorBrowsableState.Never)]
    public string Keyword
    {
        get => MaybeInitFromReadLine(ref _keyword, "KeywordColor", "\x1b[92m");
        set => _keyword = value;
    }

    private string? _string;

    [Hidden, EditorBrowsable(EditorBrowsableState.Never)]
    public string String
    {
        get => MaybeInitFromReadLine(ref _string, "StringColor", "\x1b[36m");
        set => _string = value;
    }

    private string? _operator;

    [Hidden, EditorBrowsable(EditorBrowsableState.Never)]
    public string Operator
    {
        get => MaybeInitFromReadLine(ref _operator, "OperatorColor", "\x1b[90m");
        set => _operator = value;
    }

    private string? _variable;

    [Hidden, EditorBrowsable(EditorBrowsableState.Never)]
    public string Variable
    {
        get => MaybeInitFromReadLine(ref _variable, "VariableColor", "\x1b[92m");
        set => _variable = value;
    }

    private string? _command;

    [Hidden, EditorBrowsable(EditorBrowsableState.Never)]
    public string Command
    {
        get => MaybeInitFromReadLine(ref _command, "CommandColor", "\x1b[93m");
        set => _command = value;
    }

    private string? _parameter;

    [Hidden, EditorBrowsable(EditorBrowsableState.Never)]
    public string Parameter
    {
        get => MaybeInitFromReadLine(ref _parameter, "ParameterColor", "\x1b[90m");
        set => _parameter = value;
    }

    private string? _type;

    [Hidden, EditorBrowsable(EditorBrowsableState.Never)]
    public string Type
    {
        get => MaybeInitFromReadLine(ref _type, "TypeColor", "\x1b[37m");
        set => _type = value;
    }

    private string? _number;

    [Hidden, EditorBrowsable(EditorBrowsableState.Never)]
    public string Number
    {
        get => MaybeInitFromReadLine(ref _number, "NumberColor", "\x1b[97m");
        set => _number = value;
    }

    private string? _member;

    [Hidden, EditorBrowsable(EditorBrowsableState.Never)]
    public string Member
    {
        get => MaybeInitFromReadLine(ref _member, "MemberColor", "\x1b[97m");
        set => _member = value;
    }

    private string? _emphasis;

    [Hidden, EditorBrowsable(EditorBrowsableState.Never)]
    public string Emphasis
    {
        get => MaybeInitFromReadLine(ref _emphasis, "EmphasisColor", "\x1b[96m");
        set => _emphasis = value;
    }

    private string? _error;

    [Hidden, EditorBrowsable(EditorBrowsableState.Never)]
    public string Error
    {
        get => MaybeInitFromReadLine(ref _error, "ErrorColor", "\x1b[91m");
        set => _error = value;
    }

    private string? _selection;

    [Hidden, EditorBrowsable(EditorBrowsableState.Never)]
    public string Selection
    {
        get => MaybeInitFromReadLine(ref _selection, "SelectionColor", "\x1b[30;47m");
        set => _selection = value;
    }

    private string? _inlinePrediction;

    [Hidden, EditorBrowsable(EditorBrowsableState.Never)]
    public string InlinePrediction
    {
        get => MaybeInitFromReadLine(ref _inlinePrediction, "InlinePredictionColor", "\x1b[38;5;238m");
        set => _inlinePrediction = value;
    }

    private string? _listPrediction;

    [Hidden, EditorBrowsable(EditorBrowsableState.Never)]
    public string ListPrediction
    {
        get => MaybeInitFromReadLine(ref _listPrediction, "ListPredictionColor", "\x1b[33m");
        set => _listPrediction = value;
    }

    private string? _listPredictionSelected;

    [Hidden, EditorBrowsable(EditorBrowsableState.Never)]
    public string ListPredictionSelected
    {
        get => MaybeInitFromReadLine(ref _listPredictionSelected, "ListPredictionSelectedColor", "\x1b[48;5;238m");
        set => _listPredictionSelected = value;
    }

    private string MaybeInitFromReadLine(
        ref string? value,
        string readLinePropertyName,
        string defaultValue)
    {
        if (value is not null)
        {
            return value;
        }

        PSPropertyInfo? property = _psrlOptions?.Properties?[readLinePropertyName];
        if (property is null)
        {
            return value = defaultValue;
        }

        if (TryGetString(property, out string? str))
        {
            return value = str;
        }

        return value = defaultValue;

        static bool TryGetString(PSPropertyInfo property, [NotNullWhen(true)] out string? str)
        {
            object? value;
            try
            {
                value = property.Value;
            }
            catch
            {
                str = null;
                return false;
            }

            if (value is PSObject pso && pso.BaseObject is string stringValue0)
            {
                str = stringValue0;
                return true;
            }

            if (value is string stringValue1)
            {
                str = stringValue1;
                return true;
            }

            str = null;
            return false;
        }
    }
}

#pragma warning restore IDE1006
