# Projekt Shire

System modelowania sieci optymalizacji transportu surowców i produktów (jęczmień/piwo) z wykorzystaniem algorytmów grafowych oraz geometrii obliczeniowej.

---

## 1. Modele Danych

* **Punkt**
  * Przechowuje współrzędne dwuwymiarowe $(x, y)$.
* **Region**
  * Reprezentuje wypukły wielokąt oraz określa jednostkową ilość jęczmienia uzyskiwaną z pola leżącego w tym obszarze.
* **Pole**
  * Posiada określoną lokalizację (`Punkt`) oraz wyliczaną ilość jęczmienia (`double`), ustalaną na podstawie przynależności do danego regionu.
* **Browar**
  * Posiada lokalizację oraz maksymalną przepustowość (pojemność przetwórczą) jęczmienia.
* **Karczma**
  * Posiada lokalizację; brak ograniczeń pojemnościowych (przyjmuje dowolną ilość piwa).
* **Droga**
  * Połączenie krawędziowe pomiędzy dwoma punktami z określonymi właściwościami:
    * Pojemność dla transportu jęczmienia
    * Pojemność dla transportu piwa
    * Koszt naprawy (wykorzystywany przy minimalizacji całkowitych kosztów transportu)

---

## 2. Budowa Sieci Przepływowej

Transport zamodelowany jest jako **skierowany graf przepływowy (sieć)** o strukturze wielowarstwowej:

```text
[Źródło] ──> [Pola] ──> [Skrzyżowania (Jęczmień)] ──> [Browary (Przetwórstwo)] ──> [Skrzyżowania (Piwo)] ──> [Karczmy] ──> [Ujście]
```

### Logika przepływu:
1. **Źródło:** Wysyła sumaryczną ilość jęczmienia wygenerowaną ze wszystkich pól.
2. **Węzły pól:** Wprowadzają do sieci ilość surowca odpowiadającą plonom z danego pola.
3. **Warstwa skrzyżowań (Jęczmień):** Pola łączą się z najbliższymi węzłami-skrzyżowaniami. Skrzyżowania połączone są drogami o ograniczonej pojemności i określonym koszcie naprawy.
4. **Węzły Browarów (wejście):** Odbierają jęczmień ze skrzyżowań w granicach swojej maksymalnej pojemności.
5. **Konwersja w Browarach:** Browar konwertuje przychodzące jednostki jęczmienia na piwo w stosunku 1:1 i przekazuje je do warstwy skrzyżowań piwnych.
6. **Warstwa skrzyżowań (Piwo):** Transport piwa między skrzyżowaniami z uwzględnieniem dedykowanych pojemności i kosztów dróg.
7. **Karczmy i Ujście:** Karczmy połączone są ze skrzyżowaniami piwnymi krawędziami o nieskończonej pojemności, a z karczm przepływ trafia bezpośrednio do Ujścia.

---

## 3. Algorytmy

### 3.1. Ford–Fulkerson (z wykorzystaniem DFS)
* **Cel:** Wyznaczenie maksymalnego przepływu od źródła do ujścia (bez uwzględniania kosztów).
* **Zasada działania:**
  1. Inicjalizacja sieci zerowym przepływem.
  2. Wyszukiwanie dowolnej ścieżki powiększającej w grafie rezydualnym za pomocą algorytmu **DFS**.
  3. Aktualizacja przepływu wzdłuż znalezionej ścieżki oraz w krawędziach odwrotnych o wartość wąskiego gardła (*bottleneck*).
  4. Powtarzanie kroków 2–3 do momentu braku ścieżek powiększających.
* **Złożoność:** $O(E \cdot f_{max})$, gdzie $E$ to liczba krawędzi, a $f_{max}$ to maksymalny przepływ.

### 3.2. Bellman-Ford / SPFA (Min-Cost Max-Flow)
* **Cel:** Wyznaczenie maksymalnego przepływu przy minimalizacji sumarycznego kosztu transportu (z opcją ograniczenia budżetowego).
* **Zasada działania:**
  1. Inicjalizacja przepływu wartością 0.
  2. Wyszukiwanie **najtańszej** ścieżki powiększającej w grafie rezydualnym przy użyciu wariantu algorytmu Bellmana-Forda (SPFA z kolejką).
  3. Przepychanie maksymalnego możliwego przepływu wzdłuż najtańszej ścieżki i aktualizacja kosztów.
  4. Iteracja do wyczerpania ścieżek lub osiągnięcia docelowego wolumenu przepływu / limitu budżetu.
* **Złożoność:** $O(V \cdot E)$.

### 3.3. Reguła Parzystości (Ray Casting Algorithm)
* **Cel:** Weryfikacja, czy dane Pole leży wewnątrz określonego Regionu (wielokąta wypukłego).
* **Zasada działania:**
  * Wyznaczenie półprostej wychodzącej z testowanego punktu i zliczenie liczby jej przecięć z krawędziami wielokąta. Nieparzysta liczba przecięć oznacza, że punkt znajduje się wewnątrz obszaru.
* **Złożoność:** $O(n)$, gdzie $n$ to liczba wierzchołków wielokąta.

### 3.4. Knuth–Morris–Pratt (KMP)
* **Cel:** Liniowe wyszukiwanie wzorców tekstowych w opisach i indeksach (np. słów kluczowych takich jak „piwo”, „jęczmień”).
* **Zasada działania:**
  1. Konstrukcja tablicy prefiksowej ($\pi$) dla wyszukiwanego wzorca.
  2. Przeszukiwanie tekstu z wykorzystaniem tablicy $\pi$ do omijania niepotrzebnych porównań po wystąpieniu niezgodności.
* **Złożoność:** $O(n + m)$, gdzie $n$ to długość tekstu, a $m$ długość wzorca.

---

## 4. Podsumowanie Architektury

| Obszar | Kluczowe Elementy | Opis / Rola |
| :--- | :--- | :--- |
| **Modelowanie** | `Punkt`, `Region`, `Pole`, `Browar`, `Karczma`, `Droga` | Przypisanie pól do regionów i wyliczenie plonów, wyznaczenie parametrów sieci. |
| **Topologia** | Wielowarstwowy Graf Skierowany | Zapewnienie rozdzielności logicznej transportu surowca i produktu gotowego. |
| **Optymalizacja** | Ford–Fulkerson, Bellman-Ford | Wyznaczanie przepustowości maksymalnej oraz wariantów najtańszych. |
| **Analiza i Pomocnicze**| Reguła Parzystości, KMP | Klasyfikacja geometryczna punktów oraz szybkie indeksowanie tekstu. |