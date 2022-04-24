using namespace System
using namespace System.Text

[CmdletBinding()]
param(
    [Parameter()]
    [ValidateNotNullOrEmpty()]
    [string] $Path,

    [Parameter()]
    [switch] $AboutHelp
)
begin {
    class SigWriter {
        [StringBuilder] $sb

        SigWriter([StringBuilder] $stringBuilder) {
            $this.sb = $stringBuilder
        }

        [SigWriter] Wrap([int] $width, [string] $content) {
            $lines = $this.GetNormalizedLines($content)
            foreach ($line in $lines) {
                $this.WrapImpl($width, $line)
            }

            return $this.AppendLine()
        }

        [SigWriter] WrapImpl([int] $width, [string] $line) {
            if ($line.Length -le $width) {
                return $this.Append($line)
            }

            $words = $line -split '(?<= )'
            $column = 0
            foreach ($word in $words) {
                if (($column + $word.Length) -gt $width) {
                    $column = 0
                    $this.AppendLine()
                }

                $column += $word.Length
                $this.Append($word)
            }

            return $this
        }

        [SigWriter] Hr() { return $this.AppendLine([string]::new('-', 50)) }
        [SigWriter] New() { throw [NotImplementedException]::new() }
        [SigWriter] ToC([string[]] $headers) { throw [NotImplementedException]::new() }
        [SigWriter] Header([string] $text) {throw [NotImplementedException]::new() }
        [SigWriter] Bool([bool] $value) { throw [NotImplementedException]::new() }
        [SigWriter] StartTable() { throw [NotImplementedException]::new() }
        [SigWriter] EndTable() { throw [NotImplementedException]::new() }

        [SigWriter] DoubleRow([string] $content) {
            foreach ($line in $content -split '\r?\n') {
                $this.AppendLine($line)
            }

            return $this
        }

        [SigWriter] Row([int] $firstColumnWidth, [string[]] $cells) {
            return $this.Row($firstColumnWidth, $cells, <# isHeader: #> $false)
        }

        [SigWriter] Row([int] $firstColumnWidth, [string[]] $cells, [bool] $isHeader) { throw [NotImplementedException]::new() }
        [SigWriter] StartCodeBlock([string] $language) { throw [NotImplementedException]::new() }
        [SigWriter] EndCodeBlock() { throw [NotImplementedException]::new() }

        [SigWriter] AppendNormalizedContent([string] $value) {
            $lines = $this.GetNormalizedLines($value)
            foreach ($text in $lines) {
                $this.AppendLine($text)
            }

            return $this
        }

        [string[]] GetNormalizedLines([string] $value) {
            $lines = [string[]]($value -split '\r?\n')
            for ($i = 0; $i -lt $lines.Length; $i++) {
                $line = $lines[$i]
                if ($line[0] -eq ' '[0]) {
                    $line = $line.Substring(1)
                }

                $lines[$i] = $line -replace '\t', '    '
            }

            return $lines
        }

        [SigWriter] Append([string] $value) {
            $this.sb.Append($value)
            return $this
        }

        [SigWriter] AppendLine([string] $value) {
            $this.sb.AppendLine($value)
            return $this
        }

        [SigWriter] AppendLine() {
            $this.sb.AppendLine()
            return $this
        }

        [string] ToString() {
            return $this.sb.ToString() -replace
                '\p{Zs}+(?=\r?\n)' -replace
                '(?<=\n)\p{Zs}+(?=\r?\n)' -replace
                '(\r?\n){2,}', '$1$1'
        }
    }

    class MdSigWriter : SigWriter {
        MdSigWriter([StringBuilder] $stringBuilder) : base($stringBuilder) {
        }

        [SigWriter] New() {
            return [MdSigWriter]::new([StringBuilder]::new())
        }

        [SigWriter] Wrap([int] $width, [string] $content) {
            return $this.AppendNormalizedContent($content)
        }

        [SigWriter] Header([string] $text) {
            return $this.Append('## ').AppendLine($text).
                AppendLine().
                AppendLine('<sup>([Back to Top](#keywords))</sup>').
                AppendLine()
        }

        [SigWriter] ToC([string[]] $headers) {
            $this.AppendLine('## Keywords').AppendLine()
            foreach ($header in $headers) {
                $link = $header -replace ',', '-' -replace
                    ' ', '-' -replace
                    '`' -replace
                    '\(' -replace
                    '\)' -replace
                    '-{1,}', '-'

                $this.Append('* [').Append($header).Append('](#').
                    Append($link.ToLowerInvariant()).
                    AppendLine(')')
            }

            return $this.AppendLine()
    }
        [SigWriter] Bool([bool] $value) {
            if ($value) {
                return $this.Append(':heavy_check_mark:')
            }

            return $this.Append(':x:')
        }

        [SigWriter] StartTable() {
            return $this.AppendLine('<table>')
        }

        [SigWriter] EndTable() {
            return $this.AppendLine('</table>')
        }

        [SigWriter] DoubleRow([string] $content) {
            return $this.AppendLine('<tr>').
                AppendLine('<td colspan="2" width="1000">').AppendLine().
                AppendNormalizedContent($content).AppendLine().AppendLine().
                AppendLine('</td>').
                AppendLine('</tr>')
        }

        [SigWriter] Row([int] $firstColumnWidth, [string[]] $cells, [bool] $isHeader) {
            $cellElement = 'td'
            if ($isHeader) {
                $cellElement = 'th'
            }

            $this.AppendLine('<tr>').
                Append('<').Append($cellElement).Append(' width="').Append($firstColumnWidth).AppendLine('">').AppendLine()

            $this.AppendNormalizedContent($cells[0]).AppendLine().AppendLine().
                Append('</').Append($cellElement).AppendLine('>').
                Append('<').Append($cellElement).AppendLine('>').AppendLine()

            return $this.AppendNormalizedContent($cells[1]).AppendLine().AppendLine().
                Append('</').Append($cellElement).AppendLine('>').
                AppendLine('</tr>')
        }

        [SigWriter] StartCodeBlock([string] $language) {
            return $this.Append('```').AppendLine($language)
        }

        [SigWriter] EndCodeBlock() {
            return $this.AppendLine('```')
        }
    }

    class PlainTextSigWriter : SigWriter {
        [int] $IndentSize

        PlainTextSigWriter([StringBuilder] $stringBuilder) : base($stringBuilder) {
            $this.IndentSize = 4
            $this.sb.Append('    ')
        }

        PlainTextSigWriter([int] $indentSize) : base([StringBuilder]::new()) {
            $this.IndentSize = $indentSize
        }

        [SigWriter] AppendLine([string] $value) {
            $this.sb.AppendLine($value)
            return $this.Indent()
        }

        [SigWriter] AppendLine() {
            $this.sb.AppendLine()
            return $this.Indent()
        }

        [SigWriter] AppendLineNoIndent() {
            $this.sb.AppendLine()
            return $this
        }

        [SigWriter] Indent() {
            if ($this.IndentSize -eq 0) {
                return $this
            }

            return $this.Append('    ')
        }

        [SigWriter] New() {
            return [PlainTextSigWriter]::new(0)
        }

        [SigWriter] Header([string] $text) {
            $this.sb.Remove($this.sb.Length - 3, 2)
            return $this.AppendLine(($text -replace '`').ToUpperInvariant())
        }

        [SigWriter] ToC([string[]] $headers) {
            return $this
        }

        [SigWriter] Bool([bool] $value) {
            if ($value) {
                return $this.Append([string][char]0x2713)
            }

            return $this.Append('x')
        }

        [SigWriter] StartTable() { return $this }
        [SigWriter] EndTable() { return $this }
        [SigWriter] Row([int] $firstColumnWidth, [string[]] $cells, [bool] $isHeader) {
            $firstColumnWidth += 4
            $firstCellLines = [string[]]($cells[0].Trim() -split '\r?\n')
            $secondCellLines = [string[]]($cells[1].Trim() -split '\r?\n')

            if ($firstCellLines.Length -lt $secondCellLines.Length) {
                [array]::Resize([ref] $firstCellLines, $secondCellLines.Length)
            }

            $blank = [string]::new([char]' ', $firstColumnWidth)
            for ($i = 0; $i -lt $secondCellLines.Length; $i++) {
                $line = $firstCellLines[$i]
                if ($null -eq $line) {
                    $firstCellLines[$i] = $blank
                    continue
                }

                $firstCellLines[$i] = $line.
                    PadLeft([Math]::Ceiling($firstColumnWidth / 2)).
                    PadRight($firstColumnWidth)
            }

            for ($i = 0; $i -lt $secondCellLines.Length; $i++) {
                $this.Append($firstCellLines[$i]).Append(' | ').AppendLine($secondCellLines[$i])
            }

            $this.Hr()

            return $this
        }

        [SigWriter] StartCodeBlock([string] $language) {
            return $this
        }

        [SigWriter] EndCodeBlock() { return $this }
    }
}
end {
    $yaml = Get-Content -Raw $PSScriptRoot/Signatures.yaml -ErrorAction Stop |
        ConvertFrom-Yaml

    if (-not $AboutHelp) {
        $w = [MdSigWriter]::new([StringBuilder]::new())
        $null = $w.
            Append('# Type Signatures').AppendLine().AppendLine().
            AppendNormalizedContent($yaml['long'])
    } else {
        $w = [PlainTextSigWriter]::new([StringBuilder]::new())
        $null = $w.sb.Clear()
        $null = $w.Append('TOPIC').AppendLine().Append('about_Type_Signatures').
            AppendLineNoIndent().
            AppendLineNoIndent().
            AppendLine('SHORT DESCRIPTION').
            Wrap(76, $yaml['short']).
            AppendLineNoIndent().
            AppendLine('LONG DESCRIPTION').
            Wrap(76, $yaml['long']).AppendLine().
            Wrap(76, 'See https://seemingly.dev/about-type-signatures for a markdown version of this document.').
            AppendLine()
    }


    $null = $w.AppendLine()

    $keywords = $yaml['keywords'].ToArray()

    $null = $w.ToC([string[]]$keywords.ForEach{ $PSItem['header'] })

    $null = foreach ($section in $keywords) {
        $w.Header($section['header']).AppendLine()
        $w.Wrap(76, $section['description']).AppendLine()

        foreach ($example in $section['examples']) {
            $syntaxWriter = $w.New().StartCodeBlock('powershell')
            foreach ($syntax in $example['syntax']) {
                if ($AboutHelp) {
                    $syntaxWriter.Append('PS> ')
                }

                $syntaxWriter.AppendNormalizedContent($syntax)
            }

            $syntaxWriter.EndCodeBlock()
            $w.StartTable().
                DoubleRow($syntaxWriter.ToString()).
                Row(1, ('', 'Signature'), <# isHeader: #> $true)

            foreach ($sig in $example['signatures']) {
                $match = $w.New().Bool($sig['match']).ToString()
                $csharpSig = $w.New().StartCodeBlock('csharp').
                    AppendNormalizedContent($sig['sig']).
                    EndCodeBlock().
                    ToString()

                $w.Row(1, ($match, $csharpSig))
            }

            $w.EndTable().AppendLine()
        }
    }

    $content = $w.ToString().Trim()
    if ($AboutHelp) {
        $extension = '.help.txt'
    } else {
        $extension = '.help.md'
    }

    $resultFile = $null
    if ($PSBoundParameters.ContainsKey('Path')) {
        if ($Path.EndsWith($extension, [StringComparison]::OrdinalIgnoreCase)) {
            $resultFile = $Path
        } else {
            $resultFile = Join-Path $Path -ChildPath 'about_Type_Signatures.help'
            $resultFile = [IO.Path]::ChangeExtension($resultFile, $extension)
        }
    } else {
        $resultFile = [IO.Path]::ChangeExtension(
            "$PSScriptRoot\..\docs\en-US\about_Type_Signatures.help",
            $extension)
    }

    Set-Content -Value $content -LiteralPath $resultFile -ErrorAction Stop
}
