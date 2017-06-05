# RC2Json - Community Edition
Tool di trasformazione dei files di descrizione interfaccia .RC in equivalenti in formato TB Json. 
Per una descrizione completa si veda [RC2Json Community Edition Wiki](https://github.com/Microarea/RC2Json-CE/wiki)
## Prerequisiti
Si tratta di una applicazione C#, l'unico prerequisito è Visual Studio 2015.
## Installazione
Clonare o scaricare il source code, quindi effettuare la build della solution.
## Uso
È un tool da command-line, quindi aprire un prompt di comandi per eseguirlo.  
La sintassi per eseguirlo è:

    RC2Json [comando] [file o cartelle]

### Comando: /rc
Questa è la funzionalità principale del tool: trasformare uno o più file in formato RC nella corrispondente sintassi TB Json.
Vengono generati sia i files di tipo `.tbjson` che `.hjson`, questi ultimi da includersi nei source files C++ (al posto dei `.hrc`) per avere i riferimenti agli ID.  
Se si indica il nome di un file, viene analizzato solo quello, se si indica una cartella vengono analizzati tutti i files contenuti nella cartella e ricorsivamente in tutte le sottocartelle.  
Vengono analizzati solo i files `.rc` che hanno un corrispondente `.hrc`.  
Vedi più sotto [Funzionamento](#funzionamento) per i dettagli della trasformazione effettuata.

### Comando /checkrc
TODO 

### Comando /intellisense
TODO 

### Comando /updateprojects
TODO 

### Comando /updatesources
TODO 

### Comando /compact
TODO 

### Comando /cmp
TODO 

## Funzionamento
Il tool effettua la trasformazione applicando alcune regole e valutazioni euristiche.  
I file di partenza (`.rc` e `.hrc`) non vengono modificati, quindi l'operazione si può effettuare anche più volte.

Per ogni risorsa dialog (`DIALOG` e `DIALOGEX`) viene creata una coppia `.tbjson`-`.hjson` dallo stesso nome (es.: `IDD_BOXES.tbjson`).  
Per decidere il folder di destinazione, si cerca di attribuire alla risorsa un namespace di documento, in questo modo:
* Se il file inizia con `UI` (es.: `UIBoxes.rc`) si rimuove il prefisso e si cerca in `DocumentObjects.xml` e `ClientDocumentObjects.xml` un oggetto con lo stesso nome (es.: `Boxes`)
* Se esiste un file `Jsonusers.xml` nella cartella del `.rc` o in quella dell'applicazione, si cerca un match tra nome file e namespace documento, il formato è il seguente:
    ````xml
        <Documents>
          <Document hjson="UICollections.hjson" name="Collections" />
          <Document ... />
        </Documents>
    ````
Se il match viene trovato, i `.tbjson`-`.hjson` vengono generati nella cartella di documento: `ModuleObjects\<nome documento>\JsonForms`.  
Altrimenti i file sono generati nella cartella delle risorse json generiche di modulo: `JsonForms\<nome .rc>\`.

Il tool genera inoltre nella cartella del `.rc` un file `.hjson` dallo stesso nome, che contiene le `#include` di tutti gli altri `.hjson` delle singole risorse generate: tale file si può includere nei sorgenti C++ al posto del corrispondente `.hrc`.
