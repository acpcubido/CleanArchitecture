# 301 Moved Permanently [Loop > Cubido General > Softwareentwicklung > Guidelines > Code-Styling > .NET / C#](https://cubidocloud.sharepoint.com/:fl:/g/contentstorage/CSP_2a6b28bd-cc20-42d5-816e-208f68ff7a3f/ES-Zz7jKG7ZMhcmnKxoQH_0BQfJxXVXEnRn8hi5Ot8Rw5A?e=TLleTT&nav=cz0lMkZjb250ZW50c3RvcmFnZSUyRkNTUF8yYTZiMjhiZC1jYzIwLTQyZDUtODE2ZS0yMDhmNjhmZjdhM2YmZD1iJTIxdlNocktpRE0xVUtCYmlDUGFQOTZQMFFoN0hRMVBTdEh0Q2NYa012cTQ4NGJ2dGJhOUdlRlI3SmJ0TGVfc1JtMCZmPTAxUTdNWkJGWlBUSEgzUlNRM1daR0lMU05IRk1OQkFINzUmYz0lMkYmYT1Mb29wQXBwJnA9JTQwZmx1aWR4JTJGbG9vcC1wYWdlLWNvbnRhaW5lciZ4PSU3QiUyMnclMjIlM0ElMjJUMFJUVUh4amRXSnBaRzlqYkc5MVpDNXphR0Z5WlhCdmFXNTBMbU52Ylh4aUlYWlRhSEpMYVVSTk1WVkxRbUpwUTFCaFVEazJVREJSYURkSVVURlFVM1JJZEVOaldHdE5kbkUwT0RSaWRuUmlZVGxIWlVaU04wcGlkRXhsWDNOU2JUQjhNREZSTjAxYVFrWTNWREpWVWtKTlEwNDFVazVGVEVaQ1RrNVNTazlhVVU4MlJBJTNEJTNEJTIyJTJDJTIyaSUyMiUzQSUyMmJmMTFhNmFjLTVlZDYtNDFmOC1hNTQzLWQ1N2QwMDk1MTVmYSUyMiU3RA%3D%3D)

[[_TOC_]] 

# Scope

✅ **Hauptfokus:** Code-Style\
⚠️ **Nebenfocus:** Code-Formatting

**Warum?** Formatting benötigt erweitertes tooling. (z.B StyleCop oder CSharpier)\
**Warum?** 3rd Party Software muss auch gewartet werden. Mehraufwand gilt es zu vermeiden\
**Warum?** Pipeline-Support muss zu jeder Zeit gewährleistet sein\
**Warum?** Software kann nicht ohne weiteres auf Kundengeräten nachinstalliert werden\
**Warum?** Automatische Formatierungen sind im Entwicklungsprozess störend\
**Warum?** Formatierung ist Teil des Entwicklungsstils und ein _hot topic_

# .editorconfig

Für die Einhaltung des Code-Styles wird eine [_.editorconfig_](#) zur Verfügung gestellt.

**Warum?** Einfache Verteilung/Wartung der Konfiguration\
**Warum?** Lebendiges Dokument und anpassbar via PR\
**Warum?** Wurde bereits bis jetzt unbewusst verwendet\
**Warum?** Out-of-the-box support in Visual Studio. Datei einfügen und fertig\
**Warum?** Out-of-the-box support in build-pipelines

## How to use
Bitte wenn möglich für alle .NET Projekte verwenden. Auch für Alt-Projekte gedacht. Kundeneinschränkungen und Ausnahmen für Uralt-Projekte sind aber natürlich möglich.
1. `.editorconfig` file aus dem Repo in den Solution Root Folder kopieren (wo die .sln liegt).\
[Cubido.Guidelines/editorconfig/.editorconfig](https://dev.azure.com/cubido/Cubido_Development/_git/Cubido.Guidelines?path=editorconfig/.editorconfig)\
**Achtung**: Chrome entfernt den anfänglichen Punkt beim Download.
2. `Directory.Build.props` file aus dem Repo in den Solution Root Folder kopieren.\
[Cubido.Guidelines/editorconfig/Directory.Build.props](https://dev.azure.com/cubido/Cubido_Development/_git/Cubido.Guidelines?path=editorconfig/Directory.Build.props)\
*Checkt und enforced Code-Style Fehler on build.*
3. Normal arbeiten. Files wenn möglich vor jedem Commit formattieren.\
*Wir wollen keinen pre-commit hook einführen.*

### Initial Setup Zusatz
4. Einmalig "Run Code Cleanup on Solution"
    - Kontrolle und Fehler-Behebung
    - Einchecken, wenn die Änderungen vertretbar sind.\
    Sonst Diskussion oder Projektregel Anpassung - siehe nächster Punkt.

### Änderungsprozess
Es kann natürlich unterschiedliche Meinungen geben. Es gibt 3 Stufen von Ablehnung.
1. "Die Regel stört mich ein bisschen"\
-> Bitte einfach akzeptieren.
2. "Die Regel stört in unserem Projekt"\
-> Project-specific Regel am Ende der editorconfig einfügen.
3. "Die Regel stört mich grundsätzlich"\
-> Pull Request aufmachen und Begründen warum die Regel geändert werden sollte

Bitte auch gerne Pull Requests für sinnvolle fehlende Regeln aufmachen!

### CI-Pipeline

Um folgende Warning zu beheben

> Warning EnableGenerateDocumentationFile: Set MSBuild property 'GenerateDocumentationFile' to 'true' in project file to enable IDE0005 (Remove unnecessary usings/imports) on build

einfach in der CLI die MSBuild-Property setzen:

```bash
dotnet build -p:GenerateDocumentationFile=true
```

## Contents

In der _.editorconfig_ können Regeln mehrmals definiert werden. Das rule-set setzt sich aus den folgenden Quellen zusammen:

1. [Microsoft Default .NET Code Style](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/code-style-rule-options)
2. [Microsoft .NET Docs Code Conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
3. Cubido-specific rule-set
4. Project-specific rule-set

**Warum?** Änderungen seitens MS können mit copy+paste schnell übernommen werden\
**Warum?** Die Visual-Studio default _.editorconfig_ umfasst bereits diese Regeln - wir sind diese gewohnt\
**Warum?** Regeln sind, ohne andere überschreiben zu müssen, einfach hinzuzufügen.

## Order

Da eine Regel mehrmals definiert werden kann, gilt die zuletzt vorhandene Defintion. Somit gilt: **Project > Cubido > Code Conventions > Code Style**

**Warum?** Kundenspezifische Projektanpassungen gelten\
**Warum?** Projektbezogene "Spezialregeln" gelten nur für das Projekt\
**Warum?** Möglicher Wohlfühlfaktor bei Einzelarbeiten\
**Warum?** Einfachere Einigung bei den allgemeinen Cubido Regeln

## Severity

Suggestions und Warnings werden meist ignoriert. Deshalb, wenn wir eine Regel definieren, resultiert ein Regelverstoß in einem **Error**.

**Warum?** Nicht alle beachten Warnings - noch weniger die Vorschläge\
**Warum?** Ein Vorschlag oder eine Warnung kann dann wohl nicht so wichtig sein\
**Warum?** Regeln **müssen** eingehalten werden. Rest ist kreative Entfaltung

## Enforcement

Die Regeln werden, wie wir es bereits gewöhnt sind, beim Programmieren durchgesetzt.

**Warum?** Default behaviour 🤷\
**Warum?** Keine Überraschungen beim PR\
**Warum?** Schnellere Erziehung des Entwicklers

## Modifications

Cubido Code-Style Anpassungen sind nur über einen PR möglich. Projektbezogene Anpassungen bedarf lediglich einer Abstimmung innerhalb des betroffenen Projektteams. Beim hinzufügen einer Regel muss ein Code-Snippet als Kommentar hinterlassen werden.

```
# YES   private const int VALUE = 0;
# NO    private const int Value = 0;
dotnet_naming_rule.constants_should_be_upper_case.severity = error
dotnet_naming_rule.constants_should_be_upper_case.symbols = constants
dotnet_naming_rule.constants_should_be_upper_case.style = constant_style
dotnet_naming_symbols.constants.applicable_kinds = field, local
dotnet_naming_symbols.constants.required_modifiers = const
dotnet_naming_style.constant_style.capitalization = all_upper
```

**Warum?** Keine Alleingänge - jeder soll die Möglichkeit haben ein Veto einzulegen\
**Warum?** Die Regeln sind meistens nicht selbsterklärend

# Cubido Code-Style

**Hot-Topic:** Diskutiert und keine eindeutige Mehrheitsmeinung vorhanden\
**Zombie-Topic:** Diskutiert und mehrheitlich für schlecht befunden

Beide Unterpunkte werden nicht durchgesetzt und liegen, bis auf weiteres, auf Eis.


## 🔥 Hot-Topics

// removed, because CoPilot started to enforce them

## Enforced Rules

### Naming: Konstanten in ALL_CAPS

✅ `const int VALUE = 0;`\
⛔ `const int Value = 0;`

**Warum?** Auf einen Blick ersichtlich, dass es sich um einen konstanten Wert handelt\
**Warum?** Konsistentes Naming

### Naming: Interfaces beginnen mit I

✅ `public interface IService { }`\
⛔ `public interface ServiceInterface { }`

**Warum?** Konsistentes Naming

### Naming: Types in PascalCase

✅ `public class ShopService { }`\
⛔ `public class shopService { }`

**Warum?** Konsistentes Naming

### Naming: Non-field members in PascalCase

✅ `public int MyProperty { get; set; }`\
⛔ `public int myProperty { get; set; }`

**Warum?** Konsistentes Naming

### Naming: Internal und private fields in camelCase

✅ `private string myField;`\
⛔ `private string MyField;`

**Warum?** Konsistentes Naming

### Naming: Static fields in camelCase

✅ `private static string myField;`\
⛔ `private static string MyField;`

**Warum?** Konsistentes Naming

### Async calls immer awaiten oder behandeln (`await`, `_`, ...)

✅ `await GetCustomer();`\
✅ `_ = FireAndForget();`\
⛔ `GetCustomer();`

**Warum?** Es kann nicht mehr versehentlich auf ein `await` vergessen werden

### Naming: Verwende keinen Async-Suffix; auch nicht für asynchrone Methoden

✅ `public UserDto[] GetUsers()`
✅ `public async Task<UserDto[]> GetUsers(CancellationToken cancellationToken)`
✅ `public async Task Should_get_all_users() // Unit-Test`
⛔ `public async Task<UserDto> GetUsersAsync(CancellationToken cancellationToken);`

**Warum?** Asynchrone Methoden sind mittlerweile der Standard und dafür braucht man sich nicht die Codebasis mit Suffixen zumüllen.
**Warum?** Es gibt einen .NET Code Analyzer, der sich meldet, wenn man ein `await` vergessen sollte.

### Verwende keine public instance fields

✅ `private ILogger _logger;`\
⛔ `public ILogger _logger;`

**Warum?** Verhindert die Manipulation von Instanzvariablen\
**Warum?** Durch das verwenden von Properties ist der Zugriff regulier- und leicht austauschbar

### Entferne unused parameters (außer public)

✅ `private void MyMethod() { }`\
⛔ `private void MyMethod(string value) { }`

**Warum?** Obsoleter Code cluttert die Codebase

### Names vereinfachen

✅ `FileInfo file;`\
⛔ `System.IO.FileInfo file;`

**Warum?** Obsoleter Code cluttert die Codebase

### Entferne unnötige casts

✅ `float sum = 1 + 2f;`\
⛔ `float sum = (float)1 + 2f;`

**Warum?** Obsoleter Code cluttert die Codebase

### Entferne unnötige imports

✅ `-`\
⛔ `using System.IO;`

**Warum?** Obsoleter Code cluttert die Codebase

### Auto properties statt backing fields

✅ `public int Prop { get; set; }`\
⛔ `public int Prop { get => prop; set => prop = value; }`

**Warum?** Obsoleter Code cluttert die Codebase

### Format Argumente müssen mit string übereinstimmen

✅ `string.Format("{0}: {1}", file, errors)`\
⛔ `string.Format("{0}", file, errors)`

**Warum?** Fehler - kann nicht beabsichtigt sein

### Klammere alle Code-Blöcke

✅ `if (condition) { foo(); } else { bar(); }`\
⛔ `if (condition) foo(); else bar();`

**Warum?** Bessere Lesbarkeit\
**Warum?** kann zu Fehlern führen

### Keine erzwungene XML Dokumentation

✅ `/// <summary>My method</summary>`\
✅ `-`

**Warum?** Nervig, nicht alles kann dokumentiert werden

## Non-Enforceable Rules

Nicht alle Regeln - welche wir gerne hätten - können bereits mit der _.editorconfig_ abgebildet werden. Deshalb bildet diese Sektion eine Sammelstelle für zukünftige und bereits genehmigte Regelübernahmen. Aufgrund der nicht vorhandenen Durchsetzbarkeit basiert die Umsetzung dieser Regelmatrix auf freiwilliger Basis.

### Interpolated Strings verwenden

✅ `$"{a} and {b}"`\
⛔ `a + " and " + b`

**Warum?** Interpolated strings sind besser lesbar als Concats.

### Operatoren an den Anfang der Zeile, wenn mehrzeilig

✅
```
bool test = someCondition1 
    && someCondition2 
    && someCondition3;
```
⛔
```
bool test = someCondition1 &&
    someCondition2 &&
    someCondition3;
```

**Warum?** Bessere Lesbarkeit, Operator schneller erkennbar.


## 🪦 Zombie-Topics

### Var nur für _apparent types_ erlauben

✅ `var program = new Program()`\
⛔ `var program = Factory.GetProgram()`

**Warum für?** Lambdas verursachen unnötig viel Aufwand\
**Warum für?** Typ ist leichter erkennbar. Ist auch ohne IDE lesbar.\
**Warum gegen?** Typ ist oft irrelevant, man muss nicht sehen was es ist.\
**Warum beerdigt?** var oder nicht-var enforcen wäre beides ein großer Eingriff
