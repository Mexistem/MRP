# MRP – Testplan für TDD

Dieses Dokument beschreibt die geplanten Unit-Tests für das Projekt "Media Ratings Platform" (MRP).  
Die Entwicklung erfolgt nach dem Prinzip des Test Driven Development (TDD).  
Zuerst werden Tests erstellt (Red), danach der Code implementiert (Green) und zuletzt überarbeitet (Refactor).

---

## A. Benutzerverwaltung

### 1. Benutzer erstellen
- [ ] Benutzer kann erstellt werden
- [ ] Benutzername darf nicht leer sein
- [ ] Benutzername darf keine Sonderzeichen enthalten
- [ ] Passwort darf nicht leer sein
- [ ] Passwort wird verschlüsselt gespeichert
- [ ] Erstellungsdatum wird richtig gesetzt

### 2. Anmeldung (Login)
- [ ] Login mit korrekten Daten gibt gültigen Token zurück
- [ ] Login mit falschem Passwort wird abgelehnt
- [ ] Token enthält Benutzername und Ablaufzeit
- [ ] Abgelaufener Token wird abgelehnt

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