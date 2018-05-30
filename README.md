# Polycubes

Welcome to the repository for an early prototype of [Cubies](https://github.com/LAG1996/PolyCube_Port). Since Cubies now lives in the browser enviornment, this project's pretty much dead. However, there's definitely room for growth if any one is interested.

## What did it do?

This project was meant to understand how to design the user experience in Cubies. How a user can 1) navigate 3D space using an orbital camera, 2) interact with a polycube, and 3) make sense of unfolding in a space where rotation kinda doesn't matter was not something we could have (or had the experience to) just imagine. Thus, we jumped into Unity to test it, since I already had experience with the engine.

The result: the user can interact with a Dali Cross floating in 3D space. The controls are not nearly as intuitive as what's in Cubies, though, and I didn't design a UI to explain them, either. I was planning on fixing that, but then the decision was made to do it with Three.js.

## How was it made?

I made this in the Unity engine, scripted in C#. The components of the cubes are simple meshes I made in Blender.

## How to Navigate the Directory

Go to the file titled `Cubies` to see the bulk of this project's code. If you have some experience with the Unity engine, the project's organization should make sense.

Then go through `Assets > Scripts > CUBESv0_1` to find the code that goes into the main application (every thing else was mostly for testing purposes). The place to start to follow my code would be the file called `TestSystemScript.cs`.

## Can I Contribute?

Go for it! Unfortunately, I did not document my code as well as I should have. I hammered it out between my classes, after all. However, if there's an interest in getting Cubies working on the Unity engine, I will be happy to provide guidance.

## This is a prototype

As of June 2017, Cubies was ported to the browser-based environment, where it is currently being supported by myself and some research students. Check out the [repository](https://github.com/LAG1996/PolyCube_Port) or check out the [application](http://andrewwinslow.com/cubies/) for yourself!
