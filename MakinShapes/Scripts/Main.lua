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
sceneEntity:setColor(.6, .2, .4, 1)
scene:addChild(sceneEntity)


--[[
##########Squares from mesh#################
]]--

floater = Entity()
floater:setPosition(0, 0, 0)
squareMesh:getMesh():addVertex(.5, -.5, 0)
squareMesh:getMesh():addVertex(.5, .5, 0)
squareMesh:getMesh():addVertex(-.5, .5, 0)
squareMesh:getMesh():addVertex(-.5, -.5, 0)
squareMesh:setColor(1, 0, 0, 1)
squareMesh:setAnchorPoint(Vector3(0.0, -1.0, 0.0))
squareMesh:setPosition(.5, .5, 0)

--Set an empty object as the parent of the square to act as the anchor point of the square.
floater:addChild(squareMesh)
floater:setPosition(0.0, 0.0, 0.0)
squareMesh:setPosition(.5, 0, 0)



backsquareMesh = SceneMesh.SceneMeshWithType(Mesh.QUAD_MESH)
backsquareMesh:getMesh():addVertex(.5, -.5, 1)
backsquareMesh:getMesh():addVertex(.5, .5, 1)
backsquareMesh:getMesh():addVertex(-.5, .5, 1)
backsquareMesh:getMesh():addVertex(-.5, -.5, 1)

backsquareMesh:setColor(0, 0, 1, 1)

sideSquareMesh = SceneMesh.SceneMeshWithType(Mesh.QUAD_MESH)
sideSquareMesh:getMesh():addVertex(.5, -.5, -1)
sideSquareMesh:getMesh():addVertex(.5, .5, -1)
sideSquareMesh:getMesh():addVertex(-.5, .5, -1)
sideSquareMesh:getMesh():addVertex(-.5, -.5, -1)

sideSquareMesh:setColor(0, 1, 0, 1)
sideSquareMesh:setRoll(45)

squareMesh.backfaceCulled = false
backsquareMesh.backfaceCulled = false
sideSquareMesh.backfaceCulled = false

scene:addChild(floater)
scene:addChild(backsquareMesh)
scene:addChild(sideSquareMesh)

scene:setActiveCamera(scene:getDefaultCamera())
scene:getDefaultCamera():setPosition(0, 0, 10)
scene:getDefaultCamera():lookAt(Vector3(0, 0, 0), Vector3(0, 1, 0))

function Update(elapsed)
	floater:setRoll(floater:getRoll() + .5) --Rotate the square's floater so that the square will seem to rotate over an anchor point
	backsquareMesh:setPitch(backsquareMesh:getPitch() + 1)
	sideSquareMesh:setYaw(sideSquareMesh:getYaw() + .7)
	
	sceneEntity:setPitch(sceneEntity:getPitch() + .3)
	sceneEntity:setRoll(sceneEntity:getRoll() + .2)
end