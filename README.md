![](https://raw.githubusercontent.com/Mikescher/BefunUtils/master/README-FILES/icon_BefunGen.png) BefunGen [![Build status](https://ci.appveyor.com/api/projects/status/2a0bp9dem42uru2j/branch/master?svg=true)](https://ci.appveyor.com/project/Mikescher/befungen/branch/master)
========

BefunGen is  a compiler for TextFunge and a code generator for Befunge.  
Essentially it performs the conversion between TextFunge and Befunge. Most of the generated Befunge programs could be a lot smaller if an actual person would take the time writing them.  
That is the case because the generated Befunge code has a lot of organisation code included. It needs to manage the global variables and also the local ones. The local variables need initialization and in case of a different method call their current state needs to be saved. Also there has to be a call-stack to return to previous methods and re-initialization code when you jump back into methods.

This is important to understand, while I always try to optimize the generated code as much as I can it will always be a good amount bigger (and slower) than actual human-made code. This is also the case because there are neat little "tricks" and design concepts in Befunge that you just can't express in a procedural language.

But thats not really the problem, because the target of BefunGen is **not** generate code that could also be made by hand. The target code size is code so big that it would be totally impractical to write by hand (without spending days and weeks on it).

Running
=======

You can call BefunGen simply via the command line and supply it with an *.tf file to compile. (call with `--help` to see all the options).

~~~
$> BefunGen input.tf output.b93
~~~

But there is also a small IDE [BefunWrite](https://github.com/Mikescher/BefunWrite) which makes it a lot easier to write a program.

Download
========

You can download the binaries from my website [www.mikescher.com](http://www.mikescher.com/programs/view/BefunUtils)

Or you can get the latest [Github release](https://github.com/Mikescher/BefunGen/releases/latest) (In case AppVeyor is down)

Or you can download the latest (nightly) version from the **[AppVeyor build server](https://ci.appveyor.com/project/Mikescher/BefunGen/build/artifacts)**

Set Up
======

*This program was developed under Windows with Visual Studio.*

You don't need other [BefunUtils](https://github.com/Mikescher/BefunUtils) projects to use this.  
Theoretically you can only clone this repository and start using it.  
But it could be useful to get the whole BefunUtils solution like described [here](https://github.com/Mikescher/BefunUtils/blob/master/README.md)  
Especially BefunDebug could be useful for testing.

To see the this in action look at my [BefunWrite](https://github.com/Mikescher/BefunExec) source code or my [BefunDebug](https://github.com/Mikescher/BefunDebug) code.


Contributions
=============

Yes, please
