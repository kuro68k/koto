# Koto - static site generator

Koto (Japanese for "thing", but not a physical thing, more like an idea or concept) is a simple static site generator. I was looking for something to make a set of notes pages with, that didn't have massive external dependencies, and which wasn't designed for blogs.

Koto uses a simple HTML template, with substitution variables. Source files are written in Markdown, with support for extended syntax and code highlighting. For code highlighting, [Highlight](http://www.andre-simon.de/doku/highlight/en/highlight.php) is used. Highlight is the only external dependency, and is optional.

Koto was inspired by simple Japanese homepages, that use only basic HTML and concentrate on delivering information clearly and efficiently.

## Getting started

Create a directory for your site with the following hierarchy:

```
site_root
	⤷ .koto
		⤷ config.txt
		⤷ template.html
	⤷ index.md
```

You can add markdown files to the site_root directory, or create sub-directories for them. Each sub-directory is a logical group of related files, which will be grouped in the auto-generated list of files that can be inserted into your template.

Run Koto by supplying the address of the root directory:

```
Windows: koto d:\path\to\site_root
Linux:   koto /path/to/site_root
```

Koto will create a `.output` directory in site_root. Any .md files will be converted to HTML using the `template.html` file in the `.koto` directory. Any other files and directories that do not start with a period (`.`) will also be copied to the `.output` directory, so you can add things like `.css` and image files as needed.

### Markdown

Markdown files names end with `.md`. Extended syntax is supported via [Markdig](https://github.com/xoofx/markdig), see its page for a list of extensions that are enabled by default.

In addition to Markdown code, all `.md` files must have a variables section at the top. It can be empty, but at a minimum the delimiting `---` must be there.

```
# comment
var1 = value1
var2 = value2
---
Markdown code here
```

### Variables

Variables are defined using a very simple pattern.

```
variable_name = value
```

While it is possible to have spaces in the variable name, it's not recommended.

The value is everything up to the end of the line, and can include spaces. It will be inserted as-is, including things like quotation marks.

TODO: Multi line value support.

There are two types of variable: **global** and **local**. Global variables are set in `config.txt` (see below) or by Koto internally. Local variables are set in the header of each source Markdown file.

Local variables take priority over global ones, so for example you can redefine `page_title` in each source file if you don't want to use the file name as the title.

Koto has some built in global variables:

| Variable | Function |
|----------|----------|
|`markdown`| Inserts the Markdown code converted to HTML.
|`page_title`| Inserts the name of the file, minus the extension.
|||
|`filelist`| Inserts a list of all `.md` files in the site.<br>Use CSS to format.
|||
|`source_file`| Inserts the name of the source file.
|`source_file_path`| Inserts the path of the source file relative to the site root directory.
|`source_file_full_path`| Inserts the full path of the source file.
|||
|`modified_time_utc_iso`<br>`creation_time_utc_iso`<br>`build_time_utc_iso`| Inserts a UTC date stamp in ISO format.<br>e.g. `2022-07-01T20:55:07Z`.
|`modified_time_iso`<br>`creation_time_iso`<br>`build_time_iso`| Inserts a local timezone date stamp in ISO format.<br>e.g. `2022-07-01T20:55:07JST`.
|`modified_time_utc`<br>`creation_time_utc`<br>`build_time_utc`| Inserts a UTC date stamp in a simplified format.<br>e.g. `2022-07-01 20:55:07`.
|`modified_time`<br>`creation_time`<br>`build_time`| Inserts a local timezone date stamp in a simplified format.<br>e.g. `2022-07-01 20:55:07`.

| Time stamp type | Data source |
|-|-|
| Modified | File last modified date from filesystem.
| Created | File created date from filesystem.
| Build | Time that the site build was started.

### template.html

The template file is used to convert Markdown to HTML. It is a normal HTML file that supports simple substitution variables. Variables are inserted by surrounding them by `$$`, e.g. ``$$markdown$$`` will insert the source Markdown code converted to HTML.

### config.txt

The config file uses a simple format, the same as the variables section at the top of Markdown files, except there is no need for the delimiting `---`. Config file variables are global.

```
variable_name = value
```

## Code highlighting with Highlight

Install Highlight wherever you like. Use the following variables to control highlighting. It is recommended that you place them in `config.txt`.

| Variable | Function |
|-|-|
| `highlighter_exe` | Full path to the `highlight.exe` file
| `highlighter_theme` | The name of the theme you wish to use. Can be omitted if you supply your own CSS.
| `highlighter_css_output` | Generates a `.css` file with the CSS code for the selected theme. Value is the path to the generated CSS file, relative to the root of the site filesystem.
| `highlighter_options` | Other command line options that will be passed to Highlight.

TODO: Add support for GNU Source-highlight.

## To-do

- Proper exception handling
- More useful error messages
