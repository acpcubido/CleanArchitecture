[[_TOC_]]

# Git Repository

## Repository Name

**Bezeichne** Git Repositories mit dem vollen PascalCase Namespace wie zum Beispiel `DerStandard.CMC.SyncFlow`

**Warum?** Vollständige Namespaces führen zu keiner Namenskonflikten im Dateiverzeichnis, wenn mehrere Repositories lokal ausgecheckt sind.

**Warum?** Vollständige Namespaces enthalten den Kunden **DerStandard**.CMC.SyncFlow

**Warum?** Vollständige Namespaces enthalten den Projektnamen DerStandard.**CMC.SyncFlow**


# Git Branch

## Branch Name

**Bezeichne** Git Branches in kebap-case wie zum Beispiel `feat/desk-net-client`

**Warum?** Kebap-case funktioniert super in der Konsole

## Semantische Git Branch Namen

**Verwende** einen Präfix für Git Branch Namen.

| Präfix | Beschreibung                                                  |
| ------ | ------------------------------------------------------------ |
| chore  | Housekeeping tasks (formatting, build system, pipeline, ...) |
| docs   | Documentation only changes                                   |
| feat   | A new feature, dependency upgrade, refactoring               |
| fix    | A bug fix                                                    |
| hotfix | A fix to production                                          |

Dies ist eine vereinfachte Liste von `Git Commit Hygiene > Semantische Commit Messages`

**Warum?** Präfixe trennen kurzlebige Branches vom `main` Branch.

**Warum?** Aufgeräumte Baumansicht in Azure DevOps oder anderen clients.

**Beispiel**: Branch Ansicht in Gitkraken  
![](./img/semantic-branch-gitkraken.png)

## Aufräumen von Branches

**Entferne** Branches sobald sie im `main` Branch sind.

**Warum?** Veraltete und Branches müllen sonst die Branch Ansicht zu.

**Warum?** Git fetch synchronisiert standardmäßig _alle_ Branches, was bei vielen (veralteten) Branches dementsprechend länger dauert.

# Branching Strategie

![](./img/cuFlow.drawio.png)

## GitHub Flow

Siehe https://docs.github.com/en/get-started/using-github/github-flow

Nicht zu verwechseln mit Git Flow, einer älteren und umständlicheren Variante von GitHub Flow

**Verwende** kurzlebige feature Branches.

**Verwende** Pull Requests um Änderungen zu mergen.

**Warum?** Git Branches sind leichtgewichtig.

**Warum?** Möglichst nahe am `main` Branch zu bleiben verhindert Chaos und merge-Konflikte.

**Warum?** Harmoniert mit Continuous Integration und Continuous Development.

**Warum?** Fördert die Feedbackkultur.

## Entwicklungs-Branch

**Vermeide** die alte (rassistische) Bezeichnung `master`

**Bezeichne** den development Branch `main`.

**Warum?** Git verwendet standardmäßig `main` für den Hauptbranch.

## Agile development

**Halte** den `main`-Branch production ready.

**Warum?** Reduziert die Hemmschwelle für ein Deployment, was wiederum Feedback von Endusern fördert.

**Warum?** Reduziert den Bedarf an Hotfixes nur am production-Branch.

**Erwäge** Feature Flags im Code für größere Features.

**Warum?** Feature Flags sind ein einfacher uns sauberer Weg, um große Features zu entwickeln und trotzdem gleichzeitig kleinere Features und Fehlerbehebungen zu deployen.

## Production Branch

**Verwende** einen production/release-Branch, wenn die Production-Umgebung unabhängig deployed wird.

**Warum?** Ein dezidierter Branch erleichtert Hotfixes.

**Warum?** So sieht man jederzeit am production/release-Branch, was in der Production-Umgebung läuft.

**Vermeide** einzelne Branches für jedes manuelle Deployment.

**Warum?** Ältere Release-Branches müllen das Repository zu, wo man doch meistens eh nur an der aktuellen Version interessiert ist.

**Bezeichne** den production/relase-Branch `release`

**Warum?** `release` ist neben `production` ein sprechender Name für "das wird/ist released".


# Git Commit Hygiene

## Sprache der Commit Message

**Verwende** Englisch für Commit messages

**Verwende** Fachbegriffe in der jeweiligen Sprache so wie Firmennamen.

**Warum?** Die Commit message landet so wie dein Code in der Branch Historie lesbar für jeden.

**Verwende** Imperativ und Präsens, wie zum Beispiel "fix" ~~"fixed"~~ ~~"fixes"~~

**Vermeide** Großschreibung und den Punkt am Ende des Satzes.

**Beispiel** `feat: apply control flow migration from Angular 17`

## Semantische Commit Messages

**Verwende** folgende Vorlage für Commit messages:

```
<type>(<scope>): <subject>
<BLANK LINE>
<body>
<BLANK LINE>
<footer>
```

Lediglich `type` und `subject` sind verpflichtend.

| type     | Beschreibung                                                                                           |
| -------- | ------------------------------------------------------------------------------------------------------ |
| ops      | Changes that affect the build-system																	|
| docs     | Documentation only changes                                                                             |
| feat     | A new feature or dependency upgrade                                                                    |
| fix      | A bug fix                                                                                              |
| test     | Adding missing tests or correcting existing tests                                                      |
| chore    | Other housekeeping tasks                                                                               |

**Verwende** `scope` für größere Projekte bzw. Monorepos mit vielen microapps, um anzuzeigen, welcher Teil betroffen ist.

**Vermeide** mehr als 80 Zeichen in der ersten Zeile.

**Warum?** Mit dieser Vorlage sieht man auf einen Blick, was der Commit macht.

**Verwende** den `body`, um zusätzliche Details zu beschreiben, speziell, wenn es sich um einen abstrusen Fehler handelt.

**Verwende** den `footer` um relevante work items zu verlinken.

**Beispiel**

> fix: fix a bunch of style issues
>
> included bootstrap, introduced variables that were missing
>
> Related Work Items: \#1234

**Verwende** den Präfix "revert: " und den vollen Hash der Commit message, wenn du einen Commit revertest.

**Warum?** So machen es Azure DevOps und Visual Studio.

[Inspiration](https://github.com/angular/angular/blob/22b96b9/CONTRIBUTING.md#-commit-message-guidelines)

## Single Responsibility Commit

**Verwende** separate Commits für nicht zusammenhängende Änderungen

**Vermeide** zu viele unzusammenhängende Änderungen in einem einzelnen Commit oder Pull request.

**Warum?** Cherry picking oder reverten von einzelnen Commits wird unübersichtlich/unmöglich, wenn Commits mit unzusammenhängen Änderungen zugemüllt sind.

**Warum?** Handliche Commits sind leichter zu verstehen.

**Warum?** Dafür gibt es interactive staging (line-staging).

**Warum?** Dafür gibt es in Azure DevOps bei Pull Requests eine eigene Commits-Ansicht.

**Verwende** ammend Commits, um Änderungen zu einem bestehenden Commit dazuzugeben.

**Warum?** Hält alle zusammenhängen Änderungen in einem Commit zusammen.

**Tipp** Mit interactive rebase `git rebase -i` kannst du die Reihenfolge von Commits zu ändern. 

**Tipp** Mit interactive rebase `git rebase -i` mit einem dummy sqash Commit kannst du einen früheren Commit bearbeiten.

Siehe https://git-scm.com/book/en/v2/Git-Tools-Rewriting-History

## Push Strategie

**Verwende** git push oft.

**Warum?** Wenn du krank wirst, möchte vielleicht ein Kollege weiterentwickeln, wo du aufgehört hast.

**Warum?** Du möchtest vielleicht deine lokalen Änderungen rückgängig machen.

**Warum?** Fördert frühes Feedback mit Draft Pull Requests.

**Verwende** force push, wenn du Commits bearbeitest mit ammend oder interactive rebase.

**Tipp** Du findest alle Änderungen unabhängig von push oder force push im git reflog, wenn du mal was unabsichtlich überschrieben/gelöscht hast.

**Vermeide** force push auf einen Branch eines Arbeitskollegen.

**Benachrichtige** deine Arbeitskollegen, wenn du auf ihre Branches hinpusht.

## Zusammen arbeiten

**Verwende** `git rebase` so oft wie möglich, um deinen lokalen Branch so nah am `main`-Branch zu halten wie möglich.

**Warum?** Kleine Änderungen zu mergen ist weniger fehleranfällig.

**Vermeide** merge changes vom `main`-Branch in deinen lokalen Branch.

**Warum?** merge Commits verhunzen die Commit Historie.

**Warum?** Pull requests sind leichter zu verstehen, wenn sie sich auf die aktuelle Version am `main`-Branch beziehen.

**Warum?** Features sind leichter ohne Konflikte zu reverten, wenn sie sich auf die aktuelle Version beziehen.

## Work Item Verlinkung

**Verlinke** work items in Commit messages mit dem #-Präfix.

**Vermeide** das Verlinken von Parent-Work Items, da diese sonst unabsichtlich geschlossen werden könnten durch `Complete associated work items after merging`.

**Warum?** Work items liefern zusätzlichen Kontext.

**Warum?** Commits können über work items gefunden werden, wie zum Beispiel alle Änderungen eines Features über mehrere Pull Requests oder sogar über mehrere Projekte hinweg.

**Warum?** Der Fortschritt von work item ist transparent für den Projektleiter mit linked Commits.

**Warum?** Pull requests verlinken automatisch zu allen work items aus den Commits.

# Pull requests

## PR Name

**Verwende** den Pull Request Namen wie den Commit header.

**Verwende** die Pull Request Beschreibung wie Commit body and footer.

**Vermeide** Bilder in der Pull Request Beschreibung.

**Warum?** Der Pull Request Name and Beschreibung werden als merge Commit message am Ziel-Branch persistiert.

## PR Diskussion

**Verwende** die Sprache des Zielpublikums für Pull Request Kommentare, gerne auch Mundart und Emojis.

**Warum?** Kommentare werden nicht persistert am Pull Request merge Commit.

**Warum?** Reden geht am besten in der Muttersprache.

**Verwende** den Präfix `NIT: `, wenn etwas nicht so wichtig ist.

## PR Review Kommentare

**Verwende** selber Kommentare um Hinweise für die Reviewer zu hinterlassen, zum Beispiel, wenn du eine Methode von deiner Datei zu einer anderen verschoben hast, hinterlassen einen Kommentar, wo du sie hinverschoben hast.

**Warum?** Git erkennt lediglich dumme Umbenennungen von Dateien. Hinweis-Kommentare machen es einfacher Reviewer deine Änderungen zu verstehen.

**Verwende** Kommentare mit Screenshots, um deine Änderungen für Reviewer aufzubreiten.

**Warum?** Idealerweise kann der Reviewer alle Änderungen direkt im Pull Request verstehen und bewerten ohne den Branch lokal laufen lassen zu müssen.

## PR Merge Strategie

**Verwende** Branch policies um die Pull Request merge Typen einzuschränken.

**Verwende** `Semi-linear merge`.

**Verwende** `Squash commit` wenn der Commitverlauf 💩 ist oder einen merge-Commit enthält.

**Vermeide** `Merge (no fast-forward)` und `Rebase`.

**Warum?** `Merge (no fast-forward)` und `Rebase` sind `Semi-linear merge` unterlegen.

**Warum?** Siehe https://devblogs.microsoft.com/devops/pull-requests-with-rebase/

**Warum?** Der Commitverlauf kann nach `First Parent` gefiltert werden, um trotzdem eine einfache Übersicht zu erhalten.

## Work Item Verlinkung

**Verlinke** work items in Pull Requests.

**Warum?** See `Git Commit Hygiene > Work Item Verlinkung`

**Tipp** Man kann (aber muss nicht) work items automatisch mit dem Pull Request schließen lassen.

## Pull Request Completion

**Vermeide** Pull Requests deiner Arbeitskollen zu completen.

**Warum?** Der Autor des Pull Requests mag vielleicht noch etwas ändern nach deinem Feedback.

**Warum?** Der Branch bleibt dann liegen, weil Branches standardmäßig nur vom Ersteller gelöscht werden können.
