--[[
#########
# Author: Luis Angel Garcia (email: luis.a.garcia01996@gmail.com)
# Date Last Edited: 11/28/2016
# Description: First create a unit square from a sceneMesh mesh. Then create a cube from square meshes.
#########
]]--

scene = Scene(Scene.SCENE_3D)
squareMesh = SceneMesh.SceneMeshWithType(Mesh.QUAD_MESH)

--[[
##########Polycode's default cube#############
]]--

sceneEntity = SceneEntityInstance(scene, "Resources/scene.entity")
sceneEntity:setColor(1, 0, 0, 1)
scene:addChild(sceneEntity)

--[[
##########Cube from mesh#################
squareMesh:getMesh():addVertex(.5, -.5, 0)
squareMesh:getMesh():addVertex(.5, .5, 0)
squareMesh:getMesh():addVertex(-.5, .5, 0)
squareMesh:getMesh():addVertex(-.5, -.5, 0)

squareMesh:setColor(1, 0, 0, 1)

backsquareMesh = SceneMesh.SceneMeshWithType(Mesh.QUAD_MESH)
backsquareMesh:getMesh():addVertex(.5, -.5, 1)
backsquareMesh:getMesh():addVertex(.5, .5, 1)
backsquareMesh:getMesh():addVertex(-.5, .5, 1)
backsquareMesh:getMesh():addVertex(-.5, -.5, 1)

backsquareMesh:setColor(0, 0, 1, 1)

scene:addChild(squareMesh)
scene:addChild(backsquareMesh)
]]--
scene:setActiveCamera(scene:getDefaultCamera())
scene:getDefaultCamera():setPosition(7, 0, 7)
scene:getDefaultCamera():lookAt(Vector3(0, 0, 0), Vector3(0, 1, 0))