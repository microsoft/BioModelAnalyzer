#/bin/bash

cd ..

ulimit -t 1200


# Run an increasing path search for all models

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


# Run LTL model checking for a few models

# VPC stabilizing property 1

../bin/Debug/BioCheck.exe -path 100 -mc -file VPC_stabilizingAnalysisInput.xml -path 13 -formula "(Implies  (And (And (And (And (And (And (And (And (And (And (And (< v3 2) (> v3 0)) (And (< v4 2) (> v4 0))) (And (< v5 2) (> v5 0))) (And (< v6 2) (> v6 0))) (And (> v7 0) (< v7 2))) (And (> v8 0) (< v8 2))) (And (> v10 0) (< v10 2))) (< v9 1)) (And (> v2 0) (< v2 2))) (And (And (And (And (And (And (And (And (And (And (< v3 2) (> v3 0)) (And (< v4 2) (> v4 0))) (And (< v5 2) (> v5 0))) (And (< v6 2) (> v6 0))) (And (> v7 0) (< v7 2))) (And (> v8 0) (< v8 2))) (And (> v10 0) (< v10 2))) (< v9 1)) (And (> v2 0) (< v2 2))) (And (And (< v77 1) (< v23 1)) (And (< v89 1) (And (And (> v78 0) (< v78 2)) (And (And (> v24 0) (< v24 2)) (And (> v90 0) (< v90 2)))))))) (And (And (And (And (And (And (And (And (And (And (< v3 2) (> v3 0)) (And (< v4 2) (> v4 0))) (And (< v5 2) (> v5 0))) (And (< v6 2) (> v6 0))) (And (> v7 0) (< v7 2))) (And (> v8 0) (< v8 2))) (And (> v10 0) (< v10 2))) (< v9 1)) (And (> v2 0) (< v2 2))) (And (And (And (And (And (And (And (And (And (And (< v3 2) (> v3 0)) (And (< v4 2) (> v4 0))) (And (< v5 2) (> v5 0))) (And (< v6 2) (> v6 0))) (And (> v7 0) (< v7 2))) (And (> v8 0) (< v8 2))) (And (> v10 0) (< v10 2))) (< v9 1)) (And (> v2 0) (< v2 2))) (And (And (< v77 1) (< v23 1)) (And (< v89 1) (And (And (> v78 0) (< v78 2)) (And (And (> v24 0) (< v24 2)) (And (> v90 0) (< v90 2))))))))) (Until (Not (> v23 1)) (< v2 1)))" > results/VPC_stabilizing1.txt


../bin/Debug/BioCheck.exe -naive -path 100 -mc -file VPC_stabilizingAnalysisInput.xml -path 13 -formula "(Implies  (And (And (And (And (And (And (And (And (And (And (And (< v3 2) (> v3 0)) (And (< v4 2) (> v4 0))) (And (< v5 2) (> v5 0))) (And (< v6 2) (> v6 0))) (And (> v7 0) (< v7 2))) (And (> v8 0) (< v8 2))) (And (> v10 0) (< v10 2))) (< v9 1)) (And (> v2 0) (< v2 2))) (And (And (And (And (And (And (And (And (And (And (< v3 2) (> v3 0)) (And (< v4 2) (> v4 0))) (And (< v5 2) (> v5 0))) (And (< v6 2) (> v6 0))) (And (> v7 0) (< v7 2))) (And (> v8 0) (< v8 2))) (And (> v10 0) (< v10 2))) (< v9 1)) (And (> v2 0) (< v2 2))) (And (And (< v77 1) (< v23 1)) (And (< v89 1) (And (And (> v78 0) (< v78 2)) (And (And (> v24 0) (< v24 2)) (And (> v90 0) (< v90 2)))))))) (And (And (And (And (And (And (And (And (And (And (< v3 2) (> v3 0)) (And (< v4 2) (> v4 0))) (And (< v5 2) (> v5 0))) (And (< v6 2) (> v6 0))) (And (> v7 0) (< v7 2))) (And (> v8 0) (< v8 2))) (And (> v10 0) (< v10 2))) (< v9 1)) (And (> v2 0) (< v2 2))) (And (And (And (And (And (And (And (And (And (And (< v3 2) (> v3 0)) (And (< v4 2) (> v4 0))) (And (< v5 2) (> v5 0))) (And (< v6 2) (> v6 0))) (And (> v7 0) (< v7 2))) (And (> v8 0) (< v8 2))) (And (> v10 0) (< v10 2))) (< v9 1)) (And (> v2 0) (< v2 2))) (And (And (< v77 1) (< v23 1)) (And (< v89 1) (And (And (> v78 0) (< v78 2)) (And (And (> v24 0) (< v24 2)) (And (> v90 0) (< v90 2))))))))) (Until (Not (> v23 1)) (< v2 1)))" > results/VPC_stabilizing1.naive.txt

# VPC stabilizing property 2

../bin/Debug/BioCheck.exe -outputmodel -file VPC_stabilizingAnalysisInput.xml -formula "(And (And (And (And (And (And (And (And (And (And (And (And (And (< v3 2) (> v3 0)) (And (< v4 2) (> v4 0))) (And (< v5 2) (> v5 0))) (And (< v6 2) (> v6 0))) (And (> v7 0) (< v7 2))) (And (> v8 0) (< v8 2))) (And (> v10 0) (< v10 2))) (< v9 1)) (And (> v2 0) (< v2 2))) (And (And (And (And (And (And (And (And (And (And (< v3 2) (> v3 0)) (And (< v4 2) (> v4 0))) (And (< v5 2) (> v5 0))) (And (< v6 2) (> v6 0))) (And (> v7 0) (< v7 2))) (And (> v8 0) (< v8 2))) (And (> v10 0) (< v10 2))) (< v9 1)) (And (> v2 0) (< v2 2))) (And (And (< v77 1) (< v23 1)) (And (< v89 1) (And (And (> v78 0) (< v78 2)) (And (And (> v24 0) (< v24 2)) (And (> v90 0) (< v90 2)))))))) (And (And (And (And (And (And (And (And (And (And (< v3 2) (> v3 0)) (And (< v4 2) (> v4 0))) (And (< v5 2) (> v5 0))) (And (< v6 2) (> v6 0))) (And (> v7 0) (< v7 2))) (And (> v8 0) (< v8 2))) (And (> v10 0) (< v10 2))) (< v9 1)) (And (> v2 0) (< v2 2))) (And (And (And (And (And (And (And (And (And (And (< v3 2) (> v3 0)) (And (< v4 2) (> v4 0))) (And (< v5 2) (> v5 0))) (And (< v6 2) (> v6 0))) (And (> v7 0) (< v7 2))) (And (> v8 0) (< v8 2))) (And (> v10 0) (< v10 2))) (< v9 1)) (And (> v2 0) (< v2 2))) (And (And (< v77 1) (< v23 1)) (And (< v89 1) (And (And (> v78 0) (< v78 2)) (And (And (> v24 0) (< v24 2)) (And (> v90 0) (< v90 2))))))))) (Eventually (Always (And (> v7 1) (< v51 1))))) (And (Eventually (And (> v7 1) (Next (Eventually (And (< v2 1) (Next (Eventually (> v77 1)))))))) (Eventually (And (> v51 1) (Next (Eventually (And (> v77 1) (Next (Eventually (> v46 1))))))))))" > results/VPC_stabilizing2.txt 

../bin/Debug/BioCheck.exe -naive -path 16 -outputmodel -file VPC_stabilizingAnalysisInput.xml -formula "(And (And (And (And (And (And (And (And (And (And (And (And (And (< v3 2) (> v3 0)) (And (< v4 2) (> v4 0))) (And (< v5 2) (> v5 0))) (And (< v6 2) (> v6 0))) (And (> v7 0) (< v7 2))) (And (> v8 0) (< v8 2))) (And (> v10 0) (< v10 2))) (< v9 1)) (And (> v2 0) (< v2 2))) (And (And (And (And (And (And (And (And (And (And (< v3 2) (> v3 0)) (And (< v4 2) (> v4 0))) (And (< v5 2) (> v5 0))) (And (< v6 2) (> v6 0))) (And (> v7 0) (< v7 2))) (And (> v8 0) (< v8 2))) (And (> v10 0) (< v10 2))) (< v9 1)) (And (> v2 0) (< v2 2))) (And (And (< v77 1) (< v23 1)) (And (< v89 1) (And (And (> v78 0) (< v78 2)) (And (And (> v24 0) (< v24 2)) (And (> v90 0) (< v90 2)))))))) (And (And (And (And (And (And (And (And (And (And (< v3 2) (> v3 0)) (And (< v4 2) (> v4 0))) (And (< v5 2) (> v5 0))) (And (< v6 2) (> v6 0))) (And (> v7 0) (< v7 2))) (And (> v8 0) (< v8 2))) (And (> v10 0) (< v10 2))) (< v9 1)) (And (> v2 0) (< v2 2))) (And (And (And (And (And (And (And (And (And (And (< v3 2) (> v3 0)) (And (< v4 2) (> v4 0))) (And (< v5 2) (> v5 0))) (And (< v6 2) (> v6 0))) (And (> v7 0) (< v7 2))) (And (> v8 0) (< v8 2))) (And (> v10 0) (< v10 2))) (< v9 1)) (And (> v2 0) (< v2 2))) (And (And (< v77 1) (< v23 1)) (And (< v89 1) (And (And (> v78 0) (< v78 2)) (And (And (> v24 0) (< v24 2)) (And (> v90 0) (< v90 2))))))))) (Eventually (Always (And (> v7 1) (< v51 1))))) (And (Eventually (And (> v7 1) (Next (Eventually (And (< v2 1) (Next (Eventually (> v77 1)))))))) (Eventually (And (> v51 1) (Next (Eventually (And (> v77 1) (Next (Eventually (> v46 1))))))))))" > results/VPC_stabilizing2.naive.txt

# VPC non stabilizing property 1

../bin/Debug/BioCheck.exe -path 100 -outputmodel -file VPC_Non_stabilizingAnalysisInput.xml -formula "(And (And (And (And (And (And (And (And (And (And (And (And (And (And (< v3 2) (> v3 0)) (And (< v4 2) (> v4 0))) (And (< v5 2) (> v5 0))) (And (< v6 2) (> v6 0))) (And (> v7 0) (< v7 2))) (And (> v8 0) (< v8 2))) (And (> v10 0) (< v10 2))) (< v9 1)) (And (> v2 0) (< v2 2))) (And (And (And (And (And (And (And (And (And (And (< v3 2) (> v3 0)) (And (< v4 2) (> v4 0))) (And (< v5 2) (> v5 0))) (And (< v6 2) (> v6 0))) (And (> v7 0) (< v7 2))) (And (> v8 0) (< v8 2))) (And (> v10 0) (< v10 2))) (< v9 1)) (And (> v2 0) (< v2 2))) (And (And (< v77 1) (< v23 1)) (And (< v89 1) (And (And (> v78 0) (< v78 2)) (And (And (> v24 0) (< v24 2)) (And (> v90 0) (< v90 2)))))))) (And (And (And (And (And (And (And (And (And (And (< v3 2) (> v3 0)) (And (< v4 2) (> v4 0))) (And (< v5 2) (> v5 0))) (And (< v6 2) (> v6 0))) (And (> v7 0) (< v7 2))) (And (> v8 0) (< v8 2))) (And (> v10 0) (< v10 2))) (< v9 1)) (And (> v2 0) (< v2 2))) (And (And (And (And (And (And (And (And (And (And (< v3 2) (> v3 0)) (And (< v4 2) (> v4 0))) (And (< v5 2) (> v5 0))) (And (< v6 2) (> v6 0))) (And (> v7 0) (< v7 2))) (And (> v8 0) (< v8 2))) (And (> v10 0) (< v10 2))) (< v9 1)) (And (> v2 0) (< v2 2))) (And (And (< v77 1) (< v23 1)) (And (< v89 1) (And (And (> v78 0) (< v78 2)) (And (And (> v24 0) (< v24 2)) (And (> v90 0) (< v90 2))))))))) (Eventually (Always (And (> v7 1) (> v51 1))))) (Until (Not (< v2 1)) (> v23 1))) (Until (Not (< v46 1)) (> v77 1)))" > results/VPC_non_stabilizing1.txt 

../bin/Debug/BioCheck.exe -naive -path 100 -outputmodel -file VPC_Non_stabilizingAnalysisInput.xml -formula "(And (And (And (And (And (And (And (And (And (And (And (And (And (And (< v3 2) (> v3 0)) (And (< v4 2) (> v4 0))) (And (< v5 2) (> v5 0))) (And (< v6 2) (> v6 0))) (And (> v7 0) (< v7 2))) (And (> v8 0) (< v8 2))) (And (> v10 0) (< v10 2))) (< v9 1)) (And (> v2 0) (< v2 2))) (And (And (And (And (And (And (And (And (And (And (< v3 2) (> v3 0)) (And (< v4 2) (> v4 0))) (And (< v5 2) (> v5 0))) (And (< v6 2) (> v6 0))) (And (> v7 0) (< v7 2))) (And (> v8 0) (< v8 2))) (And (> v10 0) (< v10 2))) (< v9 1)) (And (> v2 0) (< v2 2))) (And (And (< v77 1) (< v23 1)) (And (< v89 1) (And (And (> v78 0) (< v78 2)) (And (And (> v24 0) (< v24 2)) (And (> v90 0) (< v90 2)))))))) (And (And (And (And (And (And (And (And (And (And (< v3 2) (> v3 0)) (And (< v4 2) (> v4 0))) (And (< v5 2) (> v5 0))) (And (< v6 2) (> v6 0))) (And (> v7 0) (< v7 2))) (And (> v8 0) (< v8 2))) (And (> v10 0) (< v10 2))) (< v9 1)) (And (> v2 0) (< v2 2))) (And (And (And (And (And (And (And (And (And (And (< v3 2) (> v3 0)) (And (< v4 2) (> v4 0))) (And (< v5 2) (> v5 0))) (And (< v6 2) (> v6 0))) (And (> v7 0) (< v7 2))) (And (> v8 0) (< v8 2))) (And (> v10 0) (< v10 2))) (< v9 1)) (And (> v2 0) (< v2 2))) (And (And (< v77 1) (< v23 1)) (And (< v89 1) (And (And (> v78 0) (< v78 2)) (And (And (> v24 0) (< v24 2)) (And (> v90 0) (< v90 2))))))))) (Eventually (Always (And (> v7 1) (> v51 1))))) (Until (Not (< v2 1)) (> v23 1))) (Until (Not (< v46 1)) (> v77 1)))" > results/VPC_non_stabilizing1.naive.txt

# VPC non stabilizing property 2

../bin/Debug/BioCheck.exe -path 50 -outputmodel -file VPC_Non_stabilizingAnalysisInput.xml -formula "(And (And (And (And (And (And (And (And (And (And (And (And (And (< v3 2) (> v3 0)) (And (< v4 2) (> v4 0))) (And (< v5 2) (> v5 0))) (And (< v6 2) (> v6 0))) (And (> v7 0) (< v7 2))) (And (> v8 0) (< v8 2))) (And (> v10 0) (< v10 2))) (< v9 1)) (And (> v2 0) (< v2 2))) (And (And (And (And (And (And (And (And (And (And (< v3 2) (> v3 0)) (And (< v4 2) (> v4 0))) (And (< v5 2) (> v5 0))) (And (< v6 2) (> v6 0))) (And (> v7 0) (< v7 2))) (And (> v8 0) (< v8 2))) (And (> v10 0) (< v10 2))) (< v9 1)) (And (> v2 0) (< v2 2))) (And (And (< v77 1) (< v23 1)) (And (< v89 1) (And (And (> v78 0) (< v78 2)) (And (And (> v24 0) (< v24 2)) (And (> v90 0) (< v90 2)))))))) (And (And (And (And (And (And (And (And (And (And (< v3 2) (> v3 0)) (And (< v4 2) (> v4 0))) (And (< v5 2) (> v5 0))) (And (< v6 2) (> v6 0))) (And (> v7 0) (< v7 2))) (And (> v8 0) (< v8 2))) (And (> v10 0) (< v10 2))) (< v9 1)) (And (> v2 0) (< v2 2))) (And (And (And (And (And (And (And (And (And (And (< v3 2) (> v3 0)) (And (< v4 2) (> v4 0))) (And (< v5 2) (> v5 0))) (And (< v6 2) (> v6 0))) (And (> v7 0) (< v7 2))) (And (> v8 0) (< v8 2))) (And (> v10 0) (< v10 2))) (< v9 1)) (And (> v2 0) (< v2 2))) (And (And (< v77 1) (< v23 1)) (And (< v89 1) (And (And (> v78 0) (< v78 2)) (And (And (> v24 0) (< v24 2)) (And (> v90 0) (< v90 2))))))))) (Eventually (Always (And (> v7 1) (> v51 1))))) (And (Eventually (And (> v7 1) (Next (Eventually (And (< v2 1) (Next (Eventually (> v77 1)))))))) (Eventually (And (> v51 1) (Next (Eventually (And (< v46 1) (Next (Eventually (> v23 1))))))))))" > results/VPC_non_stabilizing2.txt

../bin/Debug/BioCheck.exe -naive -path 50 -outputmodel -file VPC_Non_stabilizingAnalysisInput.xml -formula "(And (And (And (And (And (And (And (And (And (And (And (And (And (< v3 2) (> v3 0)) (And (< v4 2) (> v4 0))) (And (< v5 2) (> v5 0))) (And (< v6 2) (> v6 0))) (And (> v7 0) (< v7 2))) (And (> v8 0) (< v8 2))) (And (> v10 0) (< v10 2))) (< v9 1)) (And (> v2 0) (< v2 2))) (And (And (And (And (And (And (And (And (And (And (< v3 2) (> v3 0)) (And (< v4 2) (> v4 0))) (And (< v5 2) (> v5 0))) (And (< v6 2) (> v6 0))) (And (> v7 0) (< v7 2))) (And (> v8 0) (< v8 2))) (And (> v10 0) (< v10 2))) (< v9 1)) (And (> v2 0) (< v2 2))) (And (And (< v77 1) (< v23 1)) (And (< v89 1) (And (And (> v78 0) (< v78 2)) (And (And (> v24 0) (< v24 2)) (And (> v90 0) (< v90 2)))))))) (And (And (And (And (And (And (And (And (And (And (< v3 2) (> v3 0)) (And (< v4 2) (> v4 0))) (And (< v5 2) (> v5 0))) (And (< v6 2) (> v6 0))) (And (> v7 0) (< v7 2))) (And (> v8 0) (< v8 2))) (And (> v10 0) (< v10 2))) (< v9 1)) (And (> v2 0) (< v2 2))) (And (And (And (And (And (And (And (And (And (And (< v3 2) (> v3 0)) (And (< v4 2) (> v4 0))) (And (< v5 2) (> v5 0))) (And (< v6 2) (> v6 0))) (And (> v7 0) (< v7 2))) (And (> v8 0) (< v8 2))) (And (> v10 0) (< v10 2))) (< v9 1)) (And (> v2 0) (< v2 2))) (And (And (< v77 1) (< v23 1)) (And (< v89 1) (And (And (> v78 0) (< v78 2)) (And (And (> v24 0) (< v24 2)) (And (> v90 0) (< v90 2))))))))) (Eventually (Always (And (> v7 1) (> v51 1))))) (And (Eventually (And (> v7 1) (Next (Eventually (And (< v2 1) (Next (Eventually (> v77 1)))))))) (Eventually (And (> v51 1) (Next (Eventually (And (< v46 1) (Next (Eventually (> v23 1))))))))))" > results/VPC_non_stabilizing2.naive.txt

# VPC non stabilizing property 3

../bin/Debug/BioCheck.exe -path 100 -outputmodel -file VPC_Non_stabilizingAnalysisInput.xml -formula "(And (And (And (And (And (And (And (And (And (And (And (And (And (< v3 2) (> v3 0)) (And (< v4 2) (> v4 0))) (And (< v5 2) (> v5 0))) (And (< v6 2) (> v6 0))) (And (> v7 0) (< v7 2))) (And (> v8 0) (< v8 2))) (And (> v10 0) (< v10 2))) (< v9 1)) (And (> v2 0) (< v2 2))) (And (And (And (And (And (And (And (And (And (And (< v3 2) (> v3 0)) (And (< v4 2) (> v4 0))) (And (< v5 2) (> v5 0))) (And (< v6 2) (> v6 0))) (And (> v7 0) (< v7 2))) (And (> v8 0) (< v8 2))) (And (> v10 0) (< v10 2))) (< v9 1)) (And (> v2 0) (< v2 2))) (And (And (< v77 1) (< v23 1)) (And (< v89 1) (And (And (> v78 0) (< v78 2)) (And (And (> v24 0) (< v24 2)) (And (> v90 0) (< v90 2)))))))) (And (And (And (And (And (And (And (And (And (And (< v3 2) (> v3 0)) (And (< v4 2) (> v4 0))) (And (< v5 2) (> v5 0))) (And (< v6 2) (> v6 0))) (And (> v7 0) (< v7 2))) (And (> v8 0) (< v8 2))) (And (> v10 0) (< v10 2))) (< v9 1)) (And (> v2 0) (< v2 2))) (And (And (And (And (And (And (And (And (And (And (< v3 2) (> v3 0)) (And (< v4 2) (> v4 0))) (And (< v5 2) (> v5 0))) (And (< v6 2) (> v6 0))) (And (> v7 0) (< v7 2))) (And (> v8 0) (< v8 2))) (And (> v10 0) (< v10 2))) (< v9 1)) (And (> v2 0) (< v2 2))) (And (And (< v77 1) (< v23 1)) (And (< v89 1) (And (And (> v78 0) (< v78 2)) (And (And (> v24 0) (< v24 2)) (And (> v90 0) (< v90 2))))))))) (Eventually (Always (And (> v7 1) (< v51 1))))) (And (Eventually (And (> v7 1) (Next (Eventually (And (< v2 1) (Next (Eventually (> v77 1)))))))) (Eventually (And (> v51 1) (Next (Eventually (And (> v77 1) (Next (Eventually (> v46 1))))))))))" > results/VPC_non_stabilizing3.txt

../bin/Debug/BioCheck.exe -naive -path 100 -outputmodel -file VPC_Non_stabilizingAnalysisInput.xml -formula "(And (And (And (And (And (And (And (And (And (And (And (And (And (< v3 2) (> v3 0)) (And (< v4 2) (> v4 0))) (And (< v5 2) (> v5 0))) (And (< v6 2) (> v6 0))) (And (> v7 0) (< v7 2))) (And (> v8 0) (< v8 2))) (And (> v10 0) (< v10 2))) (< v9 1)) (And (> v2 0) (< v2 2))) (And (And (And (And (And (And (And (And (And (And (< v3 2) (> v3 0)) (And (< v4 2) (> v4 0))) (And (< v5 2) (> v5 0))) (And (< v6 2) (> v6 0))) (And (> v7 0) (< v7 2))) (And (> v8 0) (< v8 2))) (And (> v10 0) (< v10 2))) (< v9 1)) (And (> v2 0) (< v2 2))) (And (And (< v77 1) (< v23 1)) (And (< v89 1) (And (And (> v78 0) (< v78 2)) (And (And (> v24 0) (< v24 2)) (And (> v90 0) (< v90 2)))))))) (And (And (And (And (And (And (And (And (And (And (< v3 2) (> v3 0)) (And (< v4 2) (> v4 0))) (And (< v5 2) (> v5 0))) (And (< v6 2) (> v6 0))) (And (> v7 0) (< v7 2))) (And (> v8 0) (< v8 2))) (And (> v10 0) (< v10 2))) (< v9 1)) (And (> v2 0) (< v2 2))) (And (And (And (And (And (And (And (And (And (And (< v3 2) (> v3 0)) (And (< v4 2) (> v4 0))) (And (< v5 2) (> v5 0))) (And (< v6 2) (> v6 0))) (And (> v7 0) (< v7 2))) (And (> v8 0) (< v8 2))) (And (> v10 0) (< v10 2))) (< v9 1)) (And (> v2 0) (< v2 2))) (And (And (< v77 1) (< v23 1)) (And (< v89 1) (And (And (> v78 0) (< v78 2)) (And (And (> v24 0) (< v24 2)) (And (> v90 0) (< v90 2))))))))) (Eventually (Always (And (> v7 1) (< v51 1))))) (And (Eventually (And (> v7 1) (Next (Eventually (And (< v2 1) (Next (Eventually (> v77 1)))))))) (Eventually (And (> v51 1) (Next (Eventually (And (> v77 1) (Next (Eventually (> v46 1))))))))))" > results/VPC_non_stabilizing3.naive.txt

# VPC non stabilizing property 4

../bin/Debug/BioCheck.exe -path 50 -outputmodel -file VPC_Non_stabilizingAnalysisInput.xml -formula "(And (And (And (And (And (And (And (And (And (And (And (And (And (< v3 2) (> v3 0)) (And (< v4 2) (> v4 0))) (And (< v5 2) (> v5 0))) (And (< v6 2) (> v6 0))) (And (> v7 0) (< v7 2))) (And (> v8 0) (< v8 2))) (And (> v10 0) (< v10 2))) (< v9 1)) (And (> v2 0) (< v2 2))) (And (And (And (And (And (And (And (And (And (And (< v3 2) (> v3 0)) (And (< v4 2) (> v4 0))) (And (< v5 2) (> v5 0))) (And (< v6 2) (> v6 0))) (And (> v7 0) (< v7 2))) (And (> v8 0) (< v8 2))) (And (> v10 0) (< v10 2))) (< v9 1)) (And (> v2 0) (< v2 2))) (And (And (< v77 1) (< v23 1)) (And (< v89 1) (And (And (> v78 0) (< v78 2)) (And (And (> v24 0) (< v24 2)) (And (> v90 0) (< v90 2)))))))) (And (And (And (And (And (And (And (And (And (And (< v3 2) (> v3 0)) (And (< v4 2) (> v4 0))) (And (< v5 2) (> v5 0))) (And (< v6 2) (> v6 0))) (And (> v7 0) (< v7 2))) (And (> v8 0) (< v8 2))) (And (> v10 0) (< v10 2))) (< v9 1)) (And (> v2 0) (< v2 2))) (And (And (And (And (And (And (And (And (And (And (< v3 2) (> v3 0)) (And (< v4 2) (> v4 0))) (And (< v5 2) (> v5 0))) (And (< v6 2) (> v6 0))) (And (> v7 0) (< v7 2))) (And (> v8 0) (< v8 2))) (And (> v10 0) (< v10 2))) (< v9 1)) (And (> v2 0) (< v2 2))) (And (And (< v77 1) (< v23 1)) (And (< v89 1) (And (And (> v78 0) (< v78 2)) (And (And (> v24 0) (< v24 2)) (And (> v90 0) (< v90 2))))))))) (Eventually (Always (And (> v7 1) (< v51 1))))) (Eventually (And (> v51 1) (Next (Eventually (And (< v46 1) (Next (Eventually (And (> v23 1) (Next (Eventually (< v2 1))))))))))))" > results/VPC_non_stabilizing4.txt

../bin/Debug/BioCheck.exe -naive -path 50 -outputmodel -file VPC_Non_stabilizingAnalysisInput.xml -formula "(And (And (And (And (And (And (And (And (And (And (And (And (And (< v3 2) (> v3 0)) (And (< v4 2) (> v4 0))) (And (< v5 2) (> v5 0))) (And (< v6 2) (> v6 0))) (And (> v7 0) (< v7 2))) (And (> v8 0) (< v8 2))) (And (> v10 0) (< v10 2))) (< v9 1)) (And (> v2 0) (< v2 2))) (And (And (And (And (And (And (And (And (And (And (< v3 2) (> v3 0)) (And (< v4 2) (> v4 0))) (And (< v5 2) (> v5 0))) (And (< v6 2) (> v6 0))) (And (> v7 0) (< v7 2))) (And (> v8 0) (< v8 2))) (And (> v10 0) (< v10 2))) (< v9 1)) (And (> v2 0) (< v2 2))) (And (And (< v77 1) (< v23 1)) (And (< v89 1) (And (And (> v78 0) (< v78 2)) (And (And (> v24 0) (< v24 2)) (And (> v90 0) (< v90 2)))))))) (And (And (And (And (And (And (And (And (And (And (< v3 2) (> v3 0)) (And (< v4 2) (> v4 0))) (And (< v5 2) (> v5 0))) (And (< v6 2) (> v6 0))) (And (> v7 0) (< v7 2))) (And (> v8 0) (< v8 2))) (And (> v10 0) (< v10 2))) (< v9 1)) (And (> v2 0) (< v2 2))) (And (And (And (And (And (And (And (And (And (And (< v3 2) (> v3 0)) (And (< v4 2) (> v4 0))) (And (< v5 2) (> v5 0))) (And (< v6 2) (> v6 0))) (And (> v7 0) (< v7 2))) (And (> v8 0) (< v8 2))) (And (> v10 0) (< v10 2))) (< v9 1)) (And (> v2 0) (< v2 2))) (And (And (< v77 1) (< v23 1)) (And (< v89 1) (And (And (> v78 0) (< v78 2)) (And (And (> v24 0) (< v24 2)) (And (> v90 0) (< v90 2))))))))) (Eventually (Always (And (> v7 1) (< v51 1))))) (Eventually (And (> v51 1) (Next (Eventually (And (< v46 1) (Next (Eventually (And (> v23 1) (Next (Eventually (< v2 1))))))))))))" > results/VPC_non_stabilizing4.naive.txt

# Bcr-Abl has a loop where STAT 5 oscilates between high and medium

../bin/Debug/BioCheck.exe -file Bcr-AblAnalysisInput.xml -outputmodel -path 10 -formula "(And (Always (Eventually (< v24 2))) (Always (Eventually (> v24 1))))" > results/Bcr-Abl1.10.txt

../bin/Debug/BioCheck.exe -file Bcr-AblAnalysisInput.xml -outputmodel -path 10 -formula "(And (Always (Eventually (< v24 2))) (Always (Eventually (> v24 1))))" -naive > results/Bcr-Abl1.10.naive.txt

../bin/Debug/BioCheck.exe -file Bcr-AblAnalysisInput.xml -outputmodel -path 20 -formula "(And (Always (Eventually (< v24 2))) (Always (Eventually (> v24 1))))" > results/Bcr-Abl1.20.txt

../bin/Debug/BioCheck.exe -file Bcr-AblAnalysisInput.xml -outputmodel -path 20 -formula "(And (Always (Eventually (< v24 2))) (Always (Eventually (> v24 1))))" -naive > results/Bcr-Abl1.20.naive.txt

../bin/Debug/BioCheck.exe -file Bcr-AblAnalysisInput.xml -outputmodel -path 30 -formula "(And (Always (Eventually (< v24 2))) (Always (Eventually (> v24 1))))" > results/Bcr-Abl1.30.txt

../bin/Debug/BioCheck.exe -file Bcr-AblAnalysisInput.xml -outputmodel -path 30 -formula "(And (Always (Eventually (< v24 2))) (Always (Eventually (> v24 1))))" -naive > results/Bcr-Abl1.30.naive.txt

../bin/Debug/BioCheck.exe -file Bcr-AblAnalysisInput.xml -outputmodel -path 40 -formula "(And (Always (Eventually (< v24 2))) (Always (Eventually (> v24 1))))" > results/Bcr-Abl1.40.txt

../bin/Debug/BioCheck.exe -file Bcr-AblAnalysisInput.xml -outputmodel -path 40 -formula "(And (Always (Eventually (< v24 2))) (Always (Eventually (> v24 1))))" -naive > results/Bcr-Abl1.40.naive.txt

../bin/Debug/BioCheck.exe -file Bcr-AblAnalysisInput.xml -outputmodel -path 50 -formula "(And (Always (Eventually (< v24 2))) (Always (Eventually (> v24 1))))" > results/Bcr-Abl1.50.txt

../bin/Debug/BioCheck.exe -file Bcr-AblAnalysisInput.xml -outputmodel -path 50 -formula "(And (Always (Eventually (< v24 2))) (Always (Eventually (> v24 1))))" -naive > results/Bcr-Abl1.50.naive.txt

# Bcr-Abl has no loops where STAT5 has the value off/low

../bin/Debug/BioCheck.exe -file Bcr-AblAnalysisInput.xml -outputmodel -path 10 -formula "(Always (Eventually (< v24 1)))" > results/Bcr-Abl2.10.txt

../bin/Debug/BioCheck.exe -file Bcr-AblAnalysisInput.xml -outputmodel -path 10 -formula "(Always (Eventually (< v24 1)))" -naive > results/Bcr-Abl2.10.naive.txt

../bin/Debug/BioCheck.exe -file Bcr-AblAnalysisInput.xml -outputmodel -path 20 -formula "(Always (Eventually (< v24 1)))" > results/Bcr-Abl2.20.txt

../bin/Debug/BioCheck.exe -file Bcr-AblAnalysisInput.xml -outputmodel -path 20 -formula "(Always (Eventually (< v24 1)))" -naive > results/Bcr-Abl2.20.naive.txt

../bin/Debug/BioCheck.exe -file Bcr-AblAnalysisInput.xml -outputmodel -path 30 -formula "(Always (Eventually (< v24 1)))" > results/Bcr-Abl2.30.txt

../bin/Debug/BioCheck.exe -file Bcr-AblAnalysisInput.xml -outputmodel -path 30 -formula "(Always (Eventually (< v24 1)))" -naive > results/Bcr-Abl2.30.naive.txt

../bin/Debug/BioCheck.exe -file Bcr-AblAnalysisInput.xml -outputmodel -path 40 -formula "(Always (Eventually (< v24 1)))" > results/Bcr-Abl2.40.txt

../bin/Debug/BioCheck.exe -file Bcr-AblAnalysisInput.xml -outputmodel -path 40 -formula "(Always (Eventually (< v24 1)))" -naive > results/Bcr-Abl2.40.naive.txt

../bin/Debug/BioCheck.exe -file Bcr-AblAnalysisInput.xml -outputmodel -path 50 -formula "(Always (Eventually (< v24 1)))" > results/Bcr-Abl2.50.txt

../bin/Debug/BioCheck.exe -file Bcr-AblAnalysisInput.xml -outputmodel -path 50 -formula "(Always (Eventually (< v24 1)))" -naive > results/Bcr-Abl2.50.naive.txt


# Bcr-Abl has no loop where STAT 3 is high at some point 

../bin/Debug/BioCheck.exe -file Bcr-AblAnalysisInput.xml -outputmodel -path 10 -formula "(Always (Eventually (> v12 1)))" > results/Bcr-Abl3.10.txt

../bin/Debug/BioCheck.exe -file Bcr-AblAnalysisInput.xml -outputmodel -path 10 -formula "(Always (Eventually (> v12 1)))" -naive > results/Bcr-Abl3.10.naive.txt

../bin/Debug/BioCheck.exe -file Bcr-AblAnalysisInput.xml -outputmodel -path 20 -formula "(Always (Eventually (> v12 1)))" > results/Bcr-Abl3.20.txt

../bin/Debug/BioCheck.exe -file Bcr-AblAnalysisInput.xml -outputmodel -path 20 -formula "(Always (Eventually (> v12 1)))" -naive > results/Bcr-Abl3.20.naive.txt

../bin/Debug/BioCheck.exe -file Bcr-AblAnalysisInput.xml -outputmodel -path 30 -formula "(Always (Eventually (> v12 1)))" > results/Bcr-Abl3.30.txt

../bin/Debug/BioCheck.exe -file Bcr-AblAnalysisInput.xml -outputmodel -path 30 -formula "(Always (Eventually (> v12 1)))" -naive > results/Bcr-Abl3.30.naive.txt

../bin/Debug/BioCheck.exe -file Bcr-AblAnalysisInput.xml -outputmodel -path 40 -formula "(Always (Eventually (> v12 1)))" > results/Bcr-Abl3.40.txt

../bin/Debug/BioCheck.exe -file Bcr-AblAnalysisInput.xml -outputmodel -path 40 -formula "(Always (Eventually (> v12 1)))" -naive > results/Bcr-Abl3.40.naive.txt

../bin/Debug/BioCheck.exe -file Bcr-AblAnalysisInput.xml -outputmodel -path 50 -formula "(Always (Eventually (> v12 1)))" > results/Bcr-Abl3.50.txt

../bin/Debug/BioCheck.exe -file Bcr-AblAnalysisInput.xml -outputmodel -path 50 -formula "(Always (Eventually (> v12 1)))" -naive > results/Bcr-Abl3.50.naive.txt

# Bcr-Abl has a loop where STAT 3 oscilates between off and low

../bin/Debug/BioCheck.exe -file Bcr-AblAnalysisInput.xml -outputmodel -path 10 -formula "(And (Always (Eventually (< v12 1))) (Always (Eventually (> v12 0))))" > results/Bcr-Abl4.10.txt

../bin/Debug/BioCheck.exe -file Bcr-AblAnalysisInput.xml -outputmodel -path 10 -formula "(And (Always (Eventually (< v12 1))) (Always (Eventually (> v12 0))))" -naive > results/Bcr-Abl4.10.naive.txt

../bin/Debug/BioCheck.exe -file Bcr-AblAnalysisInput.xml -outputmodel -path 20 -formula "(And (Always (Eventually (< v12 1))) (Always (Eventually (> v12 0))))" > results/Bcr-Abl4.20.txt

../bin/Debug/BioCheck.exe -file Bcr-AblAnalysisInput.xml -outputmodel -path 20 -formula "(And (Always (Eventually (< v12 1))) (Always (Eventually (> v12 0))))" -naive > results/Bcr-Abl4.20.naive.txt

../bin/Debug/BioCheck.exe -file Bcr-AblAnalysisInput.xml -outputmodel -path 30 -formula "(And (Always (Eventually (< v12 1))) (Always (Eventually (> v12 0))))" > results/Bcr-Abl4.30.txt

../bin/Debug/BioCheck.exe -file Bcr-AblAnalysisInput.xml -outputmodel -path 30 -formula "(And (Always (Eventually (< v12 1))) (Always (Eventually (> v12 0))))" -naive > results/Bcr-Abl4.30.naive.txt

../bin/Debug/BioCheck.exe -file Bcr-AblAnalysisInput.xml -outputmodel -path 40 -formula "(And (Always (Eventually (< v12 1))) (Always (Eventually (> v12 0))))" > results/Bcr-Abl4.40.txt

../bin/Debug/BioCheck.exe -file Bcr-AblAnalysisInput.xml -outputmodel -path 40 -formula "(And (Always (Eventually (< v12 1))) (Always (Eventually (> v12 0))))" -naive > results/Bcr-Abl4.40.naive.txt

../bin/Debug/BioCheck.exe -file Bcr-AblAnalysisInput.xml -outputmodel -path 50 -formula "(And (Always (Eventually (< v12 1))) (Always (Eventually (> v12 0))))" > results/Bcr-Abl4.50.txt

../bin/Debug/BioCheck.exe -file Bcr-AblAnalysisInput.xml -outputmodel -path 50 -formula "(And (Always (Eventually (< v12 1))) (Always (Eventually (> v12 0))))" -naive > results/Bcr-Abl4.50.naive.txt

# Bcr-AblNoFeedbacks has a loop where STAT 5 is high

../bin/Debug/BioCheck.exe -file Bcr-AblNoFeedbacksAnalysisInput.xml -outputmodel -path 10 -formula "(Always (Eventually (> v24 1)))" > results/Bcr-AblNoFeedbacks1.10.txt

../bin/Debug/BioCheck.exe -file Bcr-AblNoFeedbacksAnalysisInput.xml -outputmodel -path 10 -formula "(Always (Eventually (> v24 1)))" -naive > results/Bcr-AblNoFeedbacks1.10.naive.txt

../bin/Debug/BioCheck.exe -file Bcr-AblNoFeedbacksAnalysisInput.xml -outputmodel -path 20 -formula "(Always (Eventually (> v24 1)))" > results/Bcr-AblNoFeedbacks1.20.txt

../bin/Debug/BioCheck.exe -file Bcr-AblNoFeedbacksAnalysisInput.xml -outputmodel -path 20 -formula "(Always (Eventually (> v24 1)))" -naive > results/Bcr-AblNoFeedbacks1.20.naive.txt

../bin/Debug/BioCheck.exe -file Bcr-AblNoFeedbacksAnalysisInput.xml -outputmodel -path 30 -formula "(Always (Eventually (> v24 1)))" > results/Bcr-AblNoFeedbacks1.30.txt

../bin/Debug/BioCheck.exe -file Bcr-AblNoFeedbacksAnalysisInput.xml -outputmodel -path 30 -formula "(Always (Eventually (> v24 1)))" -naive > results/Bcr-AblNoFeedbacks1.30.naive.txt

../bin/Debug/BioCheck.exe -file Bcr-AblNoFeedbacksAnalysisInput.xml -outputmodel -path 40 -formula "(Always (Eventually (> v24 1)))" > results/Bcr-AblNoFeedbacks1.40.txt

../bin/Debug/BioCheck.exe -file Bcr-AblNoFeedbacksAnalysisInput.xml -outputmodel -path 40 -formula "(Always (Eventually (> v24 1)))" -naive > results/Bcr-AblNoFeedbacks1.40.naive.txt

../bin/Debug/BioCheck.exe -file Bcr-AblNoFeedbacksAnalysisInput.xml -outputmodel -path 50 -formula "(Always (Eventually (> v24 1)))" > results/Bcr-AblNoFeedbacks1.50.txt

../bin/Debug/BioCheck.exe -file Bcr-AblNoFeedbacksAnalysisInput.xml -outputmodel -path 50 -formula "(Always (Eventually (> v24 1)))" -naive > results/Bcr-AblNoFeedbacks1.50.naive.txt

# Bcr-AblNoFeedbacks has no loop where STAT5 is off/low 

../bin/Debug/BioCheck.exe -file Bcr-AblNoFeedbacksAnalysisInput.xml -outputmodel -path 10 -formula "(Always (Eventually (< v24 2)))" > results/Bcr-AblNoFeedbacks2.10.txt

../bin/Debug/BioCheck.exe -file Bcr-AblNoFeedbacksAnalysisInput.xml -outputmodel -path 10 -formula "(Always (Eventually (< v24 2)))" -naive > results/Bcr-AblNoFeedbacks2.10.naive.txt

../bin/Debug/BioCheck.exe -file Bcr-AblNoFeedbacksAnalysisInput.xml -outputmodel -path 20 -formula "(Always (Eventually (< v24 2)))" > results/Bcr-AblNoFeedbacks2.20.txt

../bin/Debug/BioCheck.exe -file Bcr-AblNoFeedbacksAnalysisInput.xml -outputmodel -path 20 -formula "(Always (Eventually (< v24 2)))" -naive > results/Bcr-AblNoFeedbacks2.20.naive.txt

../bin/Debug/BioCheck.exe -file Bcr-AblNoFeedbacksAnalysisInput.xml -outputmodel -path 30 -formula "(Always (Eventually (< v24 2)))" > results/Bcr-AblNoFeedbacks2.30.txt

../bin/Debug/BioCheck.exe -file Bcr-AblNoFeedbacksAnalysisInput.xml -outputmodel -path 30 -formula "(Always (Eventually (< v24 2)))" -naive > results/Bcr-AblNoFeedbacks2.30.naive.txt

../bin/Debug/BioCheck.exe -file Bcr-AblNoFeedbacksAnalysisInput.xml -outputmodel -path 40 -formula "(Always (Eventually (< v24 2)))" > results/Bcr-AblNoFeedbacks2.40.txt

../bin/Debug/BioCheck.exe -file Bcr-AblNoFeedbacksAnalysisInput.xml -outputmodel -path 40 -formula "(Always (Eventually (< v24 2)))" -naive > results/Bcr-AblNoFeedbacks2.40.naive.txt

../bin/Debug/BioCheck.exe -file Bcr-AblNoFeedbacksAnalysisInput.xml -outputmodel -path 50 -formula "(Always (Eventually (< v24 2)))" > results/Bcr-AblNoFeedbacks2.50.txt

../bin/Debug/BioCheck.exe -file Bcr-AblNoFeedbacksAnalysisInput.xml -outputmodel -path 50 -formula "(Always (Eventually (< v24 2)))" -naive > results/Bcr-AblNoFeedbacks2.50.naive.txt


# Bcr-AblNoFeedbacks has a loop where STAT 3 is high 

../bin/Debug/BioCheck.exe -file Bcr-AblNoFeedbacksAnalysisInput.xml -outputmodel -path 10 -formula "(Always (Eventually (> v12 1)))" > results/Bcr-AblNoFeedbacks3.10.txt

../bin/Debug/BioCheck.exe -file Bcr-AblNoFeedbacksAnalysisInput.xml -outputmodel -path 10 -formula "(Always (Eventually (> v12 1)))" -naive > results/Bcr-AblNoFeedbacks3.10.naive.txt

../bin/Debug/BioCheck.exe -file Bcr-AblNoFeedbacksAnalysisInput.xml -outputmodel -path 20 -formula "(Always (Eventually (> v12 1)))" > results/Bcr-AblNoFeedbacks3.20.txt

../bin/Debug/BioCheck.exe -file Bcr-AblNoFeedbacksAnalysisInput.xml -outputmodel -path 20 -formula "(Always (Eventually (> v12 1)))" -naive > results/Bcr-AblNoFeedbacks3.20.naive.txt

../bin/Debug/BioCheck.exe -file Bcr-AblNoFeedbacksAnalysisInput.xml -outputmodel -path 30 -formula "(Always (Eventually (> v12 1)))" > results/Bcr-AblNoFeedbacks3.30.txt

../bin/Debug/BioCheck.exe -file Bcr-AblNoFeedbacksAnalysisInput.xml -outputmodel -path 30 -formula "(Always (Eventually (> v12 1)))" -naive > results/Bcr-AblNoFeedbacks3.30.naive.txt

../bin/Debug/BioCheck.exe -file Bcr-AblNoFeedbacksAnalysisInput.xml -outputmodel -path 40 -formula "(Always (Eventually (> v12 1)))" > results/Bcr-AblNoFeedbacks3.40.txt

../bin/Debug/BioCheck.exe -file Bcr-AblNoFeedbacksAnalysisInput.xml -outputmodel -path 40 -formula "(Always (Eventually (> v12 1)))" -naive > results/Bcr-AblNoFeedbacks3.40.naive.txt

../bin/Debug/BioCheck.exe -file Bcr-AblNoFeedbacksAnalysisInput.xml -outputmodel -path 50 -formula "(Always (Eventually (> v12 1)))" > results/Bcr-AblNoFeedbacks3.50.txt

../bin/Debug/BioCheck.exe -file Bcr-AblNoFeedbacksAnalysisInput.xml -outputmodel -path 50 -formula "(Always (Eventually (> v12 1)))" -naive > results/Bcr-AblNoFeedbacks3.50.naive.txt

# Bcr-AblNoFeedbacks has no loop where STAT 3 is not high

../bin/Debug/BioCheck.exe -file Bcr-AblNoFeedbacksAnalysisInput.xml -outputmodel -path 10 -formula "(Always (Eventually (< v12 2)))" > results/Bcr-AblNoFeedbacks4.10.txt

../bin/Debug/BioCheck.exe -file Bcr-AblNoFeedbacksAnalysisInput.xml -outputmodel -path 10 -formula "(Always (Eventually (< v12 2)))" -naive > results/Bcr-AblNoFeedbacks4.10.naive.txt

../bin/Debug/BioCheck.exe -file Bcr-AblNoFeedbacksAnalysisInput.xml -outputmodel -path 20 -formula "(Always (Eventually (< v12 2)))" > results/Bcr-AblNoFeedbacks4.20.txt

../bin/Debug/BioCheck.exe -file Bcr-AblNoFeedbacksAnalysisInput.xml -outputmodel -path 20 -formula "(Always (Eventually (< v12 2)))" -naive > results/Bcr-AblNoFeedbacks4.20.naive.txt

../bin/Debug/BioCheck.exe -file Bcr-AblNoFeedbacksAnalysisInput.xml -outputmodel -path 30 -formula "(Always (Eventually (< v12 2)))" > results/Bcr-AblNoFeedbacks4.30.txt

../bin/Debug/BioCheck.exe -file Bcr-AblNoFeedbacksAnalysisInput.xml -outputmodel -path 30 -formula "(Always (Eventually (< v12 2)))" -naive > results/Bcr-AblNoFeedbacks4.30.naive.txt

../bin/Debug/BioCheck.exe -file Bcr-AblNoFeedbacksAnalysisInput.xml -outputmodel -path 40 -formula "(Always (Eventually (< v12 2)))" > results/Bcr-AblNoFeedbacks4.40.txt

../bin/Debug/BioCheck.exe -file Bcr-AblNoFeedbacksAnalysisInput.xml -outputmodel -path 40 -formula "(Always (Eventually (< v12 2)))" -naive > results/Bcr-AblNoFeedbacks4.40.naive.txt

../bin/Debug/BioCheck.exe -file Bcr-AblNoFeedbacksAnalysisInput.xml -outputmodel -path 50 -formula "(Always (Eventually (< v12 2)))" > results/Bcr-AblNoFeedbacks4.50.txt

../bin/Debug/BioCheck.exe -file Bcr-AblNoFeedbacksAnalysisInput.xml -outputmodel -path 50 -formula "(Always (Eventually (< v12 2)))" -naive > results/Bcr-AblNoFeedbacks4.50.naive.txt


cd bin
