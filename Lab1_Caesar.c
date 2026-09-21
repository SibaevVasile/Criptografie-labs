#include <stdio.h>
#include <string.h>

#define NR_LITERE 31   
#define MAX 300        

const char *LITERE_MARI[NR_LITERE] = {
    "A","Ă","Â","B","C","D","E","F","G","H","I","Î","J","K","L","M",
    "N","O","P","Q","R","S","Ș","T","Ț","U","V","W","X","Y","Z"
};
const char *LITERE_MICI[NR_LITERE] = {
    "a","ă","â","b","c","d","e","f","g","h","i","î","j","k","l","m",
    "n","o","p","q","r","s","ș","t","ț","u","v","w","x","y","z"
};

int lungimeCaracter(unsigned char primulOctet) {
    if ((primulOctet & 0x80) == 0x00) return 1;
    return 2;
}

int cautaCod(const char *litera, const char *tabel[]) {
    for (int i = 0; i < NR_LITERE; i++)
        if (strcmp(litera, tabel[i]) == 0) return i;
    return -1;
}

int textLaCoduri(const char *text, int coduri[]) {
    int n = 0, i = 0;
    while (text[i] != '\0') {
        int lung = lungimeCaracter((unsigned char)text[i]);
        char litera[3] = {0};
        for (int k = 0; k < lung; k++) litera[k] = text[i + k];

        int cod = cautaCod(litera, LITERE_MARI);
        if (cod == -1) cod = cautaCod(litera, LITERE_MICI);
        if (cod != -1) coduri[n++] = cod;

        i += lung;
    }
    return n;
}

void afiseazaCoduri(int coduri[], int n) {
    for (int i = 0; i < n; i++)
        printf("%s", LITERE_MARI[coduri[i]]);
    printf("\n");
}

void deplaseaza(int coduri[], int n, int cheie) {
    for (int i = 0; i < n; i++) {
        int nou = (coduri[i] + cheie) % NR_LITERE;
        if (nou < 0) nou += NR_LITERE;
        coduri[i] = nou;
    }
}

void construiestePermutare(int coduriCheie2[], int lungCheie2, int permutare[]) {
    int folosit[NR_LITERE] = {0};
    int n = 0;

    for (int i = 0; i < lungCheie2; i++) {
        int c = coduriCheie2[i];
        if (!folosit[c]) {
            permutare[n++] = c;
            folosit[c] = 1;
        }
    }
    for (int c = 0; c < NR_LITERE; c++)
        if (!folosit[c]) permutare[n++] = c;
}

int pozitieIn(int c, int permutare[]) {
    for (int i = 0; i < NR_LITERE; i++)
        if (permutare[i] == c) return i;
    return -1;
}

void deplaseazaCuPermutare(int coduri[], int n, int cheie, int permutare[]) {
    for (int i = 0; i < n; i++) {
        int pos = pozitieIn(coduri[i], permutare);
        int nouaPos = (pos + cheie) % NR_LITERE;
        if (nouaPos < 0) nouaPos += NR_LITERE;
        coduri[i] = permutare[nouaPos];
    }
}

void citesteLinie(char *buf, int dim) {
    fgets(buf, dim, stdin);
    int lung = strlen(buf);
    if (lung > 0 && buf[lung - 1] == '\n') buf[lung - 1] = '\0';
}

int citesteCheie(void) {
    int cheie;
    while (1) {
        printf("Introduceti cheia (numar intreg intre 1 si %d): ", NR_LITERE - 1);
        if (scanf("%d", &cheie) == 1 && cheie >= 1 && cheie <= NR_LITERE - 1) break;
        printf("Valoare invalida. Incercati din nou.\n");
        while (getchar() != '\n');   
    }
    while (getchar() != '\n');      
    return cheie;
}

char citesteOperatie(void) {
    char linie[10];
    while (1) {
        printf("Alegeti operatia (C = criptare, D = decriptare): ");
        citesteLinie(linie, sizeof(linie));
        if (linie[0] == 'C' || linie[0] == 'c') return 'C';
        if (linie[0] == 'D' || linie[0] == 'd') return 'D';
        printf("Valoare invalida. Introduceti C sau D.\n");
    }
}

void optiuneOCheie(void) {
    char op = citesteOperatie();
    int cheie = citesteCheie();

    printf("Introduceti mesajul: ");
    char text[MAX];
    citesteLinie(text, sizeof(text));

    int coduri[MAX];
    int n = textLaCoduri(text, coduri);

    if (op == 'D') cheie = -cheie;
    deplaseaza(coduri, n, cheie);

    printf("%s - rezultat: ", (op == 'C') ? "Criptare" : "Decriptare");
    afiseazaCoduri(coduri, n);
}

void optiuneDouaChei(void) {
    char op = citesteOperatie();
    int cheie = citesteCheie();

    int coduriCheie2[MAX], lungCheie2;
    while (1) {
        printf("Introduceti cheia 2 (minim 7 litere din alfabetul roman): ");
        char cheie2Text[MAX];
        citesteLinie(cheie2Text, sizeof(cheie2Text));
        lungCheie2 = textLaCoduri(cheie2Text, coduriCheie2);
        if (lungCheie2 >= 7) break;
        printf("Cheia 2 trebuie sa aiba cel putin 7 litere ale alfabetului roman.\n");
    }

    int permutare[NR_LITERE];
    construiestePermutare(coduriCheie2, lungCheie2, permutare);

    printf("Introduceti mesajul: ");
    char text[MAX];
    citesteLinie(text, sizeof(text));

    int coduri[MAX];
    int n = textLaCoduri(text, coduri);

    if (op == 'D') cheie = -cheie;
    deplaseazaCuPermutare(coduri, n, cheie, permutare);

    printf("%s - rezultat: ", (op == 'C') ? "Criptare" : "Decriptare");
    afiseazaCoduri(coduri, n);
}

int main(void) {
    int optiune;
    do {
        printf("\n===== CIFRUL CEZAR (alfabet roman, %d litere) =====\n", NR_LITERE);
        printf("1. Cifrul Cezar cu o cheie\n");
        printf("2. Cifrul Cezar cu doua chei\n");
        printf("0. Iesire\n");
        printf("Alegeti optiunea: ");

        if (scanf("%d", &optiune) != 1) {
            while (getchar() != '\n');
            printf("Optiune invalida.\n");
            continue;
        }
        while (getchar() != '\n');

        switch (optiune) {
            case 1: optiuneOCheie(); break;
            case 2: optiuneDouaChei(); break;
            case 0: printf("La revedere!\n"); break;
            default: printf("Optiune invalida.\n");
        }
    } while (optiune != 0);

    return 0;
}