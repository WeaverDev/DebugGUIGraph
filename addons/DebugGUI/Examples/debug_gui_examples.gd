extends Node

# DebugGUIGraph
var i_am_a_graph_variable
# DebugGUILog
var i_am_a_log_variable; var i_am_not;

# empty line (invalid)
# DebugGUILog

# func (invalid)
# DebugGUILog
func test():
	# inside func (invalid)
	# DebugGUILog
	var i_should_not_be_logged = 0
	pass 
