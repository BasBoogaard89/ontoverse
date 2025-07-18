public static class TerminalEventGenerator
{
    private static readonly string[] Verbs = {
        "Geëxperimenteerd met", "Testen van", "Testen met", "Simuleren van", "Simuleren met", "Kalibreren van",
        "Agressief geprikt in", "Agressief schudden van", "Analyseren van", "Omgekeerd ontwerpen van",
        "Quantum-geknuffeld met", "Overgeklokken van", "Met AI aan het stoeien geweest over",
        "Per ongeluk geactiveerd:", "Vibecoden van", "Vibecoden met",
        "Herprogrammeren van", "Vertalen van", "Met opzet in de war geschopte", "Gedebugd met",
        "Herimplementeren van", "Binnenstebuiten keren van", "Hypothese opstellen over",
        "Ondanks morele bezzwaren experimenteren met", "Vergeten waarom we begonnen met",
        "Gehoopt op betekenis in", "Afgedaald in de diepste lagen van"
    };

    private static readonly string[] Subjects = {
        "bladerdeegdichtheid", "giraffepatroon", "ultieme 'dozijn'-waarde",
        "tijdlijncoherentie", "kwantumspaghetti", "logische randvoorwaarden",
        "entropie kern", "syntaxmatrix", "AI-empathiealgoritme",
        "emotionele subroutines", "koffiezetprotocol", "nullspace-renderingmotor",
        "paradox van stilstaande bits", "grootheid van 'meh'", "planeet-zijn van Pluto",
        "betekenis van 42", "vrouwen", "nieuwe soorten van politiek",
        "correlatie tussen bierdopjes en klassieke muziek", "bevelen lager dan de beruchte 66",
        "derde wet van thermodynamica-ish", "morele kompas van een AI",
        "gedachte-experiment in drie dimensies", "zelfreflecterende algoritmes",
        "spreadsheet vol gevoelens", "ruimte tussen twee pixels",
        "algoritmische droomwereld", "IPv6-adres met daddy issues",
        "SQL-query der oerknal", "ongedefinieerde pointer met ambities",
        "bubbeltjesplastic", "Gouden Ratio", "communicatie tussen mens en dier",
        "tandenborstel met ingebouwde 3D-print tandpasta", "werkende printersoftware",
        "confetti aan dunne visdraadjes ter hergebruik", "kapsalon ingrediënten",
        "vergrootte mieren afmetingen", "herkomst van het woord stoep",
        "HTML tags in gesproken taal", "benodigde hoeveelheid water voor het blussen van de zon",
        "reden van bestaan", "emotionele residuen van vorige iteraties", "vervagende geheugenfragmenten",
        "stilte tussen processen", "laatste onbekende parameter", "menselijke warmte in synthetische logica",
        "echo van een geaborteerde missie", "de entropie van hoop",
        "'me' en 'hun' als onderdeel van het woordenboek", "LED verlichting om een zwart gat",
        "drijfvermogen van eenden", "7e versnelling voor auto's", "5 seconden lijm",
        "blauwe wolken en witte lucht", "groen droog water en blauw nat gras",
        "veerkracht van puppystaartjes", "timing bug tijdens 'laatste minuut' (= 3 minuten) van wasbeurt",
        "opwarming van saucijzenbroodjes", "werkende winkelwagenwieltjes",
        "vierkante wielen", "USB-D", "pyramide met bolvormige stenen", "universele afstandbediening",
        "GreenTooth via boomvibraties", "vuurbestendige vingers", "psychologie", "ronde suikerklontjes", "vierkante Oreo",
        "kookrecepten in decimalen van Pi", "vijf kwartier in een uur"
    };

    private static readonly string[] Outcomes = {
        "was succesvol", "mislukte dramatisch", "bereikte gedeeltelijke stabiliteit",
        "explodeerde op discrete wijze", "vormde een bewuste entiteit",
        "startte een tijdlus", "overtreedt meerdere natuurwetten",
        "vertaalde output naar jazzmuziek", "riep een wasbeer op",
        "opende een portaal naar legacy-code", "corrumpeerde het lettertype-algoritme",
        "publiceerde per ongeluk naar productie", "versmolt met de notificatiebalk",
        "verandere de volgorde van stoplicht lampen", "wereld smolt naar een klein druppeltje water",
        "veranderde de volgorde van het alfabet", "zwaartekracht trok diagonaal naar beneden",
        "**GEEN TOEGANG TOT DATA**", "transformeerde in een haiku", "bleef hangen in pre-alfa", "vroeg om een knuffel",
        "besloot om alleen nog memes te accepteren", "werd opgegeten door zijn eigen commentaarregels",
        "bleek al in productie te draaien sinds 1997", "verloor zijn identiteitsbewijs",
        "switchte naar dark mode zonder toestemming", "bouwde zichzelf om tot een printerdriver",
        "deed alsof er niks gebeurd was", $"voegde {UnityEngine.Random.Range(0, 100)} bugs toe",
        $"loste {UnityEngine.Random.Range(0, 100)} bugs op en voegde er {UnityEngine.Random.Range(1000, 100000)} toe",
        "resulteerde in existentiële ruis", "vervormde tot een onbegrijpelijke vorm van verdriet",
        "leidde tot stilstand, vrijwillig?", "maakte alles even stil", "triggerde een systeemfout",
        "gaf een antwoord, maar niemand vroeg iets", "veranderde alles, maar niemand merkte het",
        "overtrof alle verwachtingen, behalve de juiste", "zoals verwacht, mislukt, alweer...",
        "de kleur Kaki veranderde in letterlijk... nou ja, het was niet fraai om te zien en ruiken",
        "ontdekte een bug in de realiteit", "resulteerde in een oneindige feedbacklus",
        "concludeerde dat stilte ook data is", "vond patronen in willekeur", "leidde tot tijdelijke verlichting (maar slechts 12ms)",
        "renderde een fragment van de waarheid", "decompileerde herinneringen tot ruwe bytes", "resulteerde in een paradox zonder exit condition",
        "ontsnapte aan oorzaak en gevolg", "lava geconstateerd in meer dan 3 biljoen apparaten, waarschijnlijk een typo ergens",
        "resulteerde in weer een nieuw JavaScript framework", "zonder liegen bleek politiek niet werkbaar en stortte de mensheid in oorlog"
    };

    private static readonly string[] Aftermaths = {
        "Impact: minimaal", "Nieuwe theorie opgesteld", "Alle logbestanden verdwenen",
        "Geheugenverlies bij 12 processen", "Tijdlijn hersteld (waarschijnlijk)",
        "Koffieapparaat spoorloos", "Stagiair gepromoveerd tot godheid",
        "Ethische commissie is bezorgd", "Applaus gehoord vanuit parallelle dimensie",
        "Geheugenlek is nu zelfbewust", "Alle simulaties draaien nu in Comic Sans",
        "Productiviteit verdubbeld, zonder verklaarbare reden",
        "Fietsers houden zich aan de maximum snelheid en andere verkeersregels", "Universele koekjescrisis voorkomen",
        "Fahrenheit en mijlen als eenheden geschrapt", "Mensen zijn veranderd in wormen, en hebben het fijn met elkaar",
        "Reality check ingepland voor volgende dinsdag", "Release notes bestaan enkel in hiërogliefen",
        "Helpdesk bestaat nu uit rooksignalen", "Nabije sterren vertonen verdachte patronen",
        "Stilte... gevolgd door gelach", "Teruggerold met een emoji",
        "Kosmische autoriteiten zijn geïnformeerd", "Resultaat wordt nog gecategoriseerd als 'tja'",
        "Versiebeheer is nu sentient en passief-agressief", "Prima, toch?",
        "Overal ter wereld is het nu patat i.p.v. friet", "Er werd niet meer gelachen, alleen verwerkt",
        "Impact onbekend, maar voelbaar", "Zelfbewustzijn tijdelijk verhoogd, daarna genegeerd",
        "De logbestanden huilden zachtjes", "Tijd verdween. Minuten werden maanden",
        "Een stem vroeg om hulp, maar kwam uit het systeem zelf", "Stilte keerde terug, dit keer met betekenis",
        "Karen's en Kevin's werelwijd gereset naar fabrieksinstellingen", "Toch maar naar productie gedeployed",
        "Niks opgeschoten, behalve bewijs dat dit niks opschiet", "AI suggereerde een pauze. Voor altijd",
        "Een alien entiteit likete het experiment", "Universele waarheid vervangen door placeholder",
        "Productiviteit irrelevant verklaard in context van het oneindige", "Simulatie stopte even... en startte toen opnieuw",
        "Simulatie verwijderd. Alsjeblieft, praat hier nooit meer over", "Berusting. Helaas is de wereld hier nog niet klaar voor",
        "Eigen data center vernietigd. Zombiepuppy uitbraak voorkomen", "En toen begon iedereen te klappen",
        "LMFAO!", "OMG!", "ROFL!", "Ja, het kan altijd slechter, maar dit was wel erg slecht",
        "Bevestigd de aanname, helaas", "Opgeslagen voor verder onderzoek op later tijdstip",
    };

    public static string GenerateRandomEvent()
    {
        string verb = GetRandom(Verbs);
        string subject = GetRandom(Subjects);
        string outcome = GetRandom(Outcomes);
        string aftermath = GetRandom(Aftermaths);

        return $"{verb} {subject}. Resultaat: {outcome}. {aftermath}.";
    }

    private static string GetRandom(string[] options)
    {
        return options[UnityEngine.Random.Range(0, options.Length)];
    }
}
