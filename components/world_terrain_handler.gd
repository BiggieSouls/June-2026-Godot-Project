extends Node3D
#This node expects to be the target of a remote transform3d, set to only position
@export var terrain_node : GridMap
@export var inpute_rotation_speed : float = 0.01

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	#terrain_node.transform.origin = Vector3.DOWN
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	rotate_terrain_from_input(Input.get_vector("move_left","move_right","move_forward","move_back"))

	pass

func infinite_terrain_generate():
	var closestcell : Vector3i = terrain_node.local_to_map(self.position)
	closestcell.y = 0
	#print(terrain_node.get_cell_item(closestcell))
	terrain_node.set_cell_item(closestcell, 0)
	
func rotate_terrain_from_input(inputvector : Vector2): #uses Vector2 from input.getvector and rotates the handler, causing an 'orbit' rotation of terrain
	var playerinput  : Vector2 = inputvector  * inpute_rotation_speed #xyzw
	terrain_node.top_level = false #enables translation inheritence for the rotation function
	self.rotate(Vector3.RIGHT, playerinput.y)
	self.rotate(Vector3.FORWARD, playerinput.x)
	terrain_node.top_level = true
	pass
