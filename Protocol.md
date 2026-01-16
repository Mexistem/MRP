# MRP Projekt – Entwicklungsprotokoll

## 1. Überblick über die Projektstruktur

Das Projekt wurde in mehrere Schichten aufgeteilt. Diese Trennung macht den Code übersichtlicher, verständlicher und leichter testbar:

- **HTTP-Schicht**  
  Nimmt Anfragen entgegen, prüft Header, liest JSON und sendet Antworten zurück.

- **Business-Logik (Manager)**  
  Führt alle fachlichen Regeln aus, z. B. „Rating muss zwischen 1 und 5 sein“ oder „Medientitel darf nicht doppelt vorkommen“.

- **Datenzugriff (Repository)**  
  Speichert Daten in einfachen In-Memory-Listen. Durch Interfaces können diese später problemlos durch Datenbankimplementierungen ersetzt werden.

- **Modelle & Validatoren**  
  Modelle stellen die eigentlichen Datenobjekte dar.  
  Validatoren prüfen Eingaben und Regeln, bevor ein Objekt erstellt wird.

Diese Struktur verhindert, dass Verantwortung vermischt wird, und erleichtert zukünftige Erweiterungen.

## 2. Architektur- und Designentscheidungen

### Klare Verantwortlichkeiten
Jede Klasse übernimmt genau eine Aufgabe:

- **Handler**: rein für HTTP zuständig  
- **Manager**: führt die Hauptlogik aus  
- **Validatoren**: prüfen Eingaben und Regeln  
- **Repositories**: kümmern sich um Speicherung  

Dies entspricht dem *Single Responsibility Principle*.

### Nutzung der Manager statt Direktzugriff
Der Handler ruft niemals Model-Konstruktoren oder Repository-Methoden direkt auf.  
Alle Aktionen laufen über Manager-Klassen