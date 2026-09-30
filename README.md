#Projekt Shire

1. Modele danych

    1. Punkt
        ◦ Przechowuje współrzędne (x, y)

    2. Region
        ◦ Reprezentuje wypukły wielokąt oraz wartość ilości jęczmienia uzyskiwana z pola w tym regionie

    3. Pole
        ◦ Ma lokalizację (Punkt Lokalizacja) oraz pole z ilością jęczmienia (double), wyliczane na podstawie tego, w którym regionie się znajduje.

    4. Browar
        ◦ Lokalizacja i maksymalna ilość jęczmienia, którą może przerobić.

    5. Karczma
        ◦ Lokalizacja, brak ograniczenia pojemności tzn. przyjmuje dowolną ilość piwa

    6. Droga
        ◦ Połączenie z punktu do punktu z właściwościami:
            i. Pojemność Jęczmienia
            ii. Pojemność Piwa
            iii. Koszt naprawy  - Używany, gdy minimalizujemy całkowite koszty transportu

2. Budowanie sieci przepływowej

Cała logika transportu zamodelowana jest jako skierowany graf przepływowy (sieć), w której:
    1. Źródło wysyła na początek tyle jednostek, ile wynosi łączna ilość jęczmienia dla wszystkich pól.
    2. Węzły reprezentujące pola – z każdego pola wypływa do sieci tyle jednostek, ile jest jęczmienia.
    3. Warstwa skrzyżowań w części „jęczmiennej” – pola łączą się z najbliższymi węzłami-skrzyżowaniami, a skrzyżowania między sobą są połączone drogami, gdzie każda droga ma ograniczoną pojemność na jęczmienie i koszt naprawy tej drogi.
    4. Węzły „browarów” w warstwie jęczmiennej – do nich trafia od skrzyżowań ograniczona ilość (według pojemności browaru).
    5. Warstwa „browarów-piwo” -> warstwa skrzyżowań w części „piwnej” – browar konwertuje przychodzące jednostki jęczmienia na jednostki piwa (pojemność konwersji = pojemność wejściowa) i przekazuje je dalej do skrzyżowań piwnych.
    6. Warstwa skrzyżowań w części „piwnej” – drogi między nimi mają swoją pojemność dla piwa oraz koszty naprawy (analogicznie jak w warstwie jęczmiennej).
    7. Węzły „karczm” – ze skrzyżowań piwnych idą krawędzie o pojemności nieskończoność (karczmy mogą przyjąć dowolną ilość piwa) do ujścia gdzie kończy się przepływ.





3. Algorytmy
1. Ford–Fulkerson z DFS
    • Celem jest wyznaczenie maksymalnego przepływu od źródła do ujścia bez kosztów.
    • Zasada działania:
        1. Startujemy z całkowicie pustym przepływem.
        2. Szukamy w grafie rezydualnym dowolnej ścieżki powiększającej używając DFS.
        3. Znalezioną ścieżką „przepychamy” możliwie najwięcej razy, aktualizujemy przepływ w każdej krawędzi oraz w odwróconych krawędziach.
        4. Powtarzamy kroki 2–3, aż nie da się znaleźć więcej ścieżek z dodatnią rezydualną pojemnością.
    • Złożoność: W najgorszym przypadku O(E * przepływ), ale na praktycznych rozmiarach działa zwykle wystarczająco szybko.
2. Bellman-Ford
    • Cel: Znaleźć maksymalny przepływ lub dokładnie określoną jego wartość, jednocześnie minimalizując całkowite koszty z dodatkiem do ograniczenia kosztów poprzez budżet. 
    • Zasada działania:
        1. Początkowo (przepływ=0) nie ma żadnych kosztów.
        2. W każdej iteracji znajdujemy w grafie rezydualnym najtańszą ścieżkę od źródła do ujścia, przy uwzględnieniu rezydualnych pojemności i kosztów krawędzi (algorytm SPFA / Bellman-Ford w wersji z kolejką).
        3. Wyznaczamy maksymalny możliwy do przepchnięcia przepływ na tej ścieżce, przesyłamy go i aktualizujemy koszty.
        4. Powtarzamy, aż nie będzie ścieżki lub osiągniemy wymaganą ilość przepływu.
    • Złożoność: O(V * E).
3. Reguła parzystości
    • Cel: Sprawdzenie, czy punkt leży wewnątrz wielokąta wypukłego.
    • Zasada działania:
        ◦ Dla danego punktu i listy wierzchołków wypukłego wielokąta wybieramy półprostą wychodzącą z punktu oraz liczymy liczbę przecięć z krawędziami wielokąta.
    • Złożoność: Działa w czasie O(n), gdzie n = liczba wierzchołków wielokąta.
4. Knuth–Morris–Pratt (KMP)
    • Cel: Wyszukiwanie wszystkich wystąpień wzorca w danym tekście w czasie liniowym O(n+m).
    • Zasada działania:
        1. Najpierw budujemy tablicę prefiksów (tzw. pi[i]) dla wzorca.
        2. Przechodzimy po tekście, w razie niezgodności cofamy się zgodnie z tablicą pi, co pozwala uniknąć ponownego porównywania znaków.
        3. Działa w czasie O(n+m).
Użycie w projekcie: Pozwala szybko wyszukać w dowolnym tekście interesujące słowa (np. „piwo”, „jęczmień”) nawet przy ignorowaniu wielkości liter.


4. Podsumowanie
    1. Modele:
        ◦ Punkt, Region, Pole, Browar, Karczma, Droga
        ◦ Obliczyć, w którym regionie znajduje się każde pole → przypisać plon.
        ◦ Zgromadzić informacje o pojemnościach i kosztach dróg, browarów, karczm.





    2. Budowa sieci:
        ◦ Sieć jest wielowarstwowa:
            ▪ Źródło  -> Pola
            ▪ Pola  -> skrzyżowania (jęczmień)  -> browary (jęczmień)  -> browary (piwo)  -> karczmy  -> ujście
        ◦ Każda krawędź ma:
            ▪ Pojemność (jęczmienia albo piwa)
            ▪ Koszt naprawy drogi

    3. Algorytmy:
        ◦ Ford–Fulkerson (DFS)  - maksymalny przepływ bez kosztów; szybki do implementacji, szukamy dowolnej ścieżki powiększającej.
        ◦ Belmann-Ford - maksymalny przepływ przy minimalnym sumarycznym koszcie; w każdej iteracji wybieramy najtańszą ścieżkę w residualnym grafie.
        ◦ Reguła parzystości  - przypisywanie pola do regionu (wypukły wielokąt)
        ◦ KMP  - szybkie wyszukiwanie wzorców w tekście, przydatne do prostego indeksowania i wyszukiwania słów kluczowych w dowolnych opisach.


