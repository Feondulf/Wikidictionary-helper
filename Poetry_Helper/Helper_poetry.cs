namespace Poetry_Helper
{
    internal sealed class NounEntryDraft
    {
        public string Lemma { get; init; } = "";
        public string AlternativeForms { get; init; } = "";
        public string HeadwordLabels { get; init; } = "";
        public string Definitions { get; init; } = "";

        public string EtymologyTemplate { get; init; } = "com";
        public string[] EtymologyTerms { get; init; } = Array.Empty<string>();
        public string[] EtymologyGlosses { get; init; } = Array.Empty<string>();
        public string CustomEtymologyText { get; init; } = "";
        public string AdditionalEtymologyText { get; init; } = "";

        public string Gender { get; init; } = "";
        public string DeclensionClass { get; init; } = "";

        public string QuoteText { get; init; } = "";
        public string QuoteBoldPhrase { get; init; } = "";
        public string QuoteTitle { get; init; } = "";
        public string QuoteYear { get; init; } = "";
        public List<string> TranslationLines { get; init; } = new();
        public string TranslationBoldPhrase { get; init; } = "";

        public string DerivedTerms { get; init; } = "";
        public string RelatedTerms { get; init; } = "";
        public string Descendants { get; init; } = "";

        public string BosworthEntry { get; init; } = "";
        public string BosworthReferenceId { get; init; } = "";
        public string DoeEntry { get; init; } = "";
        public string DoeId { get; init; } = "";
    }

    internal static class OldEnglishWikitextGenerator
    {
        public static string Generate(NounEntryDraft draft)
        {
            string lemma = draft.Lemma.Trim();
            if (lemma.Length == 0)
                throw new ArgumentException("Enter the Old English headword first.");

            var sections = new List<string> { "==Old English==" };

            List<string> alternativeForms = SplitList(draft.AlternativeForms);
            if (alternativeForms.Count > 0)
            {
                sections.Add(
                    "===Alternative forms===" + Environment.NewLine +
                    "* {{alt|ang|" + string.Join("|", alternativeForms.Select(W)) + "}}");
            }

            string etymology = BuildEtymology(draft);
            if (etymology.Length > 0)
                sections.Add("===Etymology===" + Environment.NewLine + etymology);

            sections.Add(
                "===Pronunciation===" + Environment.NewLine +
                "* {{ang-IPA|" + W(lemma) + "|pos=noun}}");

            string headword = "{{ang-noun|head=" + W(lemma);
            string gender = draft.Gender.Trim();
            if (gender.Length > 0) headword += "|" + W(gender);
            headword += "}}";

            List<string> labels = SplitList(draft.HeadwordLabels);
            if (labels.Count > 0)
                headword += " {{tlb|ang|" + string.Join("|", labels.Select(W)) + "}}";

            var nounLines = new List<string> { "===Noun===", headword };
            foreach (string sense in SplitDefinitionLines(draft.Definitions))
                nounLines.Add("# " + sense);

            string quote = BuildQuote(draft);
            if (quote.Length > 0)
                nounLines.Add("#* " + quote);
            sections.Add(string.Join(Environment.NewLine, nounLines));

            string declensionClass = draft.DeclensionClass.Trim();
            if (declensionClass.Length > 0)
            {
                if (gender.Length == 0)
                    throw new ArgumentException("Select a gender before adding noun declension.");
                sections.Add(
                    "====Declension====" + Environment.NewLine +
                    "{{ang-decl-noun-" + W(declensionClass) + "-" + W(gender) + "|" + W(lemma) + "}}");
            }

            AppendLinkedTermSection(sections, "Derived terms", draft.DerivedTerms);
            AppendLinkedTermSection(sections, "Related terms", draft.RelatedTerms);
            AppendDescendantsSection(sections, draft.Descendants);
            AppendReferencesSection(sections, draft);

            return string.Join(Environment.NewLine + Environment.NewLine, sections) + Environment.NewLine;
        }

        private static string BuildEtymology(NounEntryDraft draft)
        {
            string custom = draft.CustomEtymologyText.Trim();
            string result = "";

            if (string.Equals(draft.EtymologyTemplate.Trim(), "Custom text", StringComparison.Ordinal))
            {
                if (custom.Length == 0)
                    throw new ArgumentException("Enter the complete etymology text or choose com/af.");
                result = custom;
            }
            else if (custom.Length > 0)
            {
                result = custom;
            }
            else
            {
                string[] terms = draft.EtymologyTerms
                    .Concat(Enumerable.Repeat("", Math.Max(0, 3 - draft.EtymologyTerms.Length)))
                    .Take(3).Select(value => value.Trim()).ToArray();
                string[] glosses = draft.EtymologyGlosses
                    .Concat(Enumerable.Repeat("", Math.Max(0, 3 - draft.EtymologyGlosses.Length)))
                    .Take(3).Select(value => value.Trim()).ToArray();

                if (glosses.Where((gloss, index) => gloss.Length > 0 && terms[index].Length == 0).Any())
                    throw new ArgumentException("Each etymology gloss needs a term in the same row.");

                if (terms.Any(value => value.Length > 0))
                {
                    string template = draft.EtymologyTemplate.Trim() == "af" ? "af" : "com";
                    var expression = new System.Text.StringBuilder();
                    expression.Append("From {{").Append(template).Append("|ang");

                    int componentNumber = 0;
                    for (int i = 0; i < terms.Length; i++)
                    {
                        if (terms[i].Length == 0) continue;
                        componentNumber++;
                        expression.Append('|').Append(W(terms[i]));
                        if (glosses[i].Length > 0)
                            expression.Append("|t").Append(componentNumber).Append('=').Append(W(glosses[i]));
                    }

                    expression.Append("}}.");
                    result = expression.ToString();
                }
            }

            string additional = draft.AdditionalEtymologyText.Trim();
            if (additional.Length > 0)
                result = result.Length == 0 ? additional : result + Environment.NewLine + additional;

            return result;
        }

        private static string BuildQuote(NounEntryDraft draft)
        {
            string rawQuote = draft.QuoteText.Trim();
            if (rawQuote.Length == 0) return "";

            string title = draft.QuoteTitle.Trim();
            if (title.Length == 0)
                throw new ArgumentException("Enter the source title for the quotation.");

            string quotedText = FormatLineBreaks(rawQuote);
            if (draft.QuoteBoldPhrase.Trim().Length > 0)
                quotedText = Emphasize(quotedText, draft.QuoteBoldPhrase.Trim());
            quotedText = "''" + W(quotedText) + "''";

            List<string> translations = draft.TranslationLines
                .Select(line => line.Trim())
                .ToList();
            while (translations.Count > 0 && translations[^1].Length == 0)
                translations.RemoveAt(translations.Count - 1);

            string translation = FormatLineBreaks(string.Join(Environment.NewLine, translations));
            if (draft.TranslationBoldPhrase.Trim().Length > 0)
                translation = Emphasize(translation, draft.TranslationBoldPhrase.Trim());
            translation = W(translation);

            var result = new System.Text.StringBuilder();
            result.Append("{{quote-book|ang|text=").Append(quotedText);
            if (translation.Length > 0) result.Append("|t=").Append(translation);
            result.Append("|title=").Append(W(title));

            string year = draft.QuoteYear.Trim();
            if (year.Length > 0) result.Append("|year=").Append(W(year));
            result.Append("}}");
            return result.ToString();
        }

        private static void AppendLinkedTermSection(List<string> sections, string heading, string rawTerms)
        {
            List<string> terms = SplitList(rawTerms);
            if (terms.Count == 0) return;

            var lines = new List<string> { "====" + heading + "====" };
            foreach (string term in terms)
                lines.Add("* {{l|ang|" + W(term) + "}}");
            sections.Add(string.Join(Environment.NewLine, lines));
        }

        private static void AppendDescendantsSection(List<string> sections, string rawDescendants)
        {
            var lines = new List<string> { "====Descendants====" };
            foreach (string rawLine in NormalizeLines(rawDescendants).Split('\n'))
            {
                string line = rawLine.Trim();
                if (line.Length == 0) continue;

                int colon = line.IndexOf(':');
                if (colon <= 0 || colon == line.Length - 1)
                    throw new ArgumentException(
                        "Descendants must use 'language: term1, term2' on each non-empty line.");

                string language = line[..colon].Trim();
                List<string> terms = SplitList(line[(colon + 1)..]);
                if (language.Length == 0 || terms.Count == 0)
                    throw new ArgumentException(
                        "Descendants must use 'language: term1, term2' on each non-empty line.");

                lines.Add("* {{desc|" + W(language) + "|" +
                    string.Join("|", terms.Select(W)) + "}}");
            }

            if (lines.Count > 1) sections.Add(string.Join(Environment.NewLine, lines));
        }

        private static void AppendReferencesSection(List<string> sections, NounEntryDraft draft)
        {
            var lines = new List<string> { "===References===" };

            string btEntry = draft.BosworthEntry.Trim();
            string btId = draft.BosworthReferenceId.Trim();
            if (btEntry.Length > 0 || btId.Length > 0)
            {
                var template = new System.Text.StringBuilder("* {{R:ang:BT");
                if (btEntry.Length > 0) template.Append('|').Append(W(btEntry));
                if (btId.Length > 0) template.Append("|ref=").Append(W(btId));
                template.Append("}}");
                lines.Add(template.ToString());
            }

            string doeEntry = draft.DoeEntry.Trim();
            string doeId = draft.DoeId.Trim();
            if (doeEntry.Length > 0 || doeId.Length > 0)
            {
                var template = new System.Text.StringBuilder("* {{R:ang:Dictionary of Old English");
                if (doeEntry.Length > 0)
                    template.Append('|').Append(W(doeEntry));
                if (doeId.Length > 0)
                {
                    if (doeEntry.Length > 0) template.Append('|').Append(W(doeId));
                    else template.Append("|id=").Append(W(doeId));
                }
                template.Append("}}");
                lines.Add(template.ToString());
            }

            if (lines.Count > 1) sections.Add(string.Join(Environment.NewLine, lines));
        }

        private static List<string> SplitList(string value) =>
            NormalizeLines(value).Split(new[] { ',', ';', '\n' },
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Distinct(StringComparer.OrdinalIgnoreCase).ToList();

        private static List<string> SplitDefinitionLines(string value) =>
            NormalizeLines(value).Split('\n',
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

        private static string FormatLineBreaks(string value) =>
            string.Join("<br />", NormalizeLines(value).Split('\n').Select(line => line.Trim()));

        private static string NormalizeLines(string value) =>
            value.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n');

        private static string Emphasize(string text, string phrase)
        {
            int index = text.IndexOf(phrase, StringComparison.Ordinal);
            if (index < 0) return text;
            return text.Substring(0, index) + "'''" + text.Substring(index, phrase.Length) + "'''" +
                text.Substring(index + phrase.Length);
        }

        private static string W(string value) =>
            value.Replace("|", "{{!}}", StringComparison.Ordinal);
    }
}
