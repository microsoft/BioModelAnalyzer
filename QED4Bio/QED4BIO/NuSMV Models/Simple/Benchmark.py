import sys, subprocess, os

filename = sys.argv[1]

number_cells = int(sys.argv[2])

def find_cex(predicate,outfile,marker,filename,old_predicates):
	#print "Testing predicate", predicate
	'''
	job = subprocess.Popen("/cygdrive/c/Program\ Files/NuSMV/2.5.4/bin/NuSMV.exe -int", stdin=subprocess.PIPE, stdout=subprocess.PIPE, stderr=subprocess.PIPE, shell=True)
	read_model = 'read_model -i %s;' % ( filename )
	job.stdin.write(read_model)
	job.stdin.write('go;')
	ltlcommand = 'check_ctlspec -o %s -p "AG(%s)"' % ( outfile, predicate )
	job.stdin.write(ltlcommand)
	job.communicate()
	'''
	#Create predicate file
	with open('testing_predicate.txt','w') as pred:
		read_model = 'read_model -i %s;' % ( filename )
		print >> pred, read_model
		print >> pred, 'go;'
		for item in old_predicates:
			print >> pred, item
		ctlcommand = 'check_ctlspec -o %s -p "AG(%s)"' % ( outfile, predicate )
		print >> pred, ctlcommand
		print >> pred, 'quit;'
		#job = subprocess.Popen("/cygdrive/c/Program\ Files/NuSMV/2.5.4/bin/NuSMV.exe -load testing_predicate.txt", stdin=subprocess.PIPE, stdout=subprocess.PIPE, stderr=subprocess.PIPE, shell=True)
		#job.communicate()
	os.system('time /cygdrive/c/Program\ Files/NuSMV/2.5.4/bin/NuSMV.exe -load testing_predicate.txt')

	with open(outfile) as result:
		line = result.next()
		if "true" in line:
			return (False, {}, ctlcommand)
		pattern = {}
		for line in result:
			if marker in line:
				fate = line.split('=')
				name = fate[0].split('.')
				pattern[name[0].strip()] = fate[1].strip()
		return (True, pattern, ctlcommand)

def generate_outcome(cell,prop,fate):
	outcome = "%s.%s=%s" % (cell,prop,fate)
	return outcome
	
base_predicate = '((p1.path<4&p1.path>0)'
for i in range(number_cells-1):
	base_predicate += '|(p'+str(i+2)+'.path<4&p'+str(i+2)+'.path>0)'
base_predicate += ')'

tested_predicates = []

[cex, pattern, ctl] = find_cex(base_predicate, 'sane_000.txt', 'path', filename, tested_predicates)

#print pattern
cells = ['p'+str(item+1) for item in range(number_cells)]
if not cex:
	print "Complete with no cex"
else:
	print "Found a cex. Further testing..."
	tested_predicates.append(ctl)
	print cells[0], ':', pattern[cells[0]],
	for item in cells[1:]:
		print ',', item, ':', pattern[item], 
	print ""

def generate_result(pattern, prop):
	cells = pattern.keys()
	pred = "(%s.%s=%s" % (cells[0], prop, pattern[cells[0]])
	for item in cells[1:]:
		cpred = " %s.%s=%s" % (item, prop, pattern[item])
		pred += ' & ' + cpred
	pred += ')'
	return pred

fate_number = 0

while(cex):
	base_predicate += ' | ' + generate_result(pattern, 'path')
	#print base_predicate
	fate_number += 1
	outfilename = "fate_%03d.txt" % ( fate_number )
	[cex, pattern, ctl] = find_cex(base_predicate, outfilename, 'path', filename, tested_predicates)
	if cex:
		print 'Found a cex. Further testing...'
		tested_predicates.append(ctl)
		print cells[0], ':', pattern[cells[0]],
		for item in cells[1:]:
			print ',', item, ':', pattern[item],
		print ''
	else:
		print 'Complete following exclusion of cex'
