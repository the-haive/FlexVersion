# Todo

## Refactor functions, and variables

* Only previous outputs are to be in angle-brackets, <>
* Function-list:

         string $If(bool expression, string then, string else)
         string $IfNot(bool expression, string then, string else)

         string $IfBlank(string text, string then, string else)
         string $IfNotBlank(string text, string then, string else)

         bool $Equal(dynamic a, dynamic b)
         bool $NotEqual(dynamic a, dynamic b)

         number $Index(original, toFind)
         number $IndexLast(original, toFind)

         number $Length(text)

         number $Calc(string numericExpression)

         bool $RegexMatch(input, pattern)
         string $RegexReplace(original, regex, replacementText)

         string $PadLeft(string text, int totalLength, char padCharacter)
         string $PadRight(string text, int totalLength, char padCharacter)

         string $Substring(string text, int startIndex, int stopIndex)

         string $Trim(string input)
         string $TrimRight(string input)
         string $TrimLeft(string input)

         string $DateTime(string input, string format)
         string $DateTimeNow(string format)

         string $Env(string identifier)

         string $Arg(string identifier)

         string $Match(string identifier)

         string $VersionSource(string identifier)

         string $GitInfo(string identifier)

         string $Head(string identifier)

## Allow third party function-dlls to be loaded

* Probably use reflection to find matching methods, and check the data-types for what each param expects. And use reflection to pick up documentation and show in help-list or when the method fails?

## Set up Cake build

* Load itself from NuGet 
* This will work after first package has been published. Will always use an older version to version itself, but - that is ok...

## Setup app-veyor (or similar)

* What is needed to build dotnet core stuff? Can I build executables for any target?

## Push library to NuGet

* During build, create and push library package to NuGet.org
* During build, create and push Cake Addin library to NuGet.org
  
## Push executable to chocolatey

* During build, create windows version and publish to chocolatey.
* During build, TODO: How to create linux/mac version and publish where?

## Marketing

* Create website for the project (separate nice page, squarespace?)
* Write article on Medium
* Create video for YouTube
* Create tag on StackOverflow
