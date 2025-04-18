/* Document Parse
Version: v0.0.0

Parse markdown document into hierarchical components instead of as sequence of blocks, which will then make it trivial to re-organize things. We do not make use of existing libraries for maximum compatibility, lightweight-ness, and a more reasonable level of abstraction. This file is self-contained and a single file. (Probably should publish this)
For more advanced implementation, consult Parcel NExT DSL.
*/

public enum MarkdownBlockType
{
    Paragraph,
    Bullets,
    Image,
    Header,
    CodeBlock,
    Footnotes,
    Table,
    BlockQuote,
    HTML
}
public class MarkdownBlock
{

}
public class HierarchicalMarkdownBlock: MarkdownBlock
{

}

string filePath = Arguments[0];

MarkdownBlock[] blocks = ParseBlocks(File.ReadLines(filePath));

MarkdownBlock[] ParseBlocks(IEnumerable<string> lines)
{

}