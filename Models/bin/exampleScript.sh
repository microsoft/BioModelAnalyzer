#/bin/bash

###################################################################
############ FUNCTION DEFINITIONS #################################
###################################################################
accumulateResultsInCSV() {
    local arrayOfFiles=(CompilationAllResult.txt CompilationAllResult.txt \
	CompilationTestResult.txt CompilationTestResult.txt 
	TestResult.txt TestResult.txt TestResult.txt TestResult.txt \
	    TestResult.txt TestResult.txt TestResult.txt)
    local arrayOfWords=(error warning error warning "AckermannRun run OK" \
	"AckermannRun passed" "Wrong protection of array bounds" \
	"Wrong initialization" \
	"Values were computed in wrong locations" \
	"Value is wrong" "Test Dyn Mem Failed")
    
    local result="$1"
    for i in {0..10}
    do
	local fileName=${arrayOfFiles[$i]}
	local word=${arrayOfWords[$i]}
	echo "The file is $fileName"
	echo "The word is $word"
	if grep -i "$word" $fileName 
	then
		result="$result,1"
	else
		result="$result,0"
	fi
    done
    
    echo $result > $1.csv
}

headerOfFile() {
	echo "Student,Compile All,Compile All,Compile Test,Compile Test,Ackermann Run,Ackermann Run,Test Init,Test Init,Test Values,Test Values,Test Dynamic" >> students.csv
	echo ",Error,Warning,Error,Warning,Run Fine,Same Result,Bound Protect,Initialize,Wrong Locations,Wrong Values,Dynamic Mem" >> students.csv
}
###################################################################
############ MAIN PROGRAM #########################################
###################################################################

for i in *; do
	echo $i;
	cd $i;

	# Copy the test files and the makefile
	cp ../../TestCode/Test*.cpp .;
	cp ../../Makefile .;

	# Make the binary and the test binaries
	make All > CompilationAllResult.txt;
	make Test > CompilationTestResult.txt;

	# Run the test code
	../../TestCode/testIndividual.sh > TestResult.txt;

	# Clean
	make clean
	make deepclean

	# Create a PDF file with all the code and the results
	mkdir scrapdir
	for file in  Ackermann.cpp Ackermann.hpp AckermannRun.cpp \
			CompilationAllResult.txt CompilationTestResult.txt \
			TestResult.txt
	do
		enscript --color -GE -U2 $file -p scrapdir/$file.ps
		ps2pdf scrapdir/$file.ps scrapdir/$file.pdf
	done
	gs -dBATCH -dNOPAUSE -q -sDEVICE=pdfwrite -sOutputFile=$i.pdf \
		scrapdir/Ackermann.cpp.pdf scrapdir/Ackermann.hpp.pdf \
		scrapdir/AckermannRun.cpp.pdf \
		scrapdir/CompilationAllResult.txt.pdf \
		scrapdir/CompilationTestResult.txt.pdf \
		scrapdir/TestResult.txt.pdf

	cd scrapdir
	rm -Rf *
	cd ..
	rmdir scrapdir

	accumulateResultsInCSV $i

	# Return to original directory
	cd ..
done

# Create a csv file with all the accumulated results of all
# the students
headerOfFile
for i in *; do
	if [ "$i" != "students.csv" ]
	then
		cat $i/$i.csv >> students.csv
	fi
done
