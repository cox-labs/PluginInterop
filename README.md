# PluginInterop

[![NuGet version](https://badge.fury.io/nu/PluginInterop.svg)](https://www.nuget.org/packages/PluginInterop)

This repository contains the source code for `PluginInterop`, the Perseus plugin that provides the foundation for plugins developed in e.g. `R` and `Python`, and allows them to be executed from within the Perseus workflow.

The plugin is designed to work with other Perseus interop efforts such as:

 * [PerseusR](https://www.github.com/jdrudolph/PerseusR) for developing plugins in `R`.
 * [perseuspy](https://www.github.com/jdrudolph/perseuspy) for developing plugins in `Python`.

# Running plugins
## Generic scripts 
Generic scripts do not have a dedicated button in the GUI. They are run via the generic activities such as `Matrix -> Processing -> External -> Python => Matrix`.

![Screen shot of the Matrix => Python activity](https://raw.githubusercontent.com/jdrudolph/plugininterop/master/docs/matrix_python.PNG)

 1. Select the script file you want to run. Consult the documentation below on how to create such scripts.
 2. Provide additional parameters to the script. Parameters should be described in the documentation of the specific script you are running.
 3. Specify the location of the `Python` executable. Perseus will try to automatically detect your `Python` installation (see Technical notes below). If not detected automatically, manually navigate to the `python.exe` of your `Python` installation. The button should turn green to indicate success. If the button turns red, mouseover it to obtain more information.
 4. Run the script.

## Hybrid scripts
Hybrid scripts have a dedicated button in the GUI. Refer to point 3. in the section on generic scripts for instructions.

# Developing plugins
`PluginInterop` proviedes two new approaches to plugin development for Perseus. We recommend everyone to get started with the generic approach which provides all functionality in the simplest way. Moving from the generic to the hybrid approach improves the user experience when using the plugin but requires basic knowledge of `C#` development.

## Generic approach: Adapt or write a script
Scripts written in `R` or `Python` can be run from within the Perseus workflow via generic activities, such as `Matrix -> Processing -> External -> R => Matrix`. In order to enable smooth communication between Perseus and `R`/`Python` it is highly recommended to make use of the [PerseusR](https://www.github.com/jdrudolph/PerseusR) and [perseuspy](https://www.github.com/jdrudolph/perseuspy) companion libraries. Visit their websites for instructions on how to adapt or write scripts, including a number of simple examples.

The advantage of the generic approach is the elimination of the need to write any `C#` code. Anyone knowledgable in Python or R can write plugins for Perseus. Due to its generic nature, this approach has some limitations. There will be no dedicated in the GUI of Perseus. Additionally, parameters are passed to the plugin in an unstructured way, using command line parameters. If a dedicated GUI and structured parameter selection is required the hybrid approach described below is the way to go.

## Hybrid approach: Implement a wrapper class
To specify the representation of the plugin in the GUI of Perseus, developers can extend the classes provided by `PluginInterop`. Interested developers can refer to `PluginPHOTON` implemented in [PHOTON](https://www.github.com/jdrudolph/photon) as a complete example of the hybrid approach.

### Providing structured parameters and package dependencies
For `C#` plugins, Perseus automatically generates a GUI for parameter selection. To leverage this functionality create a new class deriving from `PluginInterop.MatrixProcessing` and overwrite the default `.AddParameters(...)` function. For inspiration on how to create the desired parameters see [perseus-plugins](https://www.github.com/jurgencox/perseus-plugins). The parameters selected by the user are written to an `.xml` file and its file path is passed to the script instead of the unstructured additional parameters used in the generic approach. In the same derived class were parameters are specified one can also specify the icon of the plugin, automatically load the script file from resources and even check for required package dependencies.

# Plugin programming bootcamp
Check out the step-by-step tutorial on [PluginTutorial](https://github.com/jdrudolph/plugintutorial) and the associated bootcamp from the MaxQuant summer school.
[![Summer school bootcamp on plugin programming](https://img.youtube.com/vi/fYGx4oplCpI/0.jpg)](https://youtu.be/fYGx4oplCpI?t=5164)

# Technical notes
## Automatic detection of R/Python installations
Perseus will try to automatically detect your `R` or `Python` installation, by looking at the default installation directories and the `%PATH%` variable. Therefore, installations at non-standard locations can be detected, if added to the `%PATH%`. Adding programs to the `%PATH%` is different for different Windows versions, so please refer to online help for your specific Windows installation.

The `Python` installer has an option during the installation to automatically add it to the `%PATH%`.

## Communication between Perseus and external tools
Data is passed between Perseus and the external tool via the file system. Perseus will write all input files to the hard drive and will expect to find the output files at a specified location. The file formats of the input file will be equivalent to the established Perseus export functionality. Matrices will be written as a `.txt` tab-separated text file with annotation rows. Networks will be written in their folder format. Perseus will expect the ouputs of the scripts to have a vaild `.txt` or folder format.

When a script is executed Perseus will provide it with at least 2 command line arguments:
```
<additional arguments> inFile outFile
```
The exact nature of the `<additional arguments>` differs between the generic and the hybrid approach. In the generic approach, the additional arguments contain the unstructured command line parameters specified in the GUI. In the hybrid approach, the path to the `parameters.xml` file will be passed to the script.

The `inFile` contains the matrix in Perseus txt format. The output is expected to be written to `outFile` once the execution of the script finishes. The addtional arguments are specified in the GUI. More advanced scripts can have multiple input and output files.

If an external tool crashes (returns exit code 1) anything written to `stdout` and `stderr` will be reported as error message.

# Example script

Check the [examples](examples/).

# Build from source

Clone the repository and build it using Visual Studio.

# Contributing

Contributions are welcome!

`PluginInterop` is published under the MIT licence.
