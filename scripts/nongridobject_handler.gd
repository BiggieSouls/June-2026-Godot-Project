extends Node3D
class_name NonGridHandlerComponent


@export var unrender_distance_multiplier : float = 1.5
@export var length_to_unrender : float = 100
var parent_node : Node3D
@export var terrain_handler_node : WorldTerrainHandler


# Called when the node enters the scene tree for the first time.

func _ready() -> void:
	if parent_node == null:
		parent_node = get_parent()
	%UnrenderCylinder.shape.radius = length_to_unrender
	pass # Replace with function body.

#func create(parent : Node3D, terrainhandler : WorldTerrainHandler, unrenderlength : float) -> NonGridHandlerComponent:
	#var newnode : NonGridHandlerComponent = NonGridHandlerComponent.new()
	#newnode.parent_node = parent
	#newnode.terrain_handler_node = terrainhandler
	#newnode.length_to_unrender = unrenderlength
	#
	#parent.add_child(newnode)
	#return newnode
	
func execute_parent():
	parent_node.queue_free()
# Called every frame. 'delta' is the elapsed time since the previous frame.

func _physics_process(delta: float) -> void:
	pass
	
func unrender_from_distance():
	var deltavector : Vector3
	deltavector = terrain_handler_node.global_position - self.global_position
	if deltavector.length() >= length_to_unrender * unrender_distance_multiplier:
		parent_node.queue_free()
		pass
	pass



func _on_unrender_area_body_exited(body: Node3D) -> void:
	execute_parent()
	pass # Replace with function body.
