# PluginInterop

[![NuGet version](https://badge.fury.io/nu/PluginInterop.svg)](https://www.nuget.org/packages/PluginInterop)

A plugin for Perseus enabling the execution of e.g. `R` and `Python`
scripts from within the Perseus workflow.

The plugin in made to work with other Perseus interop efforts such as:

 * [PerseusR](https://www.github.com/jdrudolph/PerseusR) for interacting with `R`.
 * [perseuspy](https://www.github.com/jdrudolph/perseuspy) for interacting with `Python`.

# Build from source

Clone the repository and build it using Visual Studio.

# Usage

New processing activities `External => R => matrix` and `External => Python => matrix` etc. should be
available from Perseus. You can choose a custom interpreter and script file as well as provide additional arguments
which should be run on the selected data. Perseus will try to automatically detect
your `R` or `Python` installation, by looking at the default installation
directories and the `%PATH%` variable. Therefore, installations at non-standard locations
can be detected, if added to the `%PATH%` ([lmgtfy](http://lmgtfy.com/?q=windows+add+program+to+path)).

The selected script will receive at least 2 commandline arguments:
```
<additional arguments> inFile outFile
```
The additional arguments contain unstructured command line parameters
that can be parsed by the script. If a GUI was created for the parameters,
a `parameters.xml` containing all specified parameters will be passed
instead. The `inFile` contains
the matrix in Perseus txt format. The output is expected to be written
to `outFile` once the execution of the script finishes. The addtional arguments
are specified in the GUI.

Ideally for your external tool, there are libraries easing the reading/writing
and interpretation of the Perseus specific file formats.

If an external tool crashes (returns exit code 1) anything written to stdout
and stderr will be reported as error message.

# Example script

Check the [examples](examples/).

# Providing structured parameters and package dependencies

Perseus can automatically generate a GUI for parameter selection which are
passed to the script via a `parameters.xml` passed in instead of the
unstructured additional parameters.
In order for scripts to leverage this functionality one
has to create a new class deriving from `PluginInterop.MatrixProcessing`
and overwrite the default `.AddParameters(...)` function. For inspiration
on how to create the desired parameters see 
[perseus-plugins](https://www.github.com/jurgencox/perseus-plugins).

In the same derived class were parameters are specified one can
also specify required e.g. Python packages that will be checked
for existance. This conveniece feature provides more helpful
feedback/error message to the user if things go wrong.

You can create a reference to `PluginInterop` via `nuget`.

# Contributing

Contributions are welcome!

`PluginInterop` is published under the MIT licence.
