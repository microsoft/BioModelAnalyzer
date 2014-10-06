proc cuboid { origin dimensions } {
	#below are the transformation vectors 
	#(ie not absolute locations in space)
	#$origin is effectively lowerfrontleft
	set lowerfrontright "[lindex $dimensions 0] 0 0"
	set upperfrontleft  "0 [lindex $dimensions 1] 0"
	set upperfrontright "[lindex $dimensions 0] [lindex $dimensions 1] 0"
	set lowerbackleft   "0 0 [lindex $dimensions 2]"
	set upperbackleft   "0 [lindex $dimensions 1] [lindex $dimensions 2]"
	set lowerbackright  "[lindex $dimensions 0] 0 [lindex $dimensions 2]"
	set upperbackright $dimensions
	#front
	graphics top triangle $origin [vecadd $origin $lowerfrontright] [vecadd $origin $upperfrontright]
	graphics top triangle $origin [vecadd $origin $upperfrontleft ] [vecadd $origin $upperfrontright]
	#bottom
	graphics top triangle $origin [vecadd $origin $lowerbackleft  ] [vecadd $origin $lowerbackright ]
	graphics top triangle $origin [vecadd $origin $lowerbackright ] [vecadd $origin $lowerfrontright]
	#left
	graphics top triangle $origin [vecadd $origin $upperfrontleft ] [vecadd $origin $upperbackleft  ]
	graphics top triangle $origin [vecadd $origin $lowerbackleft  ] [vecadd $origin $upperbackleft  ]
	#back
	graphics top triangle         [vecadd $origin $lowerbackleft  ] [vecadd $origin $upperbackright ] [vecadd $origin $upperbackleft  ]
	graphics top triangle         [vecadd $origin $lowerbackleft  ] [vecadd $origin $upperbackright ] [vecadd $origin $lowerbackright ]
	#right
	graphics top triangle         [vecadd $origin $lowerfrontright] [vecadd $origin $upperbackright ] [vecadd $origin $upperfrontright]
	graphics top triangle         [vecadd $origin $lowerfrontright] [vecadd $origin $upperbackright ] [vecadd $origin $lowerbackright ]
	#top
	graphics top triangle         [vecadd $origin $upperfrontleft ] [vecadd $origin $upperbackright ] [vecadd $origin $upperbackleft  ]
	graphics top triangle         [vecadd $origin $upperfrontleft ] [vecadd $origin $upperbackright ] [vecadd $origin $upperfrontright]
	return 0
}

proc contactSurfacePlates { id cutoff sel } {
	set atom [atomselect top "index $id"]
	set origin [lindex [$atom get {x y z}] 0]
	set neighbours [lindex [measure contacts $cutoff $atom $sel] 1]
	foreach i $neighbours {
		if {$i < $id} {continue}
		foreach j $neighbours {
			if {$j < $i} {continue}
			set iC [lindex [[atomselect top "index $i"] get {x y z}] 0]
			set jC [lindex [[atomselect top "index $j"] get {x y z}] 0]
			graphics top triangle $origin $iC $jC
			}
		}
}

proc contactSurface { sel cutoff } {
	set atoms [atomselect top "$sel"]
	set indices [$atoms get index]
	foreach id $indices {
		contactSurfacePlates $id $cutoff $atoms
	}
	$atoms delete
}

proc updateRadius {args} {
	global molid
	set sys [atomselect $molid "all"]
	foreach id [$sys get index] {
		set atom [atomselect $molid "index $id"]
		$atom set radius [$atom get user]
		$atom delete
	}
	$sys delete
}
#Sadly names can't change in VMD without breaking things
#proc updateRadiusAndState {args} {
#	global molid
#	set name {"ZN" "ALA" "PHE" "ASP" "TYR" "HIS" "CYS" "ARG" "ASN" "GLU" "GLN" "PRO" "THR" "VAL" "ILE" "URA"}
#	foreach id [[atomselect $molid "user < 0.0"] get index] {
#		set atom [atomselect $molid "index $id"]
#		set state [expr int([$atom get user4])]
#		$atom set resname [lindex $name $state]
#		$atom set radius [$atom get user2]
#	}
#}

#proc user4ToResname {args} {
#	global molid 
#	set name {0 "ZN" 1 "ALA" 2 "PHE" 3 "ASP" 4 "TYR" 5 "HIS" 6 "CYS" 7 "ARG" 8 "ASN" 9 "GLU" 10 "GLN" 11 "" 12 "PRO" 13 "THR" 14 "VAL" 15 "URA"}
#	foreach id [[atomselect $molid "all"] get index] {
#		set atom [atomselect $molid "index $id"]
#		set state [$atom get user4]
#		$atom set resname [dict get $name $state]
#	}
#}


proc numberMachines { line } {
	set mach [regexp -line -all ",;" $line]
	return [expr $mach+1]
}

proc processRawMachines { splitStates varNum variables } {
	#first value is an empty list due to the way split works
	set machines []
	set limit [expr [llength $splitStates] - $varNum]
	for {set i 0} {$i<=$limit} {incr i $varNum} {
		set complete [lrange $splitStates [expr $i+1] [expr $i+$varNum-1]]
		set processed []
		for {set j 0} {$j < [llength $variables] } {incr j} {
			lappend processed [lindex $variables $j]
			lappend processed [dict get $complete [lindex $variables $j]]
		}
		unset complete
		lappend machines $processed
	}
	return $machines	
}


proc processStateLine { line variables } {
		set machinestates [split $line ";,"]
		#Format is varID/state/varID/State
		set noMach [numberMachines $line]
		set lms [llength $machinestates]
		set noVar [expr $lms/$noMach]
		#puts "Machines $noMach Variables $noVar Total keys/values $lms"
		if {[expr [llength $machinestates]%$noMach] != 0} {
			puts "ERROR- machine number/state mismatch"
		}
		return [processRawMachines $machinestates $noVar $variables]
		#for {set i 0} {$i<$noMach} {incr i} {
		#	set state {}
		#	for {set j 0} {$j<($noVar*2)} {incr j} {
		#		set v [expr int([lindex $machinestates [expr $i*$noVar+$j]])]
		#		puts $v
		#		lappend state $v
		#	}
		#	lappend timepoint $state
		#}
}

proc amod { id f val } {
	global molid
	set atom [atomselect $molid "index $id" frame $f]
	$atom set user4 $val
	$atom delete
}

proc nullmod { f val } {
	global molid 
	set atom [atomselect $molid "user <= 0" frame $f]
	$atom set user4 $val
	$atom delete
}

proc processState { csvfile variables } {
	global molid
	set inp [open $csvfile r]
	set f 0
	while {[gets $inp line] >= 0} {
		puts "Frame $f"	
		set machines [processStateLine $line $variables]
		set number [llength $machines]
		for {set i 0} {$i<$number} {incr i} {
			set nV [llength $variables]
			set combo 0
			for {set j 0} {$j<$nV} {incr j} {
				set modVar [dict get [lindex $machines $i] [lindex $variables $j]]
				set combo [expr $combo + $modVar*2**$j]
			}
			amod $i $f $combo
		}
		nullmod $f $combo
		incr f
	}
	close $inp
}

proc zeroUser4 { f } {
	global molid
	set atom [atomselect $molid "all" frame $f]
       $atom set user4 0.0
       $atom delete
       }	

proc genMap { lastGen currentGen dT } {
	#Return a list of lists
	#Each element of the list corresponds to the fate of the cells between generations
	#Essentially inference... In future we need to explicitly record this
	#Cells can live, die or divide
	set total [llength $currentGen]
	set oTotal [llength $lastGen]
	set progeny []
	set parents []
	set inh []
	for {set i 0} {$i < $total} {incr i} {
		set age [lindex $currentGen $i]
		if {$age < $dT} {
			lappend progeny $i
			#if a cell is born, its parent cannot have died.
			#We can therefore add it, and its neighbour to the genMap 
			#and skip the next cell
			set family "$i [expr $i+1]"
			incr i
			lappend inh $family
			#puts "Birth $family"
		} else {
			lappend parents $i
			#If we are looking at an old cell, it could be one of two things
			#If the ancestor kept living, it is the ancestor
			set ageDiff [expr [lindex $currentGen $i] - [lindex $lastGen [llength $inh]]]
			#puts "Age difference $ageDiff"
			#We need a further test here to make sure that we don't miss anything
			if { [expr round($ageDiff)] == [expr round($dT)] } {
				lappend inh $i
				#puts "Life $i"
			} else {
				#If the ancestor died, we record an empty list and try again
				lappend inh []
				set i [expr $i - 1]
				#puts "Death $i"
			}
		}
	}
	if {[llength $inh] != [llength $lastGen]} {
		puts "ERROR generating map"
		set gL [llength $inh]
		set lGL [llength $lastGen]
		puts "Last gen = $lGL ; Map = $gL"
	}
	#puts "Completed genmap"
	return $inh

}

proc genMapP { lastGen currentGen dT lastGenPositions currentGenPositions } {
	#More powerful version of genMap which uses the positions to discern twin behaviour
	#Return a list of lists
	#Each element of the list corresponds to the fate of the cells between generations
	#Essentially inference... In future we need to explicitly record this
	#Cells can live, die or divide
	set total [llength $currentGen]
	set oTotal [llength $lastGen]
	set progeny []
	set parents []
	set inh []
	for {set i 0} {$i < $total} {incr i} {
		set age [lindex $currentGen $i]
		set position [lindex $currentGenPositions $i]
		if {$age < $dT} {
			lappend progeny $i
			#if a cell is born, its parent cannot have died.
			#We can therefore add it, and its neighbour to the genMap 
			#and skip the next cell
			set family "$i [expr $i+1]"
			incr i
			lappend inh $family
			#puts "Birth $family"
		} else {
			lappend parents $i
			#If we are looking at an old cell, it could be one of two things
			#If the ancestor kept living, it is the ancestor
			#puts "H $i $inh"
			set ageDiff [expr [lindex $currentGen $i] - [lindex $lastGen [llength $inh]]]
			#puts "T"
			#puts [llength $lastGen]
			#puts [llength $lastGenPositions]
			#puts [llength $inh]
			#puts "HereA: $ageDiff"
			set positionDiff [veclength [vecsub $position [lindex $lastGenPositions [llength $inh]]]]
			#puts "HereP: $positionDiff"
			#puts "Age difference $ageDiff"
			#We include a further test here to make sure that we don't miss a death and missassign an inheritance
			#puts "$ageDiff $positionDiff"
			if { ( ([expr round($ageDiff)] == [expr round($dT)]) && ($positionDiff < 2.5) ) } {
				lappend inh $i
				#puts "Life $i"
			} else {
				#If the ancestor died, we record an empty list and try again
				lappend inh []
				set i [expr $i - 1]
				#puts "Death $i"
			}
		}
	}
	if {[llength $inh] != [llength $lastGen]} {
		puts "ERROR generating map"
		set gL [llength $inh]
		set lGL [llength $lastGen]
		puts "Last gen = $lGL ; Map = $gL"
	}
	#puts "Completed genmap"
	return $inh

}

proc inferAllAncestors { ids f dT } {
	global molid
	set nf [molinfo $molid get numframes]
	set follow []
	set lastGen []
	set currentGen []
	set compSet "user > 0"
	foreach id $ids { lappend follow $id }
	for {set i 0} {$i < $nf} {incr i} {
		set fl [llength $follow]
		puts "Frame $i"
		if {$i < $f} { 
			zeroUser4 $i
		} elseif {$i == $f} {
			zeroUser4 $i
			set marker 0
			for {set j 0} {$j < [llength $follow]} {incr j} {
				incr marker
				set id [lindex $follow $j]
				set atom [atomselect $molid "index $id" frame $i]
				$atom set user4 $marker
				$atom delete
				}
			set ageAtoms [atomselect $molid "$compSet" frame $i]
		        set lastGen [$ageAtoms get user3]
	       		$ageAtoms delete	       
		} else {

			zeroUser4 $i
			#If everyones dead, don't do anything else
			if { [llength $follow] == 0 } { continue } 
			set ageAtoms [atomselect $molid "$compSet" frame $i]
			set currentGen [$ageAtoms get user3]
			#inheritance is a list of lists, each element with the ids of the progeny
			set inheritance [genMap $lastGen $currentGen $dT]
			set lastGen $currentGen

			set newFollow []
			set marker 0
			#Now- follow is a list of lists
			#for each list (cellLine) in follow, we need to find their progeny and update that list
			foreach cellLine $follow {
				incr marker
				#each cellLine is a set of cells
				set localNewFollow []
				#puts $cellLine
				foreach iCell $cellLine {	
					set cell [lindex $inheritance $iCell]
					#puts "$iCell -> $cell"
					if { [llength $cell] == 0 } {
						#I'm dead
						continue
					} elseif { [llength $cell] == 1 } {
						#I've survived
						set index [lindex $cell 0]
						set atom [atomselect $molid "index $index" frame $i]
				       		$atom set user4 $marker
						$atom delete
						lappend localNewFollow $index
					} elseif { [llength $cell] == 2 } {
						#I've divided
						#puts "Div"
						set i1 [lindex $cell 0]
						set i2 [lindex $cell 1]

						set atom [atomselect $molid "index $i1 $i2" frame $i]
						$atom set user4 $marker
						$atom delete
						lappend localNewFollow $i1
						lappend localNewFollow $i2
					} else {
						puts "ERROR-Too many Children"
						puts $cell
					}
				#lappend newFollow $localNewFollow
				#puts $localNewFollow
				#unset localNewFollow
				}
			lappend newFollow $localNewFollow
			#puts $localNewFollow
			unset localNewFollow
			}
			#puts $follow
			#puts $newFollow
			unset follow
			set follow $newFollow
			unset newFollow
			unset inheritance
		}
	}
}


proc inferAncestors { id f dT } {
	global molid
	set nf [molinfo $molid get numframes]
	set follow []
	set lastGen []
	set currentGen []
	lappend follow $id
	set compSet "user > 0"
	for {set i 0} {$i < $nf} {incr i} {
		set fl [llength $follow]
		puts "Frame $i -> Following $fl cells\r"
		if {$i < $f} {
		       zeroUser4 $i
	       	       continue 
		} elseif { $i == $f } {
			zeroUser4 $i
			set atom [atomselect $molid "index $id" frame $i]
			$atom set user4 1.0
			$atom delete
			set ageAtoms [atomselect $molid "$compSet" frame $i]
		        set lastGen [$ageAtoms get user3]
	       		$ageAtoms delete	       
		} else {
			zeroUser4 $i
			#If everyones dead, don't do anything else
			if { [llength $follow] == 0 } { continue } 
			set ageAtoms [atomselect $molid "$compSet" frame $i]
			set currentGen [$ageAtoms get user3]
			#inheritance is a list of lists, each element with the ids of the progeny
			set inheritance [genMap $lastGen $currentGen $dT]
			set lastGen $currentGen
			set newFollow []
			foreach iCell $follow {
				set cell [lindex $inheritance $iCell]
				if { [llength $cell] == 0 } {
					#I'm dead
					continue
				} elseif { [llength $cell] == 1 } {
					#I've survived
					set index [lindex $cell 0]
					set atom [atomselect $molid "index $index" frame $i]
				        $atom set user4 1.0
					$atom delete
					lappend newFollow $index
				} elseif { [llength $cell] == 2 } {
					#I've divided
					set i1 [lindex $cell 0]
					set i2 [lindex $cell 1]
					set atom [atomselect $molid "index $i1 $i2" frame $i]
					$atom set user4 1.0
					$atom delete
					lappend newFollow $i1
					lappend newFollow $i2
				} else {
					puts "ERROR- too many children"
				}
			}
			unset follow
			set follow $newFollow
			unset newFollow
			unset inheritance
		}

 }
} 

proc dynamicPhylogenyAncestors { id f dT } {
	global molid
	set nf [molinfo $molid get numframes]
	set follow []
	set lastGen []
	set currentGen []
	set problems 0
	
	lappend follow $id


	set compSet "user > 0"
	for {set i 0} {$i < $nf} {incr i} {
		set fl [llength $follow]
		puts "Frame $i -> Following $fl cells ($follow)\r"

		if {$i < $f} {
	       	       continue 
		} elseif { $i == $f } {
			set atom [atomselect $molid "index $id" frame $i]
			set cart [lindex [$atom get {x y z}] 0]
			graphics top sphere $cart radius 0.2
			$atom delete
			set ageAtoms [atomselect $molid "$compSet" frame $i]
		        set lastGen [$ageAtoms get user3]
			set lastGenPos [$ageAtoms get {x y z}]
	       		$ageAtoms delete	       
		} else {
			#If everyones dead, don't do anything else
			if { [llength $follow] == 0 } { continue } 
			set ageAtoms [atomselect $molid "$compSet" frame $i]
			set currentGen [$ageAtoms get user3]
			set currentGenPos [$ageAtoms get {x y z}]
			#inheritance is a list of lists, each element with the ids of the progeny
			set inheritance [genMapP $lastGen $currentGen $dT $lastGenPos $currentGenPos]
			set lastGen $currentGen
			set lastGenPos $currentGenPos
			set newFollow []

			foreach iCell $follow {
				set cell [lindex $inheritance $iCell]
				if { [llength $cell] == 0 } {
					#I'm dead
					set oldFrame [expr $i - 1]
					set oldAtom [atomselect $molid "index $iCell" frame $oldFrame]
					set oldCart [lindex [$oldAtom get {x y z}] 0]
					$oldAtom delete
					graphics top color 1
					graphics top sphere $oldCart radius 0.4
					graphics top color 0
					#puts "$iCell died $oldCart"
					continue
				} elseif { [llength $cell] == 1 } {
					#I've survived
					set index [lindex $cell 0]
					set atom [atomselect $molid "index $index" frame $i]
					set cart [lindex [$atom get {x y z}] 0]
					set oldFrame [expr $i - 1]
					set oldAtom [atomselect $molid "index $iCell" frame $oldFrame]
					set oldCart [lindex [$oldAtom get {x y z}] 0]
					if {[veclength [vecsub $cart $oldCart]] > 10 } {
						puts "$iCell -> $index is too long"
						puts "$oldFrame -> $i"
						puts "$oldCart -> $cart"
						puts "The order has been mixed up between siblings"
						incr problems
						break
						}

					graphics top line $cart $oldCart
					$atom delete
					$oldAtom delete
					#puts "$iCell survived (now $index)"

					lappend newFollow $index
				} elseif { [llength $cell] == 2 } {
					#I've divided
					set i1 [lindex $cell 0]
					set i2 [lindex $cell 1]
					set atom1 [atomselect $molid "index $i1" frame $i]
					set cart1 [lindex [$atom1 get {x y z}] 0]
					$atom1 delete
					set atom2 [atomselect $molid "index $i2" frame $i]
					set cart2 [lindex [$atom2 get {x y z}] 0]
					$atom2 delete
					lappend newFollow $i1
					lappend newFollow $i2
					#puts "$iCell divided into $i1 $i2"
					set oldFrame [expr $i - 1]
					set oldAtom [atomselect $molid "index $iCell" frame $oldFrame]
					set oldCart [lindex [$oldAtom get {x y z}] 0]
					$oldAtom delete
					
					graphics top sphere $oldCart radius 0.4

					graphics top line $cart1 $oldCart
					graphics top line $cart2 $oldCart
					
				} else {
					puts "ERROR- too many children"
				}
			}
			unset follow
			set follow $newFollow
			unset newFollow
			unset inheritance
			if {$problems > 0} {
				break
				}
		}

 }
}

proc selectionPressure { selection } {
	global molid
	set nf [molinfo $molid get numframes]
	set outfile [open "pressures.txt" w]
	for {set i 0} {$i < $nf} {incr i} {
		set atoms [atomselect $molid "$selection" frame $i]
		set pressures [$atoms get user2]
		set sum 0
		foreach p $pressures {
			set sum [expr $sum + $p]
			}
		set mean [expr $sum/[llength $pressures]]
		puts $outfile "$i $mean"
		$atoms delete
	}
	close $outfile
}

proc deathPlot { id f dT} {
	global molid
	set nf [molinfo $molid get numframes]
	set follow []
	set lastGen []
	set currentGen []
	set problems 0
	lappend follow $id
	set compSet "user > 0"
	for {set i 0} {$i < $nf} {incr i} {
		set fl [llength $follow]
		puts "Frame $i -> Following $fl cells ($follow)\r"

		if {$i < $f} {
	       	       continue 
		} elseif { $i == $f } {
			set atom [atomselect $molid "index $id" frame $i]
			set cart [lindex [$atom get {x y z}] 0]
			graphics top sphere $cart radius 0.2
			$atom delete
			set ageAtoms [atomselect $molid "$compSet" frame $i]
		        set lastGen [$ageAtoms get user3]
			set lastGenPos [$ageAtoms get {x y z}]
	       		$ageAtoms delete	       
		} else {
			#If everyones dead, don't do anything else
			if { [llength $follow] == 0 } { continue } 
			set ageAtoms [atomselect $molid "$compSet" frame $i]
			set currentGen [$ageAtoms get user3]
			set currentGenPos [$ageAtoms get {x y z}]
			#inheritance is a list of lists, each element with the ids of the progeny
			set inheritance [genMapP $lastGen $currentGen $dT $lastGenPos $currentGenPos]
			set lastGen $currentGen
			set lastGenPos $currentGenPos
			set newFollow []
			foreach iCell $follow {
				set cell [lindex $inheritance $iCell]
				if { [llength $cell] == 0 } {
					#I'm dead
					set oldFrame [expr $i - 1]
					set oldAtom [atomselect $molid "index $iCell" frame $oldFrame]
					set oldCart [lindex [$oldAtom get {x y z}] 0]
					$oldAtom delete
					graphics top color 1
					graphics top sphere $oldCart radius 0.2
					graphics top color 0
					#puts "$iCell died $oldCart"
					continue
				} elseif { [llength $cell] == 1 } {
					#I've survived
					set index [lindex $cell 0]
					lappend newFollow $index
				} elseif { [llength $cell] == 2 } {
					#I've divided
					set i1 [lindex $cell 0]
					set i2 [lindex $cell 1]
					lappend newFollow $i1
					lappend newFollow $i2
				} else {
					puts "ERROR- too many children"
				}
			}
			unset follow
			set follow $newFollow
			unset newFollow
			unset inheritance
			if {$problems > 0} {
				break
				}
		}

	}

}

proc deathLoc { id f dT} {
	global molid
	set nf [molinfo $molid get numframes]
	set follow []
	set lastGen []
	set currentGen []
	set problems 0
	set oup [open "death_location.txt" "w"]
	lappend follow $id
	set compSet "user > 0"
	for {set i 0} {$i < $nf} {incr i} {
		set fl [llength $follow]
		puts "Frame $i -> Following $fl cells ($follow)\r"

		if {$i < $f} {
	       	       continue 
		} elseif { $i == $f } {
			set atom [atomselect $molid "index $id" frame $i]
			set cart [lindex [$atom get {x y z}] 0]
			graphics top sphere $cart radius 0.2
			$atom delete
			set ageAtoms [atomselect $molid "$compSet" frame $i]
		        set lastGen [$ageAtoms get user3]
			set lastGenPos [$ageAtoms get {x y z}]
	       		$ageAtoms delete	       
		} else {
			#If everyones dead, don't do anything else
			if { [llength $follow] == 0 } { continue } 
			set ageAtoms [atomselect $molid "$compSet" frame $i]
			set currentGen [$ageAtoms get user3]
			set currentGenPos [$ageAtoms get {x y z}]
			#inheritance is a list of lists, each element with the ids of the progeny
			set inheritance [genMapP $lastGen $currentGen $dT $lastGenPos $currentGenPos]
			set lastGen $currentGen
			set lastGenPos $currentGenPos
			set newFollow []
			foreach iCell $follow {
				set cell [lindex $inheritance $iCell]
				if { [llength $cell] == 0 } {
					#I'm dead
					set oldFrame [expr $i - 1]
					set oldAtom [atomselect $molid "index $iCell" frame $oldFrame]
					set oldCart [lindex [$oldAtom get {x y z}] 0]
					$oldAtom delete
					graphics top color 1
					#graphics top sphere $oldCart radius 0.2
					puts $oup "$oldCart" 
					graphics top color 0
					#puts "$iCell died $oldCart"
					continue
				} elseif { [llength $cell] == 1 } {
					#I've survived
					set index [lindex $cell 0]
					lappend newFollow $index
				} elseif { [llength $cell] == 2 } {
					#I've divided
					set i1 [lindex $cell 0]
					set i2 [lindex $cell 1]
					lappend newFollow $i1
					lappend newFollow $i2
				} else {
					puts "ERROR- too many children"
				}
			}
			unset follow
			set follow $newFollow
			unset newFollow
			unset inheritance
			if {$problems > 0} {
				break
				}
		}

	}
	close $oup

}

proc selectionPressure { sel fname } {
	global molid
	set numframes [molinfo $molid get numframes]
	set oup [open $fname "w"]
	for {set i 0} {$i < $numframes} {incr i} {
		set cells [atomselect $molid "$sel" frame $i]
		set info [$cells get {x y z user2}]
		foreach cell $info {
			puts $oup "$cell"
			}
		
		$cells delete
		}
	close $oup
}

proc selectionAge { sel fname } {
	global molid
	set numframes [molinfo $molid get numframes]
	set oup [open $fname "w"]
	for {set i 0} {$i < $numframes} {incr i} {
		set cells [atomselect $molid "$sel" frame $i]
		set info [$cells get {x y z user3}]
		foreach cell $info {
			puts $oup "$cell"
			}
		
		$cells delete
		}
	close $oup
}

proc showConfluence { } {
	global molid
	set numframes [molinfo $molid get numframes]
	set cells [atomselect $molid "all" frame $i]
	set info [$cells get index]	
	$cells delete
	for {set i 0} {$i < $numframes} {incr i} {
		puts $i
		foreach id $info {
			set icell [atomselect $molid "index $id" frame $i]
			$icell set user4 [$icell get vx]
			$icell delete
			}

		}
}
