# PluginInterop

[![NuGet version](https://badge.fury.io/nu/PluginInterop.svg)](https://www.nuget.org/packages/PluginInterop)

A plugin for Perseus enabling the execution of e.g. `R` and `Python`
scripts from within the Perseus workflow.

The plugin in made to work with other Perseus interop efforts such as:

 * [PerseusR](https://www.github.com/jdrudolph/PerseusR) for interacting with `R`.
 * [perseuspy](https://www.github.com/jdrudolph/perseuspy) for interacting with `Python`.

# Installation

There are two options. You can download the latest build of the DLL from nuget.org
(click badge on top). Once downloaded you can unzip the `.nupkg` and find the
DLL inside.

Alternatively you can clone the repository and build it using Visual Studio.

To use, drop the `PluginInterop.dll` in the `Perseus/bin` folder.

# Usage

New matrix processing activities `R => matrix` and `Python => matrix` should be
available from Perseus. You can choose a custom interpreter and script file
which should be run on the selected matrix.

The selected script will receive 3 commandline arguments:
```
parameters.xml inFile outFile
```
The `parameters.xml` contains all specified parameters. The `inFile` contains
the matrix in Perseus txt format. The output is expected to be written
to `outFile` once the execution of the script finishes.

Ideally for your external tool, there are libraries easing the reading/writing
and interpretation of the Perseus specific file formats.

If an external tool crashes (returns exit code 1) anything written to stdout
and stderr will be reported as error message.

# Extending PluginInterop / Providing parameters

In order to provide paramters to scripts via the `parameters.xml` file one
has to create a new class deriving from `PluginInterop.MatrixProcessing`
and overwrite the default `.AddParameters(...)` function. For inspiration
on how to create the desired parameters see 
[perseus-plugins](https://www.github.com/jurgencox/perseus-plugins).

You can create a reference to `PluginInterop` via `nuget`.

# Contributing

Contributions are welcome!

`PluginInterop` is published under the MIT licence.
