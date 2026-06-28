extends Node3D

#radiuses of effects
@export var terrain_rotation_radius : float = 100
@export var terrain_rotation_quaternion : Quaternion = Quaternion.from_euler(Vector3i.FORWARD)


var parent_node : Node3D
# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	parent_node = self.get_parent()
	
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	
	pass

func _on_rotation_moat_area_entered(area: Area3D) -> void:
	pass # Replace with function body.



func _on_rotation_moat_area_exited(area: Area3D) -> void:
	pass # Replace with function body.
