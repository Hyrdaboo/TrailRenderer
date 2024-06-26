class_name TrailPiece


var on_delete_complete: Callable

var _time: float = 0.0
var _last_position: Vector3 = Vector3()
var _last_spawn_point: Vector3 = Vector3()
var _remaining_lifetime: float = 0.0
var _alive_time: float = 0.0
var _is_moving: bool = false
var _is_dirty: bool = false
var _first_point_original: Point
var _line_renderer: LineRenderer = LineRenderer.new()
var _trail_renderer: TrailRenderer


func _init(source_trail_renderer: TrailRenderer) -> void:
	self._trail_renderer = source_trail_renderer
	_trail_renderer.add_child(_line_renderer)

	_last_position = _trail_renderer.global_position
	_last_spawn_point = _trail_renderer.global_position
	_remaining_lifetime = _trail_renderer.lifetime


func process(delta: float) -> void:
	_line_renderer.copy_values(_trail_renderer)
	_time = Time.get_ticks_msec() / 1000.0

	if not _trail_renderer.is_emitting and _line_renderer.points.size() > 0:
		_is_dirty = true

	if _line_renderer.points.size() > 0 and _remaining_lifetime > 0:
		_alive_time += delta

	if _line_renderer.points.size() == 0:
		_alive_time = 0

	_is_moving = _last_position != _line_renderer.global_position
	_last_position = _line_renderer.global_position
	_remaining_lifetime = (
		(_remaining_lifetime - delta) 
		if _line_renderer.points.size() > 0 
		else _trail_renderer.lifetime
	)
	_remaining_lifetime = min(_remaining_lifetime, _trail_renderer.lifetime)

	_remove_points()

	if _line_renderer.points.size() == 0 and is_dirty():
		on_delete_complete.call()
		_line_renderer.queue_free()

	if not _trail_renderer.is_emitting:
		_last_spawn_point = _trail_renderer.global_position
		return
	_add_points()


func is_dirty() -> bool:
	return _is_dirty


func _add_points() -> void:
	if is_dirty():
		return

	if _line_renderer.points.size() == 0 and _is_moving:
		_line_renderer.points.append(Point.new(_line_renderer.global_position))
		_line_renderer.points.append(Point.new(_line_renderer.global_position))

	if (
		_last_spawn_point.distance_to(_line_renderer.global_position) > _trail_renderer.min_vertex_distance 
		and _line_renderer.points.size() > 0
	):
		var previous_position: Vector3 = _line_renderer.points[-2].position
		_line_renderer.points[-2].position = _line_renderer.global_position
		_line_renderer.points.insert(_line_renderer.points.size() - 2, Point.new(previous_position, _trail_renderer.global_basis.x.normalized()))
		_last_spawn_point = _line_renderer.global_position

	if _line_renderer.points.size() > 1:
		_line_renderer.points[-1].position = _line_renderer.global_position
		_line_renderer.points[-1].alignment_vector = _trail_renderer.global_basis.x
		_line_renderer.points[-2].alignment_vector = _trail_renderer.global_basis.x


func _remove_points() -> void:
	if _remaining_lifetime > 0:
		return

	if _first_point_original == null:
		_first_point_original = Point.new(_line_renderer.get_point(0).position)

	while (
		_line_renderer.points.size() > 0
		and _time >= _line_renderer.get_point(0).time + _alive_time
	):
		_line_renderer.points.remove_at(0)
		_first_point_original = (
			Point.new(_line_renderer.get_point(0).position)
			if _line_renderer.points.size() > 0
			else null
		)

	if _line_renderer.points.size() >= 2:
		var t: float = inverse_lerp(_first_point_original.time, _line_renderer.get_point(0).time + _alive_time, _time)
		_line_renderer.points[0].position = _first_point_original.position.lerp(_line_renderer.get_point(1).position, t)
