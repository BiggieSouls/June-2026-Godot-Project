extends Node3D
class_name WorldTerrainHandler
#This node expects to be the target of a remote transform3d, set to only position
@export var remote_terrain_transform_target : Node3D

@export var remote_transform : RemoteTransform3D
@export var terrain_node : GridMap
@export var inpute_rotation_speed : float = 0.01
@export var infinite_terrain_radius : int = 30
@export var noisesize : int = 1024 #MORE THEN 512
@export var cell_clear_distance : int = 120
@export var rotation_desire_node : Node3D 

@export var spring_scene : PackedScene = preload("res://scenes/actors/spring.tscn")

var array_of_non_grid_item_handlers : Array = []
var orthogonal_diagonal_adjacency : Array = [Vector3i(1,0,0),Vector3i(1,0,1),Vector3i(1,0,-1),Vector3i(-1,0,1),Vector3i(-1,0,0),Vector3i(-1,0,-1),Vector3i(0,0,-1),Vector3i(0,0,1)]
var terrain_rotation_desire: Quaternion  = Quaternion.from_euler(Vector3.DOWN) 
var infinite_terrain_relative_coord_array : Array
# Called when the node enters the scene tree for the first time.
var noisedata : Image
var acceptinginput  : bool = true

func _ready() -> void:
	init_infinite_terrain_relative_coord_array()
	generate_noise2d()
	remote_transform.remote_path = remote_terrain_transform_target.get_path()
	generate_non_grid_piece_spring(Vector3.ZERO)
	pass

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	infinite_terrain_generate()
	clear_distant_cells() #distant noncells clear themselves
	rotation_process()
	orthonormalize()
	terrain_node.orthonormalize()
	pass

#    ░█████████    ░██████   ░██████████   ░███    ░██████████░██████  ░██████   ░███    ░██ 
#    ░██     ░██  ░██   ░██      ░██      ░██░██       ░██      ░██   ░██   ░██  ░████   ░██ 
#    ░██     ░██ ░██     ░██     ░██     ░██  ░██      ░██      ░██  ░██     ░██ ░██░██  ░██ 
#    ░█████████  ░██     ░██     ░██    ░█████████     ░██      ░██  ░██     ░██ ░██ ░██ ░██ 
#    ░██   ░██   ░██     ░██     ░██    ░██    ░██     ░██      ░██  ░██     ░██ ░██  ░██░██ 
#    ░██    ░██   ░██   ░██      ░██    ░██    ░██     ░██      ░██   ░██   ░██  ░██   ░████ 
#    ░██     ░██   ░██████       ░██    ░██    ░██     ░██    ░██████  ░██████   ░██    ░███ 

func rotation_process():
	if acceptinginput == true:
		rotate_terrain_from_input(Input.get_vector("second_move_left","second_move_right","second_move_forward","second_move_back"))
	#rotate_from_camera_input()
	pass
	
var desire_node_last_quaternion : Quaternion = Quaternion.from_euler(Vector3.FORWARD)
var desire_node_last : Vector3 = Vector3.ZERO

func rotate_from_mouse_input():
	var inputvector = Input.get_last_mouse_screen_velocity().normalized() * 0.001
	if Input.is_action_pressed("pull") == true:
		terrain_node.top_level = false
		self.rotate(Vector3.LEFT, inputvector.y)
		self.rotate(Vector3.BACK , inputvector.x)
		terrain_node.top_level = true

func rotate_from_camera_input(): #uses rotation desire node
	if desire_node_last == Vector3.ZERO:
		desire_node_last = rotation_desire_node.global_rotation
	var delta_cam_rotation : Vector3 = rotation_desire_node.global_rotation - desire_node_last 
	desire_node_last = rotation_desire_node.global_rotation
	#print(delta_cam_rotation)
	if Input.is_action_pressed("pull") == true:
		terrain_node.top_level = false
		#self.global_rotation = self.global_rotation - delta_cam_rotation
		rotate(Vector3i.RIGHT ,delta_cam_rotation.x)
		rotate(Vector3i.UP, delta_cam_rotation.y)
		terrain_node.top_level = true
	pass

func rotate_terrain_towards_desire(desiredquaternion : Quaternion):
	terrain_node.top_level = false
	self.quaternion = desiredquaternion #just use this instead
	terrain_node.top_level = true
	pass

func rotate_terrain_from_input(inputvector : Vector2): #uses Vector2 from input.getvector and rotates the handler, causing an 'orbit' rotation of terrain
	var playerinput  : Vector2 = inputvector  * inpute_rotation_speed #xyzw
	terrain_node.top_level = false #enables translation inheritence for the rotation function
	self.rotate(Vector3.LEFT, playerinput.y)
	self.rotate(Vector3.BACK , playerinput.x)
	terrain_node.top_level = true
	pass

func get_terrain_desire_from_node():
	if rotation_desire_node != null:
		#print(Quaternion.from_euler(rotation_desire_node.global_rotation))
		#return Quaternion.from_euler(rotation_desire_node.global_rotation)
		#terrain_rotation_desire = terrain_rotation_desire.slerp(Quaternion.from_euler(rotation_desire_node.global_rotation), 0.5)
		return terrain_rotation_desire
	else:
		return Quaternion.from_euler(Vector3.DOWN)
	pass

#    ░██████████░██████████ ░█████████  ░█████████     ░███    ░██████░███    ░██ 
#        ░██    ░██         ░██     ░██ ░██     ░██   ░██░██     ░██  ░████   ░██ 
#        ░██    ░██         ░██     ░██ ░██     ░██  ░██  ░██    ░██  ░██░██  ░██ 
#        ░██    ░█████████  ░█████████  ░█████████  ░█████████   ░██  ░██ ░██ ░██ 
#        ░██    ░██         ░██   ░██   ░██   ░██   ░██    ░██   ░██  ░██  ░██░██ 
#        ░██    ░██         ░██    ░██  ░██    ░██  ░██    ░██   ░██  ░██   ░████ 
#        ░██    ░██████████ ░██     ░██ ░██     ░██ ░██    ░██ ░██████░██    ░███ 
   
@export var array_of_3x_replacable_cell_indexes : Array = [0, -1]


func generate_noise2d():
	var texture = NoiseTexture2D.new()
	texture.seamless = true
	texture.width = noisesize
	texture.height = noisesize
	texture.noise = FastNoiseLite.new()
	texture.noise.frequency = 0.1
	await texture.changed
	noisedata = texture.get_image()

func init_infinite_terrain_relative_coord_array(): #generates a 'square' array of Vector3s (x, 0, z) from -radius to +radius
	var terrain_square_length : int = infinite_terrain_radius*2 + 1
	var xi : int = 1
	var zi : int = 1
	var newcoord : Vector3i = Vector3i(-infinite_terrain_radius, 0 ,-infinite_terrain_radius)
	infinite_terrain_relative_coord_array.append(newcoord)
	while xi <= terrain_square_length:
		while zi < terrain_square_length:
			newcoord += Vector3i.BACK
			zi += 1
			infinite_terrain_relative_coord_array.append(newcoord)
		newcoord.z = -infinite_terrain_radius
		zi = 1
		newcoord += Vector3i.RIGHT
		xi += 1 
		if(newcoord.x <= infinite_terrain_radius):
			infinite_terrain_relative_coord_array.append(newcoord)	

func infinite_terrain_generate():
	var closestcell : Vector3i = terrain_node.local_to_map(terrain_node.to_local(self.position))
	closestcell.y = 0 #finds the cell under the player
	var newcell : Vector3i
	var currentcellnoisefloat : float
	for radcoord in infinite_terrain_relative_coord_array: #for each cell in gridmap that is blank, make a new cell
		newcell = closestcell + radcoord
		currentcellnoisefloat = noisedata.get_pixel(abs(newcell.x % noisesize), abs(newcell.z % noisesize)).r
		if terrain_node.get_cell_item(newcell) == -1:
			if randf() < 0.05:
				#terrain_node.set_cell_item(newcell, 4)
				generate_3x_terrain_piece(newcell, randi_range(7,8))
			else: if currentcellnoisefloat < 0.15:
				terrain_node.set_cell_item(newcell, 3)
			else: if currentcellnoisefloat < 0.4:
				terrain_node.set_cell_item(newcell, 1, randi_range(0,23))
				if randf() < 0.1:
					generate_non_grid_piece_spring(newcell)
			else:
				terrain_node.set_cell_item(newcell, 0)
	
func generate_non_grid_piece_spring(gridmapcell : Vector3i):
	var newspring : Node3D = spring_scene.instantiate()
	var newspringpos : Vector3 = terrain_node.to_global(terrain_node.map_to_local(gridmapcell)) #TODO ADD DIST in local up
	newspringpos += terrain_node.rotation * Vector3.UP
	#var newspringpos : Vector3 = terrain_node.map_to_local(gridmapcell + Vector3i.UP)
	
	terrain_node.add_child(newspring)
	newspring.global_position = newspringpos
	#NonGridHandlerComponent.create(newspring, self, cell_clear_distance)
	create_nongrid_handler(newspring)
	pass
	

	
func create_nongrid_handler(parent : Node3D) -> NonGridHandlerComponent:
	var newnode : NonGridHandlerComponent = NonGridHandlerComponent.new()
	newnode.parent_node = parent
	newnode.terrain_handler_node = self
	newnode.length_to_unrender = cell_clear_distance
	parent.add_child(newnode)
	return newnode
	

	
func generate_3x_terrain_piece(gridmapcell : Vector3i, gridmapindex : int):	
	var testcell : Vector3i
	var isvalid : bool = true
	var testcellindex : int
	for cellmodifier in orthogonal_diagonal_adjacency:
		testcell = cellmodifier + gridmapcell
		testcellindex = terrain_node.get_cell_item(testcell)
		if check_against_3x_replaceables(testcellindex) == false:
			isvalid = false
	if isvalid == true:
		for cellmodifier in orthogonal_diagonal_adjacency:
			terrain_node.set_cell_item(gridmapcell + cellmodifier, 9)
		terrain_node.set_cell_item(gridmapcell, gridmapindex)
	#Inputs a Gridmap Index for a 3x tile and attempt generation
	pass
	
func check_against_3x_replaceables(checkedcell : int): 
	var legality : bool = false
	for index in array_of_3x_replacable_cell_indexes:
		if index == checkedcell:
			legality = true
	return legality
			

#    ░██           ░██████   ░███████   
#    ░██          ░██   ░██  ░██   ░██  
#    ░██         ░██     ░██ ░██    ░██ 
#    ░██         ░██     ░██ ░██    ░██ 
#    ░██         ░██     ░██ ░██    ░██ 
#    ░██          ░██   ░██  ░██   ░██  
#    ░██████████   ░██████   ░███████   

func clear_distant_cells():
	var closestcell : Vector3i = terrain_node.local_to_map(terrain_node.to_local(self.position))
	for cell in terrain_node.get_used_cells():
		var dif : Vector3i = cell - closestcell
		if dif.length() > cell_clear_distance:
			terrain_node.set_cell_item(cell, -1)
	
