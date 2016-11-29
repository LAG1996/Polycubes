scene = Scene(Scene.SCENE_3D)
cubeMesh = SceneMesh.SceneMeshWithType(Mesh.QUAD_MESH)

center = Entity()

anchorPoint = Entity()

side_1 = SceneMesh.SceneMeshWithType(Mesh.QUAD_MESH)
side_1:getMesh():addVertex(.5, -.5, -.5)
side_1:getMesh():addVertex(.5, .5, -.5)
side_1:getMesh():addVertex(-.5, .5, -.5)
side_1:getMesh():addVertex(-.5, -.5, -.5)
side_1:setColor(.1, 0, 0, 1)

side_2 = SceneMesh.SceneMeshWithType(Mesh.QUAD_MESH)
side_2:getMesh():addVertex(.5, -.5, .5)
side_2:getMesh():addVertex(.5, .5, .5)
side_2:getMesh():addVertex(-.5, .5, .5)
side_2:getMesh():addVertex(-.5, -.5, .5)
side_2:setColor(.3, 0, 0, 1)

side_3 = SceneMesh.SceneMeshWithType(Mesh.QUAD_MESH)
side_3:getMesh():addVertex(.5, .5, -.5)
side_3:getMesh():addVertex(.5, .5, .5)
side_3:getMesh():addVertex(-.5, .5, .5)
side_3:getMesh():addVertex(-.5, .5, -.5)
side_3:setColor(.5, 0, 0, 1)

side_4 = SceneMesh.SceneMeshWithType(Mesh.QUAD_MESH)
side_4:getMesh():addVertex(.5, -.5, -.5)
side_4:getMesh():addVertex(.5, -.5, .5)
side_4:getMesh():addVertex(-.5, -.5, .5)
side_4:getMesh():addVertex(-.5, -.5, -.5)
side_4:setColor(.7, 0, 0, 1)

side_5 = SceneMesh.SceneMeshWithType(Mesh.QUAD_MESH)
side_5:getMesh():addVertex(.5, -.5, .5)
side_5:getMesh():addVertex(.5, .5, .5)
side_5:getMesh():addVertex(.5, .5, -.5)
side_5:getMesh():addVertex(.5, -.5, -.5)
side_5:setColor(.9, 0, 0, 1)

side_6 = SceneMesh.SceneMeshWithType(Mesh.QUAD_MESH)
side_6:getMesh():addVertex(-.5, -.5, .5)
side_6:getMesh():addVertex(-.5, .5, .5)
side_6:getMesh():addVertex(-.5, .5, -.5)
side_6:getMesh():addVertex(-.5, -.5, -.5)
side_6:setColor(1, .7, .7, 1)

side_1.backfaceCulled = false
side_2.backfaceCulled = false
side_3.backfaceCulled = false
side_4.backfaceCulled = false
side_5.backfaceCulled = false
side_6.backfaceCulled = false

anchorPoint:addChild(side_1)

side_1:setPosition(0, 0, 0)

center:addChild(anchorPoint)
center:addChild(side_2)
center:addChild(side_3)
center:addChild(side_4)
center:addChild(side_5)
center:addChild(side_6)


scene:addChild(center)

scene:setActiveCamera(scene:getDefaultCamera())
scene:getDefaultCamera():setPosition(0, 0, -10)
scene:getDefaultCamera():lookAt(Vector3(0, 0, 0), Vector3(0, 1, 0))


--[[
anchorPoint:setPosition(0, 0, -1)
anchorPoint:setYaw(-90)
]]--
function Update(elapsed)
	center:setRoll(center:getRoll() + .5)
	center:setPitch(center:getPitch() + .3)
	center:setYaw(center:getYaw() + .7)
	
	if anchorPoint:getPosition().z - .01 > -1
	then
		anchorPoint:setPosition(anchorPoint:getPosition().x, anchorPoint:getPosition().y, anchorPoint:getPosition().z - .01)
	end
	
	if anchorPoint:getYaw() > -90
	then
		anchorPoint:setYaw(anchorPoint:getYaw() - .9)
	end
end