extends Node3D

const BALL = preload("res://addons/TrailRenderer/Samples/Prefabs/ball.tscn")
@onready var camera_3d = $".."

# Called when the node enters the scene tree for the first time.
func _ready():
	pass # Replace with function body.

func _input(event):
	if (event is InputEventMouseButton && event.is_pressed() && !event.is_echo() && event.button_index == MOUSE_BUTTON_LEFT):
		var ball = BALL.instantiate() as RigidBody3D;
		get_tree().current_scene.add_child(ball);
		ball.global_position = global_position;
		ball.apply_impulse(-global_basis.z.normalized() * 10.0);

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	pass
