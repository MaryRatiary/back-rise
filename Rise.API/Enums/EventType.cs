namespace Rise.API.Enums
{
    public enum EventType
    {
        Hackathon,
        Excursion,
        Integration,
        StudentExchange,
        Election,
        Survey,
        Other
    }

    public enum QuestionType
    {
        ShortText,      // Texte court (nom, email, etc.)
        LongText,       // Texte long (paragraphes)
        Email,          // Email uniquement
        Number,         // Nombres
        MultipleChoice, // Choix unique (radio button)
        Checkboxes,     // Plusieurs choix
        Scale,          // Échelle 1-5
        Dropdown,       // Liste déroulante
        Team            // Équipe (avec autocomplétion)
    }
}
