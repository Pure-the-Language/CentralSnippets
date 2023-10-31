# CentralSnippets

A Github page central snippet repo. Accessible through experimental CentralSnippets add-on library. This repo is available on [Github](https://github.com/Pure-the-Language/CentralSnippets).

Access to this repo is implemented inside the [CentralSnippets](https://github.com/Pure-the-Language/Pure/tree/main/StandardLibraries/CentralSnippets) standard library.

## Rules

Anyone is welcome to contribute!

* For non-official snippets, consider putting them in folder structure like `Snippets/Spaces/@<Contributor-Name>`, e.g. [`Spaces/@Charles-Zhang/MyFirstScript.cs`](./Snippets/Spaces/@Charles-Zhang/MyFirstScript.cs) and modify `` variable to point to your site.
* Snippets should be self-contained and generally short and may import other libraries.
* Snippets should NOT depend on other snippets.
* Users of snippets should be aware of the dynamic nature of snippets and that their contents and paths may change at any time.

## Usage

In Pure:

```C#
Import(CentralSnippets)
Pull("Markdown/BuildToC", disableSSL: false);
```

To use another repo/develop locally:

```C#
Import(CentralSnippets)
SnippetsHostSite = @"C:\GithubProjects\CentralSnippets";
Preview("Markdown/BuildToC");
```
