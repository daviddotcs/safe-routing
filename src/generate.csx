// Run with dotnet-script https://github.com/filipw/dotnet-script

#load "lib.csx"

using System;

#nullable enable

Steps.SetCurrentDirectory();

Steps.UpdateReadme();
Steps.UpdateCopyright();
