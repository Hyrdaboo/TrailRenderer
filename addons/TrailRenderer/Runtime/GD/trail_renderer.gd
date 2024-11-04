class_name TrailRenderer
extends LineRenderer


@export var lifetime: float = 1.0
@export var min_vertex_distance: float = 0.5
@export var is_emitting: bool = true

var _trail_pieces: Array = []
var _is_emitting_last_frame: bool


func _enter_tree() -> void:
	super._enter_tree()
	_trail_pieces.append(TrailPiece.new(self))


func _process(delta: float) -> void:
	if not _is_emitting_last_frame and is_emitting:
		if _trail_pieces.size() == 0 or _trail_pieces[0].is_dirty():
			_trail_pieces.insert(0, TrailPiece.new(self))
	_is_emitting_last_frame = is_emitting

	if _trail_pieces.size() > 0:
		_trail_pieces[0].on_delete_complete = _on_delete_complete

	for i: int in range(_trail_pieces.size()):
		var trail_piece: TrailPiece = _trail_pieces[i]
		trail_piece.process(delta)


func _on_delete_complete() -> void:
	var last_idx: int = _trail_pieces.size() - 1
	_trail_pieces.remove_at(last_idx)
