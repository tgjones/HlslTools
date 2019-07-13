# HLSL Tools for VS Code

[![Join the chat at https://gitter.im/tgjones/HlslTools](https://badges.gitter.im/tgjones/HlslTools.svg)](https://gitter.im/tgjones/HlslTools)

*This extension is Visual Studio Code. [Go here for the Visual Studio 2017 / 2019 extension](https://marketplace.visualstudio.com/items?itemName=TimGJones.HLSLToolsforVisualStudio).*

HLSL Tools provides enhanced support for editing High Level Shading Language (HLSL) files in VS Code.

### System requirements

To use HLSL Tools you need to be on Windows x64 or macOS. Linux support will come in a future version.

### Why use HLSL Tools?

Here's the feature list:
* [Statement completion](#statement-completion)
* [Signature help](#signature-help)
* [Reference highlighting](#reference-highlighting)
* [Go to symbols](#go-to-symbols)
* [Live errors](#live-errors)
* [Go to definition](#go-to-definition)
* [Quick info](#quick-info)

### Features

#### Statement completion

Just start typing, and HLSL Tools will show you a list of the available symbols (variables, functions, etc.)
at that location. You can manually trigger this with the usual `Ctrl+Space` shortcut.

![Statement completion demo](src/ShaderTools.VSCode/art/statement-completion.gif)

#### Signature help

Signature help (a.k.a. parameter info) shows you all the overloads for a function call, along with information (from MSDN)
about the function, its parameters, and return types. Typing an open parenthesis will trigger statement
completion, as will the standard `Ctrl+Shift+Space` shortcut. Signature help is available for all HLSL functions and methods,
including the older `tex2D`-style texture sampling functions, and the newer `Texture2D.Sample`-style methods.

![Signature help demo](src/ShaderTools.VSCode/art/signature-help.gif)

#### Reference highlighting

Placing the cursor within a symbol (local variable, function name, etc.) will cause all references to
that symbol to be highlighted.

![Reference highlighting demo](src/ShaderTools.VSCode/art/reference-highlighting.gif)

#### Go to Symbol

Use `Ctrl+Shift+O` and start typing the name
of the variable, function, or other symbol that you want to find.

![Navigate To demo](src/ShaderTools.VSCode/art/document-symbols.gif)

#### Live errors

HLSL Tools shows you syntax and semantic errors immediately. No need to wait till compilation!
Errors are shown as squigglies and in the error list.

![Live errors demo](src/ShaderTools.VSCode/art/live-errors.gif)

#### Go to definition

Press F12 to go to a symbol definition. Go to definition works for variables, fields, functions, classes,
macros, and more. You can also "peek definition" with `Alt+F12`.

![Go to definition demo](src/ShaderTools.VSCode/art/go-to-definition.gif)

#### Quick info

Hover over almost anything (variable, field, function call, macro, semantic, type, etc.) to see a Quick Info tooltip.

![Quick info demo](src/ShaderTools.VSCode/art/quick-info.gif)

#### Preprocessor support

HLSL Tools evaluates preprocessor directives as it parses your code.
If you want to make a code block visible to, or hidden from, HLSL Tools, use the `__INTELLISENSE__` macro.

### Custom preprocessor definitions and additional include directories

HLSL Tools will, by default, only use the directory containing the source file to search for `#include` files.

You can customise this, and add additional preprocessor definitions, by creating a file named `shadertoolsconfig.json`:

``` json
{
  "hlsl.preprocessorDefinitions": {
    "MY_PREPROCESSOR_DEFINE_1": "Foo",
    "MY_PREPROCESSOR_DEFINE_2": 1
  },
  "hlsl.additionalIncludeDirectories": [
    "C:\\Code\\MyDirectoryA",
    "C:\\Code\\MyDirectoryB",
    ".",
    "..\\RelativeDirectory"
  ]
}
```

HLSL Tools will look for a file named `shadertoolsconfig.json` in the directory of an opened file,
and in every parent directory. A search for `shadertoolsconfig.json` files will stop when the drive
root is reached or a `shadertoolsconfig.json` file with `"root": true` is found. If multiple config
files are found during this search, they will be combined, with properties in closer files taking
precedence.

Config files are cached for performance reasons. If you make make changes to a config file,
you'll need to close and re-open any source files that use that config file.

### Assocating other file types with HLSL Tools

By default, HLSL Tools only recognises a few file extensions as being HLSL files. You can associate any other file extension like this:

![Associate Files demo](src/ShaderTools.VSCode/art/associate-files.gif)

### Getting involved

You can ask questions in our [Gitter room](https://gitter.im/tgjones/HlslTools).
If you find a bug or want to request a feature, [create an issue here ](https://github.com/tgjones/HlslTools/issues).
You can find me on Twitter at [@\_tim_jones\_](https://twitter.com/_tim_jones_) and I tweet about HLSL Tools using the
[#hlsltools](https://twitter.com/hashtag/hlsltools) hashtag.

Contributions are always welcome. [Please read the contributing guide first.](https://github.com/tgjones/HlslTools/blob/master/CONTRIBUTING.md)

### Maintainer(s)

* [@tgjones](https://github.com/tgjones)