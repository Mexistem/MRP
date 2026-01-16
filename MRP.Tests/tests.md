# MRP – Testplan für TDD

Dieses Dokument beschreibt die geplanten Unit-Tests für das Projekt "Media Ratings Platform" (MRP).  
Die Entwicklung erfolgt nach dem Prinzip des Test Driven Development (TDD).  
Zuerst werden Tests erstellt (Red), danach der Code implementiert (Green) und zuletzt überarbeitet (Refactor).

---

## A. Benutzerverwaltung

### 1. Benutzer erstellen
- [x] Benutzer kann erstellt werden
- [x] Benutzername darf nicht leer sein oder nur aus Leerzeichen bestehen
- [x] Benutzername soll richtig gespeichert werden
- [x] Benutzername darf keine Sonderzeichen enthalten
- [x] Benutzername darf nicht zu lang sein
- [x] Benutzername darf nicht zu kurz sein
- [x] Neuer Benutzer mit gleichem Namen darf nicht erstellt werden
- [x] Passwort darf nicht leer sein
- [x] Passwort muss Mindestlänge haben, mit mindestens einem Sonderzeichen und einer Zahl
- [x] Passwort darf nicht den Benutzernamen enthalten
- [x] Passwort wird verschlüsselt gespeichert
- [x] Erstellungsdatum wird richtig gesetzt

### 2. Anmeldung (Login)
- [x] Login mit korrekten Daten erzeugt zufälligen Sicherheits-Token (Base64)
- [x] Login mit falschem Passwort wird abgelehnt
- [x] Login mit unbekanntem Benutzer wird abgelehnt
- [x] Login ist case-insensitive
- [x] Token wird intern gespeichert
- [x] Token Ablaufzeit ist korrekt gesetzt (+30 Minuten)
- [x] Token wird angenommen oder abgelehnt wenn abgelaufen
- [x] Token kann nur vom richtigen Benutzer verwendet werden
- [x] Abgemeldeter Benutzer verliert Gültigkeit seines Tokens
- [x] Abgelaufene Tokens werden nach Überprüfen aus der Liste gelöscht

### 3. Benutzerstatistik
- [x] Neuer Benutzer hat 0 Bewertungen
- [x] Durchschnittliche Bewertung wird richtig berechnet
- [x] Lieblingsgenre ergibt sich aus den meistbewerteten Medien
- [x] Anzahl der bewerteten Medien wird korrekt gezählt
- [x] Anzahl der Favoriten wird korrekt gezählt
- [x] Höchste vergebene Bewertung wird korrekt ermittelt
- [x] Niedrigste vergebene Bewertung wird korrekt ermittelt
- [x] Letzter Bewertungszeitpunkt wird korrekt gesetzt
- [x] Benutzer ohne Bewertungen hat kein Lieblingsgenre
- [x] Benutzer ohne Bewertungen hat keinen letzten Bewertungszeitpunkt

### 4. Benutzer löschen (Admin)

- [x] Ein nicht-administrativer Benutzer kann keinen anderen Benutzer löschen (Request wird mit Status 403 Forbidden abgelehnt)
- [x] Ein Administrator kann einen Benutzer erfolgreich löschen
- [x] Beim Löschen eines Benutzers werden alle zugehörigen Authentifizierungs-Tokens des Benutzers entfernt und ungültig gemacht
- [x] Beim Löschen eines Benutzers werden alle vom Benutzer erstellten Bewertungen entfernt
- [x] Beim Löschen eines Benutzers werden alle vom Benutzer vergebenen Likes entfernt
- [x] Beim Löschen eines Benutzers werden alle vom Benutzer gespeicherten Favoriten entfernt
- [x] Beim Löschen eines Benutzers werden alle vom Benutzer erstellten Medien ebenfalls gelöscht, inklusive der zugehörigen Bewertungen, Likes und Favoriten

---

## B. Medienverwaltung (CRUD)

### 1. Medien erstellen
- [x] Neues Medium kann erstellt werden (Titel, Beschreibung, Jahr, Genre(s), Altersfreigabe)
- [x] Titel darf nicht leer sein oder nur aus Leerzeichen bestehen
- [x] Titel wird vor der Speicherung getrimmt (Whitespace am Anfang/Ende entfernt)
- [x] Titel-Länge muss unter 150 Zeichen liegen
- [x] Beschreibung darf nicht leer sein oder nur aus Leerzeichen bestehen
- [x] Beschreibung wird vor der Speicherung getrimmt
- [x] Beschreibung-Länge muss zwischen 10 und 2000 Zeichen liegen
- [x] Genre-Liste darf nicht leer sein
- [x] Jedes Genre darf nicht leer sein oder nur aus Leerzeichen bestehen
- [x] Genre-Namen werden getrimmt
- [x] Anzahl der Genres ist begrenzt (maximal 5 Genres pro Medium)
- [x] Genre-Länge muss unter 40 Zeichen liegen
- [x] Keine zwei gleichen Genres
- [x] Altersfreigabe muss gültig sein (>= 0)
- [x] Altersfreigabe darf einen realistischen Maximalwert nicht überschreiten (z. B. <= 21)
- [x] Release-Jahr muss gültig sein (z. B. >= 1900)
- [x] Release-Jahr darf nicht in zu ferner Zukunft liegen (z. B. <= aktuelles Jahr + 1)
- [x] Ungültiger Medientyp wird abgelehnt
- [x] Erstellungsdatum CreatedAt wird beim Erstellen korrekt gesetzt
- [x] LastModifiedAt wird beim Erstellen auf denselben Wert wie CreatedAt gesetzt
- [x] Es darf keine zwei Medien mit gleichem Titel geben (case-insensitive)

### 2. Medien bearbeiten
- [x] Nur Ersteller darf Medium bearbeiten
- [x] Änderungen werden korrekt übernommen
- [x] Änderungen werden abgelehnt, wenn Titel/Beschreibung/Genre-Liste leer sind
- [x] Ungültige Änderungen (z. B. ungültiger Typ, Alter < 0, ungültiges Jahr) werden abgelehnt
- [x] Änderungszeitpunkt wird richtig gesetzt
- [x] Titel darf durch ein Update nicht zu einem Duplikat eines anderen Mediums werden

### 3. Medien löschen
- [x] Nur Ersteller darf Medium löschen
- [x] Beim Löschen werden zugehörige Bewertungen entfernt
- [x] Beim Löschen werden Favoriten entfernt
- [x] Statistiken werden nach dem Löschen aktualisiert

---

## C. Bewertungen

### 1. Bewertung abgeben
- [x] Benutzer kann Bewertung mit Wert 1–5 abgeben
- [x] Bewertungen außerhalb 1–5 werden abgelehnt
- [x] Bewertung hat einen CreatedAt-Zeitstempel
- [x] Kommentar ist optional
- [x] Kommentar wird vor der Speicherung getrimmt

### 2. Bewertung bearbeiten oder löschen
- [x] Nur Ersteller kann Bewertung ändern
- [x] Löschen entfernt Bewertung aus Durchschnitt

### 3. Durchschnittsberechnung
- [x] Durchschnittswert eines Mediums wird richtig berechnet
- [x] Durchschnitt ändert sich nach neuer Bewertung
- [x] Durchschnitt ignoriert ungültige Bewertungen

---

## D. Likes und Favoriten

### Likes
- [x] Benutzer kann fremde Bewertungen liken
- [x] Benutzer kann ein Rating nur einmal liken
- [x] Like kann wieder entfernt werden
- [x] Like-Zähler wird richtig aktualisiert

### Favoriten
- [x] Benutzer kann Medium als Favorit speichern
- [x] Favorit kann wieder entfernt werden
- [x] Favoritenliste zeigt alle gespeicherten Medien

---

## E. Suche und Filter

### Suche
- [ ] Suche nach Titel funktioniert (Teilstring)
- [ ] Suche ist nicht case-sensitive
- [ ] Leeres Ergebnis liefert leere Liste

### Filter
- [ ] Filter nach Genre funktioniert
- [ ] Filter nach Jahr funktioniert
- [ ] Filter nach Altersfreigabe funktioniert
- [ ] Kombination mehrerer Filter funktioniert

### Sortierung
- [ ] Sortierung nach Titel aufsteigend
- [ ] Sortierung nach Jahr absteigend
- [ ] Sortierung nach Bewertung absteigend

---

## F. Empfehlungen

### Empfehlung nach Genre
- [ ] Empfehlung enthält Medien mit ähnlichem Genre
- [ ] Keine doppelten Empfehlungen
- [ ] Nur Medien mit mindestens einer Bewertung werden empfohlen

### Empfehlung nach Inhalt
- [ ] Empfehlung berücksichtigt Medientyp
- [ ] Empfehlung berücksichtigt Altersfreigabe
- [ ] Empfehlung bevorzugt ähnliche Genres


## G. Statistiken

- [ ] Leaderboard listet Benutzer nach Anzahl Bewertungen
- [ ] Benutzerprofil zeigt Gesamtzahl Bewertungen und Likes
- [ ] Durchschnittsbewertung pro Benutzer wird richtig berechnet
- [ ] Leaderboard ist nach Aktivität sortiert

---

## Vorgehensweise
1. Einen Test nach dem anderen schreiben (Red)
2. Minimalen Code schreiben, bis der Test besteht (Green)
3. Code verbessern (Refactor)
4. Tests regelmäßig ausführen
5. Nach jedem erfolgreichen Test committen