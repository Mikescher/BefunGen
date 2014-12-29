![](https://raw.githubusercontent.com/Mikescher/BefunUtils/master/README-FILES/icon_BefunGen.png) BefunGen
========

BefunGen is  a compiler for TextFunge and a code generator for Befunge.  
Essentially it performs the conversion between TextFunge and Befunge. Most of the generated Befunge programs could be a lot smaller if an actual person would take the time writing them.  
That is the case because the generated Befunge code has a lot of organisation code included. It needs to manage the global variables and also the local ones. The local variables need initialization and in case of a different method call their current state needs to be saved. Also there has to be a call-stack to return to previous methods and re-initialization code when you jump back into methods.

This is important to understand, while I always try to optimize the generated code as much as I can it will always be a good amount bigger (and slower) than actual human-made code. This is also the case because there are neat little "tricks" and design concepts in Befunge that you just can't express in a procedural language.

But thats not really the problem, because the target of BefunGen is **not** generate code that could also be made by hand. The target code size is code so big that it would be totally impractical to write by hand (without spending days and weeks on it).

BefunGen itself is not a standalone program, it's a simple library. You are free to use the DLL in your own program (but beware of the license, please give me credits...). If you need help how to use it you can either simply look at the source code (of BefunGen or BefunWrite) or write me a friendly mail.


Download
========

You can download the binaries from my website [www.mikescher.de](http://www.mikescher.de/programs/view/BefunUtils)

Set Up
======

You need the other [BefunUtils](https://github.com/Mikescher/BefunUtils) projects to run this.  
Follow the setup instructions from BefunUtils: [README](https://github.com/Mikescher/BefunUtils/blob/master/README.md)
