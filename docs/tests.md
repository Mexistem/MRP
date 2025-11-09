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
- [x] Zwei Benutzer mit gleichem Namen gelten als gleich
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
- [ ] Login mit korrekten Daten gibt gültigen Token zurück
- [ ] Login mit falschem Passwort wird abgelehnt
- [ ] Login mit unbekanntem Benutzer wird abgelehnt
- [ ] Login ist case-insensitive
- [ ] Token enthält korrekten Benutzernamen und festen Suffix „-mrpToken“
- [ ] Token wird intern gespeichert und ist eindeutig pro Benutzer
- [ ] Token Ablaufzeit ist korrekt gesetzt (+30 Minuten)
- [ ] Gültiger Token wird akzeptiert
- [ ] Abgelaufener Token wird abgelehnt
- [ ] Token kann nur vom richtigen Benutzer verwendet werden
- [ ] Abgemeldeter Benutzer verliert Gültigkeit seines Tokens

### 3. Benutzerstatistik
- [ ] Neuer Benutzer hat 0 Bewertungen
- [ ] Durchschnittliche Bewertung wird richtig berechnet
- [ ] Lieblingsgenre ergibt sich aus den meistbewerteten Medien

---

## B. Medienverwaltung (CRUD)

### 1. Medien erstellen
- [ ] Neues Medium kann erstellt werden (Titel, Jahr, Genre)
- [ ] Titel darf nicht leer sein
- [ ] Medientyp muss gültig sein (Film, Serie, Spiel)
- [ ] Erstellungsdatum wird richtig gespeichert

### 2. Medien bearbeiten
- [ ] Nur Ersteller darf Medium bearbeiten
- [ ] Änderungen werden korrekt übernommen
- [ ] Ungültige Änderungen werden abgelehnt

### 3. Medien löschen
- [ ] Nur Ersteller darf Medium löschen
- [ ] Beim Löschen werden zugehörige Bewertungen entfernt
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