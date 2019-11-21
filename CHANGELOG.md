# Changelog

## 1.1.300

**2019-11-21**

Note: v1.1.300 supports VS2019, VS2017, and VSCode. VS2015 is no longer supported.

- [x] Add support for double-bracket annotation syntax [#174](https://github.com/tgjones/HlslTools/issues/174)

## 1.1.185

**2017-02-14**

Note: v1.1.185 supports both VS2015 and VS2017. VS2013 is no longer supported.

- [x] Add support for matrix types in StructuredBuffer template declarations ([@mrvux](https://github.com/mrvux)) (#45)
- [x] Add support for min16float, min10float, min16int, min12int, min16uint types ([@UpwindSpring01](https://github.com/UpwindSpring01)) (#48)
- [x] Implement config files that can add preprocessor definitions and additional include directories (#8)
- [x] Implement tri-state (move to new line, keep on same line with leading space, don't move) open-brace formatting options (#51)
- [x] Fix class field binding ([@OndrejPetrzilka](https://github.com/OndrejPetrzilka)) (#55)
- [x] Add support for globallycoherent keyword ([@OndrejPetrzilka](https://github.com/OndrejPetrzilka)) (#54)
- [x] Add support for struct methods ([@OndrejPetrzilka](https://github.com/OndrejPetrzilka)) (#57)
- [x] Make "Go to definition" work when overload resolution fails (#71)
- [x] Add default argument values to IntelliSense display and navigation bar (#70)

## 1.0.119

**2016-11-25**

- [x] Fix namespace member parsing (#38)
- [x] Implement integer suffixes, octal prefix, floating point specials (#43)
- [x] Allow lineadj as parameter modifier (#39)
- [x] Implement typedef support (#42)
- [x] Implement snorm and unorm modifiers (#35)
- [x] Fix error when casting array with const variable size (#41)
- [x] Implement struct inheritance (#40)
- [x] Remove unwanted completions when typing keywords

## 1.0.94

**2016-04-25**

- [x] Semantic highlighting
- [x] Live semantic errors
- [x] Go to definition (full support)
- [x] Quick info (full support)
- [x] Symbol completion
- [x] Signature help (aka "parameter info")
- [x] Reference highlighting

## 0.9.42

**2016-03-10**

- [x] Custom file extensions

## 0.9.8

**2015-10-02**

- [x] Syntax highlighting
- [x] Navigation bar
- [x] Navigate to (Ctrl+,)
- [x] Live syntax errors
- [x] Automatic formatting
- [x] Outlining
- [x] Brace matching
- [x] Brace completion
- [x] Go to definition (limited to preprocessor directives)
- [x] Quick info (limited to preprocessor directives and syntactic constructs)
- [x] Syntax visualizer