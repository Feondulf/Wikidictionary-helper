namespace Poetry_Helper
{
    partial class Form1
    {
        private RadioButton nounRadio = null!;
        private RadioButton verbRadio = null!;
        private RadioButton adjRadio = null!;
        private RadioButton advRadio = null!;
        private RadioButton[] partOfSpeechRadios = null!;
        private Panel translationRowsPanel = null!;
        private readonly List<TextBox> translationLineInputs = new();
        private TextBox derivedTermsInput = null!;
        private TextBox relatedTermsInput = null!;
        private TextBox descendantsInput = null!;

        private const int TranslationRowHeight = 27;
        private const int TranslationRowStride = 32;

        private void GlobalInit()
        {
            nounRadio = radioButton1;
            verbRadio = radioButton2;
            adjRadio = radioButton3;
            advRadio = radioButton4;
            partOfSpeechRadios = new[] { nounRadio, verbRadio, adjRadio, advRadio };

            WordBox.PlaceholderText = "Lemma / headword";
            textBox1.PlaceholderText = "Alternative forms, comma-separated";
            textBox2.PlaceholderText = "First etymology term";
            textBox3.PlaceholderText = "Second etymology term";
            textBox4.PlaceholderText = "First term gloss (t1)";
            textBox5.PlaceholderText = "Second term gloss (t2)";
            textBox6.PlaceholderText = "Third etymology term";
            textBox7.PlaceholderText = "Third term gloss (t3)";
            textBox8.PlaceholderText = "Definitions; separate senses with ;";
            bosTitle.PlaceholderText = "Bosworth entry";
            bosNum.PlaceholderText = "Bosworth reference ID";
            OEDTitle.PlaceholderText = "Dictionary of Old English entry";
            OEDnum.PlaceholderText = "DOE reference ID";
            textInput.Multiline = true;
            textInput.ScrollBars = RichTextBoxScrollBars.Vertical;
            tInput.Visible = false;
            tInput.Enabled = false;
            titleInput.PlaceholderText = "Source title";
            yearInput.PlaceholderText = "Year";

            InitializeTranslationLineInputs();
            InitializeOptionalTermInputs();

            new ToolTip().SetToolTip(textInput, "Old English quotation. Line breaks are preserved.");
            new ToolTip().SetToolTip(translationRowsPanel, "Use one textbox per translation line. The + button adds a new line.");
            new ToolTip().SetToolTip(descendantsInput,
                "One language group per line, in the format: enm: andweard, aundward, anwerd");

            button1.Click += (_, _) => GenerateWikitext();
            button2.Click += (_, _) => ClearForm();
            AddLineButtpn.Click += (_, _) => AddQuoteLine();
            checkBox1.CheckedChanged += (_, _) => textBox1.Enabled = checkBox1.Checked;
            quoteBox.CheckedChanged += (_, _) => SetQuoteInputsEnabled(quoteBox.Checked);
            checkBox2.CheckedChanged += (_, _) =>
            {
                bosTitle.Enabled = checkBox2.Checked;
                bosNum.Enabled = checkBox2.Checked;
            };
            checkBox3.CheckedChanged += (_, _) =>
            {
                OEDTitle.Enabled = checkBox3.Checked;
                OEDnum.Enabled = checkBox3.Checked;
            };

            textBox1.Enabled = checkBox1.Checked;
            SetQuoteInputsEnabled(quoteBox.Checked);
            bosTitle.Enabled = bosNum.Enabled = checkBox2.Checked;
            OEDTitle.Enabled = OEDnum.Enabled = checkBox3.Checked;
            AutoScroll = true;
            ClientSize = new Size(ClientSize.Width, 820);
            UpdateAutoScrollExtent();
        }

        private void InitializeTranslationLineInputs()
        {
            translationRowsPanel = new Panel
            {
                Location = tInput.Location,
                Size = new Size(tInput.Width, TranslationRowHeight),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            Controls.Add(translationRowsPanel);
            AddTranslationRow(shiftFollowingControls: false, focus: false);
        }

        private void InitializeOptionalTermInputs()
        {
            int top = Math.Max(bosNum.Bottom, OEDnum.Bottom) + 14;
            derivedTermsInput = AddOptionalListInput(
                "Derived terms", "derivedTermsInput",
                "Old English terms, comma-separated or one per line", top, 42);

            top += 52;
            relatedTermsInput = AddOptionalListInput(
                "Related terms", "relatedTermsInput",
                "Related Old English terms, comma-separated or one per line", top, 42);

            top += 52;
            descendantsInput = AddOptionalListInput(
                "Descendants", "descendantsInput",
                "lang: term1, term2 (one language group per line)", top, 52);

            button2.Top = top + descendantsInput.Height + 14;
            AutoScroll = true;
            ClientSize = new Size(ClientSize.Width, Math.Max(820, button2.Bottom + 18));
        }

        private TextBox AddOptionalListInput(
            string labelText, string name, string placeholder, int top, int height)
        {
            var label = new Label
            {
                AutoSize = true,
                Text = labelText,
                Location = new Point(12, top + 8)
            };
            var input = new TextBox
            {
                Name = name,
                PlaceholderText = placeholder,
                Location = new Point(130, top),
                Size = new Size(ClientSize.Width - 142, height),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            Controls.Add(label);
            Controls.Add(input);
            return input;
        }

        private void SetQuoteInputsEnabled(bool enabled)
        {
            textInput.Enabled = enabled;
            foreach (TextBox line in translationLineInputs) line.Enabled = enabled;
            titleInput.Enabled = enabled;
            yearInput.Enabled = enabled;
        }

        private void NamesInit()
        {
            nounRadio.Name = "Noun"; nounRadio.Text = "Noun";
            verbRadio.Name = "Verb"; verbRadio.Text = "Verb";
            adjRadio.Name = "Adjective"; adjRadio.Text = "Adjective";
            advRadio.Name = "Adverb"; advRadio.Text = "Adverb";
            foreach (RadioButton radio in partOfSpeechRadios) radio.AutoSize = true;
            nounRadio.Checked = true;
            comboBox1.Text = "com";
            comboBox2.SelectedItem = "m";

            // Usage labels are optional; do not silently insert poetic or hapax.
            comboBox3.Text = "";
            comboBox4.Text = "";
            CentralizeNames();
        }

        private void CentralizeNames()
        {
            if (partOfSpeechRadios is null || partOfSpeechRadios.Length == 0) return;
            const int spacing = 16;
            int width = partOfSpeechRadios.Sum(r => r.Width) + spacing * (partOfSpeechRadios.Length - 1);
            int x = Math.Max(0, (ClientSize.Width - width) / 2);
            foreach (RadioButton radio in partOfSpeechRadios) { radio.Left = x; x += radio.Width + spacing; }
        }

        private void AddQuoteLine()
        {
            if (!quoteBox.Checked) quoteBox.Checked = true;
            AddTranslationRow(shiftFollowingControls: true, focus: true);
        }

        private void AddTranslationRow(bool shiftFollowingControls, bool focus)
        {
            int previousHeight = translationRowsPanel.Height;
            int rowIndex = translationLineInputs.Count;
            var line = new TextBox
            {
                Name = "translationLine" + (rowIndex + 1),
                PlaceholderText = "Translation line " + (rowIndex + 1),
                Location = new Point(0, rowIndex * TranslationRowStride),
                Size = new Size(Math.Max(80, translationRowsPanel.ClientSize.Width - 6), TranslationRowHeight),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Enabled = quoteBox.Checked
            };

            translationRowsPanel.Controls.Add(line);
            translationLineInputs.Add(line);
            translationRowsPanel.Height = Math.Max(
                TranslationRowHeight,
                translationLineInputs.Count * TranslationRowStride - (TranslationRowStride - TranslationRowHeight));

            int delta = translationRowsPanel.Height - previousHeight;
            if (shiftFollowingControls && delta != 0)
                ShiftControlsBelowTranslationRows(previousHeight, delta);

            if (focus)
            {
                line.Focus();
                line.SelectionStart = line.TextLength;
            }
            UpdateAutoScrollExtent();
        }

        private void ShiftControlsBelowTranslationRows(int previousPanelHeight, int delta)
        {
            int cutoff = translationRowsPanel.Top + previousPanelHeight;
            foreach (Control control in Controls.Cast<Control>().ToArray())
            {
                if (control == translationRowsPanel || control == tInput) continue;
                if (control.Top >= cutoff) control.Top += delta;
            }

            PerformLayout();
            UpdateAutoScrollExtent();
        }

        private void ResetTranslationRows()
        {
            while (translationLineInputs.Count > 1)
            {
                int oldHeight = translationRowsPanel.Height;
                TextBox last = translationLineInputs[^1];
                translationLineInputs.RemoveAt(translationLineInputs.Count - 1);
                translationRowsPanel.Controls.Remove(last);
                last.Dispose();

                translationRowsPanel.Height = Math.Max(
                    TranslationRowHeight,
                    translationLineInputs.Count * TranslationRowStride - (TranslationRowStride - TranslationRowHeight));
                int delta = translationRowsPanel.Height - oldHeight;
                if (delta != 0) ShiftControlsBelowTranslationRows(oldHeight, delta);
            }

            foreach (TextBox line in translationLineInputs) line.Clear();
            UpdateAutoScrollExtent();
        }

        private void UpdateAutoScrollExtent()
        {
            if (!AutoScroll) return;
            int bottom = Controls.Cast<Control>()
                .Where(control => control.Visible)
                .Select(control => control.Bottom)
                .DefaultIfEmpty(ClientSize.Height)
                .Max();
            AutoScrollMinSize = new Size(0, Math.Max(ClientSize.Height, bottom + 12));
        }

        private void ClearForm()
        {
            ResetTranslationRows();

            foreach (TextBox box in new[]
            {
                textBox1, textBox2, textBox3, textBox4, textBox5, textBox6, textBox7,
                textBox8, WordBox, bosTitle, OEDTitle, bosNum, OEDnum, titleInput, yearInput,
                derivedTermsInput, relatedTermsInput, descendantsInput
            })
            {
                box.Clear();
            }

            textInput.Clear();
            tInput.Clear();
            checkBox1.Checked = quoteBox.Checked = checkBox2.Checked = checkBox3.Checked = false;
            nounRadio.Checked = true;
            comboBox1.Text = "com";
            comboBox2.SelectedItem = "m";
            comboBox3.Text = comboBox4.Text = comboBox5.Text = "";
            WordBox.Focus();
        }

        private void GenerateWikitext()
        {
            string lemma = WordBox.Text.Trim();
            if (lemma.Length == 0)
            {
                MessageBox.Show("Enter the lemma/headword first.", "Missing headword",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                WordBox.Focus();
                return;
            }

            string pos = nounRadio.Checked ? "noun" : verbRadio.Checked ? "verb" :
                adjRadio.Checked ? "adj" : "noun";
            string posTemplate = nounRadio.Checked ? "ang-noun" : verbRadio.Checked ? "ang-verb" :
                adjRadio.Checked ? "ang-adj" : "ang-adv";
            string section = nounRadio.Checked ? "Noun" : verbRadio.Checked ? "Verb" :
                adjRadio.Checked ? "Adjective" : "Adverb";

            var output = new System.Text.StringBuilder();
            output.AppendLine("==Old English==").AppendLine();

            if (checkBox1.Checked)
            {
                List<string> alternatives = SplitValues(textBox1.Text);
                if (alternatives.Count > 0)
                {
                    EnsureBlankLine(output);
                    output.AppendLine("===Alternative forms===");
                    output.Append("* {{alt|ang|")
                        .Append(string.Join("|", alternatives.Select(W)))
                        .AppendLine("}}");
                }
            }

            string etymology = comboBox1.Text.Trim();
            string[] terms = { textBox2.Text.Trim(), textBox3.Text.Trim(), textBox6.Text.Trim() };
            string[] glosses = { textBox4.Text.Trim(), textBox5.Text.Trim(), textBox7.Text.Trim() };

            if (terms.Any(value => value.Length > 0) || glosses.Any(value => value.Length > 0))
            {
                EnsureBlankLine(output);
                output.AppendLine("===Etymology===");
                string template = etymology == "af" ? "af" : "com";
                output.Append("From {{").Append(template).Append("|1=ang");

                // Explicit term indexes keep each t1/t2/t3 gloss attached to its intended component.
                for (int i = 0; i < terms.Length; i++)
                {
                    if (terms[i].Length > 0)
                        output.Append('|').Append(i + 2).Append('=').Append(W(terms[i]));
                }
                for (int i = 0; i < glosses.Length; i++)
                {
                    if (glosses[i].Length > 0)
                        output.Append("|t").Append(i + 1).Append('=').Append(W(glosses[i]));
                }
                output.AppendLine("}}.");
            }

            EnsureBlankLine(output);
            output.AppendLine("===Pronunciation===");
            output.Append("* {{ang-IPA|").Append(W(lemma)).Append("|pos=").Append(pos).AppendLine("}}");

            EnsureBlankLine(output);
            output.Append("===").Append(section).AppendLine("===");

            if (nounRadio.Checked)
            {
                output.Append("{{ang-noun|head=").Append(W(lemma));
                if (comboBox2.Text.Trim().Length > 0)
                    output.Append('|').Append(W(comboBox2.Text.Trim()));
                output.Append("}}");
            }
            else
            {
                output.Append("{{").Append(posTemplate).Append('|').Append(W(lemma)).Append("}}");
            }

            string[] tags = { comboBox3.Text.Trim(), comboBox4.Text.Trim() };
            tags = tags.Where(value => value.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
            if (tags.Length > 0)
                output.Append(" {{tlb|ang|").Append(string.Join("|", tags.Select(W))).Append("}}");
            output.AppendLine().AppendLine();

            foreach (string sense in SplitSenses(textBox8.Text))
                output.Append("# ").AppendLine(sense);

            if (quoteBox.Checked && textInput.Text.Trim().Length > 0)
            {
                string title = titleInput.Text.Trim();
                if (title.Length == 0)
                {
                    MessageBox.Show("Enter the source title for the quotation.", "Missing quotation title",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    titleInput.Focus();
                    return;
                }

                string quotedText = FormatQuoteText(textInput.Text, lemma);
                string translations = FormatMultiline(string.Join(
                    Environment.NewLine, translationLineInputs.Select(line => line.Text)));
                output.Append("#* {{quote-book|ang|title=").Append(W(title));
                if (yearInput.Text.Trim().Length > 0)
                    output.Append("|year=").Append(W(yearInput.Text.Trim()));
                output.Append("|text=").Append(quotedText);
                if (translations.Length > 0) output.Append("|t=").Append(translations);
                output.AppendLine("}}");
            }

            if (nounRadio.Checked && comboBox5.Text.Trim().Length > 0)
            {
                EnsureBlankLine(output);
                output.AppendLine("====Declension====");
                output.Append("{{ang-decl-noun-")
                    .Append(W(comboBox5.Text.Trim())).Append('-')
                    .Append(W(comboBox2.Text.Trim())).Append('|')
                    .Append(W(lemma)).AppendLine("}}");
            }
            else if (adjRadio.Checked)
            {
                EnsureBlankLine(output);
                output.AppendLine("====Declension====");
                output.Append("{{ang-adecl|").Append(W(lemma)).AppendLine("}}");
            }

            AppendLinkedTermSection(output, "Derived terms", derivedTermsInput.Text);
            AppendLinkedTermSection(output, "Related terms", relatedTermsInput.Text);
            AppendDescendantSection(output, descendantsInput.Text);

            if (checkBox2.Checked || checkBox3.Checked)
            {
                EnsureBlankLine(output);
                output.AppendLine("===References===");

                if (checkBox2.Checked)
                {
                    string btEntry = bosTitle.Text.Trim();
                    string btRef = bosNum.Text.Trim();
                    output.Append("* {{R:ang:BT");
                    if (btEntry.Length > 0) output.Append('|').Append(W(btEntry));
                    if (btRef.Length > 0) output.Append("|ref=").Append(W(btRef));
                    output.AppendLine("}}");
                }

                if (checkBox3.Checked)
                {
                    string doeEntry = OEDTitle.Text.Trim();
                    string doeId = OEDnum.Text.Trim();
                    output.Append("* {{R:ang:Dictionary of Old English");
                    if (doeEntry.Length > 0) output.Append("|entry=").Append(W(doeEntry));
                    if (doeId.Length > 0) output.Append("|id=").Append(W(doeId));
                    output.AppendLine("}}");
                }
            }

            ShowGeneratedWikitext(output.ToString(), lemma);
        }

        private static void AppendLinkedTermSection(
            System.Text.StringBuilder output, string heading, string rawTerms)
        {
            List<string> terms = SplitValues(rawTerms);
            if (terms.Count == 0) return;

            EnsureBlankLine(output);
            output.Append("====").Append(heading).AppendLine("====");
            foreach (string term in terms)
                output.Append("* {{l|ang|").Append(W(term)).AppendLine("}}");
        }

        private static void AppendDescendantSection(
            System.Text.StringBuilder output, string rawDescendants)
        {
            var groups = new List<(string Language, List<string> Terms)>();
            string normalized = rawDescendants.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n');

            foreach (string rawLine in normalized.Split('\n'))
            {
                string line = rawLine.Trim();
                if (line.Length == 0) continue;

                int colon = line.IndexOf(':');
                if (colon <= 0 || colon == line.Length - 1) continue;

                string language = line[..colon].Trim();
                List<string> terms = SplitValues(line[(colon + 1)..]);
                if (language.Length > 0 && terms.Count > 0)
                    groups.Add((language, terms));
            }

            if (groups.Count == 0) return;
            EnsureBlankLine(output);
            output.AppendLine("====Descendants====");
            foreach ((string language, List<string> terms) in groups)
                output.Append("* {{desc|").Append(W(language)).Append('|')
                    .Append(string.Join("|", terms.Select(W))).AppendLine("}}");
        }

        private void ShowGeneratedWikitext(string wikitext, string lemma)
        {
            using Form preview = new()
            {
                Text = "Generated Wiktionary wikitext",
                StartPosition = FormStartPosition.CenterParent,
                Width = 900,
                Height = 700,
                MinimizeBox = false
            };
            var text = new RichTextBox
            {
                Dock = DockStyle.Fill,
                Font = new System.Drawing.Font("Consolas", 10),
                WordWrap = false,
                DetectUrls = false,
                ReadOnly = false,
                Text = wikitext
            };
            var buttons = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 44,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(6)
            };
            var close = new Button { Text = "Close", AutoSize = true, DialogResult = DialogResult.Cancel };
            var save = new Button { Text = "Save…", AutoSize = true };
            var copy = new Button { Text = "Copy", AutoSize = true };
            copy.Click += (_, _) =>
            {
                Clipboard.SetText(text.Text);
                MessageBox.Show(preview, "Wikitext copied to the clipboard.", "Copied", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            save.Click += (_, _) =>
            {
                using var dialog = new SaveFileDialog
                {
                    Title = "Save Wiktionary wikitext",
                    Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                    FileName = lemma + ".txt",
                    DefaultExt = "txt",
                    AddExtension = true
                };
                if (dialog.ShowDialog(preview) == DialogResult.OK)
                    System.IO.File.WriteAllText(dialog.FileName, text.Text, new System.Text.UTF8Encoding(true));
            };
            buttons.Controls.Add(close);
            buttons.Controls.Add(save);
            buttons.Controls.Add(copy);
            preview.Controls.Add(text);
            preview.Controls.Add(buttons);
            preview.CancelButton = close;
            preview.ShowDialog(this);
        }

        private static List<string> SplitValues(string value) =>
            value.Split(new[] { ',', ';', '\n', '\r' },
                    StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Distinct(StringComparer.OrdinalIgnoreCase).ToList();

        private static List<string> SplitSenses(string value) =>
            value.Split(new[] { ';', '\n', '\r' },
                    StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(sense => sense.Length > 0).ToList();

        private static string FormatQuoteText(string value, string lemma)
        {
            string normalized = value.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n');
            string[] lines = normalized.Split('\n');
            int count = lines.Length;
            while (count > 0 && lines[count - 1].Trim().Length == 0) count--;
            if (count == 0) return "";

            for (int i = 0; i < count; i++)
            {
                lines[i] = lines[i].Trim();
                if (lines[i].Length > 0) lines[i] = Emphasize(lines[i], lemma);
                lines[i] = W(lines[i]);
            }
            return string.Join("<br />", lines.Take(count));
        }

        private static string FormatMultiline(string value)
        {
            string normalized = value.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n');
            string[] lines = normalized.Split('\n');
            int count = lines.Length;
            while (count > 0 && lines[count - 1].Trim().Length == 0) count--;
            for (int i = 0; i < count; i++) lines[i] = W(lines[i].Trim());
            return string.Join("<br />", lines.Take(count));
        }

        private static void EnsureBlankLine(System.Text.StringBuilder output)
        {
            string current = output.ToString();
            if (current.Length == 0) return;

            string newline = Environment.NewLine;
            if (current.EndsWith(newline + newline, StringComparison.Ordinal)) return;
            if (!current.EndsWith(newline, StringComparison.Ordinal)) output.AppendLine();
            output.AppendLine();
        }

        private static string Emphasize(string text, string phrase)
        {
            int index = text.IndexOf(phrase, StringComparison.Ordinal);
            if (index < 0) return text;
            return text.Substring(0, index) + "'''"
                + text.Substring(index, phrase.Length) + "'''"
                + text.Substring(index + phrase.Length);
        }

        private static string W(string value) => value.Replace("|", "{{!}}", StringComparison.Ordinal);
    }
}
