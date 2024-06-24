# What is this?

## The main thing of interest here is the notes, because the Nuget packages in the Phi-3 lab don't work on Linux yet.

I am specifically referring to https://github.com/microsoft/Phi-3CookBook/tree/main/md/07.Labs/Csharp/src/LabsPhi303 .

## TL;DR (`Notes.md` for detail)

Playing with onnxruntime-genai under .NET on Linux, which [does not currently have a Linux-compatible Nuget package](https://github.com/microsoft/onnxruntime-genai/issues/273). 

I'm running Debian Testing without containerization on a system where I don't want to just unpack random things into system directories; some things like `apt` package names or how to make it find that one share lib mentioned in `Notes.md` will probably be different if you're not, or if you're looking at this from the future.
