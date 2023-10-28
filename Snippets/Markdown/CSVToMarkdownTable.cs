/* CSV to Markdown Table
Version: v0.1

This snippet converts CSV/TSV delimited texts into Github flavoured Markdown table format. This script converts raw CSV/TSV to markdown format, with commonly helpful options including optional headers and formatting options. Notice such functionality is not easily possible with known tools and too inconvinient as a dedicated program and thus proving superiority of a scripting based solution and in particular Pure Notebook for extremely lightweight accessibility (the only thing that could be better would be the GUI interface itself can be more compact and efficient, e.g. allow collapsing or alternatively allow node-based containerization of functionalities as in Parcel).
*/

using System.Text;
string ConvertCSVToMarkdown(string input, char delimiter, bool containsHeader, bool centered = true, bool trim = false, Func<string, string> linePreprocessor = null, bool print = false, Func<string, bool> lineFilteringRule = null)
{
	StringBuilder builder = new();
	string joinString = centered ? " | " : "|";
	string leftString = centered ? "| " : "|";
	string rightString = centered ? " |" : "|";
	string[][] lines = input
		.Split(new char[]{ '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
		.Where(line => lineFilteringRule == null ? true : lineFilteringRule(line))
		.Select(line => linePreprocessor != null ? linePreprocessor(line) : line)
		.Select(line => line
			.Split(delimiter)
			.Select(part => trim ? part.Trim() : part)
			.ToArray()
		)
		.ToArray();
	
	// Header
	string[] headerLine = lines.First();
	if (containsHeader)
		builder.AppendLine($"{leftString}{string.Join(joinString, headerLine)}{rightString}");
	else
		builder.AppendLine($"{leftString}{string.Join(joinString, headerLine.Select(i => string.Empty))}{rightString}");
	builder.AppendLine($"{leftString}{string.Join(joinString, headerLine.Select(i => "-"))}{rightString}");
	
	// Body
	foreach(string[] items in lines.Skip(containsHeader ? 1 : 0))
		builder.AppendLine($"{leftString}{string.Join(joinString, items)}{rightString}");
	
	// Print
	if (print)
		WriteLine(builder.ToString().Trim());
	
	return builder.ToString().Trim();
}

// Doc
WriteLine(""""
Method:
  string ConvertCSVToMarkdown(string input, char delimiter, bool containsHeader, bool centered = true, bool trim = false, Func<string, string> linePreprocessor = null, bool print = false, Func<string, bool> lineFilteringRule = null)

Template:
ConvertCSVToMarkdown(
	"""
	Shortcut/Key: Function
	* `LMB` on expression parameter: Show value
	* `Ctrl+MMB`: Clear parameteres to default
	""", 
	delimiter: ':',
	containsHeader: true, 
	centered: false, 
	trim: true, 
	lineFilteringRule: null,
	linePreprocessor: line => line.TrimStart('*').Trim()
)

Option: Trim Entries
ConvertCSVToMarkdown("""
	Full name: Erika Tanaka
	Born in: Native Tellus (East of Axis in Kapada)
	Nationality: Kapada
	Ethinity: Axian
	Native Language/Culture: Kapadis
	Father: Eien Tanaka
	Mother: Shinhai Akagi
	""", delimiter: ':', containsHeader: false, centered: false, trim: true, print: false)

Format: KMD Bullet Points
ConvertCSVToMarkdown("""
	* `Spacebar/Double Click`: Create node
	* Enter `//-1`: Gives a single number
	* `MMB`: Quick wheel actions (disable preview etc.)
	""", 
	linePreprocessor: line => line.TrimStart('*').Trim(),
	delimiter: ':', containsHeader: false, centered: false, trim: true, print: false)
"""");