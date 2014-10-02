for i in $(seq 0 9); do
cat $1 | sed "s/timer(0)/timer(${i})/" > testing.smv
cat $2 | sed "s/$1/testing.smv/" > current_pred.txt
echo $i
export result=$(time -p /cygdrive/c/Program\ Files/NuSMV/2.5.4/bin/NuSMV.exe -load current_pred.txt | grep real)
echo "===="
done

for i in "19" "29" "39" "49"; do
cat $1 | sed "s/timer(0)/timer(${i})/" > testing.smv
cat $2 | sed "s/$1/testing.smv/" > current_pred.txt
echo $i
export result=$(time -p /cygdrive/c/Program\ Files/NuSMV/2.5.4/bin/NuSMV.exe -load current_pred.txt | grep real)
echo "===="
done
