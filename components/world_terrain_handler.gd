extends Node3D

@export var terrain_node : GridMap
@export var inpute_rotation_speed : float = 0.01

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	#terrain_node.transform.origin = Vector3.DOWN

	
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	#terrain_node.rotated(Vector3.RIGHT, 0.01)
	#terrain_node.transform.basis.z.x += 1
	var playerinput  : Vector2 = Input.get_vector("move_left","move_right","move_forward","move_back") * inpute_rotation_speed #xyzw
	terrain_node.top_level = false
	self.rotate(Vector3.RIGHT, playerinput.y)
	self.rotate(Vector3.FORWARD, playerinput.x)
	terrain_node.top_level = true
	
	pass
