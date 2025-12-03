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
- [ ] Abgelaufene Tokens werden nach Überprüfen aus der Liste gelöscht

### 3. Benutzerstatistik
- [ ] Neuer Benutzer hat 0 Bewertungen
- [ ] Durchschnittliche Bewertung wird richtig berechnet
- [ ] Lieblingsgenre ergibt sich aus den meistbewerteten Medien

---

## B. Medienverwaltung (CRUD)

### 1. Medien erstellen
- [x] Neues Medium kann erstellt werden (Titel, Beschreibung, Jahr, Genre(s), Altersfreigabe)
- [ ] Titel darf nicht leer sein oder nur aus Leerzeichen bestehen
- [ ] Titel wird vor der Speicherung getrimmt (Whitespace am Anfang/Ende entfernt)
- [ ] Titel-Länge muss zwischen unter 150 Zeichen liegen
- [ ] Beschreibung darf nicht leer sein oder nur aus Leerzeichen bestehen
- [ ] Beschreibung wird vor der Speicherung getrimmt
- [ ] Beschreibung-Länge muss zwischen 10 und 2000 Zeichen liegen
- [ ] Genre-Liste darf nicht leer sein
- [ ] Jedes Genre darf nicht leer sein oder nur aus Leerzeichen bestehen
- [ ] Genre-Namen werden getrimmt
- [ ] Anzahl der Genres ist begrenzt (z. B. maximal 5 Genres pro Medium)
- [ ] Genre-Länge muss zwischen 2 und 40 Zeichen liegen
- [ ] Altersfreigabe muss gültig sein (>= 0)
- [ ] Altersfreigabe darf einen realistischen Maximalwert nicht überschreiten (z. B. <= 21)
- [ ] Release-Jahr muss gültig sein (z. B. >= 1900)
- [ ] Release-Jahr darf nicht in zu ferner Zukunft liegen (z. B. <= aktuelles Jahr + 1)
- [ ] Medientyp muss gültig sein
- [ ] Ungültiger Medientyp wird abgelehnt
- [ ] Erstellungsdatum CreatedAt wird beim Erstellen korrekt gesetzt
- [ ] LastModifiedAt wird beim Erstellen auf denselben Wert wie CreatedAt gesetzt
- [ ] Es darf keine zwei Medien mit gleichem Titel geben (case-insensitive)
- [ ] Titel-Vergleich für Duplikate ignoriert führende und folgende Leerzeichen
- [ ] Duplikatversuch liefert eine aussagekräftige Fehlermeldung

### 2. Medien bearbeiten
- [ ] Nur Ersteller darf Medium bearbeiten
- [ ] Änderungen werden korrekt übernommen
- [ ] Änderungen werden abgelehnt, wenn Titel/Beschreibung/Genre-Liste leer sind
- [ ] Ungültige Änderungen (z. B. ungültiger Typ, Alter < 0, ungültiges Jahr) werden abgelehnt
- [ ] Änderungszeitpunkt wird richtig gesetzt
- [ ] Titel darf durch ein Update nicht zu einem Duplikat eines anderen Mediums werden

### 3. Medien löschen
- [ ] Nur Ersteller darf Medium löschen
- [ ] Beim Löschen werden zugehörige Bewertungen entfernt
- [ ] Beim Löschen werden Favoriten entfernt
- [ ] Statistiken werden nach dem Löschen aktualisiert

---

## C. Bewertungen

### 1. Bewertung abgeben
- [ ] Benutzer kann Bewertung mit Wert 1–5 abgeben
- [ ] Bewertungen außerhalb 1–5 werden abgelehnt
- [ ] Bewertung hat Zeitstempel
- [ ] Kommentar ist optional

### 2. Bewertung bearbeiten oder löschen
- [ ] Nur Ersteller kann Bewertung ändern
- [ ] Kommentaränderung muss bestätigt werden
- [ ] Löschen entfernt Bewertung aus Durchschnitt

### 3. Durchschnittsberechnung
- [ ] Durchschnittswert eines Mediums wird richtig berechnet
- [ ] Durchschnitt ändert sich nach neuer Bewertung
- [ ] Durchschnitt ignoriert ungültige Bewertungen

---

## D. Likes und Favoriten

### Likes
- [ ] Benutzer kann fremde Bewertungen liken
- [ ] Benutzer kann ein Rating nur einmal liken
- [ ] Like kann wieder entfernt werden
- [ ] Like-Zähler wird richtig aktualisiert

### Favoriten
- [ ] Benutzer kann Medium als Favorit speichern
- [ ] Favorit kann wieder entfernt werden
- [ ] Favoritenliste zeigt alle gespeicherten Medien

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

---

## G. Sicherheit

### Token-Validierung
- [ ] Request ohne Token wird abgelehnt (401)
- [ ] Ungültiger Token führt zu 401
- [ ] Gültiger Token erlaubt Zugriff

### Zugriffskontrolle
- [ ] Benutzer darf nur eigene Daten bearbeiten
- [ ] Administrator darf alle Daten lesen
- [ ] Ungültige Anfragen liefern korrekte Statuscodes (400, 403, 404)

---

## H. Statistiken

- [ ] Leaderboard listet Benutzer nach Anzahl Bewertungen
- [ ] Benutzerprofil zeigt Gesamtzahl Bewertungen und Likes
- [ ] Durchschnittsbewertung pro Benutzer wird richtig berechnet
- [ ] Leaderboard ist nach Aktivität sortiert

---

## I. Datenbank

- [ ] Daten werden korrekt in PostgreSQL gespeichert
- [ ] Daten können korrekt ausgelesen werden
- [ ] Nach Neustart bleiben Daten erhalten
- [ ] Tokens werden nicht gespeichert

---

## Testframeworks
- Unit-Tests mit MSTest
- Mocking mit Moq (für Datenbank oder Services)
- Integrationstests mit Postman

---

## Vorgehensweise
1. Einen Test nach dem anderen schreiben (Red)
2. Minimalen Code schreiben, bis der Test besteht (Green)
3. Code verbessern (Refactor)
4. Tests regelmäßig ausführen
5. Nach jedem erfolgreichen Test committen