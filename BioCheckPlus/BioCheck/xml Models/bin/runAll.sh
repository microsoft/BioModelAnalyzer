#/bin/bash

cd ..

ulimit -t 1200

for length in "10" "20" "30" "40" "50"
do
    echo "Working on length $length"
    for i in *
    do
	if [ -d $i ]
	then 
	    echo "$i is a directory"
	else
	    echo "Running on $i"
	    
#	echo "$i $length"
	    if [ -f results/"$i.loop_$length.naive.txt" ]
	    then
		echo "file results/$i.loop_$length.naive.txt exists"
	    else
		../bin/Debug/BioCheck.exe -file "$i" -path "$length" -naive \
		    > results/"$i.loop_$length.naive.txt"
	    fi

	    if [ -f results/"$i.loop_$length.txt" ]
	    then
		echo "file results/$i.loop_$length.txt exists"
	    else
	    ../bin/Debug/BioCheck.exe -file "$i" -path "$length" \
		> results/"$i.loop_$length.txt"
	    fi
	fi
    done
done

cd bin
