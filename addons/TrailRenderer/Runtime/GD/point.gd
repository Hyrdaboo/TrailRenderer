class_name Point


var position: Vector3
var alignment_vector: Vector3
## DO NOT MODIFY THIS. Used internally by the LineRenderer.
var texture_offset: float
var time: float


func _init(source_position: Vector3, bitangent: Vector3 = Vector3.INF) -> void:
	bitangent = bitangent if bitangent != Vector3.INF else Vector3.FORWARD

	self.position = source_position
	self.alignment_vector = bitangent.normalized()
	self.time = Time.get_ticks_msec() / 1000.0