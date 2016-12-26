<h1> Polycubes - Changelog/Chronicle </h1>

<h3>12/26/2016</h3>
<p>First, a note: this should've been started ages ago. Now I will hastily list out everything in the project up until now.

<br>I will try to organize everything in these categories: SCENES, SCRIPTS, MISC.

<br>NOTE: IF YOU WANT TO JUST MESS WITH THE SCRIPT AND LOOK AT THE PRETTY ANIMATIONS OF CUBES UNFOLDING SCROLL DOWN TO THE END OF THE SCRIPTS SECTION TO SEE THE LIST OF VALID KEY WORDS
</p>

<h2>SCENES:</h2>
<h4>Two scenes.</h4> 
<p>	
	"_scene" simply generates a few cubes.
	<br>"hardcoded_unfolding" shows a couple of cubes getting partially unfolded. There isn't much spectacle here. The unfolding itself is data-driven, but the commands for the unfolding are baked into the script and can only be changed in an editor.

	<br>There is no GUI for making unfolding commands, so it is necessary to directly manipulate the script. I'll explain how in the SCRIPT section.
	</p>

-The System Game Object
	Acts as the manager for all of the polycube-based shenanigans. You can see that this game object holds all the scripts and objects necessary to get our project working.
	Upon running the project, you'll also notice that the cubes generated are children of the System object.

-The Assembled Cube Prefab Object
	Pretty self-explanatory. It's just a cube I slapped together on Blender out of cylinders and rectangular prisms. Each hinge and face is its own game object (probably eats up quite a bit of disk space), making it quite easy to manipulate.

SCRIPTS:
It's all in C#!

-Cube Face
	Pretty self-explanatory. This is just a class that holds all the methods and data needed to manipulate and define each face of a cube (which is defined as a MonoCube object in this project).

-MonoCube
	This is a chunky class that holds the actual cube model, data on all its faces (and hinges), and methods for manipulating the cube. Think of it as the middle-man for playing with the faces.

-The Main Scripts
	Each scene has a main script. "_scene" is rather uninteresting, honestly, so I'll just talk about "HardUnfoldCube.cs" (which should just be called "System.cs" or something lame like that).
	
	It's quite simple to see what this script does. Currently, it generates two MonoCubes (generating is the same as instantiating, which is the same as rendering). Then, the system partially unfolds them according to the parameters for "StartRotateFaceByHinge(string, string)".

	That hideously verbose method, along with ContinueRotate(), makes up the core of this project. The first parameter chooses the face of the cube we'd like to manipulate, and then the second chooses which "hinge" to rotate around. It's weird to think about, and if it is confusing, imagine looking at a door whose hinges are on its right side. You could say then that, if the door were to represent the "BackFace" of a cube, then the second parameter would be "Left".

	I'd say to anybody looking at this who is confused to play around with the script a little by just changing the parameters in "StartRotateFaceByHinge(string, string)" and seeing what works and what does not.

	Here's the list of valid keywords:
	
	-Faces: "FrontFace", "BackFace", "TopFace", "BottomFace", "RightFace", "LeftFace"

	-Hinges: "Top", "Bottom", "Left", "Right"

	If it doesn't work, then you more than likely typed in an invalid keyword somewhere (and if you're certain you have, then please email me (unless it's me who found it (fix it yourself then -me))).

MISC:
	Not much to talk about here. There are a few blender objects that are obviously being used to create the prefab for the cube.


And that's it. This is the state of the project thus far, and hopefully I remember to update this file as I go along.