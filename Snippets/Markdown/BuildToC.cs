/* BuildToC
Build table of contents for markdown, optionally as links to ids.
Version: v0.1
*/

Import(Markdig)

string Build(string markdown)
{
    var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
    var document = Markdown.ToHtml("This is a text with some *emphasis*", pipeline);
    return document;
}