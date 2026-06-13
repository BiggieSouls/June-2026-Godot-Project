extends Node3D
#This node expects to be the target of a remote transform3d, set to only position
@export var terrain_node : GridMap
@export var inpute_rotation_speed : float = 0.01
@export var infinite_terrain_radius : int = 20
@export var noisesize : int = 1024 #MORE THEN 512
@export var cell_clear_distance : int = 200
var infinite_terrain_relative_coord_array : Array
# Called when the node enters the scene tree for the first time.
var noisedata : Image

func _ready() -> void:
	init_infinite_terrain_relative_coord_array()
	generate_noise2d()
	pass

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	rotate_terrain_from_input(Input.get_vector("move_left","move_right","move_forward","move_back"))
	infinite_terrain_generate()
	clear_distant_cells()
	pass

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
	for radcoord in infinite_terrain_relative_coord_array: #for each cell in gridmap that is blank, make a new cell
		var newcell : Vector3i = closestcell + radcoord
		if terrain_node.get_cell_item(newcell) == -1:
			if noisedata.get_pixel(abs(newcell.x % noisesize), abs(newcell.z % noisesize)).r < 0.5:
				terrain_node.set_cell_item(newcell, 1, randi_range(0,23))
			else:
				terrain_node.set_cell_item(newcell, 0)
	print(noisedata.get_pixel(abs(closestcell.x % noisesize), abs(closestcell.z % noisesize)))

func rotate_terrain_from_input(inputvector : Vector2): #uses Vector2 from input.getvector and rotates the handler, causing an 'orbit' rotation of terrain
	var playerinput  : Vector2 = inputvector  * inpute_rotation_speed #xyzw
	terrain_node.top_level = false #enables translation inheritence for the rotation function
	self.rotate(Vector3.RIGHT, playerinput.y)
	self.rotate(Vector3.FORWARD, playerinput.x)
	terrain_node.top_level = true
	pass

func clear_distant_cells():
	var closestcell : Vector3i = terrain_node.local_to_map(terrain_node.to_local(self.position))
	for cell in terrain_node.get_used_cells():
		var dif : Vector3i = cell - closestcell
		if dif.length() > cell_clear_distance:
			terrain_node.set_cell_item(cell, -1)
	
