class_name LineRenderer
extends Node3D


enum Alignment { View, TransformZ, Static }
enum TextureMode { Stretch, Tile, PerSegment }

@export var curve: Curve
@export var alignment: Alignment = Alignment.TransformZ
@export var world_space: bool = true

@export_group("Appearance")
@export var material: Material:
	get:
		return material
	set (value):
		material = value
		if _mesh_instance != null:
			_mesh_instance.material_override = material
@export var cast_shadows: GeometryInstance3D.ShadowCastingSetting = GeometryInstance3D.SHADOW_CASTING_SETTING_OFF
@export var color_gradient: Gradient:
	get:
		return color_gradient
	set (value):
		if value != null:
			color_gradient = value
@export var texture_mode: TextureMode

var points: Array = []

var _mesh: ImmediateMesh = ImmediateMesh.new()
var _mesh_instance: MeshInstance3D
var _camera: Camera3D


func _enter_tree() -> void:
	if color_gradient == null:
		color_gradient = Gradient.new()
		color_gradient.add_point(0.0, Color(1.0, 1.0, 1.0))
		color_gradient.add_point(1.0, Color(1.0, 1.0, 1.0))

	if curve == null:
		curve = Curve.new()
		curve.add_point(Vector2(0.0, 0.5), 0.0, 0.0, Curve.TANGENT_FREE, Curve.TANGENT_LINEAR)
		curve.add_point(Vector2(1.0, 0.5), 0.0, 0.0, Curve.TANGENT_LINEAR)

	_mesh_instance = MeshInstance3D.new()
	_mesh_instance.name = "MeshInstance3D"
	add_child(_mesh_instance)

	_mesh_instance.mesh = _mesh
	_mesh_instance.material_override = material
	_mesh_instance.top_level = true


func _process(_delta: float) -> void:
	_camera = get_viewport().get_camera_3d()
	_mesh_instance.cast_shadow = cast_shadows
	_mesh_instance.global_transform = (
		Transform3D.IDENTITY if world_space else global_transform
	)

	_mesh.clear_surfaces()
	if points.size() < 2:
		return

	_mesh.surface_begin(Mesh.PRIMITIVE_TRIANGLE_STRIP)

	for i: int in range(points.size()):
		var current_point: Point = points[i]

		var tangent: Vector3
		if i == 0:
			var next_point: Point = points[1]
			tangent = current_point.position.direction_to(next_point.position)
		else:
			var previous_point: Point = points[i - 1]
			tangent = -current_point.position.direction_to(previous_point.position)

		var alignment_vec: Vector3
		if alignment == Alignment.View and world_space:
			alignment_vec = _camera.global_basis.z.normalized()
		elif alignment == Alignment.TransformZ and world_space:
			alignment_vec = global_basis.z.normalized()
		else:
			alignment_vec = current_point.alignment_vector.normalized()

		var bitangent: Vector3 = alignment_vec.cross(tangent).normalized()
		var normal: Vector3 = tangent.cross(bitangent).normalized()

		var t: float = i / (points.size() - 1.0)
		var color: Color = color_gradient.sample(t)
		bitangent *= curve.sample(t)

		match texture_mode:
			TextureMode.Stretch:
				current_point.texture_offset = i / (points.size() - 1.0)
			TextureMode.Tile:
				if i > 0:
					var previous: Point = points[i - 1]
					current_point.texture_offset = (
						current_point.position.distance_to(previous.position)
						+ previous.texture_offset
					)
			TextureMode.PerSegment:
				current_point.texture_offset = i

		_mesh.surface_set_uv(Vector2(0, 1 - current_point.texture_offset))
		_mesh.surface_set_normal(normal)
		_mesh.surface_set_color(color)
		_mesh.surface_add_vertex(current_point.position - bitangent)

		_mesh.surface_set_uv(Vector2(1, 1 - current_point.texture_offset))
		_mesh.surface_set_normal(normal)
		_mesh.surface_set_color(color)
		_mesh.surface_add_vertex(current_point.position + bitangent)

	_mesh.surface_end()


func copy_values(lr: LineRenderer) -> void:
	curve = lr.curve
	alignment = lr.alignment
	world_space = lr.world_space
	material = lr.material
	cast_shadows = lr.cast_shadows
	color_gradient = lr.color_gradient
	texture_mode = lr.texture_mode


func get_point(index: int) -> Point:
	return points[index]
